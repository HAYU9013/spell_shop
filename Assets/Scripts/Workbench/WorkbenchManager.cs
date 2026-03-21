using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 工作台管理器（MonoBehaviour Singleton）。
/// 負責：
///   - 選擇卷軸
///   - 符文槽位管理（放入 / 取出）
///   - 送出（觸發 Phase 4 解除阻塞）
///   - 維護送出歷史 List（index 0 = 最新）
///
/// 與其他系統的關係：
///   DeckManager     — 放入符文時從手牌移除；取消時歸還；送出後依類型棄/消耗
///   TurnManager     — Submit() 最後呼叫 TurnManager.PlayerSubmit() 解除阻塞
///   EnvironmentResolveState — 從 LastSubmit 取得本回合送出資料交給 ScrollProcessor
/// </summary>
public class WorkbenchManager : MonoBehaviour
{
    // =========================================================
    // Singleton
    // =========================================================

    public static WorkbenchManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // =========================================================
    // 事件
    // =========================================================

    /// <summary>選擇的卷軸或槽位內容改變時觸發（供工作台 UI 重繪）</summary>
    public event System.Action OnWorkbenchChanged;

    /// <summary>送出成功後觸發（供 UI 播放動畫）</summary>
    public event System.Action<SubmitRecord> OnSubmitted;

    // =========================================================
    // 私有欄位
    // =========================================================

    private ScrollData _selectedScroll;

    /// <summary>目前槽位內的符文（null = 空槽）</summary>
    private RuneInstance[] _slots = new RuneInstance[0];

    /// <summary>送出歷史，index 0 = 最新</summary>
    private readonly List<SubmitRecord> _history = new List<SubmitRecord>();

    private TurnManager _turnManager;

    // =========================================================
    // 只讀屬性
    // =========================================================

    public ScrollData SelectedScroll => _selectedScroll;

    /// <summary>目前槽位快照（唯讀）</summary>
    public IReadOnlyList<RuneInstance> Slots => _slots;

    /// <summary>最後一次送出紀錄（Phase 5 用）</summary>
    public SubmitRecord LastSubmit => _history.Count > 0 ? _history[0] : null;

    /// <summary>送出歷史（index 0 = 最新）</summary>
    public IReadOnlyList<SubmitRecord> History => _history;

    // =========================================================
    // 初始化
    // =========================================================>

    public void Init(TurnManager turnManager)
    {
        _turnManager = turnManager;
    }

    // =========================================================
    // 卷軸選擇
    // =========================================================

    /// <summary>
    /// 選擇卷軸，重置所有槽位（已放入的符文歸還手牌）。
    /// </summary>
    public void SelectScroll(ScrollData scroll)
    {
        if (scroll == null) return;

        // 歸還所有槽位中的符文
        ClearSlots();

        _selectedScroll = scroll;
        _slots = new RuneInstance[scroll.slotCount];

        OnWorkbenchChanged?.Invoke();
        Debug.Log($"[WorkbenchManager] 選擇卷軸：{scroll.scrollName}（槽×{scroll.slotCount}）");
    }

    // =========================================================
    // 符文槽位操作
    // =========================================================

    /// <summary>
    /// 將符文放入下一個空槽。
    /// 同時從 DeckManager 手牌移除。
    /// 回傳放入的槽位 index，-1 表示無空槽或符文不在手牌。
    /// </summary>
    public int PlaceRune(RuneInstance rune)
    {
        if (_selectedScroll == null)
        {
            Debug.LogWarning("[WorkbenchManager] 尚未選擇卷軸");
            return -1;
        }

        int slotIndex = FindNextEmptySlot();
        if (slotIndex < 0)
        {
            Debug.LogWarning("[WorkbenchManager] 所有槽位已填滿");
            return -1;
        }

        if (!DeckManager.Instance.RemoveFromHand(rune))
        {
            Debug.LogWarning($"[WorkbenchManager] 符文不在手牌中：{rune.Data.runeName}");
            return -1;
        }

        _slots[slotIndex] = rune;
        OnWorkbenchChanged?.Invoke();
        Debug.Log($"[WorkbenchManager] 放入符文：{rune.Data.runeName} → 槽[{slotIndex}]");
        return slotIndex;
    }

    /// <summary>
    /// 移除指定槽位的符文，歸還手牌。
    /// </summary>
    public void RemoveRune(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _slots.Length) return;
        if (_slots[slotIndex] == null) return;

        var rune = _slots[slotIndex];
        _slots[slotIndex] = null;

        DeckManager.Instance.AddToHand(rune);
        OnWorkbenchChanged?.Invoke();
        Debug.Log($"[WorkbenchManager] 取消符文：{rune.Data.runeName} ← 槽[{slotIndex}]");
    }

    /// <summary>所有槽位均已填入符文時回傳 true</summary>
    public bool CanSubmit()
    {
        if (_selectedScroll == null || _slots.Length == 0) return false;
        foreach (var slot in _slots)
            if (slot == null) return false;
        return true;
    }

    // =========================================================
    // 送出
    // =========================================================

    /// <summary>
    /// 驗證 CanSubmit → 建立 SubmitRecord → 處理符文後續
    ///   Cycle 符文    → DeckManager.DiscardRune()
    ///   Consumable 符文 → DeckManager.ExhaustRune()
    /// → 推入歷史 List 頭部（index 0）
    /// → 呼叫 TurnManager.PlayerSubmit() 解除 Phase 4 阻塞
    /// </summary>
    public void Submit()
    {
        if (!CanSubmit())
        {
            Debug.LogWarning("[WorkbenchManager] 尚未填滿所有槽位，無法送出");
            return;
        }

        int turnNumber = _turnManager != null ? _turnManager.CurrentTurn : -1;

        // 收集本次 Rune 資料
        var runeData = new RuneData[_slots.Length];
        for (int i = 0; i < _slots.Length; i++)
            runeData[i] = _slots[i].Data;

        // 建立歷史紀錄，插入 index 0（最新）
        var record = new SubmitRecord(turnNumber, _selectedScroll, runeData);
        _history.Insert(0, record);

        // 處理符文後續
        foreach (var rune in _slots)
        {
            if (rune.Data.runeType == RuneType.Cycle)
                DeckManager.Instance.DiscardRune(rune);
            else
                DeckManager.Instance.ExhaustRune(rune);
        }

        // 清空槽位（不重置卷軸，玩家下回合可繼續用同一張）
        for (int i = 0; i < _slots.Length; i++)
            _slots[i] = null;

        OnSubmitted?.Invoke(record);
        OnWorkbenchChanged?.Invoke();

        Debug.Log($"[WorkbenchManager] 送出：{record}");

        // 解除 Phase 4 阻塞
        _turnManager?.PlayerSubmit();
    }

    // =========================================================
    // 重置（新局 / 選新卷軸）
    // =========================================================

    /// <summary>歸還所有槽位符文至手牌並清空</summary>
    private void ClearSlots()
    {
        if (DeckManager.Instance == null) return;
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] != null)
            {
                DeckManager.Instance.AddToHand(_slots[i]);
                _slots[i] = null;
            }
        }
    }

    // =========================================================
    // 工具
    // =========================================================

    private int FindNextEmptySlot()
    {
        for (int i = 0; i < _slots.Length; i++)
            if (_slots[i] == null) return i;
        return -1;
    }

    // =========================================================
    // Debug
    // =========================================================

    [ContextMenu("Debug_PrintWorkbench")]
    private void Debug_PrintWorkbench()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        Debug.Log($"[WorkbenchManager] 卷軸：{(_selectedScroll != null ? _selectedScroll.scrollName : "未選擇")}");
        for (int i = 0; i < _slots.Length; i++)
            Debug.Log($"  槽[{i}]：{(_slots[i] != null ? _slots[i].Data.runeName : "空")}");
    }

    [ContextMenu("Debug_PrintHistory")]
    private void Debug_PrintHistory()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        if (_history.Count == 0) { Debug.Log("[WorkbenchManager] 歷史紀錄為空"); return; }
        foreach (var r in _history)
            Debug.Log($"  {r}");
    }

    [ContextMenu("Debug_ForceSubmit")]
    private void Debug_ForceSubmit()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        Submit();
    }
}

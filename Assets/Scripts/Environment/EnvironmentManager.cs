using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 環境管理器（MonoBehaviour Singleton）。
/// 持有 EnvironmentData 並提供統一的讀寫介面。
/// 負責：
///   - 套用符文效果（ApplyEffects）
///   - 業績修改（ModifyScore）
///   - 環境快照（SnapshotCurrent）
///   - 極端事件檢查與觸發（CheckAndTriggerExtremeEvents）
///   - 發出數值變化事件供 UI 訂閱
/// </summary>
public class EnvironmentManager : MonoBehaviour
{
    // =========================================================
    // Singleton
    // =========================================================

    public static EnvironmentManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _extremeEventHandler = new ExtremeEventHandler();
        _extremeEventHandler.OnDarknessTriggered += HandleDarknessTriggered;

        _env = new EnvironmentData();
    }

    private void OnDestroy()
    {
        if (_extremeEventHandler != null)
            _extremeEventHandler.OnDarknessTriggered -= HandleDarknessTriggered;
    }

    // =========================================================
    // 事件（供 UI 及其他系統訂閱）
    // =========================================================

    /// <summary>任何環境屬性數值改變後觸發</summary>
    public event System.Action<EnvAttribute, int> OnValueChanged;

    /// <summary>業績改變後觸發，參數為新業績值</summary>
    public event System.Action<int> OnScoreChanged;

    /// <summary>極端事件觸發後通知，參數為觸發的事件列表</summary>
    public event System.Action<List<ExtremeEvent>> OnExtremeEventTriggered;

    /// <summary>黑暗降臨時轉發給 RelicManager</summary>
    public event System.Action OnDarknessTriggered;

    // =========================================================
    // 私有欄位
    // =========================================================

    private EnvironmentData _env;
    private ExtremeEventHandler _extremeEventHandler;

    /// <summary>極端事件在同一回合的連鎖觸發最大次數（防無限迴圈）</summary>
    private const int MAX_CHAIN_ITERATIONS = 10;

    /// <summary>環境歷史紀錄，頭（First）= 最新，尾（Last）= 最舊</summary>
    private readonly LinkedList<EnvironmentData> _history = new LinkedList<EnvironmentData>();

    /// <summary>歷史紀錄最大保留回合數（Inspector 可調）</summary>
    [SerializeField] private int _maxHistorySize = 20;

    // =========================================================
    // 只讀屬性
    // =========================================================

    public int Brightness => _env.brightness;
    public int Moisture => _env.moisture;
    public int Temperature => _env.temperature;
    public int Score => _env.score;
    public bool IsScoreZero => _env.score <= 0;

    public int GetValue(EnvAttribute attr) => _env.GetValue(attr);

    // =========================================================
    // 快照
    // =========================================================

    /// <summary>回傳當前環境資料的深拷貝快照（用於顧客到來時記錄基準線）</summary>
    public EnvironmentData SnapshotCurrent()
    {
        return _env.Clone();
    }

    // =========================================================
    // 歷史紀錄（Deque：頭 = 最新，尾 = 最舊）
    // =========================================================

    /// <summary>目前儲存的歷史筆數</summary>
    public int HistoryCount => _history.Count;

    /// <summary>
    /// 將當前環境快照推入歷史紀錄的頭部（最新）。
    /// 超過 _maxHistorySize 時自動移除尾部（最舊）。
    /// 建議在每回合 Phase 5 結算結束後呼叫。
    /// </summary>
    public void RecordSnapshot()
    {
        _history.AddFirst(_env.Clone());

        while (_history.Count > _maxHistorySize)
            _history.RemoveLast();
    }

    /// <summary>
    /// 取得歷史紀錄中第 index 筆快照。
    /// index 0 = 最新（本回合），index 1 = 上回合，以此類推。
    /// 超出範圍回傳 null。
    /// </summary>
    public EnvironmentData GetHistoryAt(int index)
    {
        if (index < 0 || index >= _history.Count) return null;

        var node = _history.First;
        for (int i = 0; i < index; i++)
            node = node.Next;

        return node.Value;
    }

    /// <summary>最新一筆快照（等同 GetHistoryAt(0)），無紀錄時回傳 null</summary>
    public EnvironmentData NewestHistory => _history.First?.Value;

    /// <summary>最舊一筆快照，無紀錄時回傳 null</summary>
    public EnvironmentData OldestHistory => _history.Last?.Value;

    /// <summary>以唯讀方式存取完整歷史（頭 = 最新）</summary>
    public IReadOnlyCollection<EnvironmentData> History => _history;

    // =========================================================
    // 效果套用
    // =========================================================

    /// <summary>
    /// 套用一組 RuneEffect（來自 ScrollProcessor 計算結果）。
    /// 套用完畢後自動呼叫 CheckAndTriggerExtremeEvents。
    /// </summary>
    /// <param name="effects">要套用的效果列表</param>
    /// <returns>本次觸發的極端事件列表</returns>
    public List<ExtremeEvent> ApplyEffects(List<RuneEffect> effects)
    {
        var hitBoundaryFlags = new Dictionary<EnvAttribute, int>();

        foreach (var effect in effects)
        {
            bool hit = _env.ApplyDelta(effect.attribute, effect.value);
            if (hit)
            {
                int newVal = _env.GetValue(effect.attribute);
                // 記錄觸碰的邊界值（0 或 MAX_VALUE）
                hitBoundaryFlags[effect.attribute] =
                    (newVal <= EnvironmentData.MIN_VALUE) ? EnvironmentData.MIN_VALUE : EnvironmentData.MAX_VALUE;
            }
            OnValueChanged?.Invoke(effect.attribute, _env.GetValue(effect.attribute));
        }

        return CheckAndTriggerExtremeEvents(hitBoundaryFlags);
    }

    /// <summary>直接設定某屬性的數值（遺物強制效果用）</summary>
    public void ForceSetValue(EnvAttribute attr, int value)
    {
        bool hit = _env.SetValue(attr, value);
        OnValueChanged?.Invoke(attr, _env.GetValue(attr));

        if (hit)
        {
            int newVal = _env.GetValue(attr);
            var flags = new Dictionary<EnvAttribute, int>
            {
                { attr, (newVal <= EnvironmentData.MIN_VALUE) ? EnvironmentData.MIN_VALUE : EnvironmentData.MAX_VALUE }
            };
            var triggered = CheckAndTriggerExtremeEvents(flags);
            if (triggered.Count > 0)
                OnExtremeEventTriggered?.Invoke(triggered);
        }
    }

    // =========================================================
    // 業績
    // =========================================================

    /// <summary>修改業績，觸發 OnScoreChanged 事件</summary>
    public void ModifyScore(int delta)
    {
        _env.ModifyScore(delta);
        OnScoreChanged?.Invoke(_env.score);
    }

    // =========================================================
    // 極端事件
    // =========================================================

    /// <summary>
    /// 根據 hitBoundaryFlags 觸發極端事件，支援連鎖（大洪水可再次推高水分）。
    /// 由 ApplyEffects 內部呼叫；外部一般不直接呼叫。
    /// </summary>
    private List<ExtremeEvent> CheckAndTriggerExtremeEvents(Dictionary<EnvAttribute, int> hitBoundaryFlags)
    {
        var allTriggered = new List<ExtremeEvent>();

        if (hitBoundaryFlags == null || hitBoundaryFlags.Count == 0)
            return allTriggered;

        var currentFlags = hitBoundaryFlags;
        int iterations = 0;

        while (currentFlags.Count > 0 && iterations < MAX_CHAIN_ITERATIONS)
        {
            iterations++;

            // 記錄套用前水分，以偵測洪水是否再次推到邊界
            int moistureBefore = _env.moisture;

            var triggered = _extremeEventHandler.Process(_env, currentFlags);
            allTriggered.AddRange(triggered);

            // 通知數值更新（極端事件可能改變數值）
            foreach (var attr in currentFlags.Keys)
                OnValueChanged?.Invoke(attr, _env.GetValue(attr));

            // 業績也可能被極端事件修改
            OnScoreChanged?.Invoke(_env.score);

            // 檢查是否因洪水再次觸碰水分上限（連鎖）
            currentFlags = new Dictionary<EnvAttribute, int>();
            if (triggered.Contains(ExtremeEvent.Flood) && _env.moisture >= EnvironmentData.MAX_VALUE)
            {
                currentFlags[EnvAttribute.Moisture] = EnvironmentData.MAX_VALUE;
            }
        }

        if (allTriggered.Count > 0)
            OnExtremeEventTriggered?.Invoke(allTriggered);

        return allTriggered;
    }

    // =========================================================
    // 差異計算
    // =========================================================

    /// <summary>計算 snapshot 到當前的環境差異（相對變化條件用）</summary>
    public EnvironmentDelta DiffFromSnapshot(EnvironmentData snapshot)
    {
        return EnvironmentData.Diff(snapshot, _env);
    }

    // =========================================================
    // 私有事件處理
    // =========================================================

    private void HandleDarknessTriggered()
    {
        OnDarknessTriggered?.Invoke();
    }

    // =========================================================
    // 重置（新局開始）
    // =========================================================

    /// <summary>重置環境數值為初始值（新遊戲開始時呼叫）</summary>
    public void ResetToInitial()
    {
        _env = new EnvironmentData();
        _history.Clear();
        OnValueChanged?.Invoke(EnvAttribute.Brightness, _env.brightness);
        OnValueChanged?.Invoke(EnvAttribute.Moisture, _env.moisture);
        OnValueChanged?.Invoke(EnvAttribute.Temperature, _env.temperature);
        OnScoreChanged?.Invoke(_env.score);
    }

    // =========================================================
    // Debug
    // =========================================================

    [ContextMenu("Debug_PrintEnvState")]
    private void Debug_PrintEnvState()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("請在 Play Mode 下使用");
            return;
        }
        Debug.Log($"[EnvironmentManager] {_env}");
    }

    [ContextMenu("Debug_TriggerFlood")]
    private void Debug_TriggerFlood()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("請在 Play Mode 下使用");
            return;
        }
        Debug.Log("[EnvironmentManager] 強制設定水分 = 20 以觸發大洪水");
        ForceSetValue(EnvAttribute.Moisture, EnvironmentData.MAX_VALUE);
    }

    [ContextMenu("Debug_TriggerDarkness")]
    private void Debug_TriggerDarkness()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("請在 Play Mode 下使用");
            return;
        }
        Debug.Log("[EnvironmentManager] 強制設定亮度 = 0 以觸發黑暗降臨");
        ForceSetValue(EnvAttribute.Brightness, EnvironmentData.MIN_VALUE);
    }

    [ContextMenu("Debug_PrintHistory")]
    private void Debug_PrintHistory()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        if (_history.Count == 0) { Debug.Log("[EnvironmentManager] 歷史紀錄為空"); return; }
        int i = 0;
        foreach (var snap in _history)
            Debug.Log($"[History] [{i++}] {snap}");
    }

    [ContextMenu("Debug_RecordSnapshot")]
    private void Debug_RecordSnapshot()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        RecordSnapshot();
        Debug.Log($"[EnvironmentManager] 手動記錄快照，目前歷史 {HistoryCount} 筆");
    }

    [ContextMenu("Debug_ScoreMinus10")]
    private void Debug_ScoreMinus10()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("請在 Play Mode 下使用");
            return;
        }
        ModifyScore(-10);
        Debug.Log($"[EnvironmentManager] 業績 -10 → 現在 {_env.score}");
    }

    [ContextMenu("Debug_Brightness+1")]
    private void Debug_BrightnessIncrease() => Debug_ChangeEnv(EnvAttribute.Brightness, 1);

    [ContextMenu("Debug_Brightness-1")]
    private void Debug_BrightnessDecrease() => Debug_ChangeEnv(EnvAttribute.Brightness, -1);

    [ContextMenu("Debug_Moisture+1")]
    private void Debug_MoistureIncrease() => Debug_ChangeEnv(EnvAttribute.Moisture, 1);

    [ContextMenu("Debug_Moisture-1")]
    private void Debug_MoistureDecrease() => Debug_ChangeEnv(EnvAttribute.Moisture, -1);

    [ContextMenu("Debug_Temperature+1")]
    private void Debug_TemperatureIncrease() => Debug_ChangeEnv(EnvAttribute.Temperature, 1);

    [ContextMenu("Debug_Temperature-1")]
    private void Debug_TemperatureDecrease() => Debug_ChangeEnv(EnvAttribute.Temperature, -1);

    [ContextMenu("Debug_Score+10")]
    private void Debug_ScorePlus10()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        ModifyScore(10);
        Debug.Log($"[EnvironmentManager] 業績 +10 → 現在 {_env.score}");
    }

    private void Debug_ChangeEnv(EnvAttribute attr, int delta)
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        ApplyEffects(new List<RuneEffect> { new RuneEffect { attribute = attr, value = delta } });
        Debug.Log($"[EnvironmentManager] {attr} {(delta > 0 ? "+" : "")}{delta} → 現在 {_env.GetValue(attr)}");
    }
}

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 卷軸庫存（MonoBehaviour Singleton）。
/// 持有玩家擁有的卷軸列表。
/// 卷軸使用後不消耗，可重複選擇。
/// </summary>
public class ScrollInventory : MonoBehaviour
{
    // =========================================================
    // Singleton
    // =========================================================

    public static ScrollInventory Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // =========================================================
    // 事件
    // =========================================================

    /// <summary>卷軸清單改變時觸發（新增卷軸時）</summary>
    public event System.Action OnInventoryChanged;

    // =========================================================
    // 私有欄位
    // =========================================================

    private readonly List<ScrollData> _scrolls = new List<ScrollData>();

    // =========================================================
    // 只讀屬性
    // =========================================================

    public IReadOnlyList<ScrollData> Scrolls => _scrolls;
    public int Count => _scrolls.Count;

    // =========================================================
    // 初始化
    // =========================================================

    /// <summary>由 GameManager 在遊戲開始時設定初始卷軸</summary>
    public void Init(List<ScrollData> scrolls)
    {
        _scrolls.Clear();
        _scrolls.AddRange(scrolls);
        OnInventoryChanged?.Invoke();
        Debug.Log($"[ScrollInventory] 初始化：{_scrolls.Count} 張卷軸");
    }

    // =========================================================
    // 新增（獎勵）
    // =========================================================

    /// <summary>顧客獎勵時新增卷軸</summary>
    public void AddScroll(ScrollData scroll)
    {
        _scrolls.Add(scroll);
        OnInventoryChanged?.Invoke();
        Debug.Log($"[ScrollInventory] 新增卷軸：{scroll.scrollName}");
    }

    // =========================================================
    // 查詢
    // =========================================================

    public bool HasScroll(ScrollData scroll) => _scrolls.Contains(scroll);

    // =========================================================
    // Debug
    // =========================================================

    [ContextMenu("Debug_PrintScrolls")]
    private void Debug_PrintScrolls()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        Debug.Log($"[ScrollInventory] 持有 {_scrolls.Count} 張卷軸：");
        foreach (var s in _scrolls)
            Debug.Log($"  {s.scrollName}（{s.modifierType}，槽×{s.slotCount}）");
    }
}

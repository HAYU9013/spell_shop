/// <summary>
/// 全域手牌選中狀態（純 static，不掛 GameObject）。
/// 追蹤 PlayerAction 階段中玩家目前「選中要放入槽位」的符文卡片。
///
/// 使用方式：
///   - RuneCardUI 點擊時呼叫 Select / Deselect
///   - SlotUI 點擊空槽時讀取 SelectedRuneData
///   - RuneOnHandUI 重建手牌時呼叫 Deselect（清除過期索引）
///
/// 為何同時儲存 index 與 RuneData？
///   手牌可能有多張同名符文（同一個 RuneData ScriptableObject），
///   光靠 RuneData 無法分辨是「哪一張卡」被選中；
///   光靠 index 無法讓 SlotUI 知道要放入哪種符文。
///   兩者並存，index 用於判斷高亮，RuneData 用於 PlaceRune。
/// </summary>
public static class HandSelectionState
{
    /// <summary>目前選中的手牌索引（-1 = 無選中）</summary>
    public static int SelectedIndex { get; private set; } = -1;

    /// <summary>目前選中的符文資料（null = 無選中）</summary>
    public static RuneData SelectedRuneData { get; private set; }

    /// <summary>
    /// 選中狀態改變時觸發。
    /// RuneCardUI 訂閱此事件來即時更新高亮視覺，
    /// 不需要 RuneOnHandUI 主動通知每一張卡。
    /// </summary>
    public static event System.Action OnSelectionChanged;

    /// <summary>選中指定索引的符文卡片，同時廣播 OnSelectionChanged。</summary>
    public static void Select(int index, RuneData rune)
    {
        SelectedIndex    = index;
        SelectedRuneData = rune;
        OnSelectionChanged?.Invoke();
    }

    /// <summary>
    /// 取消選中。若本來就沒有選中任何卡，直接返回（避免無謂的事件廣播）。
    /// </summary>
    public static void Deselect()
    {
        // 沒有選中任何東西時直接跳過，避免觸發不必要的 UI 刷新
        if (SelectedIndex < 0 && SelectedRuneData == null) return;

        SelectedIndex    = -1;
        SelectedRuneData = null;
        OnSelectionChanged?.Invoke();
    }

    /// <summary>
    /// 判斷指定索引的卡片是否為目前選中的那一張。
    /// 由各 RuneCardUI 在 RefreshHighlight() 中呼叫。
    /// </summary>
    public static bool IsSelected(int index) => SelectedIndex >= 0 && SelectedIndex == index;

    /// <summary>是否有任何符文被選中（SlotUI 用來決定點空槽是否有意義）</summary>
    public static bool HasSelection => SelectedIndex >= 0 && SelectedRuneData != null;
}

using UnityEngine;

/// <summary>
/// 卷軸定義資料（ScriptableObject）。
/// 決定符文槽數量與結算時的修飾規則。
///
/// 修飾規則說明：
///   DirectAdd    — 所有符文效果直接加總（預設行為）
///   MultiplyAll  — 加總後所有效果 × multiplier
///   PositiveOnly — 忽略負值效果，只計算正值
///   TagMultiply  — 符合 tagFilter 標籤的符文效果 × multiplier，其餘 DirectAdd
///   Average      — 所有效果加總後除以符文數量（保留 float，最終套用 int）
///   Invert       — 所有效果正負反轉（+→−，−→+）
///
/// 建立方式：Assets 右鍵 → Create → SpellShop → Scroll
/// </summary>
[CreateAssetMenu(fileName = "Scroll_New", menuName = "SpellShop/Scroll")]
public class ScrollData : ScriptableObject
{
    // =========================================================
    // 基本資料
    // =========================================================

    [Header("基本資料")]
    [Tooltip("顯示名稱（繁中）")]
    public string scrollName = "新卷軸";

    [Tooltip("效果說明（顯示在卷軸按鈕下方）")]
    [TextArea(2, 4)]
    public string description = "";

    [Tooltip("卷軸圖示")]
    public Sprite icon;

    // =========================================================
    // 規則
    // =========================================================

    [Header("卷軸規則")]
    [Tooltip("符文槽位數量（填入幾張符文才能送出）")]
    [Range(1, 6)]
    public int slotCount = 2;

    [Tooltip("結算修飾規則")]
    public ScrollModifierType modifierType = ScrollModifierType.DirectAdd;

    [Tooltip("倍率（MultiplyAll / TagMultiply 使用）")]
    public float multiplier = 2f;

    [Tooltip("標籤篩選（TagMultiply 時：只有符合此標籤的符文效果乘以倍率）")]
    public RuneTag tagFilter = RuneTag.None;

    [Tooltip("封鎖屬性：勾選的環境屬性本回合完全不受符文與卷軸影響（疊加於修飾規則之上）")]
    public EnvAttributeMask blockedAttributes = EnvAttributeMask.None;

    // =========================================================
    // 工具
    // =========================================================

    /// <summary>回傳修飾規則的簡短說明文字（供 UI 顯示）</summary>
    public string GetModifierDescription()
    {
        string modifier;
        switch (modifierType)
        {
            case ScrollModifierType.DirectAdd:    modifier = $"槽位 ×{slotCount}，效果直接加總"; break;
            case ScrollModifierType.MultiplyAll:  modifier = $"槽位 ×{slotCount}，所有效果 ×{multiplier}"; break;
            case ScrollModifierType.PositiveOnly: modifier = $"槽位 ×{slotCount}，僅計算正值效果"; break;
            case ScrollModifierType.TagMultiply:  modifier = $"槽位 ×{slotCount}，{tagFilter} 標籤效果 ×{multiplier}"; break;
            case ScrollModifierType.Average:      modifier = $"槽位 ×{slotCount}，效果取平均值"; break;
            case ScrollModifierType.Invert:       modifier = $"槽位 ×{slotCount}，效果正負反轉"; break;
            default:                              modifier = $"槽位 ×{slotCount}"; break;
        }

        if (blockedAttributes == EnvAttributeMask.None)
            return modifier;

        return $"{modifier}；封鎖：{GetBlockedDescription()}";
    }

    /// <summary>回傳封鎖屬性的中文說明（供 UI 顯示）</summary>
    public string GetBlockedDescription()
    {
        if (blockedAttributes == EnvAttributeMask.None) return "無";

        var parts = new System.Collections.Generic.List<string>();
        if ((blockedAttributes & EnvAttributeMask.Brightness)  != 0) parts.Add("亮度");
        if ((blockedAttributes & EnvAttributeMask.Moisture)    != 0) parts.Add("水分");
        if ((blockedAttributes & EnvAttributeMask.Temperature) != 0) parts.Add("溫度");
        return string.Join("、", parts);
    }
}

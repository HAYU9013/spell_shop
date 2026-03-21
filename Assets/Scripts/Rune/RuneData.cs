using UnityEngine;

/// <summary>
/// 符文定義資料（ScriptableObject）。
/// 代表一種符文的「模板」，由美術填入圖示與數值。
/// 執行時透過 RuneInstance 包裝，以追蹤消耗型狀態。
///
/// 建立方式：Assets 右鍵 → Create → SpellShop → Rune
/// </summary>
[CreateAssetMenu(fileName = "Rune_New", menuName = "SpellShop/Rune")]
public class RuneData : ScriptableObject
{
    // =========================================================
    // 基本資料
    // =========================================================

    [Header("基本資料")]
    [Tooltip("顯示名稱（繁中）")]
    public string runeName = "新符文";

    [Tooltip("效果說明（顯示在卡牌底部）")]
    [TextArea(2, 4)]
    public string description = "";

    [Tooltip("卡牌圖示")]
    public Sprite icon;

    // =========================================================
    // 分類
    // =========================================================

    [Header("分類")]
    [Tooltip("稀有度：Common / Rare / Legendary")]
    public Rarity rarity = Rarity.Common;

    [Tooltip("循環型：使用後進棄牌堆；消耗型：使用後永久移除")]
    public RuneType runeType = RuneType.Cycle;

    [Tooltip("符文標籤（可多選，影響標籤連鎖加成）")]
    public RuneTag tags = RuneTag.None;

    // =========================================================
    // 效果
    // =========================================================

    [Header("環境效果")]
    [Tooltip("此符文對環境屬性的影響列表（可多條）")]
    public RuneEffect[] effects;

    // =========================================================
    // 工具
    // =========================================================

    /// <summary>回傳效果的簡短描述，例如「水分 +2  溫度 -1」</summary>
    public string GetEffectSummary()
    {
        if (effects == null || effects.Length == 0)
            return "（無效果）";

        var sb = new System.Text.StringBuilder();
        foreach (var e in effects)
        {
            if (sb.Length > 0) sb.Append("  ");
            string attrName = e.attribute switch
            {
                EnvAttribute.Brightness  => "亮度",
                EnvAttribute.Moisture    => "水分",
                EnvAttribute.Temperature => "溫度",
                _                        => e.attribute.ToString()
            };
            sb.Append($"{attrName} {e.value:+0;-0;0}");
        }
        return sb.ToString();
    }
}

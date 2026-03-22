using System.Collections.Generic;
using UnityEngine;

// =========================================================
// 遺物滿意條件（序列化結構）
// =========================================================

/// <summary>
/// 遺物的環境滿意條件。
/// 每回合判定當前環境是否符合，決定是否觸發滿意效果。
/// </summary>
[System.Serializable]
public class RelicCondition
{
    [Tooltip("要判定的環境屬性")]
    public EnvAttribute attribute;

    [Tooltip("比較方式")]
    public CompareOperator compareOp;

    [Tooltip("目標數值（GreaterEqual / LessEqual / Equal 使用）")]
    public int targetValue;

    [Tooltip("範圍下限（InRange 使用）")]
    public int rangeMin;

    [Tooltip("範圍上限（InRange 使用）")]
    public int rangeMax;

    /// <summary>根據當前環境判斷條件是否成立</summary>
    public bool Evaluate(EnvironmentData env)
    {
        int val = env.GetValue(attribute);
        switch (compareOp)
        {
            case CompareOperator.GreaterEqual: return val >= targetValue;
            case CompareOperator.LessEqual:    return val <= targetValue;
            case CompareOperator.Equal:        return val == targetValue;
            case CompareOperator.InRange:      return val >= rangeMin && val <= rangeMax;
            default:                           return false;
        }
    }

    /// <summary>回傳可讀條件文字，如「亮度 >= 7」或「水分 4~8」</summary>
    public override string ToString()
    {
        string attrName = attribute switch
        {
            EnvAttribute.Brightness  => "亮度",
            EnvAttribute.Moisture    => "水分",
            EnvAttribute.Temperature => "溫度",
            _                        => attribute.ToString()
        };

        return compareOp switch
        {
            CompareOperator.GreaterEqual => $"{attrName} >= {targetValue}",
            CompareOperator.LessEqual    => $"{attrName} <= {targetValue}",
            CompareOperator.Equal        => $"{attrName} == {targetValue}",
            CompareOperator.InRange      => $"{attrName} {rangeMin}~{rangeMax}",
            _                            => $"{attrName} ?"
        };
    }
}

// =========================================================
// 遺物資料（ScriptableObject）
// =========================================================

/// <summary>
/// 遺物定義資料（ScriptableObject）。
/// 每回合 Phase 2 由 RelicManager 依 displayOrder 順序結算。
///
/// 結算邏輯：
///   滿意（Evaluate = true）→ 套用滿意效果，不滿意計數歸零
///   不滿意             → 套用不滿意效果，計數 +1
///   計數 >= 上限       → 觸發懲罰（計數不重置，下回合繼續）
///
/// 建立方式：Assets 右鍵 → Create → SpellShop → Relic
/// </summary>
[CreateAssetMenu(fileName = "Relic_New", menuName = "SpellShop/Relic")]
public class RelicData : ScriptableObject
{
    // =========================================================
    // 基本資料
    // =========================================================

    [Header("基本資料")]
    public string relicName = "新遺物";

    [TextArea(2, 4)]
    public string description = "";

    public Sprite icon;

    [Tooltip("結算順序（數字小的先結算）")]
    public int displayOrder = 0;

    // =========================================================
    // 滿意條件
    // =========================================================

    [Header("滿意條件")]
    public List<RelicCondition> satisfiedConditions = new List<RelicCondition>();

    [Tooltip("多條件時的判定邏輯：And = 全部成立；Or = 任一成立")]
    public ConditionLogic conditionLogic = ConditionLogic.And;

    /// <summary>根據當前環境判斷所有條件是否成立</summary>
    public bool EvaluateAll(EnvironmentData env)
    {
        if (satisfiedConditions == null || satisfiedConditions.Count == 0) return false;

        if (conditionLogic == ConditionLogic.And)
        {
            foreach (var c in satisfiedConditions)
                if (!c.Evaluate(env)) return false;
            return true;
        }
        else // Or
        {
            foreach (var c in satisfiedConditions)
                if (c.Evaluate(env)) return true;
            return false;
        }
    }

    // =========================================================
    // 滿意效果（每回合，條件成立時套用）
    // =========================================================

    [Header("滿意效果（條件成立時，每回合套用）")]
    [Tooltip("對環境屬性的影響")]
    public RuneEffect[] satisfiedEnvEffects;

    [Tooltip("業績變化（正=加，負=扣）")]
    public int satisfiedScoreChange = 0;

    [Tooltip("符文 / 卷軸等額外獎勵（每回合滿意時給予，注意平衡性）")]
    public List<RewardEntry> satisfiedRewards = new List<RewardEntry>();

    // =========================================================
    // 不滿意效果（每回合，條件不成立時套用）
    // =========================================================

    [Header("不滿意效果（條件不成立時，每回合套用）")]
    [Tooltip("對環境屬性的影響（無效果則留空）")]
    public RuneEffect[] unsatisfiedEnvEffects;

    [Tooltip("業績變化（無效果則填 0）")]
    public int unsatisfiedScoreChange = 0;

    // =========================================================
    // 懲罰
    // =========================================================

    [Header("不滿意懲罰")]
    [Tooltip("不滿意計數達到上限觸發懲罰（計數不重置，每回合繼續觸發）")]
    [Range(1, 10)]
    public int unsatisfiedLimit = 3;

    [Tooltip("懲罰類型")]
    public RelicPunishment punishmentType = RelicPunishment.EnvironmentShock;

    [Header("懲罰：EnvironmentShock 設定")]
    [Tooltip("要衝擊的環境屬性（EnvironmentShock 使用）")]
    public EnvAttribute shockAttribute = EnvAttribute.Brightness;

    [Tooltip("衝擊數值（EnvironmentShock 使用）")]
    public int shockValue = -5;

    [Tooltip("true = 強制設定為 shockValue；false = 對當前值套用 shockValue 作為 delta")]
    public bool shockIsForceSet = false;

    [Header("懲罰：附加給予符文")]
    [Tooltip("懲罰觸發時額外給予玩家的符文（使用 SpecificRune 類型填寫）。\n可與任何懲罰類型並用，例如 PlayerDeath + 枯萎 ×2。")]
    public List<RewardEntry> punishmentRunes = new List<RewardEntry>();
}

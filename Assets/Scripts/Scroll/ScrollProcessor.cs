using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 卷軸結算器（純靜態工具類）。
/// 接收一筆 SubmitRecord，依 ScrollData.modifierType 計算最終環境效果。
///
/// 6 種修飾規則：
///   DirectAdd    — 所有符文效果逐屬性加總
///   MultiplyAll  — 加總後每項 × multiplier（float，最終取 int）
///   PositiveOnly — 加總前過濾掉 value &lt;= 0 的單項效果
///   TagMultiply  — 符合 tagFilter 的符文效果 × multiplier，其餘 × 1
///   Average      — 加總後除以符文數量（float，最終取 int）
///   Invert       — 加總後每項正負反轉
///
/// 使用方式：
///   var effects = ScrollProcessor.Process(WorkbenchManager.Instance.LastSubmit);
///   EnvironmentManager.Instance.ApplyEffects(effects);
/// </summary>
public static class ScrollProcessor
{
    // =========================================================
    // 公開入口
    // =========================================================

    /// <summary>
    /// 計算送出卷軸的最終環境效果列表。
    /// 回傳空列表表示無效果（record 為 null 或符文無效果）。
    /// </summary>
    public static List<RuneEffect> Process(SubmitRecord record)
    {
        if (record == null || record.Scroll == null || record.Runes == null || record.Runes.Length == 0)
            return new List<RuneEffect>();

        var scroll = record.Scroll;
        var runes  = record.Runes;

        List<RuneEffect> result;

        switch (scroll.modifierType)
        {
            case ScrollModifierType.DirectAdd:
                result = ProcessDirectAdd(runes);
                break;
            case ScrollModifierType.MultiplyAll:
                result = ProcessMultiplyAll(runes, scroll.multiplier);
                break;
            case ScrollModifierType.PositiveOnly:
                result = ProcessPositiveOnly(runes);
                break;
            case ScrollModifierType.TagMultiply:
                result = ProcessTagMultiply(runes, scroll.tagFilter, scroll.multiplier);
                break;
            case ScrollModifierType.Average:
                result = ProcessAverage(runes);
                break;
            case ScrollModifierType.Invert:
                result = ProcessInvert(runes);
                break;
            default:
                Debug.LogWarning($"[ScrollProcessor] 未知的修飾類型：{scroll.modifierType}，回退至 DirectAdd");
                result = ProcessDirectAdd(runes);
                break;
        }

        result = ApplyAttributeBlock(result, scroll.blockedAttributes);

        LogResult(record, result);
        return result;
    }

    // =========================================================
    // 各規則實作
    // =========================================================

    /// <summary>DirectAdd：逐屬性加總所有效果</summary>
    private static List<RuneEffect> ProcessDirectAdd(RuneData[] runes)
    {
        return ToRuneEffects(SumPerAttribute(runes, filterPositiveOnly: false));
    }

    /// <summary>MultiplyAll：加總後每屬性 × multiplier</summary>
    private static List<RuneEffect> ProcessMultiplyAll(RuneData[] runes, float multiplier)
    {
        var sums = SumPerAttribute(runes, filterPositiveOnly: false);
        var scaled = new Dictionary<EnvAttribute, float>();
        foreach (var kvp in sums)
            scaled[kvp.Key] = kvp.Value * multiplier;
        return ToRuneEffects(scaled);
    }

    /// <summary>PositiveOnly：只計算單項 value > 0 的效果，加總後輸出</summary>
    private static List<RuneEffect> ProcessPositiveOnly(RuneData[] runes)
    {
        return ToRuneEffects(SumPerAttribute(runes, filterPositiveOnly: true));
    }

    /// <summary>
    /// TagMultiply：符合 tagFilter 的符文其效果 × multiplier，其餘符文 × 1，最後加總。
    /// tagFilter == None 時退化為 DirectAdd。
    /// </summary>
    private static List<RuneEffect> ProcessTagMultiply(RuneData[] runes, RuneTag tagFilter, float multiplier)
    {
        var sums = new Dictionary<EnvAttribute, float>();

        foreach (var rune in runes)
        {
            if (rune.effects == null) continue;

            bool matches = tagFilter != RuneTag.None && (rune.tags & tagFilter) != 0;
            float factor = matches ? multiplier : 1f;

            foreach (var effect in rune.effects)
            {
                if (!sums.ContainsKey(effect.attribute))
                    sums[effect.attribute] = 0f;
                sums[effect.attribute] += effect.value * factor;
            }
        }

        return ToRuneEffects(sums);
    }

    /// <summary>Average：加總後除以符文數量，保留 float 精度，最終取 int</summary>
    private static List<RuneEffect> ProcessAverage(RuneData[] runes)
    {
        if (runes.Length == 0) return new List<RuneEffect>();

        var sums = SumPerAttribute(runes, filterPositiveOnly: false);
        var averaged = new Dictionary<EnvAttribute, float>();
        foreach (var kvp in sums)
            averaged[kvp.Key] = kvp.Value / runes.Length;

        return ToRuneEffects(averaged);
    }

    /// <summary>Invert：加總後每屬性正負反轉</summary>
    private static List<RuneEffect> ProcessInvert(RuneData[] runes)
    {
        var sums = SumPerAttribute(runes, filterPositiveOnly: false);
        var inverted = new Dictionary<EnvAttribute, float>();
        foreach (var kvp in sums)
            inverted[kvp.Key] = -kvp.Value;
        return ToRuneEffects(inverted);
    }

    // =========================================================
    // 共用工具
    // =========================================================

    /// <summary>
    /// 將所有符文的效果逐屬性加總。
    /// filterPositiveOnly = true 時跳過 value &lt;= 0 的單項效果。
    /// </summary>
    private static Dictionary<EnvAttribute, float> SumPerAttribute(RuneData[] runes, bool filterPositiveOnly)
    {
        var sums = new Dictionary<EnvAttribute, float>();

        foreach (var rune in runes)
        {
            if (rune.effects == null) continue;
            foreach (var effect in rune.effects)
            {
                if (filterPositiveOnly && effect.value <= 0) continue;

                if (!sums.ContainsKey(effect.attribute))
                    sums[effect.attribute] = 0f;
                sums[effect.attribute] += effect.value;
            }
        }

        return sums;
    }

    /// <summary>
    /// 將 float 屬性加總表轉換為 List&lt;RuneEffect&gt;。
    /// 值為 0 的屬性不輸出。
    /// </summary>
    private static List<RuneEffect> ToRuneEffects(Dictionary<EnvAttribute, float> sums)
    {
        var results = new List<RuneEffect>();
        foreach (var kvp in sums)
        {
            int intVal = Mathf.RoundToInt(kvp.Value);
            if (intVal != 0)
                results.Add(new RuneEffect { attribute = kvp.Key, value = intVal });
        }
        return results;
    }

    // =========================================================
    // 屬性封鎖過濾
    // =========================================================

    /// <summary>移除結果中被 blockedAttributes 封鎖的屬性效果</summary>
    private static List<RuneEffect> ApplyAttributeBlock(List<RuneEffect> effects, EnvAttributeMask blocked)
    {
        if (blocked == EnvAttributeMask.None) return effects;
        return effects.FindAll(e => !IsBlocked(e.attribute, blocked));
    }

    private static bool IsBlocked(EnvAttribute attr, EnvAttributeMask mask)
    {
        switch (attr)
        {
            case EnvAttribute.Brightness:  return (mask & EnvAttributeMask.Brightness)  != 0;
            case EnvAttribute.Moisture:    return (mask & EnvAttributeMask.Moisture)    != 0;
            case EnvAttribute.Temperature: return (mask & EnvAttributeMask.Temperature) != 0;
            default: return false;
        }
    }

    // =========================================================
    // Debug 輸出
    // =========================================================

    private static void LogResult(SubmitRecord record, List<RuneEffect> result)
    {
        if (result.Count == 0)
        {
            Debug.Log($"[ScrollProcessor] {record.Scroll.scrollName}（{record.Scroll.modifierType}）→ 無效果");
            return;
        }

        var parts = new System.Text.StringBuilder();
        foreach (var e in result)
        {
            string attrName = e.attribute switch
            {
                EnvAttribute.Brightness  => "亮度",
                EnvAttribute.Moisture    => "水分",
                EnvAttribute.Temperature => "溫度",
                _                        => e.attribute.ToString()
            };
            if (parts.Length > 0) parts.Append("  ");
            parts.Append($"{attrName} {e.value:+0;-0}");
        }
        Debug.Log($"[ScrollProcessor] {record.Scroll.scrollName}（{record.Scroll.modifierType}）→ {parts}");
    }
}

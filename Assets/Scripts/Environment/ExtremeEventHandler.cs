using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 極端事件處理器。
/// 在每次環境數值套用後呼叫，檢查是否觸碰邊界並觸發一次性的極端事件。
///
/// 極端事件觸碰邊界後立即觸發，該回合結算後自然解除（不持續）：
///   黑暗降臨（亮度 = 0）  → 所有遺物不滿意計數 +1（由 OnDarknessTriggered 事件通知 RelicManager）
///   大洪水  （水分 = 20） → 水分額外 +5（可再次觸發連鎖）
///   高溫爆發（溫度 = 20） → 業績 -10
///   極寒侵襲（溫度 = 0）  → 業績 -10
/// </summary>
public class ExtremeEventHandler
{
    // RelicManager 透過訂閱此事件來處理「黑暗降臨」的遺物計數 +1
    public event System.Action OnDarknessTriggered;

    /// <summary>
    /// 檢查 env 中是否有屬性剛好觸碰邊界，並立即套用對應的極端事件效果。
    /// 由 EnvironmentManager.CheckAndTriggerExtremeEvents() 呼叫。
    /// </summary>
    /// <param name="env">當前環境資料（會直接修改）</param>
    /// <param name="hitBoundaryFlags">
    /// 哪些屬性觸碰了邊界，key = EnvAttribute，value = 觸碰的邊界值（0 或 20）
    /// </param>
    /// <returns>本次觸發的極端事件列表</returns>
    public List<ExtremeEvent> Process(EnvironmentData env, Dictionary<EnvAttribute, int> hitBoundaryFlags)
    {
        var triggered = new List<ExtremeEvent>();

        foreach (var pair in hitBoundaryFlags)
        {
            EnvAttribute attr  = pair.Key;
            int          bound = pair.Value;

            if (attr == EnvAttribute.Brightness && bound == EnvironmentData.MIN_VALUE)
            {
                triggered.Add(ExtremeEvent.Darkness);
                ApplyDarkness(env);
            }
            else if (attr == EnvAttribute.Moisture && bound == EnvironmentData.MAX_VALUE)
            {
                triggered.Add(ExtremeEvent.Flood);
                ApplyFlood(env);
            }
            else if (attr == EnvAttribute.Temperature && bound == EnvironmentData.MAX_VALUE)
            {
                triggered.Add(ExtremeEvent.Overheat);
                ApplyOverheat(env);
            }
            else if (attr == EnvAttribute.Temperature && bound == EnvironmentData.MIN_VALUE)
            {
                triggered.Add(ExtremeEvent.Overcold);
                ApplyOvercold(env);
            }
        }

        return triggered;
    }

    // =========================================================
    // 各極端事件效果
    // =========================================================

    private void ApplyDarkness(EnvironmentData env)
    {
        // 亮度強制鎖在 0（已由 Clamp 處理，此處確認）
        env.SetValue(EnvAttribute.Brightness, EnvironmentData.MIN_VALUE);
        Debug.Log("[ExtremeEvent] 黑暗降臨！所有遺物不滿意計數 +1");

        // 通知 RelicManager（透過事件解耦）
        OnDarknessTriggered?.Invoke();
    }

    private void ApplyFlood(EnvironmentData env)
    {
        // 水分額外 +5（可能再次觸發洪水，由呼叫端的 loop 處理）
        env.ApplyDelta(EnvAttribute.Moisture, 5);
        Debug.Log($"[ExtremeEvent] 大洪水！水分 +5 → 現在 {env.moisture}");
    }

    private void ApplyOverheat(EnvironmentData env)
    {
        env.ModifyScore(-10);
        Debug.Log($"[ExtremeEvent] 高溫爆發！業績 -10 → 現在 {env.score}");
    }

    private void ApplyOvercold(EnvironmentData env)
    {
        env.ModifyScore(-10);
        Debug.Log($"[ExtremeEvent] 極寒侵襲！業績 -10 → 現在 {env.score}");
    }
}

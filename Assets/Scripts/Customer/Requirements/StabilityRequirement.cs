/// <summary>
/// 穩定性需求：指定條件需連續成立達 stabilityTurns 回合才滿足。
///
/// 例：溫度維持 4~6 連續 1 回合
///   → compareOp=InRange, targetValue=4, targetValueMax=6, stabilityTurns=1
///
/// StabilityCount 由 CustomerInstance.UpdateTracking() 維護：
///   條件成立 → +1；條件不成立 → 歸零
/// </summary>
public class StabilityRequirement : IRequirement
{
    private readonly RequirementData _data;

    public StabilityRequirement(RequirementData data)
    {
        _data = data;
    }

    public bool IsSatisfied(RequirementContext ctx)
    {
        return ctx.StabilityCount >= _data.stabilityTurns;
    }

    /// <summary>判斷當前環境值是否符合穩定條件（供 CustomerInstance 更新計數用）</summary>
    public bool IsConditionMet(EnvironmentData env)
    {
        int val = env.GetValue(_data.attribute);
        switch (_data.compareOp)
        {
            case CompareOperator.GreaterEqual: return val >= _data.targetValue;
            case CompareOperator.LessEqual:    return val <= _data.targetValue;
            case CompareOperator.Equal:        return val == _data.targetValue;
            case CompareOperator.InRange:      return val >= _data.targetValue && val <= _data.targetValueMax;
            default:                           return false;
        }
    }

    public string GetProgressText(RequirementContext ctx)
    {
        string attr      = AttrName(_data.attribute);
        string condition = ConditionText();
        return $"{attr} {condition} 連續 {ctx.StabilityCount}/{_data.stabilityTurns} 回合";
    }

    // ──────────────────────────────────────────────────────────

    private string ConditionText()
    {
        switch (_data.compareOp)
        {
            case CompareOperator.GreaterEqual: return $">= {_data.targetValue}";
            case CompareOperator.LessEqual:    return $"<= {_data.targetValue}";
            case CompareOperator.Equal:        return $"== {_data.targetValue}";
            case CompareOperator.InRange:      return $"{_data.targetValue}~{_data.targetValueMax}";
            default:                           return "?";
        }
    }

    private static string AttrName(EnvAttribute attr) => attr switch
    {
        EnvAttribute.Brightness  => "亮度",
        EnvAttribute.Moisture    => "水分",
        EnvAttribute.Temperature => "溫度",
        _                        => attr.ToString()
    };
}

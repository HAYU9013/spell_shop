/// <summary>
/// 絕對值需求：環境屬性的當前值符合指定條件即滿足。
/// 例：溫度 >= 8、亮度 >= 10、溫度 <= 2 且亮度 >= 5（由多條 AbsoluteRequirement 組合）
/// </summary>
public class AbsoluteRequirement : IRequirement
{
    private readonly RequirementData _data;

    public AbsoluteRequirement(RequirementData data)
    {
        _data = data;
    }

    public bool IsSatisfied(RequirementContext ctx)
    {
        int val = ctx.CurrentEnv.GetValue(_data.attribute);
        return Evaluate(val);
    }

    public string GetProgressText(RequirementContext ctx)
    {
        int val = ctx.CurrentEnv.GetValue(_data.attribute);
        string attrName = AttrName(_data.attribute);
        string condition = ConditionText();
        return $"{attrName} {condition}（目前：{val}）";
    }

    // ──────────────────────────────────────────────────────────

    private bool Evaluate(int val)
    {
        switch (_data.compareOp)
        {
            case CompareOperator.GreaterEqual: return val >= _data.targetValue;
            case CompareOperator.LessEqual:    return val <= _data.targetValue;
            case CompareOperator.Equal:        return val == _data.targetValue;
            case CompareOperator.InRange:      return val >= _data.targetValue && val <= _data.targetValueMax;
            default:                           return false;
        }
    }

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

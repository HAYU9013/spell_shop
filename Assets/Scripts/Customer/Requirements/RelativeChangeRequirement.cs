/// <summary>
/// 相對變化需求：顧客到來後，指定屬性的累積變化量達到門檻即滿足。
///
/// requiredDelta 正值 → 需累積增加（delta >= requiredDelta）
/// requiredDelta 負值 → 需累積減少（delta <= requiredDelta）
///
/// 例：水分累積減少 >= 3 → requiredDelta = -3，累積 delta <= -3 時滿足
/// </summary>
public class RelativeChangeRequirement : IRequirement
{
    private readonly RequirementData _data;

    public RelativeChangeRequirement(RequirementData data)
    {
        _data = data;
    }

    public bool IsSatisfied(RequirementContext ctx)
    {
        int delta = ctx.AccumulatedDelta.GetDelta(_data.attribute);
        return _data.requiredDelta >= 0
            ? delta >= _data.requiredDelta
            : delta <= _data.requiredDelta;
    }

    public string GetProgressText(RequirementContext ctx)
    {
        int delta    = ctx.AccumulatedDelta.GetDelta(_data.attribute);
        string attr  = AttrName(_data.attribute);
        string dir   = _data.requiredDelta >= 0 ? "增加" : "減少";
        int    need  = System.Math.Abs(_data.requiredDelta);
        int    cur   = System.Math.Abs(delta);
        bool   ok    = IsSatisfied(ctx);
        return $"{attr}累積{dir} >= {need}（目前：{(delta >= 0 ? "+" : "")}{delta}）{(ok ? " ✓" : "")}";
    }

    private static string AttrName(EnvAttribute attr) => attr switch
    {
        EnvAttribute.Brightness  => "亮度",
        EnvAttribute.Moisture    => "水分",
        EnvAttribute.Temperature => "溫度",
        _                        => attr.ToString()
    };
}

/// <summary>
/// 根據 RequirementData.type 建立對應的 IRequirement 實例。
/// </summary>
public static class RequirementFactory
{
    public static IRequirement Create(RequirementData data)
    {
        switch (data.type)
        {
            case RequirementType.Absolute:
                return new AbsoluteRequirement(data);
            case RequirementType.RelativeChange:
                return new RelativeChangeRequirement(data);
            case RequirementType.Stability:
                return new StabilityRequirement(data);
            default:
                UnityEngine.Debug.LogWarning($"[RequirementFactory] 未知的需求類型：{data.type}，回退至 AbsoluteRequirement");
                return new AbsoluteRequirement(data);
        }
    }
}

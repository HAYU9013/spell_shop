/// <summary>
/// 傳遞給 IRequirement.IsSatisfied() 的判定上下文。
/// CustomerInstance 每回合組裝後傳入各需求實例。
/// </summary>
public class RequirementContext
{
    /// <summary>當前環境（結算後）</summary>
    public EnvironmentData CurrentEnv;

    /// <summary>顧客到來時的環境快照（RelativeChange 基準）</summary>
    public EnvironmentData ArrivalSnapshot;

    /// <summary>顧客到來至今的累積環境變化量</summary>
    public EnvironmentDelta AccumulatedDelta;

    /// <summary>此需求的穩定性連續計數（Stability 用）</summary>
    public int StabilityCount;
}

/// <summary>
/// 顧客需求介面。
/// 每種需求類型實作一份，由 RequirementFactory 依 RequirementData.type 建立。
/// </summary>
public interface IRequirement
{
    /// <summary>判斷本回合需求是否滿足</summary>
    bool IsSatisfied(RequirementContext ctx);

    /// <summary>回傳 UI 顯示用進度文字，例如「溫度 >= 8（目前：6）」</summary>
    string GetProgressText(RequirementContext ctx);
}

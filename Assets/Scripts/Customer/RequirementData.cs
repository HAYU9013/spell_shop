using UnityEngine;

/// <summary>
/// 顧客需求的序列化資料結構（供 CustomerData 的 List 使用）。
/// 依 type 填入對應欄位：
///
///   Absolute        → attribute + compareOp + targetValue（+ targetValueMax 若 InRange）
///   RelativeChange  → attribute + requiredDelta
///                     正值 = 累積增加需達標（delta >= requiredDelta）
///                     負值 = 累積減少需達標（delta <= requiredDelta）
///   Stability       → attribute + compareOp + targetValue（+ targetValueMax）+ stabilityTurns
///                     條件需連續成立 stabilityTurns 回合
/// </summary>
[System.Serializable]
public class RequirementData
{
    [Tooltip("需求類型")]
    public RequirementType type;

    [Header("共用")]
    [Tooltip("判定的環境屬性")]
    public EnvAttribute attribute;

    [Header("Absolute / Stability 用")]
    [Tooltip("比較方式")]
    public CompareOperator compareOp;

    [Tooltip("目標數值（GreaterEqual / LessEqual / Equal / InRange 下限）")]
    public int targetValue;

    [Tooltip("InRange 上限（compareOp = InRange 時使用）")]
    public int targetValueMax;

    [Header("RelativeChange 用")]
    [Tooltip("需求的累積變化量（正=增加需達標；負=減少需達標）")]
    public int requiredDelta;

    [Header("Stability 用")]
    [Tooltip("條件需連續成立的回合數")]
    public int stabilityTurns = 1;
}

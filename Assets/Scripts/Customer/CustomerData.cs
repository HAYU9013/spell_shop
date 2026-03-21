using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 顧客定義資料（ScriptableObject）。
/// 描述一位顧客的需求、耐心與獎懲。
///
/// 建立方式：Assets 右鍵 → Create → SpellShop → Customer
/// </summary>
[CreateAssetMenu(fileName = "Customer_New", menuName = "SpellShop/Customer")]
public class CustomerData : ScriptableObject
{
    // =========================================================
    // 基本資料
    // =========================================================

    [Header("基本資料")]
    public string customerName = "新顧客";

    [TextArea(2, 3)]
    public string flavorText = "";

    [Tooltip("顧客立繪")]
    public Sprite portrait;

    // =========================================================
    // 需求
    // =========================================================

    [Header("需求（所有條件同時滿足才算通過）")]
    public List<RequirementData> requirements = new List<RequirementData>();

    // =========================================================
    // 耐心
    // =========================================================

    [Header("耐心")]
    [Tooltip("最大耐心值（回合數）。耐心耗盡後顧客憤怒離開")]
    [Range(1, 10)]
    public int maxPatience = 3;

    // =========================================================
    // 獎懲
    // =========================================================

    [Header("獎懲")]
    [Tooltip("滿足需求時的業績獎勵")]
    public int scoreReward = 10;

    [Tooltip("耐心耗盡時的業績懲罰（填正值，套用時為 -N）")]
    public int scorePenalty = 5;

    [Tooltip("獎勵類型（目前僅實作 Score，其餘為後續擴充）")]
    public RewardType rewardType = RewardType.Score;
}

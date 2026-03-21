using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 單筆獎勵條目（序列化，可設定在 ScriptableObject 的 List 中）。
///
/// 使用方式：
///   CustomerData.bonusRewards     — 顧客滿足後一次性給予
///   RelicData.satisfiedRewards    — 遺物每回合滿意時給予
///
/// Score 類型獎勵建議直接使用 CustomerData.scoreReward / RelicData.satisfiedScoreChange，
/// 此處的 Score 欄位可用於疊加額外的業績變動。
/// </summary>
[System.Serializable]
public class RewardEntry
{
    // =========================================================
    // 類型
    // =========================================================

    [Tooltip("獎勵類型")]
    public RewardType type = RewardType.RandomRune;

    // =========================================================
    // Score
    // =========================================================

    [Header("Score 設定")]
    [Tooltip("額外業績數量（Score 類型使用）")]
    public int scoreAmount = 5;

    // =========================================================
    // RandomRune
    // =========================================================

    [Header("Rune 設定")]
    [Tooltip("符文獎勵池（從中隨機選取，不重複）")]
    public RuneData[] runePool;

    [Tooltip("給予符文張數（不超過池的大小）")]
    [Range(1, 5)]
    public int runeCount = 1;

    // =========================================================
    // RandomScroll
    // =========================================================

    [Header("Scroll 設定")]
    [Tooltip("卷軸獎勵池（從中隨機選取，不重複）")]
    public ScrollData[] scrollPool;

    [Tooltip("給予卷軸數量（不超過池的大小）")]
    [Range(1, 3)]
    public int scrollCount = 1;
}

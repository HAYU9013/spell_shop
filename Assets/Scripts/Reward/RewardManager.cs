using System;
using System.Collections.Generic;
using UnityEngine;

// =========================================================
// RewardResult — 本次分發的實際結果（供 UI 顯示獲得提示）
// =========================================================

/// <summary>
/// RewardManager.Grant() 的回傳值，記錄本次實際給出了什麼。
/// GameFacade 將此結果透過 OnRewardGranted 事件傳給 UI。
/// </summary>
public class RewardResult
{
    /// <summary>來源名稱，例如「落湯雞旅人」或「日晷」</summary>
    public string SourceName = "";

    public List<RuneData>    RunesGranted   = new List<RuneData>();
    public List<ScrollData>  ScrollsGranted = new List<ScrollData>();
    public int               ScoreGranted;

    /// <summary>是否有任何實際獎勵（用於決定是否顯示 UI 提示）</summary>
    public bool HasAnyReward =>
        RunesGranted.Count > 0 || ScrollsGranted.Count > 0 || ScoreGranted != 0;

    /// <summary>格式化獎勵摘要，例如「業績 +10 / 水珠 ×2 / 基礎卷軸 ×1」</summary>
    public override string ToString()
    {
        var parts = new List<string>();
        if (ScoreGranted != 0)
            parts.Add($"業績 {(ScoreGranted >= 0 ? "+" : "")}{ScoreGranted}");
        if (RunesGranted.Count > 0)
            parts.Add($"符文 ×{RunesGranted.Count}");
        if (ScrollsGranted.Count > 0)
            parts.Add($"卷軸 ×{ScrollsGranted.Count}");
        return parts.Count > 0
            ? $"[{SourceName}] {string.Join(" / ", parts)}"
            : $"[{SourceName}] 無獎勵";
    }
}

// =========================================================
// RewardManager — 靜態分發器
// =========================================================

/// <summary>
/// 靜態獎勵分發器。
///
/// 呼叫 Grant() 時：
///   1. 依 RewardEntry.type 從對應的 Manager 派發獎勵
///   2. 建立 RewardResult 紀錄實際給出的內容
///   3. 觸發靜態事件 OnRewardGranted（GameFacade 訂閱後轉發給 UI）
///
/// 目前支援類型：Score / RandomRune / RandomScroll
/// Relic 類型（需玩家接受/拒絕）留作後續 UI 流程實作。
/// </summary>
public static class RewardManager
{
    // =========================================================
    // 靜態事件（GameFacade 訂閱）
    // =========================================================

    /// <summary>每次有獎勵實際發放時觸發（含 RewardResult 內容）</summary>
    public static event Action<RewardResult> OnRewardGranted;

    // =========================================================
    // 主要入口
    // =========================================================

    /// <summary>
    /// 分發一組 RewardEntry，將獎勵實際套用至對應的 Manager，
    /// 並觸發 OnRewardGranted 事件供 UI 顯示。
    /// </summary>
    /// <param name="entries">獎勵條目列表（null 或空列表直接返回空結果）</param>
    /// <param name="sourceName">來源名稱，用於 UI 顯示（顧客名 / 遺物名）</param>
    /// <returns>本次實際發放的結果</returns>
    public static RewardResult Grant(List<RewardEntry> entries, string sourceName = "")
    {
        var result = new RewardResult { SourceName = sourceName };

        if (entries == null || entries.Count == 0)
            return result;

        foreach (var entry in entries)
        {
            if (entry == null) continue;

            switch (entry.type)
            {
                case RewardType.Score:
                    GrantScore(entry, result);
                    break;

                case RewardType.RandomRune:
                    GrantRunes(entry, result);
                    break;

                case RewardType.RandomScroll:
                    GrantScrolls(entry, result);
                    break;

                case RewardType.Relic:
                    // TODO：需要 UI 確認流程（接受/拒絕），暫不實作
                    Debug.LogWarning($"[RewardManager] Relic 類型獎勵尚未實作，來源：{sourceName}");
                    break;
            }
        }

        if (result.HasAnyReward)
        {
            Debug.Log($"[RewardManager] {result}");
            OnRewardGranted?.Invoke(result);
        }

        return result;
    }

    // =========================================================
    // 私有：各類型分發
    // =========================================================

    private static void GrantScore(RewardEntry entry, RewardResult result)
    {
        if (entry.scoreAmount == 0) return;

        EnvironmentManager.Instance?.ModifyScore(entry.scoreAmount);
        result.ScoreGranted += entry.scoreAmount;
    }

    private static void GrantRunes(RewardEntry entry, RewardResult result)
    {
        if (entry.runePool == null || entry.runePool.Length == 0)
        {
            Debug.LogWarning("[RewardManager] RandomRune：runePool 為空，跳過");
            return;
        }

        int count   = Mathf.Min(entry.runeCount, entry.runePool.Length);
        var indices = PickRandomIndices(entry.runePool.Length, count);

        foreach (int idx in indices)
        {
            var rune = entry.runePool[idx];
            if (rune == null) continue;

            DeckManager.Instance?.AddRune(rune);
            result.RunesGranted.Add(rune);
            Debug.Log($"[RewardManager] 符文獎勵：{rune.runeName}");
        }
    }

    private static void GrantScrolls(RewardEntry entry, RewardResult result)
    {
        if (entry.scrollPool == null || entry.scrollPool.Length == 0)
        {
            Debug.LogWarning("[RewardManager] RandomScroll：scrollPool 為空，跳過");
            return;
        }

        int count   = Mathf.Min(entry.scrollCount, entry.scrollPool.Length);
        var indices = PickRandomIndices(entry.scrollPool.Length, count);

        foreach (int idx in indices)
        {
            var scroll = entry.scrollPool[idx];
            if (scroll == null) continue;

            ScrollInventory.Instance?.AddScroll(scroll);
            result.ScrollsGranted.Add(scroll);
            Debug.Log($"[RewardManager] 卷軸獎勵：{scroll.scrollName}");
        }
    }

    // =========================================================
    // 工具：不重複隨機選取 count 個 index
    // =========================================================

    private static List<int> PickRandomIndices(int poolSize, int count)
    {
        var available = new List<int>(poolSize);
        for (int i = 0; i < poolSize; i++) available.Add(i);

        var picked = new List<int>(count);
        for (int i = 0; i < count && available.Count > 0; i++)
        {
            int r = UnityEngine.Random.Range(0, available.Count);
            picked.Add(available[r]);
            available.RemoveAt(r);
        }
        return picked;
    }
}

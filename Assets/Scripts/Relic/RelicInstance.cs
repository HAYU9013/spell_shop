using System.Collections.Generic;

// =========================================================
// Tick 結果（傳遞給 RelicManager 再套用至各 Manager）
// =========================================================

/// <summary>
/// RelicInstance.Tick() 的回傳結果。
/// RelicManager 根據此結果呼叫 EnvironmentManager / GameManager。
/// </summary>
public class RelicTickResult
{
    /// <summary>本回合是否滿意</summary>
    public bool IsSatisfied;

    /// <summary>要套用至環境的效果（滿意或不滿意效果，取其一）</summary>
    public RuneEffect[] EnvEffects;

    /// <summary>業績變化量</summary>
    public int ScoreChange;

    /// <summary>本回合是否觸發懲罰</summary>
    public bool PunishmentTriggered;

    /// <summary>懲罰類型（PunishmentTriggered = true 時有效）</summary>
    public RelicPunishment PunishmentType;

    /// <summary>環境衝擊屬性（EnvironmentShock 時有效）</summary>
    public EnvAttribute ShockAttribute;

    /// <summary>衝擊數值</summary>
    public int ShockValue;

    /// <summary>true = 強制設定；false = delta</summary>
    public bool ShockIsForceSet;

    /// <summary>
    /// 滿意時的額外獎勵清單（由 RelicManager 轉交 RewardManager 處理）。
    /// 不滿意時此欄位為 null。
    /// </summary>
    public System.Collections.Generic.List<RewardEntry> SatisfiedRewards;
}

// =========================================================
// 遺物執行時實例
// =========================================================

/// <summary>
/// 遺物執行時實例。
/// 包裝一份 RelicData，追蹤不滿意計數並執行每回合的結算邏輯。
///
/// 計數規則：
///   滿意  → UnsatisfiedCount 歸零
///   不滿意 → UnsatisfiedCount +1
///   計數 >= Data.unsatisfiedLimit → 懲罰觸發（計數不重置）
/// </summary>
public class RelicInstance
{
    // =========================================================
    // 欄位
    // =========================================================

    public RelicData Data { get; private set; }

    /// <summary>當前不滿意累積計數</summary>
    public int UnsatisfiedCount { get; private set; }

    /// <summary>是否已在懲罰狀態（計數 >= 上限）</summary>
    public bool IsInPunishment => UnsatisfiedCount >= Data.unsatisfiedLimit;

    // =========================================================
    // 建構子
    // =========================================================

    public RelicInstance(RelicData data)
    {
        Data             = data;
        UnsatisfiedCount = 0;
    }

    // =========================================================
    // 核心：每回合結算
    // =========================================================

    /// <summary>
    /// 判定當前環境是否滿足條件，更新計數，回傳本回合的效果與懲罰結果。
    /// 由 RelicManager.TickAll() 呼叫。
    /// </summary>
    public RelicTickResult Tick(EnvironmentData env)
    {
        bool satisfied = Data.satisfiedCondition.Evaluate(env);

        if (satisfied)
        {
            UnsatisfiedCount = 0;
            return new RelicTickResult
            {
                IsSatisfied         = true,
                EnvEffects          = Data.satisfiedEnvEffects,
                ScoreChange         = Data.satisfiedScoreChange,
                PunishmentTriggered = false,
                SatisfiedRewards    = Data.satisfiedRewards
            };
        }
        else
        {
            UnsatisfiedCount++;

            bool punishment = UnsatisfiedCount >= Data.unsatisfiedLimit;

            return new RelicTickResult
            {
                IsSatisfied         = false,
                EnvEffects          = Data.unsatisfiedEnvEffects,
                ScoreChange         = Data.unsatisfiedScoreChange,
                PunishmentTriggered = punishment,
                PunishmentType      = Data.punishmentType,
                ShockAttribute      = Data.shockAttribute,
                ShockValue          = Data.shockValue,
                ShockIsForceSet     = Data.shockIsForceSet
            };
        }
    }

    /// <summary>
    /// 黑暗降臨時強制將計數 +1（不論當前是否滿意）。
    /// 由 RelicManager 訂閱 EnvironmentManager.OnDarknessTriggered 後呼叫。
    /// </summary>
    public void ForceIncrementUnsatisfied()
    {
        UnsatisfiedCount++;
    }

    // =========================================================
    // 工具
    // =========================================================

    public override string ToString()
    {
        return $"[Relic] {Data.relicName} | 不滿意計數：{UnsatisfiedCount}/{Data.unsatisfiedLimit}" +
               (IsInPunishment ? " ⚠ 懲罰中" : "");
    }
}

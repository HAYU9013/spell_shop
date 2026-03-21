using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 顧客執行時實例。
/// 包裝 CustomerData，追蹤耐心計數、累積環境變化量與穩定性進度。
///
/// 生命週期：
///   CustomerManager.SpawnNextCustomer() 建立 → 記錄 ArrivalSnapshot
///   每回合 Phase 5 後呼叫 UpdateTracking(env) 更新追蹤數據
///   Phase 6 呼叫 CheckRequirements() 判定是否滿足
///   Phase 7 呼叫 TickPatience() 扣耐心
/// </summary>
public class CustomerInstance
{
    // =========================================================
    // 基本
    // =========================================================

    public CustomerData Data { get; private set; }

    // =========================================================
    // 耐心
    // =========================================================

    /// <summary>剩餘耐心（初始 = Data.maxPatience）</summary>
    public int RemainingPatience { get; private set; }

    public bool IsOutOfPatience => RemainingPatience <= 0;

    // =========================================================
    // 需求追蹤
    // =========================================================

    /// <summary>顧客到來時的環境快照（相對變化基準線）</summary>
    public EnvironmentData ArrivalSnapshot { get; private set; }

    /// <summary>顧客到來至今的累積環境變化量</summary>
    private EnvironmentDelta _accumulatedDelta;

    /// <summary>每條 Stability 需求的連續計數（index 對應 _requirements）</summary>
    private readonly int[] _stabilityCounters;

    /// <summary>Runtime IRequirement 實例列表</summary>
    private readonly List<IRequirement> _requirements;

    // =========================================================
    // 狀態
    // =========================================================

    /// <summary>本回合需求是否已滿足（Phase 6 設定，Phase 7 用來判斷是否跳過耐心扣除）</summary>
    public bool IsSatisfiedThisTurn { get; private set; }

    // =========================================================
    // 建構子
    // =========================================================

    public CustomerInstance(CustomerData data, EnvironmentData arrivalSnapshot)
    {
        Data              = data;
        ArrivalSnapshot   = arrivalSnapshot;
        RemainingPatience = data.maxPatience;
        IsSatisfiedThisTurn = false;

        // 建立需求實例
        _requirements = new List<IRequirement>();
        foreach (var rd in data.requirements)
            _requirements.Add(RequirementFactory.Create(rd));

        _stabilityCounters = new int[_requirements.Count];
        _accumulatedDelta  = new EnvironmentDelta();
    }

    // =========================================================
    // Phase 5 後：更新追蹤數據
    // =========================================================

    /// <summary>
    /// 每回合環境結算後呼叫，更新累積 delta 與穩定性計數。
    /// 由 EnvironmentResolveState 或 CustomerManager 呼叫。
    /// </summary>
    public void UpdateTracking(EnvironmentData currentEnv)
    {
        // 更新累積 delta
        _accumulatedDelta = EnvironmentData.Diff(ArrivalSnapshot, currentEnv);

        // 更新 Stability 計數
        for (int i = 0; i < _requirements.Count; i++)
        {
            if (_requirements[i] is StabilityRequirement stability)
            {
                if (stability.IsConditionMet(currentEnv))
                    _stabilityCounters[i]++;
                else
                    _stabilityCounters[i] = 0;
            }
        }
    }

    // =========================================================
    // Phase 6：需求判定
    // =========================================================

    /// <summary>
    /// 判斷所有需求是否同時滿足。
    /// 結果會暫存至 IsSatisfiedThisTurn，供 Phase 7 判斷是否跳過耐心扣除。
    /// </summary>
    public bool CheckRequirements(EnvironmentData currentEnv)
    {
        for (int i = 0; i < _requirements.Count; i++)
        {
            var ctx = BuildContext(currentEnv, i);
            if (!_requirements[i].IsSatisfied(ctx))
            {
                IsSatisfiedThisTurn = false;
                return false;
            }
        }
        IsSatisfiedThisTurn = true;
        return true;
    }

    // =========================================================
    // Phase 7：耐心扣除
    // =========================================================

    /// <summary>
    /// 耐心 -1，回傳耐心是否耗盡。
    /// 僅在 Phase 6 未滿足時呼叫。
    /// </summary>
    public bool TickPatience()
    {
        RemainingPatience = Mathf.Max(0, RemainingPatience - 1);
        return IsOutOfPatience;
    }

    // =========================================================
    // UI 進度文字
    // =========================================================

    /// <summary>回傳每條需求的進度文字列表（供顧客面板顯示）</summary>
    public List<string> GetProgressTexts(EnvironmentData currentEnv)
    {
        var texts = new List<string>();
        for (int i = 0; i < _requirements.Count; i++)
        {
            var ctx = BuildContext(currentEnv, i);
            texts.Add(_requirements[i].GetProgressText(ctx));
        }
        return texts;
    }

    // =========================================================
    // 工具
    // =========================================================

    private RequirementContext BuildContext(EnvironmentData currentEnv, int requirementIndex)
    {
        return new RequirementContext
        {
            CurrentEnv       = currentEnv,
            ArrivalSnapshot  = ArrivalSnapshot,
            AccumulatedDelta = _accumulatedDelta,
            StabilityCount   = _stabilityCounters[requirementIndex]
        };
    }

    public override string ToString()
    {
        return $"[Customer] {Data.customerName} | 耐心：{RemainingPatience}/{Data.maxPatience}";
    }
}

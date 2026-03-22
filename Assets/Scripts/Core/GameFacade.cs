using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 所有 UI 腳本的唯一資料讀取與操作入口。
///
/// 使用方式：
///   1. 讀取資料 → GameFacade.Instance.Environment / .Workbench / .Hand ...
///   2. 觸發操作 → GameFacade.Instance.SelectScroll() / .PlaceRune() / .SubmitScroll()
///   3. 訂閱事件 → GameFacade.Instance.OnEnvironmentChanged += Refresh
///
/// UI 腳本規則：
///   - 永遠只透過 GameFacade，不直接引用任何 Manager
///   - OnEnable 訂閱事件，OnDisable 取消訂閱
///   - Refresh 方法要冪等，不快取快照
/// </summary>
public class GameFacade : MonoBehaviour
{
    // =========================================================
    // Singleton
    // =========================================================

    public static GameFacade Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // =========================================================
    // 事件（UI 在 OnEnable/OnDisable 訂閱/取消）
    // =========================================================

    /// <summary>環境數值（亮度/水分/溫度）任一變動時觸發</summary>
    public event Action<EnvironmentData> OnEnvironmentChanged;

    /// <summary>業績數值變動，帶 delta（+N/-N），用於跳字動畫</summary>
    public event Action<int /*newScore*/, int /*delta*/> OnScoreChanged;

    /// <summary>工作台狀態變動（選卷軸、放符文、取消符文）</summary>
    public event Action<WorkbenchSnapshot> OnWorkbenchChanged;

    /// <summary>手牌更新（每回合重新抽牌後）</summary>
    public event Action<IReadOnlyList<RuneData>> OnHandUpdated;

    /// <summary>顧客切換（新顧客到來或顧客離開）</summary>
    public event Action<CustomerSnapshot> OnCustomerChanged;

    /// <summary>顧客需求進度更新（環境數值變動後即時更新）</summary>
    public event Action<CustomerSnapshot> OnRequirementProgressUpdated;

    /// <summary>遺物狀態更新（每回合 Phase 8 遺物結算後）</summary>
    public event Action<IReadOnlyList<RelicSnapshot>> OnRelicsChanged;

    /// <summary>極端事件觸發（用於全畫面閃爍、警告演出）</summary>
    public event Action<ExtremeEvent> OnExtremeEventTriggered;

    /// <summary>回合階段切換（用於鎖定/解鎖 UI 互動）</summary>
    public event Action<TurnManager.TurnPhase> OnPhaseChanged;

    /// <summary>遊戲結束（finalScore, turnsCleared）</summary>
    public event Action<int /*finalScore*/, int /*turnsCleared*/> OnGameOver;

    /// <summary>關卡通關（關卡模式：顧客隊列清空）</summary>
    public event Action OnLevelClear;

    /// <summary>
    /// 獎勵實際發放時觸發（顧客滿足 / 遺物每回合滿意）。
    /// UI 可用於顯示「獲得 水珠 ×2」等提示。
    /// </summary>
    public event Action<RewardResult> OnRewardGranted;

    // =========================================================
    // 內部狀態
    // =========================================================

    /// <summary>追蹤上一次業績，用於計算 OnScoreChanged 的 delta</summary>
    private int _lastScore;

    // =========================================================
    // 生命週期
    // =========================================================

    private void Start()
    {
        _lastScore = EnvironmentManager.Instance != null ? EnvironmentManager.Instance.Score : 0;
        SubscribeAll();
    }

    private void OnDestroy()
    {
        UnsubscribeAll();
    }

    // =========================================================
    // Read API — 環境
    // =========================================================

    /// <summary>當前環境數值快照（亮度、水分、溫度）</summary>
    public EnvironmentData Environment =>
        EnvironmentManager.Instance != null
            ? EnvironmentManager.Instance.SnapshotCurrent()
            : new EnvironmentData();

    /// <summary>當前業績</summary>
    public int Score => EnvironmentManager.Instance != null ? EnvironmentManager.Instance.Score : 0;

    /// <summary>
    /// 工作台「送出後」的預覽環境 delta。
    /// 根據目前已放入槽位的符文即時計算，送出按鈕旁的預覽文字使用此屬性。
    /// </summary>
    public EnvironmentDelta WorkbenchPreviewDelta
    {
        get
        {
            var wb = WorkbenchManager.Instance;
            if (wb == null || wb.SelectedScroll == null) return default;

            var placedRunes = wb.Slots
                .Where(slot => slot != null)
                .Select(slot => slot.Data)
                .ToArray();

            if (placedRunes.Length == 0) return default;

            var tempRecord = new SubmitRecord(0, wb.SelectedScroll, placedRunes);
            var effects = ScrollProcessor.Process(tempRecord);

            int db = 0, dm = 0, dt = 0;
            foreach (var e in effects)
            {
                switch (e.attribute)
                {
                    case EnvAttribute.Brightness:  db += e.value; break;
                    case EnvAttribute.Moisture:    dm += e.value; break;
                    case EnvAttribute.Temperature: dt += e.value; break;
                }
            }

            return new EnvironmentDelta
            {
                brightnessDelta  = db,
                moistureDelta    = dm,
                temperatureDelta = dt
            };
        }
    }

    // =========================================================
    // Read API — 工作台
    // =========================================================

    /// <summary>工作台完整狀態快照</summary>
    public WorkbenchSnapshot Workbench
    {
        get
        {
            var wb  = WorkbenchManager.Instance;
            var inv = ScrollInventory.Instance;

            if (wb == null)
                return new WorkbenchSnapshot { AvailableScrolls = new List<ScrollData>() };

            return new WorkbenchSnapshot
            {
                SelectedScroll   = wb.SelectedScroll,
                SlotCount        = wb.Slots.Count,
                PlacedRunes      = wb.Slots.Select(r => r?.Data).ToArray(),
                CanSubmit        = wb.CanSubmit(),
                AvailableScrolls = inv != null ? inv.Scrolls.ToList() : new List<ScrollData>()
            };
        }
    }

    // =========================================================
    // Read API — 手牌
    // =========================================================

    /// <summary>當前手牌列表（每回合補充後更新）</summary>
    public IReadOnlyList<RuneData> Hand =>
        DeckManager.Instance != null
            ? (IReadOnlyList<RuneData>)DeckManager.Instance.Hand.Select(r => r.Data).ToList()
            : new List<RuneData>();

    // =========================================================
    // Read API — 顧客
    // =========================================================

    /// <summary>當前顧客快照（IsPresent = false 表示目前無顧客）</summary>
    public CustomerSnapshot CurrentCustomer => BuildCurrentCustomerSnapshot();

    // =========================================================
    // Read API — 遺物
    // =========================================================

    /// <summary>所有遺物的快照列表</summary>
    public IReadOnlyList<RelicSnapshot> Relics => BuildRelicSnapshots();

    // =========================================================
    // Read API — 回合狀態
    // =========================================================

    /// <summary>目前回合階段（可用於在 PlayerAction 以外的階段鎖定 UI）</summary>
    public TurnManager.TurnPhase CurrentPhase =>
        TurnManager.Instance != null ? TurnManager.Instance.CurrentPhase : default;

    // =========================================================
    // Action API — 工作台操作
    // =========================================================

    /// <summary>
    /// 選擇一個卷軸（從 Workbench.AvailableScrolls 中選取）。
    /// 選擇後清空所有槽位，觸發 OnWorkbenchChanged。
    /// </summary>
    public void SelectScroll(ScrollData scroll)
    {
        WorkbenchManager.Instance?.SelectScroll(scroll);
    }

    /// <summary>
    /// 將手牌中的一張符文放入下一個空槽。
    /// 若手牌中找不到對應符文，或槽位已滿，回傳 false。
    /// </summary>
    public bool PlaceRune(RuneData runeData)
    {
        var wb   = WorkbenchManager.Instance;
        var deck = DeckManager.Instance;
        if (wb == null || deck == null) return false;

        // 在手牌中找第一張相符的 RuneInstance（未消耗）
        var instance = deck.Hand.FirstOrDefault(r => r.Data == runeData && !r.IsExhausted);
        if (instance == null) return false;

        int slotIndex = wb.PlaceRune(instance);
        return slotIndex >= 0;
    }

    /// <summary>
    /// 取消指定槽位的符文（符文退回手牌區）。
    /// </summary>
    public void RemoveRuneAt(int slotIndex)
    {
        WorkbenchManager.Instance?.RemoveRune(slotIndex);
    }

    /// <summary>
    /// 送出卷軸。CanSubmit 為 false 時呼叫無效。
    /// 內部呼叫 WorkbenchManager.Submit()，由 WorkbenchManager 驅動後續回合流程。
    /// </summary>
    public void SubmitScroll()
    {
        var wb = WorkbenchManager.Instance;
        if (wb == null || !wb.CanSubmit()) return;
        wb.Submit();
    }

    // =========================================================
    // Action API — 丟棄
    // =========================================================

    /// <summary>
    /// 丟棄庫存中的一張卷軸。
    /// 若工作台正在使用同一張卷軸，先取消選取再丟棄。
    /// </summary>
    public bool DiscardScroll(ScrollData scroll)
    {
        var inv = ScrollInventory.Instance;
        if (inv == null || scroll == null) return false;

        // 若工作台正在使用此卷軸，先清空工作台
        var wb = WorkbenchManager.Instance;
        if (wb != null && wb.SelectedScroll == scroll)
            wb.SelectScroll(null);

        return inv.DiscardScroll(scroll);
    }

    /// <summary>
    /// 丟棄指定索引的遺物。
    /// </summary>
    public bool DiscardRelic(int relicIndex)
    {
        return RelicManager.Instance?.DiscardRelicAt(relicIndex) ?? false;
    }

    // =========================================================
    // 訂閱 / 取消訂閱
    // =========================================================

    private void SubscribeAll()
    {
        if (EnvironmentManager.Instance != null)
        {
            EnvironmentManager.Instance.OnValueChanged           += HandleEnvValueChanged;
            EnvironmentManager.Instance.OnScoreChanged           += HandleScoreChanged;
            EnvironmentManager.Instance.OnExtremeEventTriggered  += HandleExtremeEvents;
        }

        if (WorkbenchManager.Instance != null)
            WorkbenchManager.Instance.OnWorkbenchChanged += HandleWorkbenchChanged;

        if (DeckManager.Instance != null)
            DeckManager.Instance.OnHandChanged += HandleHandChanged;

        if (CustomerManager.Instance != null)
        {
            CustomerManager.Instance.OnCustomerArrived  += HandleCustomerArrived;
            CustomerManager.Instance.OnCustomerSatisfied += HandleCustomerDeparted;
            CustomerManager.Instance.OnCustomerLeft     += HandleCustomerDeparted;
            Debug.Log("[GameFacade] 已訂閱 CustomerManager 事件");
        }
        else
        {
            Debug.LogWarning("[GameFacade] SubscribeAll：CustomerManager.Instance 為 null，未訂閱顧客事件");
        }

        if (RelicManager.Instance != null)
        {
            RelicManager.Instance.OnRelicCountChanged   += HandleRelicChanged;
            RelicManager.Instance.OnPunishmentTriggered += HandleRelicPunishment;
            RelicManager.Instance.OnRelicRemoved        += HandleRelicChanged;
        }

        if (TurnManager.Instance != null)
            TurnManager.Instance.OnPhaseChanged += HandlePhaseChanged;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver   += HandleGameOver;
            GameManager.Instance.OnLevelClear += HandleLevelClear;
        }

        RewardManager.OnRewardGranted += HandleRewardGranted;
    }

    private void UnsubscribeAll()
    {
        if (EnvironmentManager.Instance != null)
        {
            EnvironmentManager.Instance.OnValueChanged           -= HandleEnvValueChanged;
            EnvironmentManager.Instance.OnScoreChanged           -= HandleScoreChanged;
            EnvironmentManager.Instance.OnExtremeEventTriggered  -= HandleExtremeEvents;
        }

        if (WorkbenchManager.Instance != null)
            WorkbenchManager.Instance.OnWorkbenchChanged -= HandleWorkbenchChanged;

        if (DeckManager.Instance != null)
            DeckManager.Instance.OnHandChanged -= HandleHandChanged;

        if (CustomerManager.Instance != null)
        {
            CustomerManager.Instance.OnCustomerArrived  -= HandleCustomerArrived;
            CustomerManager.Instance.OnCustomerSatisfied -= HandleCustomerDeparted;
            CustomerManager.Instance.OnCustomerLeft     -= HandleCustomerDeparted;
        }

        if (RelicManager.Instance != null)
        {
            RelicManager.Instance.OnRelicCountChanged   -= HandleRelicChanged;
            RelicManager.Instance.OnPunishmentTriggered -= HandleRelicPunishment;
            RelicManager.Instance.OnRelicRemoved        -= HandleRelicChanged;
        }

        if (TurnManager.Instance != null)
            TurnManager.Instance.OnPhaseChanged -= HandlePhaseChanged;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameOver   -= HandleGameOver;
            GameManager.Instance.OnLevelClear -= HandleLevelClear;
        }

        RewardManager.OnRewardGranted -= HandleRewardGranted;
    }

    // =========================================================
    // 事件處理器
    // =========================================================

    private void HandleEnvValueChanged(EnvAttribute attr, int newValue)
    {
        var snapshot = EnvironmentManager.Instance.SnapshotCurrent();
        OnEnvironmentChanged?.Invoke(snapshot);

        // 環境變動時同步更新需求進度（讓 CustomerPanel 即時反映）
        if (CustomerManager.Instance != null && CustomerManager.Instance.HasCustomer)
            OnRequirementProgressUpdated?.Invoke(BuildCurrentCustomerSnapshot());
    }

    private void HandleScoreChanged(int newScore)
    {
        int delta  = newScore - _lastScore;
        _lastScore = newScore;
        OnScoreChanged?.Invoke(newScore, delta);
    }

    private void HandleExtremeEvents(List<ExtremeEvent> events)
    {
        foreach (var ev in events)
            if (ev != ExtremeEvent.None)
                OnExtremeEventTriggered?.Invoke(ev);
    }

    private void HandleWorkbenchChanged()
    {
        OnWorkbenchChanged?.Invoke(Workbench);
    }

    private void HandleHandChanged()
    {
        OnHandUpdated?.Invoke(Hand);
    }

    private void HandleCustomerArrived(CustomerInstance customer)
    {
        Debug.Log($"[GameFacade] HandleCustomerArrived → {customer?.Data?.customerName}，subscribers={OnCustomerChanged?.GetInvocationList()?.Length ?? 0}");
        OnCustomerChanged?.Invoke(BuildCustomerSnapshot(customer));
    }

    private void HandleCustomerDeparted(CustomerInstance customer)
    {
        // 顧客離開（滿足或憤怒），通知 UI 清空顧客面板
        OnCustomerChanged?.Invoke(new CustomerSnapshot { IsPresent = false });
    }

    private void HandleRelicChanged(RelicInstance _)
    {
        OnRelicsChanged?.Invoke(BuildRelicSnapshots());
    }

    private void HandleRelicPunishment(RelicInstance relic, RelicTickResult result)
    {
        // 懲罰觸發同樣更新遺物面板（計數、狀態可能改變）
        OnRelicsChanged?.Invoke(BuildRelicSnapshots());
    }

    private void HandlePhaseChanged(TurnManager.TurnPhase phase)
    {
        OnPhaseChanged?.Invoke(phase);

        // Phase 8（RelicUpdate）結束後，遺物計數已更新，刷新遺物面板
        if (phase == TurnManager.TurnPhase.RelicUpdate)
            OnRelicsChanged?.Invoke(BuildRelicSnapshots());
    }

    private void HandleGameOver(string reason)
    {
        int finalScore   = EnvironmentManager.Instance != null ? EnvironmentManager.Instance.Score : 0;
        int turnsCleared = TurnManager.Instance != null ? TurnManager.Instance.CurrentTurn : 0;
        OnGameOver?.Invoke(finalScore, turnsCleared);
    }

    private void HandleLevelClear()
    {
        OnLevelClear?.Invoke();
    }

    private void HandleRewardGranted(RewardResult result)
    {
        OnRewardGranted?.Invoke(result);
    }

    // =========================================================
    // 快照建立 — 顧客
    // =========================================================

    private CustomerSnapshot BuildCurrentCustomerSnapshot()
    {
        return BuildCustomerSnapshot(CustomerManager.Instance?.CurrentCustomer);
    }

    private CustomerSnapshot BuildCustomerSnapshot(CustomerInstance customer)
    {
        if (customer == null)
            return new CustomerSnapshot { IsPresent = false };

        var env      = EnvironmentManager.Instance != null
                         ? EnvironmentManager.Instance.SnapshotCurrent()
                         : new EnvironmentData();
        var statuses = customer.GetRequirementStatuses(env);

        var requirements = statuses
            .Select(s => new RequirementSnapshot
            {
                DisplayText = s.displayText,
                IsSatisfied = s.isSatisfied,
                Progress    = s.isSatisfied ? 1f : 0f
            })
            .ToList();

        return new CustomerSnapshot
        {
            IsPresent       = true,
            Name            = customer.Data.customerName,
            FlavorText      = customer.Data.flavorText,
            Portrait        = customer.Data.portrait,
            CurrentPatience = customer.RemainingPatience,
            MaxPatience     = customer.Data.maxPatience,
            Requirements    = requirements,
            QueueRemaining  = CustomerManager.Instance != null
                                  ? CustomerManager.Instance.RemainingInQueue
                                  : 0
        };
    }

    // =========================================================
    // 快照建立 — 遺物
    // =========================================================

    private IReadOnlyList<RelicSnapshot> BuildRelicSnapshots()
    {
        if (RelicManager.Instance == null) return new List<RelicSnapshot>();

        var env = EnvironmentManager.Instance != null
                      ? EnvironmentManager.Instance.SnapshotCurrent()
                      : new EnvironmentData();

        return RelicManager.Instance.Relics
            .Select(relic => new RelicSnapshot
            {
                RelicName             = relic.Data.relicName,
                Description           = relic.Data.description,
                Icon                  = relic.Data.icon,
                IsSatisfied           = relic.Data.EvaluateAll(env),
                DissatisfiedCount     = relic.UnsatisfiedCount,
                DissatisfiedLimit     = relic.Data.unsatisfiedLimit,
                SatisfiedEffectText   = BuildEffectText(relic.Data.satisfiedEnvEffects, relic.Data.satisfiedScoreChange),
                UnsatisfiedEffectText = BuildEffectText(relic.Data.unsatisfiedEnvEffects, relic.Data.unsatisfiedScoreChange),
                PunishmentText        = BuildPunishmentText(relic.Data)
            })
            .ToList();
    }

    // =========================================================
    // 文字建構輔助
    // =========================================================

    private static string BuildEffectText(RuneEffect[] effects, int scoreChange)
    {
        var parts = new List<string>();

        if (effects != null)
            foreach (var e in effects)
                parts.Add($"{AttrName(e.attribute)} {Sign(e.value)}{e.value}");

        if (scoreChange != 0)
            parts.Add($"業績 {Sign(scoreChange)}{scoreChange}");

        return parts.Count > 0 ? string.Join(" / ", parts) : "無";
    }

    private static string BuildPunishmentText(RelicData data)
    {
        string main;
        switch (data.punishmentType)
        {
            case RelicPunishment.PlayerDeath:
                main = "Game Over";
                break;
            case RelicPunishment.ScoreReset:
                main = "業績歸零";
                break;
            case RelicPunishment.DiscardSelf:
                main = "失去此遺物";
                break;
            case RelicPunishment.EnvironmentShock:
                string prefix = data.shockIsForceSet ? "設為 " : Sign(data.shockValue);
                main = $"{AttrName(data.shockAttribute)} {prefix}{data.shockValue}";
                break;
            default:
                main = "未知懲罰";
                break;
        }

        // 附加符文懲罰
        if (data.punishmentRunes != null && data.punishmentRunes.Count > 0)
        {
            var parts = new List<string>();
            foreach (var entry in data.punishmentRunes)
            {
                if (entry == null) continue;
                if (entry.type == RewardType.SpecificRune && entry.specificRune != null)
                    parts.Add($"{entry.specificRune.runeName} ×{entry.runeCount}");
                else if (entry.type == RewardType.RandomRune)
                    parts.Add($"隨機符文 ×{entry.runeCount}");
            }
            if (parts.Count > 0)
                main += $" + 給予 {string.Join("、", parts)}";
        }

        return main;
    }

    private static string AttrName(EnvAttribute attr)
    {
        switch (attr)
        {
            case EnvAttribute.Brightness:  return "亮度";
            case EnvAttribute.Moisture:    return "水分";
            case EnvAttribute.Temperature: return "溫度";
            default: return attr.ToString();
        }
    }

    /// <summary>回傳 "+" 或 ""（負號由數值本身帶）</summary>
    private static string Sign(int value) => value >= 0 ? "+" : "";
}

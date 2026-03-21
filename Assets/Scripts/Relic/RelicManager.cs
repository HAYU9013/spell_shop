using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 遺物管理器（MonoBehaviour Singleton）。
/// 持有所有 RelicInstance，負責：
///   - Phase 2：TickAll() — 依 displayOrder 結算每個遺物效果與懲罰
///   - 訂閱 EnvironmentManager.OnDarknessTriggered → ForceIncrementAllUnsatisfied
///   - 提供查詢介面供 UI 訂閱
/// </summary>
public class RelicManager : MonoBehaviour
{
    // =========================================================
    // Singleton
    // =========================================================

    public static RelicManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // 訂閱黑暗降臨事件（EnvironmentManager 在 Awake 初始化，Start 才能安全取用）
        if (EnvironmentManager.Instance != null)
            EnvironmentManager.Instance.OnDarknessTriggered += ForceIncrementAllUnsatisfied;
        else
            Debug.LogWarning("[RelicManager] 找不到 EnvironmentManager，OnDarknessTriggered 無法訂閱");
    }

    private void OnDestroy()
    {
        if (EnvironmentManager.Instance != null)
            EnvironmentManager.Instance.OnDarknessTriggered -= ForceIncrementAllUnsatisfied;
    }

    // =========================================================
    // 事件（供 UI 訂閱）
    // =========================================================

    /// <summary>任何遺物的不滿意計數改變時觸發</summary>
    public event System.Action<RelicInstance> OnRelicCountChanged;

    /// <summary>懲罰觸發時通知（含結果，供 GameManager 處理 PlayerDeath）</summary>
    public event System.Action<RelicInstance, RelicTickResult> OnPunishmentTriggered;

    // =========================================================
    // 遺物列表
    // =========================================================

    private readonly List<RelicInstance> _relics = new List<RelicInstance>();

    /// <summary>只讀的遺物列表（供 UI 遍歷顯示）</summary>
    public IReadOnlyList<RelicInstance> Relics => _relics;

    // =========================================================
    // 初始化
    // =========================================================

    /// <summary>
    /// 從 RelicData 列表建立 RelicInstance。
    /// 由 GameManager 在遊戲開始時呼叫，依 displayOrder 排序。
    /// </summary>
    public void InitRelics(List<RelicData> relicDataList)
    {
        _relics.Clear();
        foreach (var data in relicDataList)
            _relics.Add(new RelicInstance(data));

        // 依 displayOrder 排序
        _relics.Sort((a, b) => a.Data.displayOrder.CompareTo(b.Data.displayOrder));

        Debug.Log($"[RelicManager] 初始化 {_relics.Count} 個遺物");
    }

    /// <summary>加入單一遺物（顧客獎勵時使用），自動維持排序</summary>
    public void AddRelic(RelicData data)
    {
        _relics.Add(new RelicInstance(data));
        _relics.Sort((a, b) => a.Data.displayOrder.CompareTo(b.Data.displayOrder));
        Debug.Log($"[RelicManager] 新增遺物：{data.relicName}");
    }

    // =========================================================
    // Phase 2：回合結算
    // =========================================================

    /// <summary>
    /// 依序結算所有遺物。
    /// 由 RelicTriggerState.Execute() 呼叫。
    /// 每個遺物的效果透過 EnvironmentManager 套用。
    /// </summary>
    public void TickAll()
    {
        var env = EnvironmentManager.Instance;
        if (env == null)
        {
            Debug.LogWarning("[RelicManager] TickAll：找不到 EnvironmentManager");
            return;
        }

        foreach (var relic in _relics)
        {
            // 取得當前環境快照供判定（Tick 結束後環境可能已變）
            var envData = new EnvironmentData
            {
                brightness   = env.Brightness,
                moisture     = env.Moisture,
                temperature  = env.Temperature
            };

            RelicTickResult result = relic.Tick(envData);

            // 套用環境效果
            if (result.EnvEffects != null && result.EnvEffects.Length > 0)
                env.ApplyEffects(new System.Collections.Generic.List<RuneEffect>(result.EnvEffects));

            // 套用業績變化
            if (result.ScoreChange != 0)
                env.ModifyScore(result.ScoreChange);

            // 通知計數變化
            OnRelicCountChanged?.Invoke(relic);

            // 懲罰
            if (result.PunishmentTriggered)
            {
                Debug.Log($"[RelicManager] {relic.Data.relicName} 觸發懲罰：{result.PunishmentType}");
                ApplyPunishment(relic, result);
                OnPunishmentTriggered?.Invoke(relic, result);
            }

            Debug.Log($"[RelicManager] {relic}");
        }
    }

    // =========================================================
    // 黑暗降臨
    // =========================================================

    /// <summary>
    /// 黑暗降臨時所有遺物不滿意計數強制 +1。
    /// 由 EnvironmentManager.OnDarknessTriggered 事件觸發。
    /// </summary>
    private void ForceIncrementAllUnsatisfied()
    {
        Debug.Log("[RelicManager] 黑暗降臨 — 所有遺物不滿意計數 +1");
        foreach (var relic in _relics)
        {
            relic.ForceIncrementUnsatisfied();
            OnRelicCountChanged?.Invoke(relic);

            // 檢查強制 +1 後是否達到懲罰上限
            if (relic.IsInPunishment)
            {
                var envData = new EnvironmentData
                {
                    brightness   = EnvironmentManager.Instance.Brightness,
                    moisture     = EnvironmentManager.Instance.Moisture,
                    temperature  = EnvironmentManager.Instance.Temperature
                };
                // 建立一個懲罰結果並觸發
                var punishResult = new RelicTickResult
                {
                    IsSatisfied         = false,
                    PunishmentTriggered = true,
                    PunishmentType      = relic.Data.punishmentType,
                    ShockAttribute      = relic.Data.shockAttribute,
                    ShockValue          = relic.Data.shockValue,
                    ShockIsForceSet     = relic.Data.shockIsForceSet
                };
                ApplyPunishment(relic, punishResult);
                OnPunishmentTriggered?.Invoke(relic, punishResult);
            }
        }
    }

    // =========================================================
    // 懲罰套用
    // =========================================================

    private void ApplyPunishment(RelicInstance relic, RelicTickResult result)
    {
        var env = EnvironmentManager.Instance;

        switch (result.PunishmentType)
        {
            case RelicPunishment.EnvironmentShock:
                if (result.ShockIsForceSet)
                    env.ForceSetValue(result.ShockAttribute, result.ShockValue);
                else
                    env.ApplyEffects(new System.Collections.Generic.List<RuneEffect>
                    {
                        new RuneEffect { attribute = result.ShockAttribute, value = result.ShockValue }
                    });
                Debug.Log($"[RelicManager] 環境衝擊：{result.ShockAttribute} " +
                          $"{(result.ShockIsForceSet ? $"→ {result.ShockValue}" : $"{result.ShockValue:+0;-0}")}");
                break;

            case RelicPunishment.ScoreReset:
                int current = env.Score;
                env.ModifyScore(-current); // 清零
                Debug.Log("[RelicManager] 業績清零懲罰觸發");
                break;

            case RelicPunishment.PlayerDeath:
                Debug.Log("[RelicManager] 即死懲罰觸發 → 通知 GameManager");
                // GameManager 透過訂閱 OnPunishmentTriggered 處理
                break;
        }
    }

    // =========================================================
    // 查詢
    // =========================================================

    public int RelicCount => _relics.Count;

    // =========================================================
    // Debug
    // =========================================================

    [ContextMenu("Debug_PrintAllRelics")]
    private void Debug_PrintAllRelics()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        if (_relics.Count == 0) { Debug.Log("[RelicManager] 目前無遺物"); return; }
        foreach (var r in _relics)
            Debug.Log(r.ToString());
    }

    [ContextMenu("Debug_TickAll")]
    private void Debug_TickAll()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        Debug.Log("[RelicManager] 手動觸發 TickAll");
        TickAll();
    }

    [ContextMenu("Debug_ForceIncrementAll")]
    private void Debug_ForceIncrementAll()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        Debug.Log("[RelicManager] 手動觸發 ForceIncrementAllUnsatisfied（模擬黑暗降臨）");
        ForceIncrementAllUnsatisfied();
    }
}

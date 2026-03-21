using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 遊戲管理器（MonoBehaviour Singleton）。
/// 職責：
///   1. 初始化所有子 Manager（InspectorSO 資料 → InitXxx()）
///   2. 啟動回合流程
///   3. 監聽結束條件 → EndGame / TriggerLevelClear
///
/// Game Over 來源：
///   a. 業績歸零      — Phase 8 呼叫 CheckScoreGameOver()
///   b. 遺物即死懲罰  — RelicManager.OnPunishmentTriggered 事件
///
/// Level Clear 來源：
///   關卡模式隊列清空 — CustomerManager.OnQueueEmpty 事件
/// </summary>
public class GameManager : MonoBehaviour
{
    // =========================================================
    // Singleton
    // =========================================================

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // =========================================================
    // Inspector — 初始資料
    // =========================================================

    [Header("遊戲模式")]
    [SerializeField] private GameMode _gameMode = GameMode.Level;

    [Header("初始牌庫（RuneData）")]
    [SerializeField] private List<RuneData> _initialDeck = new List<RuneData>();

    [Header("初始卷軸庫存（ScrollData）")]
    [SerializeField] private List<ScrollData> _initialScrolls = new List<ScrollData>();

    [Header("初始遺物（RelicData）")]
    [SerializeField] private List<RelicData> _initialRelics = new List<RelicData>();

    [Header("關卡模式：顧客隊列（依序出場）")]
    [SerializeField] private List<CustomerData> _levelQueue = new List<CustomerData>();

    [Header("無限模式：顧客池（隨機抽取）")]
    [SerializeField] private List<CustomerData> _endlessPool = new List<CustomerData>();

    // =========================================================
    // 場景中的 Manager 引用（拖入 Inspector）
    // =========================================================

    [Header("場景 Manager 引用")]
    [SerializeField] private TurnManager _turnManager;

    // =========================================================
    // 事件
    // =========================================================

    /// <summary>Game Over 時觸發，reason：破產 / 遺物即死</summary>
    public event System.Action<string> OnGameOver;

    /// <summary>關卡模式通關時觸發</summary>
    public event System.Action OnLevelClear;

    // =========================================================
    // 狀態
    // =========================================================

    public GameMode CurrentMode   => _gameMode;

    /// <summary>遊戲是否已結束（Game Over 或 Level Clear）</summary>
    public bool IsGameEnded { get; private set; }

    public bool IsGameOver    { get; private set; }
    public bool IsLevelCleared { get; private set; }

    // =========================================================
    // 生命週期
    // =========================================================

    private void Start()
    {
        StartGame(_gameMode);
    }

    private void OnDestroy()
    {
        UnsubscribeEvents();
    }

    // =========================================================
    // 初始化與啟動
    // =========================================================

    /// <summary>初始化所有 Manager 並啟動第一回合</summary>
    public void StartGame(GameMode mode)
    {
        _gameMode     = mode;
        IsGameEnded   = false;
        IsGameOver    = false;
        IsLevelCleared = false;

        // 重置環境
        EnvironmentManager.Instance?.ResetToInitial();

        // 初始化各子系統
        DeckManager.Instance?.InitDeck(_initialDeck);
        ScrollInventory.Instance?.Init(_initialScrolls);
        RelicManager.Instance?.InitRelics(_initialRelics);

        if (mode == GameMode.Level)
            CustomerManager.Instance?.InitLevelMode(_levelQueue);
        else
            CustomerManager.Instance?.InitEndlessMode(_endlessPool);

        WorkbenchManager.Instance?.Init(_turnManager);

        // 訂閱結束條件事件
        SubscribeEvents();

        // 啟動回合
        _turnManager?.StartGame();

        Debug.Log($"[GameManager] 遊戲開始（{mode}）");
    }

    // =========================================================
    // Phase 8 呼叫：業績 Game Over 檢查
    // =========================================================

    /// <summary>
    /// Phase 8 Execute 呼叫。
    /// 業績歸零 → EndGame("破產") → 回傳 true。
    /// 否則回傳 false，回合正常繼續。
    /// </summary>
    public bool CheckScoreGameOver()
    {
        if (EnvironmentManager.Instance != null && EnvironmentManager.Instance.IsScoreZero)
        {
            EndGame("破產");
            return true;
        }
        return false;
    }

    // =========================================================
    // 結束
    // =========================================================

    /// <summary>Game Over（業績歸零 / 遺物即死）</summary>
    public void EndGame(string reason)
    {
        if (IsGameEnded) return;

        IsGameEnded = true;
        IsGameOver  = true;

        Debug.Log($"[GameManager] Game Over：{reason}");
        OnGameOver?.Invoke(reason);

        // 不在此處呼叫 StopAllTurns，讓 Phase 8 Exit 自然結束後停止
    }

    /// <summary>關卡模式通關（顧客隊列清空）</summary>
    private void TriggerLevelClear()
    {
        if (IsGameEnded) return;

        IsGameEnded    = true;
        IsLevelCleared = true;

        Debug.Log("[GameManager] Level Clear！");
        OnLevelClear?.Invoke();
    }

    // =========================================================
    // 事件訂閱
    // =========================================================

    private void SubscribeEvents()
    {
        if (RelicManager.Instance != null)
            RelicManager.Instance.OnPunishmentTriggered += HandleRelicPunishment;

        if (CustomerManager.Instance != null)
            CustomerManager.Instance.OnQueueEmpty += TriggerLevelClear;
    }

    private void UnsubscribeEvents()
    {
        if (RelicManager.Instance != null)
            RelicManager.Instance.OnPunishmentTriggered -= HandleRelicPunishment;

        if (CustomerManager.Instance != null)
            CustomerManager.Instance.OnQueueEmpty -= TriggerLevelClear;
    }

    private void HandleRelicPunishment(RelicInstance relic, RelicTickResult result)
    {
        if (result.PunishmentType == RelicPunishment.PlayerDeath)
            EndGame($"遺物即死（{relic.Data.relicName}）");
    }

    // =========================================================
    // Debug
    // =========================================================

    [ContextMenu("Debug_ForceGameOver")]
    private void Debug_ForceGameOver()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        EndGame("強制 Game Over（Debug）");
    }

    [ContextMenu("Debug_ForceLevelClear")]
    private void Debug_ForceLevelClear()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        TriggerLevelClear();
    }

    [ContextMenu("Debug_PrintGameState")]
    private void Debug_PrintGameState()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        Debug.Log($"[GameManager] Mode={_gameMode} | Ended={IsGameEnded} | GameOver={IsGameOver} | LevelClear={IsLevelCleared}");
        Debug.Log($"[GameManager] Score={EnvironmentManager.Instance?.Score}");
    }
}

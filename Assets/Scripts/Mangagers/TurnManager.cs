using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public enum TurnPhase
    {
        CustomerArrival,    // 1. 若無等待顧客 → 新顧客到來
        RelicTrigger,       // 2. 遺物效果結算
        DrawCards,          // 3. 棄掉上回手牌，重新抽取
        PlayerAction,       // 4. 玩家行動（選一個卷軸送出）
        EnvironmentResolve, // 5. 環境數值結算 + 極端事件檢查
        CustomerJudge,      // 6. 顧客需求判定
        PatienceCheck,      // 7. 顧客耐心判定
        RelicUpdate         // 8. 遺物不滿意計數更新
    }

    // --- Singleton ---
    public static TurnManager Instance { get; private set; }

    // --- 狀態機 ---
    private Dictionary<TurnPhase, IGameState> _states;
    private IGameState _currentState;
    private TurnPhase _currentPhase;
    private TurnPhase _nextPhase;
    private bool _phaseChangeRequested;

    // --- 回合數 ---
    private int _currentTurn;
    public int CurrentTurn => _currentTurn;
    public TurnPhase CurrentPhase => _currentPhase;

    // --- 對外存取特定 State（PlayerAction 需要接收送出信號） ---
    public PlayerActionState PlayerAction => (PlayerActionState)_states[TurnPhase.PlayerAction];

    // --- 事件 ---
    public event Action<TurnPhase> OnPhaseChanged;
    public event Action<int> OnTurnStarted;
    public event Action<int> OnTurnEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        InitializeStates();
    }

    private void InitializeStates()
    {
        _states = new Dictionary<TurnPhase, IGameState>
        {
            { TurnPhase.CustomerArrival,    new CustomerArrivalState(this) },
            { TurnPhase.RelicTrigger,       new RelicTriggerState(this) },
            { TurnPhase.DrawCards,          new DrawCardsState(this) },
            { TurnPhase.PlayerAction,       new PlayerActionState(this) },
            { TurnPhase.EnvironmentResolve, new EnvironmentResolveState(this) },
            { TurnPhase.CustomerJudge,      new CustomerJudgeState(this) },
            { TurnPhase.PatienceCheck,      new PatienceCheckState(this) },
            { TurnPhase.RelicUpdate,        new RelicUpdateState(this) },
        };
    }

    /// <summary>
    /// 開始遊戲的第一個回合
    /// </summary>
    public void StartGame()
    {
        _currentTurn = 0;
        StartNextTurn();
    }

    /// <summary>
    /// 開始下一個回合
    /// </summary>
    public void StartNextTurn()
    {
        _currentTurn++;
        Debug.Log($"===== Turn {_currentTurn} Start =====");
        OnTurnStarted?.Invoke(_currentTurn);

        // 從 Phase 1 開始
        _currentPhase = TurnPhase.CustomerArrival;
        _currentState = _states[_currentPhase];
        StartCoroutine(RunCurrentState());
    }

    /// <summary>
    /// 執行當前狀態的完整生命週期：Enter → Execute → Exit
    /// Exit 中會呼叫 SetState 設定下一個階段
    /// </summary>
    private IEnumerator RunCurrentState()
    {
        OnPhaseChanged?.Invoke(_currentPhase);

        // Enter
        yield return StartCoroutine(_currentState.Enter());

        // Execute
        yield return StartCoroutine(_currentState.Execute());

        // Exit（State 的 Exit 內部會呼叫 SetState 或 OnTurnEnd）
        _phaseChangeRequested = false;
        yield return StartCoroutine(_currentState.Exit());

        // 如果 Exit 中請求了下一個階段，繼續執行
        if (_phaseChangeRequested)
        {
            _currentState = _states[_nextPhase];
            _currentPhase = _nextPhase;
            StartCoroutine(RunCurrentState());
        }
        // 否則回合結束（由 RelicUpdateState.Exit 呼叫 OnTurnEnd 處理）
    }

    /// <summary>
    /// 由各 State 的 Exit() 呼叫，設定下一個要進入的階段
    /// </summary>
    public void SetState(TurnPhase nextPhase)
    {
        _nextPhase = nextPhase;
        _phaseChangeRequested = true;
    }

    /// <summary>
    /// 由 RelicUpdateState（最後一個階段）呼叫，結束當前回合並開始下一回合
    /// </summary>
    public void OnTurnEnd()
    {
        Debug.Log($"===== Turn {_currentTurn} End =====");
        OnTurnEnded?.Invoke(_currentTurn);

        // 自動開始下一回合
        StartNextTurn();
    }

    /// <summary>
    /// 外部呼叫：玩家按下送出按鈕
    /// </summary>
    public void PlayerSubmit()
    {
        if (_currentPhase == TurnPhase.PlayerAction)
        {
            PlayerAction.OnPlayerSubmit();
        }
        else
        {
            Debug.LogWarning($"PlayerSubmit called during wrong phase: {_currentPhase}");
        }
    }

    /// <summary>
    /// 強制停止回合流程（Game Over 時使用）
    /// </summary>
    public void StopAllTurns()
    {
        StopAllCoroutines();
        Debug.Log("Turn flow stopped.");
    }

    // -------------------------------------------------------------------------
    // Editor 測試用 ContextMenu
    // -------------------------------------------------------------------------

    [ContextMenu("Debug_StartGame")]
    private void Debug_StartGame()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("請在 Play Mode 下使用");
            return;
        }
        InitializeStates();
        StartGame();
    }

    [ContextMenu("Debug_GoToNextState")]
    private void Debug_GoToNextState()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("請在 Play Mode 下使用");
            return;
        }

        // PlayerAction 階段正在等待玩家送出，需先解除阻塞
        if (_currentPhase == TurnPhase.PlayerAction)
        {
            Debug.Log("[ContextMenu] 模擬玩家送出 → 解除 PlayerAction 阻塞");
            PlayerAction.OnPlayerSubmit();
            return;
        }

        // 其他階段：直接跳到下一個 Phase
        TurnPhase next = GetNextPhase(_currentPhase);
        Debug.Log($"[ContextMenu] 強制跳過 {_currentPhase} → {next}");
        StopAllCoroutines();
        _currentPhase = next;
        _currentState = _states[_currentPhase];
        StartCoroutine(RunCurrentState());
    }

    [ContextMenu("Debug_SkipToPlayerAction")]
    private void Debug_SkipToPlayerAction()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("請在 Play Mode 下使用");
            return;
        }
        Debug.Log("[ContextMenu] 直接跳到 PlayerAction");
        StopAllCoroutines();
        _currentPhase = TurnPhase.PlayerAction;
        _currentState = _states[_currentPhase];
        StartCoroutine(RunCurrentState());
    }

    [ContextMenu("Debug_PrintCurrentPhase")]
    private void Debug_PrintCurrentPhase()
    {
        Debug.Log($"[ContextMenu] Turn {_currentTurn}，目前階段：{_currentPhase}");
    }

    /// <summary>依照回合流程順序取得下一個 Phase（供 Debug 跳轉使用）</summary>
    private TurnPhase GetNextPhase(TurnPhase current)
    {
        switch (current)
        {
            case TurnPhase.CustomerArrival:    return TurnPhase.RelicTrigger;
            case TurnPhase.RelicTrigger:       return TurnPhase.DrawCards;
            case TurnPhase.DrawCards:          return TurnPhase.PlayerAction;
            case TurnPhase.PlayerAction:       return TurnPhase.EnvironmentResolve;
            case TurnPhase.EnvironmentResolve: return TurnPhase.CustomerJudge;
            case TurnPhase.CustomerJudge:      return TurnPhase.PatienceCheck;
            case TurnPhase.PatienceCheck:      return TurnPhase.RelicUpdate;
            case TurnPhase.RelicUpdate:        return TurnPhase.CustomerArrival;
            default:                           return TurnPhase.CustomerArrival;
        }
    }
}

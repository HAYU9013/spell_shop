using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 4: 玩家行動階段
/// 等待玩家選擇卷軸、放滿符文、按下送出
/// 一個回合只能送出一個卷軸
/// 此階段會暫停回合流程，直到玩家完成操作
/// </summary>
public class PlayerActionState : IGameState
{
    private TurnManager _turnManager;
    private bool _playerSubmitted;

    public PlayerActionState(TurnManager turnManager)
    {
        _turnManager = turnManager;
    }

    public IEnumerator Enter()
    {
        Debug.Log("[Phase 4] PlayerAction — Enter");
        _playerSubmitted = false;

        // TODO: 啟用 UI 互動（卷軸選擇、手牌點選、送出按鈕）
        // UIManager.EnablePlayerInput(true);

        yield break;
    }

    public IEnumerator Execute()
    {
        Debug.Log("[Phase 4] PlayerAction — Waiting for player...");

        // 等待玩家按下送出按鈕
        while (!_playerSubmitted)
        {
            yield return null;
        }

        Debug.Log("[Phase 4] PlayerAction — Player submitted!");
    }

    public IEnumerator Exit()
    {
        Debug.Log("[Phase 4] PlayerAction — Exit");

        // TODO: 禁用 UI 互動
        // UIManager.EnablePlayerInput(false);

        _turnManager.SetState(TurnManager.TurnPhase.EnvironmentResolve);
        yield break;
    }

    /// <summary>
    /// 由送出按鈕呼叫，結束玩家行動階段
    /// </summary>
    public void OnPlayerSubmit()
    {
        _playerSubmitted = true;
    }
}

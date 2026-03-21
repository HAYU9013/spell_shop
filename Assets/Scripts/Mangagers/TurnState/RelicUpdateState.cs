using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 8: 遺物不滿意計數更新
/// 檢查 Game Over 條件（業績歸零、遺物即死）
/// 回合結束，通知 TurnManager 開始下一回合
/// </summary>
public class RelicUpdateState : IGameState
{
    private TurnManager _turnManager;

    public RelicUpdateState(TurnManager turnManager)
    {
        _turnManager = turnManager;
    }

    public IEnumerator Enter()
    {
        Debug.Log("[Phase 8] RelicUpdate — Enter");
        yield break;
    }

    public IEnumerator Execute()
    {
        Debug.Log("[Phase 8] RelicUpdate — Execute");

        // 業績歸零 → Game Over
        // （遺物即死已在 Phase 2 由 RelicManager.OnPunishmentTriggered 事件觸發）
        GameManager.Instance?.CheckScoreGameOver();

        yield break;
    }

    public IEnumerator Exit()
    {
        Debug.Log("[Phase 8] RelicUpdate — Exit");

        // 遊戲已結束（Game Over 或 Level Clear）→ 停止回合，不再開始下一回合
        if (GameManager.Instance != null && GameManager.Instance.IsGameEnded)
        {
            Debug.Log("[Phase 8] 遊戲已結束，停止回合流程");
            yield break;
        }

        Debug.Log("[Phase 8] RelicUpdate — Turn End");
        _turnManager.OnTurnEnd();
        yield break;
    }
}

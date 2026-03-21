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

        // TODO:
        // 檢查 Game Over
        // if (EnvironmentManager.IsScoreZero)
        //     GameManager.EndGame("破產");
        //
        // 遺物即死檢查已在 Phase 2 處理，此處做最終確認

        yield break;
    }

    public IEnumerator Exit()
    {
        Debug.Log("[Phase 8] RelicUpdate — Exit → Turn End");

        // 回合結束，通知 TurnManager 開始下一回合
        _turnManager.OnTurnEnd();
        yield break;
    }
}

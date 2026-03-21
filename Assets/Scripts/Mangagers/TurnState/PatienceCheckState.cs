using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 7: 顧客耐心判定
/// 若顧客在 Phase 6 已滿足離開，此階段跳過
/// 未滿足 → 耐心 -1
/// 耐心 = 0 → 顧客憤怒離開，業績 -N
/// 耐心 > 0 → 顧客繼續等待
/// </summary>
public class PatienceCheckState : IGameState
{
    private TurnManager _turnManager;

    public PatienceCheckState(TurnManager turnManager)
    {
        _turnManager = turnManager;
    }

    public IEnumerator Enter()
    {
        Debug.Log("[Phase 7] PatienceCheck — Enter");
        yield break;
    }

    public IEnumerator Execute()
    {
        Debug.Log("[Phase 7] PatienceCheck — Execute");

        // TODO:
        // if (顧客已在 Phase 6 滿足離開) → 跳過
        //
        // CustomerManager.TickPatience();
        // if (customer.IsOutOfPatience)
        // {
        //     EnvironmentManager.ModifyScore(-customer.Data.scorePenalty);
        //     // 顧客憤怒離開
        //     // 迎接下一位顧客（下回合 Phase 1 處理）
        // }

        yield break;
    }

    public IEnumerator Exit()
    {
        Debug.Log("[Phase 7] PatienceCheck — Exit");
        _turnManager.SetState(TurnManager.TurnPhase.RelicUpdate);
        yield break;
    }
}

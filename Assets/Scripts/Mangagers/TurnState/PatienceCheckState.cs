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

        // Phase 6 已滿足（顧客已離開）→ 跳過
        if (CustomerManager.Instance != null && !CustomerManager.Instance.HasCustomer)
        {
            Debug.Log("[Phase 7] 顧客已滿足離開，跳過耐心判定");
            yield break;
        }

        CustomerManager.Instance?.TickPatience();
    }

    public IEnumerator Exit()
    {
        Debug.Log("[Phase 7] PatienceCheck — Exit");
        _turnManager.SetState(TurnManager.TurnPhase.RelicUpdate);
        yield break;
    }
}

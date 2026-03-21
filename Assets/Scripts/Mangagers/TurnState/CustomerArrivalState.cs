using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 1: 若無等待顧客 → 新顧客到來
/// 若隊列為空（關卡模式）→ 觸發通關
/// </summary>
public class CustomerArrivalState : IGameState
{
    private TurnManager _turnManager;

    public CustomerArrivalState(TurnManager turnManager)
    {
        _turnManager = turnManager;
    }

    public IEnumerator Enter()
    {
        Debug.Log("[Phase 1] CustomerArrival — Enter");
        yield break;
    }

    public IEnumerator Execute()
    {
        Debug.Log($"[Phase 1] CustomerArrival — Execute，HasCustomer={CustomerManager.Instance?.HasCustomer}，CustomerManager={(CustomerManager.Instance != null ? "OK" : "null")}");

        if (CustomerManager.Instance != null && !CustomerManager.Instance.HasCustomer)
            CustomerManager.Instance.SpawnNextCustomer();
        else if (CustomerManager.Instance != null && CustomerManager.Instance.HasCustomer)
            Debug.Log("[Phase 1] 已有顧客，跳過 SpawnNextCustomer");

        yield break;
    }

    public IEnumerator Exit()
    {
        Debug.Log("[Phase 1] CustomerArrival — Exit");
        _turnManager.SetState(TurnManager.TurnPhase.RelicTrigger);
        yield break;
    }
}

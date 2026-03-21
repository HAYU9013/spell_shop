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
        Debug.Log("[Phase 1] CustomerArrival — Execute");

        // TODO: 檢查是否有當前顧客
        // if (!CustomerManager.HasCustomer)
        // {
        //     var customer = CustomerManager.WelcomeNextCustomer(env);
        //     if (customer == null)
        //     {
        //         // 隊列為空 → 關卡通關
        //         LevelManager.OnLevelCleared();
        //         yield break;
        //     }
        //     // 記錄 ArrivalSnapshot（相對變化條件基準）
        // }

        yield break;
    }

    public IEnumerator Exit()
    {
        Debug.Log("[Phase 1] CustomerArrival — Exit");
        _turnManager.SetState(TurnManager.TurnPhase.RelicTrigger);
        yield break;
    }
}

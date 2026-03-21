using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 6: 顧客需求判定
/// 檢查當前顧客的所有需求是否滿足
/// 滿足 → 發放獎勵、業績 +N、顧客離開、立刻迎接下一位
/// 未滿足 → 進入耐心判定階段
/// </summary>
public class CustomerJudgeState : IGameState
{
    private TurnManager _turnManager;

    public CustomerJudgeState(TurnManager turnManager)
    {
        _turnManager = turnManager;
    }

    public IEnumerator Enter()
    {
        Debug.Log("[Phase 6] CustomerJudge — Enter");
        yield break;
    }

    public IEnumerator Execute()
    {
        Debug.Log("[Phase 6] CustomerJudge — Execute");

        // TODO:
        // bool satisfied = CustomerManager.CheckCurrentCustomer(env, submittedScroll);
        // if (satisfied)
        // {
        //     var rewards = CustomerManager.GenerateRewards();
        //     EnvironmentManager.ModifyScore(customer.Data.scoreReward);
        //     // 發放獎勵（符文加入牌庫、卷軸加入庫存、遺物詢問玩家）
        //     // 顧客離開 → 標記需要迎接下一位
        // }

        yield break;
    }

    public IEnumerator Exit()
    {
        Debug.Log("[Phase 6] CustomerJudge — Exit");
        _turnManager.SetState(TurnManager.TurnPhase.PatienceCheck);
        yield break;
    }
}

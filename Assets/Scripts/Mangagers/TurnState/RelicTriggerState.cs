using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 2: 遺物效果結算（依玩家排列順序）
/// 滿意 → 套用滿意效果，計數歸零
/// 不滿意 → 套用副作用，計數 +1
/// 計數 >= 上限 → 觸發懲罰（計數不重置）
/// </summary>
public class RelicTriggerState : IGameState
{
    private TurnManager _turnManager;

    public RelicTriggerState(TurnManager turnManager)
    {
        _turnManager = turnManager;
    }

    public IEnumerator Enter()
    {
        Debug.Log("[Phase 2] RelicTrigger — Enter");
        yield break;
    }

    public IEnumerator Execute()
    {
        Debug.Log("[Phase 2] RelicTrigger — Execute");

        if (RelicManager.Instance != null)
            RelicManager.Instance.TickAll();

        yield break;
    }

    public IEnumerator Exit()
    {
        Debug.Log("[Phase 2] RelicTrigger — Exit");
        _turnManager.SetState(TurnManager.TurnPhase.DrawCards);
        yield break;
    }
}

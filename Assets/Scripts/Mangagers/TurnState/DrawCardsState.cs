using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 3: 棄掉上回所有手牌，從牌庫重新抽取
/// 每回合全棄重抽，無手牌上限
/// 消耗型符文已使用的不會回到牌庫
/// </summary>
public class DrawCardsState : IGameState
{
    private TurnManager _turnManager;

    public DrawCardsState(TurnManager turnManager)
    {
        _turnManager = turnManager;
    }

    public IEnumerator Enter()
    {
        Debug.Log("[Phase 3] DrawCards — Enter");
        yield break;
    }

    public IEnumerator Execute()
    {
        Debug.Log("[Phase 3] DrawCards — Execute");

        var deck = DeckManager.Instance;
        if (deck == null) yield break;

        // 補牌時機為送出卷軸後，Phase 3 只在手牌為空時補牌（第一回合初始發牌）
        if (deck.HandCount == 0)
            deck.DrawHand(deck.DrawPerTurn);

        yield break;
    }

    public IEnumerator Exit()
    {
        Debug.Log("[Phase 3] DrawCards — Exit");
        _turnManager.SetState(TurnManager.TurnPhase.PlayerAction);
        yield break;
    }
}

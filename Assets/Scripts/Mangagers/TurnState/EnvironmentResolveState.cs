using System.Collections;
using UnityEngine;

/// <summary>
/// Phase 5: 環境數值結算
/// 1. 快照當前環境
/// 2. 計算卷軸最終效果（修飾規則 + 標籤連鎖）
/// 3. 套用效果到環境
/// 4. 處理消耗型符文（永久移除）
/// 5. 處理特殊效果（如驅逐石移除遺物）
/// 6. 檢查遺物標籤偏好觸發
/// 7. 檢查極端事件（邊界值一次性觸發）
/// 8. 消耗卷軸
/// </summary>
public class EnvironmentResolveState : IGameState
{
    private TurnManager _turnManager;

    public EnvironmentResolveState(TurnManager turnManager)
    {
        _turnManager = turnManager;
    }

    public IEnumerator Enter()
    {
        Debug.Log("[Phase 5] EnvironmentResolve — Enter");
        yield break;
    }

    public IEnumerator Execute()
    {
        Debug.Log("[Phase 5] EnvironmentResolve — Execute");

        // TODO:
        // 1. EnvironmentManager.SnapshotCurrent()
        // 2. var effects = ScrollProcessor.Process(submittedScroll)
        // 3. EnvironmentManager.ApplyEffects(effects)
        // 4. 消耗型符文 → DeckManager.Exhaust()
        // 5. 特殊效果 → ISpecialEffect.Execute()
        // 6. RelicManager.CheckAllTagPreferences(scroll)
        // 7. EnvironmentManager.CheckAndTriggerExtremeEvents()
        //    - Darkness → RelicManager.ForceIncrementAllUnsatisfied()
        //    - Flood → moisture += 5
        //    - Overheat → score -= 10
        //    - Overcold → score -= 10
        // 8. ScrollInventory.ConsumeScroll(submittedScroll)
        // 9. CustomerManager.UpdateCustomerTracking(env)

        yield break;
    }

    public IEnumerator Exit()
    {
        Debug.Log("[Phase 5] EnvironmentResolve — Exit");
        _turnManager.SetState(TurnManager.TurnPhase.CustomerJudge);
        yield break;
    }
}

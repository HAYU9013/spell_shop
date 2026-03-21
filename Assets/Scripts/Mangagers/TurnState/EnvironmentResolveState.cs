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

        // 取得本回合送出的卷軸紀錄，計算並套用環境效果
        var submit = WorkbenchManager.Instance?.LastSubmit;
        if (submit != null)
        {
            var effects = ScrollProcessor.Process(submit);
            if (effects.Count > 0)
                EnvironmentManager.Instance?.ApplyEffects(effects);
        }
        else
        {
            Debug.LogWarning("[Phase 5] 無送出紀錄，跳過環境結算");
        }

        // 更新顧客追蹤（累積 delta + 穩定性計數）
        CustomerManager.Instance?.UpdateCurrentTracking();

        yield break;
    }

    public IEnumerator Exit()
    {
        Debug.Log("[Phase 5] EnvironmentResolve — Exit");

        // 將本回合結算後的環境狀態推入歷史 deque
        EnvironmentManager.Instance?.RecordSnapshot();

        _turnManager.SetState(TurnManager.TurnPhase.CustomerJudge);
        yield break;
    }
}

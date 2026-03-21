using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 牌庫管理器（MonoBehaviour Singleton）。
/// 統一管理抽牌堆、棄牌堆、手牌三個區域。
///
/// 生命週期：
///   InitDeck()      → 建立所有 RuneInstance，全部放入抽牌堆並洗牌
///   DrawHand()      → 從抽牌堆抽 drawPerTurn 張到手牌
///                     抽牌堆不足時先洗牌（棄牌堆 → 抽牌堆）
///   DiscardHand()   → Phase 3 開始時棄掉上回所有手牌
///   RemoveFromHand()→ 玩家點選手牌放入工作台時呼叫
///   AddToHand()     → 從工作台取消放置時歸還
///   DiscardRune()   → 循環型符文送出後進棄牌堆
///   ExhaustRune()   → 消耗型符文送出後永久移除
///   AddRune()       → 顧客獎勵新增符文（進棄牌堆）
/// </summary>
public class DeckManager : MonoBehaviour
{
    // =========================================================
    // Singleton
    // =========================================================

    public static DeckManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // =========================================================
    // 事件
    // =========================================================

    /// <summary>手牌內容改變時觸發（供 UI 重新渲染手牌區）</summary>
    public event System.Action OnHandChanged;

    /// <summary>抽牌堆或棄牌堆數量改變時觸發（供 UI 更新計數顯示）</summary>
    public event System.Action OnDeckChanged;

    // =========================================================
    // 設定
    // =========================================================

    [Tooltip("每回合抽牌數量")]
    [SerializeField] private int _drawPerTurn = 3;
    public int DrawPerTurn => _drawPerTurn;

    // =========================================================
    // 私有欄位
    // =========================================================

    private readonly List<RuneInstance> _drawPile    = new List<RuneInstance>();
    private readonly List<RuneInstance> _discardPile = new List<RuneInstance>();
    private readonly List<RuneInstance> _hand        = new List<RuneInstance>();

    // =========================================================
    // 只讀屬性
    // =========================================================

    public IReadOnlyList<RuneInstance> Hand        => _hand;
    public int DrawPileCount    => _drawPile.Count;
    public int DiscardPileCount => _discardPile.Count;
    public int HandCount        => _hand.Count;

    // =========================================================
    // 初始化
    // =========================================================

    /// <summary>
    /// 從 RuneData 列表建立所有 RuneInstance，放入抽牌堆並洗牌。
    /// 由 GameManager 在遊戲開始時呼叫。
    /// </summary>
    public void InitDeck(List<RuneData> runeDataList)
    {
        _drawPile.Clear();
        _discardPile.Clear();
        _hand.Clear();

        foreach (var data in runeDataList)
            _drawPile.Add(new RuneInstance(data));

        Shuffle(_drawPile);
        OnDeckChanged?.Invoke();
        Debug.Log($"[DeckManager] 初始化牌庫：{_drawPile.Count} 張");
    }

    // =========================================================
    // Phase 3：棄手牌 → 抽新手牌
    // =========================================================

    /// <summary>
    /// 棄掉目前所有手牌（送回棄牌堆），再抽 drawPerTurn 張。
    /// 由 DrawCardsState.Execute() 呼叫。
    /// </summary>
    public void DiscardHandAndDraw()
    {
        DiscardHand();
        DrawHand(_drawPerTurn);
    }

    /// <summary>將手牌全部移至棄牌堆</summary>
    public void DiscardHand()
    {
        if (_hand.Count == 0) return;
        _discardPile.AddRange(_hand);
        _hand.Clear();
        OnHandChanged?.Invoke();
        OnDeckChanged?.Invoke();
        Debug.Log($"[DeckManager] 棄掉 {_discardPile.Count} 張手牌");
    }

    /// <summary>從抽牌堆抽 count 張加入手牌；不足時自動洗牌</summary>
    public void DrawHand(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (_drawPile.Count == 0)
            {
                if (_discardPile.Count == 0)
                {
                    Debug.Log("[DeckManager] 牌庫與棄牌堆均為空，無法抽牌");
                    break;
                }
                ReshuffleDiscard();
            }

            var rune = _drawPile[_drawPile.Count - 1];
            _drawPile.RemoveAt(_drawPile.Count - 1);
            _hand.Add(rune);
        }

        OnHandChanged?.Invoke();
        OnDeckChanged?.Invoke();
        Debug.Log($"[DeckManager] 抽牌後手牌 {_hand.Count} 張，抽牌堆剩 {_drawPile.Count} 張");
    }

    // =========================================================
    // 工作台互動
    // =========================================================

    /// <summary>
    /// 將符文從手牌移出（放入工作台時呼叫）。
    /// 回傳 false 表示符文不在手牌中。
    /// </summary>
    public bool RemoveFromHand(RuneInstance rune)
    {
        bool removed = _hand.Remove(rune);
        if (removed) OnHandChanged?.Invoke();
        return removed;
    }

    /// <summary>將符文歸還手牌（從工作台取消放置時呼叫）</summary>
    public void AddToHand(RuneInstance rune)
    {
        if (!_hand.Contains(rune))
        {
            _hand.Add(rune);
            OnHandChanged?.Invoke();
        }
    }

    // =========================================================
    // 送出後處理
    // =========================================================

    /// <summary>循環型符文送出後進棄牌堆（下次洗牌可再次抽到）</summary>
    public void DiscardRune(RuneInstance rune)
    {
        if (!_discardPile.Contains(rune))
        {
            _discardPile.Add(rune);
            OnDeckChanged?.Invoke();
        }
    }

    /// <summary>消耗型符文送出後永久移除（不再回到任何牌堆）</summary>
    public void ExhaustRune(RuneInstance rune)
    {
        rune.Exhaust();
        _drawPile.Remove(rune);
        _discardPile.Remove(rune);
        _hand.Remove(rune);
        OnDeckChanged?.Invoke();
        OnHandChanged?.Invoke();
        Debug.Log($"[DeckManager] 消耗符文：{rune.Data.runeName}");
    }

    // =========================================================
    // 獎勵新增
    // =========================================================

    /// <summary>顧客獎勵時新增符文，放入棄牌堆（下次洗牌可抽到）</summary>
    public void AddRune(RuneData data)
    {
        _discardPile.Add(new RuneInstance(data));
        OnDeckChanged?.Invoke();
        Debug.Log($"[DeckManager] 新增符文至棄牌堆：{data.runeName}");
    }

    // =========================================================
    // 洗牌
    // =========================================================

    /// <summary>將棄牌堆洗入抽牌堆（Fisher-Yates）</summary>
    private void ReshuffleDiscard()
    {
        Debug.Log($"[DeckManager] 洗牌：棄牌堆 {_discardPile.Count} 張 → 抽牌堆");
        _drawPile.AddRange(_discardPile);
        _discardPile.Clear();
        Shuffle(_drawPile);
        OnDeckChanged?.Invoke();
    }

    private static void Shuffle(List<RuneInstance> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    // =========================================================
    // Debug
    // =========================================================

    [ContextMenu("Debug_PrintDeckState")]
    private void Debug_PrintDeckState()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        Debug.Log($"[DeckManager] 抽牌堆：{_drawPile.Count}  棄牌堆：{_discardPile.Count}  手牌：{_hand.Count}");
        foreach (var r in _hand)
            Debug.Log($"  手牌：{r}");
    }

    [ContextMenu("Debug_PrintDrawPile")]
    private void Debug_PrintDrawPile()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        Debug.Log($"[DeckManager] 抽牌堆（{_drawPile.Count} 張，尚未抽到）：");
        for (int i = _drawPile.Count - 1; i >= 0; i--)
            Debug.Log($"  [{_drawPile.Count - 1 - i + 1}] {_drawPile[i].Data.runeName}");
    }

    [ContextMenu("Debug_PrintDiscardPile")]
    private void Debug_PrintDiscardPile()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        Debug.Log($"[DeckManager] 棄牌堆（{_discardPile.Count} 張，已出過）：");
        for (int i = _discardPile.Count - 1; i >= 0; i--)
            Debug.Log($"  [{_discardPile.Count - 1 - i + 1}] {_discardPile[i].Data.runeName}");
    }

    [ContextMenu("Debug_DrawHand")]
    private void Debug_DrawHand()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        DrawHand(_drawPerTurn);
    }

    [ContextMenu("Debug_DiscardHandAndDraw")]
    private void Debug_DiscardHandAndDraw()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        DiscardHandAndDraw();
    }
}

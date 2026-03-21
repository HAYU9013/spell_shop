using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 手牌符文卡片的點擊互動元件。
///
/// 【掛載位置】
///   Rune prefab 根物件是 Transform（非 RectTransform），沒有 Image，
///   EventSystem 無法對它做射線偵測。
///   因此此腳本掛在 Icon 子物件上——它有 Image + RaycastTarget = true，
///   可以正常接收點擊事件。
///   動畫則作用在 _cardRoot（整張卡根 Transform），視覺上縮放整張卡。
///
/// 【選中邏輯】
///   - 點擊未選中的卡 → Select（其他卡會因 OnSelectionChanged 自動取消高亮）
///   - 再點同一張卡   → Deselect
///
/// 【生命週期注意】
///   訂閱 OnSelectionChanged 在 Setup() 而非 Awake/OnEnable，
///   因為此元件由 RuneOnHandUI 動態 AddComponent 並立即 Setup，
///   Awake 時 _handIndex 尚未設定，訂閱會讀到錯誤的初始值。
///   OnDestroy 取消訂閱，確保卡片銷毀後不殘留事件監聽。
/// </summary>
public class RuneCardUI : MonoBehaviour, IPointerClickHandler
{
    private RuneData  _rune;
    private int       _handIndex;
    private Transform _cardRoot;  // 整張卡的根，動畫作用對象

    // =========================================================
    // 注入（由 RuneOnHandUI 呼叫）
    // =========================================================

    /// <param name="handIndex">此卡在手牌陣列中的索引，用於唯一識別</param>
    /// <param name="rune">此卡對應的符文資料，用於 PlaceRune</param>
    /// <param name="cardRoot">整張卡的根 Transform，動畫縮放對象</param>
    public void Setup(int handIndex, RuneData rune, Transform cardRoot)
    {
        _handIndex = handIndex;
        _rune      = rune;
        _cardRoot  = cardRoot;

        // 訂閱全域選中事件，任何卡片選中/取消時都會刷新自己的高亮
        HandSelectionState.OnSelectionChanged += RefreshHighlight;

        // 初始化高亮狀態（通常為取消高亮）
        RefreshHighlight();
    }

    // =========================================================
    // 生命週期
    // =========================================================

    private void OnDestroy()
    {
        // 卡片銷毀（手牌重建）時取消訂閱，防止對已銷毀物件呼叫
        HandSelectionState.OnSelectionChanged -= RefreshHighlight;
    }

    // =========================================================
    // 點擊
    // =========================================================

    public void OnPointerClick(PointerEventData _)
    {
        if (_rune == null) return;

        if (HandSelectionState.IsSelected(_handIndex))
        {
            // 再次點擊同一張 → 取消選中
            HandSelectionState.Deselect();
        }
        else
        {
            // 選中此符文（HandSelectionState 廣播事件，其他卡自動取消高亮）
            HandSelectionState.Select(_handIndex, _rune);
        }
    }

    // =========================================================
    // 選中視覺
    // =========================================================

    /// <summary>
    /// 由 OnSelectionChanged 事件驅動，判斷自己是否為選中狀態並更新視覺。
    /// 不直接接收「是否選中」參數，而是自行查詢 HandSelectionState，
    /// 確保任何時間點都能得到正確狀態（冪等）。
    /// </summary>
    private void RefreshHighlight()
    {
        if (HandSelectionState.IsSelected(_handIndex))
            AnimSelect();
        else
            AnimDeselect();
    }

    // =========================================================
    // 動畫預留點（DOTween）
    // =========================================================

    private void AnimSelect()
    {
        if (_cardRoot == null) return;
        _cardRoot.DOKill();
        _cardRoot.DOScale(1.1f, 0.12f).SetEase(Ease.OutBack).SetLink(_cardRoot.gameObject);
    }

    private void AnimDeselect()
    {
        if (_cardRoot == null) return;
        _cardRoot.DOKill();
        _cardRoot.DOScale(1f, 0.1f).SetEase(Ease.OutQuad).SetLink(_cardRoot.gameObject);
    }
}

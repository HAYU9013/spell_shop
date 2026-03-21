using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 掛在 ScrollsPileUI 的 scrollPrefab 上。
/// 職責：
///   - 由 ScrollsPileUI.FillCard() 呼叫 Setup(data) 注入資料
///   - 點擊後呼叫 GameFacade.SelectScroll()
///   - 訂閱 OnWorkbenchChanged 反映選中 / 未選中的高亮狀態
/// </summary>
public class ScrollCardUI : MonoBehaviour, IPointerClickHandler
{
    [Header("高亮邊框（選中時顯示）")]
    [SerializeField] private Image selectedBorder;

    private ScrollData _data;
    private bool _subscribed;

    // =========================================================
    // 注入（由 ScrollsPileUI 呼叫）
    // =========================================================

    public void Setup(ScrollData data)
    {
        _data = data;
        TrySubscribe();

        // 初始高亮狀態同步
        if (GameFacade.Instance != null)
            RefreshHighlight(GameFacade.Instance.Workbench);
    }

    // =========================================================
    // 生命週期
    // =========================================================

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        if (_subscribed && GameFacade.Instance != null)
            GameFacade.Instance.OnWorkbenchChanged -= HandleWorkbenchChanged;
        _subscribed = false;
    }

    private void TrySubscribe()
    {
        if (!_subscribed && GameFacade.Instance != null && _data != null)
        {
            GameFacade.Instance.OnWorkbenchChanged += HandleWorkbenchChanged;
            _subscribed = true;
        }
    }

    // =========================================================
    // 點擊
    // =========================================================

    public void OnPointerClick(PointerEventData _)
    {
        if (_data == null || GameFacade.Instance == null) return;

        // 已選中同一張 → 不重複呼叫
        if (GameFacade.Instance.Workbench.SelectedScroll == _data) return;

        GameFacade.Instance.SelectScroll(_data);
        // 高亮由 HandleWorkbenchChanged 事件驅動更新，無需手動呼叫
    }

    // =========================================================
    // 事件處理
    // =========================================================

    private void HandleWorkbenchChanged(WorkbenchSnapshot wb) => RefreshHighlight(wb);

    private void RefreshHighlight(WorkbenchSnapshot wb)
    {
        bool isSelected = _data != null && wb.SelectedScroll == _data;
        if (isSelected) AnimSelect();
        else            AnimDeselect();
    }

    // =========================================================
    // 動畫預留點（DOTween）
    // =========================================================

    private void AnimSelect()
    {
        if (selectedBorder != null) selectedBorder.enabled = true;
        transform.DOKill();
        transform.localScale = Vector3.one * 1.05f;
        // TODO: DOTween transform.DOScale(1.05f, 0.15f).SetEase(Ease.OutBack);
    }

    private void AnimDeselect()
    {
        if (selectedBorder != null) selectedBorder.enabled = false;
        transform.DOKill();
        transform.localScale = Vector3.one;
        // TODO: DOTween transform.DOScale(1f, 0.1f);
    }
}

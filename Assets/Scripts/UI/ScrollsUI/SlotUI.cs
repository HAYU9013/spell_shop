using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 掛在 slotPrefab 上，代表卷軸中的單一符文槽位。
///
/// 顯示規則：
///   - 空槽：顯示 emptyView（佔位圖片）
///   - 有符文：顯示 runeView（符文圖示 + 名稱）
///
/// 點擊行為：
///   - 空槽  → 無動作（符文放入由手牌卡片點擊觸發）
///   - 有符文 → RemoveRuneAt(slotIndex)，將符文退回手牌
///
/// 由 OpenScrollUI 呼叫 Setup() 和 Refresh()。
/// </summary>
public class SlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("空槽顯示")]
    [SerializeField] private GameObject emptyView;

    [Header("符文顯示")]
    [SerializeField] private GameObject runeView;
    [SerializeField] private Image      runeIcon;
    [SerializeField] private TMP_Text   runeNameText;

    private int      _slotIndex;
    private RuneData _currentRune;   // 追蹤目前內容，供動畫判斷

    // =========================================================
    // 生命週期
    // =========================================================

    private void Awake()
    {
        // Prefab 在 Editor 的初始狀態不可靠，強制從 empty 開始
        ShowEmpty();
    }

    // =========================================================
    // 注入（由 OpenScrollUI 呼叫）
    // =========================================================

    public void Setup(int slotIndex)
    {
        _slotIndex   = slotIndex;
        _currentRune = null;
        ShowEmpty();
    }

    // =========================================================
    // 刷新顯示（由 OpenScrollUI 在 OnWorkbenchChanged 後呼叫）
    // =========================================================

    public void Refresh(RuneData runeData)
    {
        bool wasEmpty  = _currentRune == null;
        bool nowFilled = runeData != null;
        _currentRune   = runeData;

        if (nowFilled)
        {
            if (runeIcon     != null) runeIcon.sprite   = runeData.icon;
            if (runeNameText != null) runeNameText.text = runeData.runeName;
            ShowRune();

            // 剛從空到有符文 → 播放放入動畫
            if (wasEmpty) AnimFill();
        }
        else
        {
            ShowEmpty();

            // 剛從有符文到空 → 播放退出動畫
            if (!wasEmpty) AnimEmpty();
        }
    }

    // =========================================================
    // 點擊
    // =========================================================

    public void OnPointerClick(PointerEventData _)
    {
        if (GameFacade.Instance == null) return;

        var wb = GameFacade.Instance.Workbench;
        bool hasRune = wb.PlacedRunes != null
                    && _slotIndex < wb.PlacedRunes.Length
                    && wb.PlacedRunes[_slotIndex] != null;

        if (hasRune)
        {
            // 有符文 → 退回手牌（Refresh 由 OnWorkbenchChanged 事件觸發）
            GameFacade.Instance.RemoveRuneAt(_slotIndex);
        }
        // 空槽 → 不處理，等待手牌符文卡片點擊後放入
    }

    // =========================================================
    // 私有顯示輔助
    // =========================================================

    private void ShowEmpty()
    {
        if (emptyView != null) emptyView.SetActive(true);
        if (runeView  != null) runeView.SetActive(false);
    }

    private void ShowRune()
    {
        if (emptyView != null) emptyView.SetActive(false);
        if (runeView  != null) runeView.SetActive(true);
    }

    // =========================================================
    // 動畫預留點（DOTween）
    // =========================================================

    private void AnimFill()
    {
        transform.DOKill();
        transform.localScale = Vector3.one;
        // TODO: DOTween transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
    }

    private void AnimEmpty()
    {
        transform.DOKill();
        // TODO: DOTween transform.DOShakePosition(0.15f, strength: 3f, vibrato: 10);
    }
}

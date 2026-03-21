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
///   - 有符文的槽位 → RemoveRuneAt(slotIndex)，將符文退回手牌
///   - 空槽 + 玩家已選中手牌符文 → PlaceRune()，放入選中的符文
///   - 空槽 + 無選中 → 無動作
///
/// UI 刷新流程（不直接更新，而是等事件）：
///   PlaceRune / RemoveRuneAt
///     → WorkbenchManager.OnWorkbenchChanged
///       → OpenScrollUI.Apply()
///         → SlotUI.Refresh()  ← 在這裡才更新顯示
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

        // 從 Workbench 快照判斷此槽位目前是否有符文
        // （不用 _currentRune，因為快照是即時資料，_currentRune 可能落後一幀）
        var wb = GameFacade.Instance.Workbench;
        bool hasRune = wb.PlacedRunes != null
                    && _slotIndex < wb.PlacedRunes.Length
                    && wb.PlacedRunes[_slotIndex] != null;

        if (hasRune)
        {
            // 點擊有符文的槽位 → 退回手牌，同時清除選中
            // UI 刷新由 WorkbenchManager 觸發 OnWorkbenchChanged → OpenScrollUI → Refresh() 驅動
            GameFacade.Instance.RemoveRuneAt(_slotIndex);
            HandSelectionState.Deselect();
        }
        else if (HandSelectionState.HasSelection)
        {
            // 有選中符文 + 空槽 → 嘗試放入
            // PlaceRune 回傳 false 表示槽位已全滿或符文不在手牌（理論上不會發生）
            bool ok = GameFacade.Instance.PlaceRune(HandSelectionState.SelectedRuneData);
            if (ok) HandSelectionState.Deselect();
        }
        // 空槽 + 無選中 → 玩家尚未選符文，無操作
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

using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 展開的卷軸面板（Open Scroll）。
///
/// 職責：
///   - 訂閱 OnWorkbenchChanged，依 WorkbenchSnapshot 決定顯示 Empty 或 NotEmpty
///   - 無選中卷軸：顯示 emptyView，隱藏 notEmptyView
///   - 有選中卷軸：隱藏 emptyView，顯示 notEmptyView
///              並依 SelectedScroll 填入 Title/EffectText
///              依 slotCount 動態生成 SlotUI prefab
///   - 每個 SlotUI.Refresh(runeData)：null = 空圖片, 有值 = 符文圖示+名稱
///
/// Inspector 設定：
///   emptyView    → 無選中卷軸時顯示的子物件
///   notEmptyView → 有選中卷軸時顯示的子物件
///   slotContainer   → Slot prefab 生成的父容器（建議掛 HorizontalLayoutGroup）
///   slotPrefab      → 掛有 SlotUI 的 prefab
/// </summary>
public class OpenScrollUI : MonoBehaviour
{
    [Header("空 / 非空 切換")]
    [SerializeField] private GameObject emptyView;
    [SerializeField] private GameObject notEmptyView;

    [Header("卷軸資訊（在 notEmptyView 內）")]
    [SerializeField] private TMP_Text   scrollNameText;
    [SerializeField] private TMP_Text   scrollEffectText;
    [SerializeField] private Image      scrollIcon;

    [Header("符文槽")]
    [SerializeField] private Transform  slotContainer;
    [SerializeField] private GameObject slotPrefab;

    [Tooltip("所有槽位展開的總寬度（世界單位），槽數多時會重疊")]
    [SerializeField] private float _spreadWidth = 300f;

    private readonly List<SlotUI> _slots = new List<SlotUI>();
    private ScrollData _lastScroll;   // 追蹤上一次的卷軸，決定是否要重建 Slot

    // =========================================================
    // 初始狀態
    // =========================================================

    private void Awake()
    {
        // 預設顯示空狀態
        SetEmpty();
    }
    private bool _subscribed;
    private bool _started;

    // =========================================================
    // 生命週期
    // =========================================================

    private void Start()
    {
        _started = true;
        TrySubscribe();
        Refresh();
    }

    private void OnEnable()
    {
        TrySubscribe();
        Refresh();
    }

    private void OnDisable()
    {
        if (_subscribed && GameFacade.Instance != null)
            GameFacade.Instance.OnWorkbenchChanged -= HandleWorkbenchChanged;
        _subscribed = false;
    }

    private void TrySubscribe()
    {
        if (!_subscribed && GameFacade.Instance != null)
        {
            GameFacade.Instance.OnWorkbenchChanged += HandleWorkbenchChanged;
            _subscribed = true;
            Debug.Log("[OpenScrollUI] 訂閱 OnWorkbenchChanged");
        }
        else if (!_subscribed && _started)
        {
            Debug.LogWarning("[OpenScrollUI] GameFacade.Instance 為 null，訂閱失敗");
        }
    }

    // =========================================================
    // 事件 / Refresh
    // =========================================================

    private void HandleWorkbenchChanged(WorkbenchSnapshot wb)
    {
        Debug.Log($"[OpenScrollUI] OnWorkbenchChanged → SelectedScroll={wb.SelectedScroll?.scrollName ?? "null"}");
        Apply(wb);
    }

    private void Refresh()
    {
        if (GameFacade.Instance != null)
            Apply(GameFacade.Instance.Workbench);
    }

    // =========================================================
    // Apply
    // =========================================================

    private void Apply(WorkbenchSnapshot wb)
    {
        if (wb.SelectedScroll == null)
        {
            _lastScroll = null;
            SetEmpty();
            return;
        }

        SetNotEmpty();
        FillScrollInfo(wb.SelectedScroll);

        // 卷軸切換或首次開啟 → 重建 Slot（Setup 已初始化為空，不需要額外 Refresh）
        if (wb.SelectedScroll != _lastScroll)
        {
            _lastScroll = wb.SelectedScroll;
            RebuildSlots(wb.SlotCount);
            return;   // 剛選卷軸時槽位全空，直接結束
        }

        // 同一張卷軸，只更新槽位內容（符文放入 / 退出）
        for (int i = 0; i < _slots.Count; i++)
        {
            RuneData runeInSlot = (wb.PlacedRunes != null && i < wb.PlacedRunes.Length)
                ? wb.PlacedRunes[i]
                : null;
            _slots[i].Refresh(runeInSlot);
        }
    }

    // =========================================================
    // 卷軸資訊填入
    // =========================================================

    private void FillScrollInfo(ScrollData data)
    {
        if (scrollNameText   != null) scrollNameText.text   = data.scrollName;
        if (scrollEffectText != null) scrollEffectText.text = data.GetModifierDescription();
        if (scrollIcon       != null) scrollIcon.sprite     = data.icon;
    }

    // =========================================================
    // 槽位重建
    // =========================================================

    private void RebuildSlots(int count)
    {
        // 清除舊槽位
        foreach (Transform t in slotContainer)
            Destroy(t.gameObject);
        _slots.Clear();

        if (slotPrefab == null)
        {
            Debug.LogError("[OpenScrollUI] slotPrefab 未設定，無法生成槽位");
            return;
        }

        for (int i = 0; i < count; i++)
        {
            var go   = Instantiate(slotPrefab, slotContainer);
            var slot = go.GetComponent<SlotUI>();
            if (slot == null)
            {
                Debug.LogError("[OpenScrollUI] slotPrefab 上找不到 SlotUI 元件");
                continue;
            }
            slot.Setup(i);
            _slots.Add(slot);
        }

        LayoutSlots();
        Debug.Log($"[OpenScrollUI] 重建槽位 ×{count}");
    }

    // =========================================================
    // 水平排列
    // =========================================================

    private void LayoutSlots()
    {
        int count = _slots.Count;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            float x = count == 1
                ? 0f
                : -_spreadWidth / 2f + i * (_spreadWidth / (count - 1));

            _slots[i].transform.localPosition = new Vector3(x, 0f, 0f);
            _slots[i].transform.SetSiblingIndex(i);
        }
    }

    // =========================================================
    // Empty / NotEmpty 切換
    // =========================================================

    private void SetEmpty()
    {
        if (emptyView    != null) emptyView.SetActive(true);
        if (notEmptyView != null) notEmptyView.SetActive(false);
        // TODO: DOTween AnimClose — notEmptyView.transform.DOScale(0.85f, 0.15f).SetEase(Ease.InBack)
        //       .OnComplete(() => notEmptyView.SetActive(false))
    }

    private void SetNotEmpty()
    {
        if (emptyView    != null) emptyView.SetActive(false);

        if (notEmptyView != null && !notEmptyView.activeSelf)
        {
            notEmptyView.SetActive(true);
            notEmptyView.transform.DOKill();
            notEmptyView.transform.localScale = Vector3.one;
            // TODO: DOTween notEmptyView.transform.DOScale(1f, 0.2f).From(0.85f).SetEase(Ease.OutBack)
        }
    }
}

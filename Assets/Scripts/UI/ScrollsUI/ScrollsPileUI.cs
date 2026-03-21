using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScrollsPileUI : MonoBehaviour
{
    [SerializeField] private GameObject scrollPrefab;

    [Tooltip("所有卷軸展開的總高度（世界單位），卡片數量多時會重疊")]
    [SerializeField] private float _spreadHeight = 400f;

    private readonly List<GameObject> _cards = new List<GameObject>();
    private bool _subscribed;
    private bool _started;

    private void Start()
    {
        _started = true;
        TrySubscribe();
        Refresh();
        StartCoroutine(WaitForInitialScrolls());
    }

    private void OnEnable()
    {
        TrySubscribe();
        Refresh();
    }

    private void OnDisable()
    {
        if (_subscribed)
        {
            if (GameFacade.Instance != null)
                GameFacade.Instance.OnWorkbenchChanged -= HandleWorkbenchChanged;
            _subscribed = false;
        }
    }

    private void TrySubscribe()
    {
        if (!_subscribed && GameFacade.Instance != null)
        {
            GameFacade.Instance.OnWorkbenchChanged += HandleWorkbenchChanged;
            _subscribed = true;
            Debug.Log("[ScrollsPileUI] 訂閱 OnWorkbenchChanged");
        }
        else if (!_subscribed && _started)
        {
            Debug.LogWarning("[ScrollsPileUI] TrySubscribe 失敗：GameFacade.Instance 為 null");
        }
    }

    // 輪詢等待卷軸初始化（OnWorkbenchChanged 在遊戲開始時不會主動 fire）
    private IEnumerator WaitForInitialScrolls()
    {
        while (GameFacade.Instance == null)
            yield return null;

        while (GameFacade.Instance.Workbench.AvailableScrolls.Count == 0)
            yield return null;

        Debug.Log($"[ScrollsPileUI] 初始卷軸已就緒，執行 Refresh");
        Refresh();
    }

    private void HandleWorkbenchChanged(WorkbenchSnapshot snapshot)
    {
        Debug.Log($"[ScrollsPileUI] OnWorkbenchChanged → {snapshot.AvailableScrolls?.Count ?? 0} 張卷軸");
        Apply(snapshot.AvailableScrolls);
    }

    private void Refresh()
    {
        if (GameFacade.Instance != null)
            Apply(GameFacade.Instance.Workbench.AvailableScrolls);
    }

    // =========================================================
    // Apply
    // =========================================================

    private void Apply(IReadOnlyList<ScrollData> scrolls)
    {
        foreach (var card in _cards)
            if (card != null) Destroy(card);
        _cards.Clear();

        if (scrolls == null || scrolls.Count == 0) return;

        if (scrollPrefab == null)
        {
            Debug.LogError("[ScrollsPileUI] scrollPrefab 未設定，無法 Instantiate");
            return;
        }

        for (int i = 0; i < scrolls.Count; i++)
        {
            var go = Instantiate(scrollPrefab, transform);
            _cards.Add(go);
            FillCard(go, scrolls[i]);
        }

        LayoutCards();
    }

    // =========================================================
    // 排列（上下展開，第一張在頂部）
    // =========================================================

    private void LayoutCards()
    {
        int count = _cards.Count;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            float y = count == 1
                ? 0f
                : _spreadHeight / 2f - i * (_spreadHeight / (count - 1));

            _cards[i].transform.localPosition = new Vector3(0f, y, 0f);

            // 上方卡排在前面（sibling index 越大越晚渲染 = 在上層）
            _cards[i].transform.SetSiblingIndex(count - 1 - i);
        }
    }

    // =========================================================
    // 填入資料
    // =========================================================

    private void FillCard(GameObject card, ScrollData data)
    {
        SetText(card, "Title",      data.scrollName);
        SetText(card, "EffectText", data.GetModifierDescription());
        SetIcon(card, "Icon",       data.icon);

        // 注入 ScrollCardUI（點擊選中 + 高亮）
        var cardUI = card.GetComponent<ScrollCardUI>();
        if (cardUI != null) cardUI.Setup(data);
    }

    private void SetText(GameObject root, string childName, string value)
    {
        var t = root.transform.Find(childName);
        if (t == null)
        {
            Debug.LogWarning($"[ScrollsPileUI] 找不到子物件 '{childName}'");
            return;
        }
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null) tmp.text = value;
    }

    private void SetIcon(GameObject root, string childName, Sprite sprite)
    {
        var t = root.transform.Find(childName);
        if (t == null)
        {
            Debug.LogWarning($"[ScrollsPileUI] 找不到子物件 '{childName}'");
            return;
        }
        var img = t.GetComponent<Image>();
        if (img != null) img.sprite = sprite;
    }
}

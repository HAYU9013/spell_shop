using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RuneOnHandUI : MonoBehaviour
{
    [SerializeField] private GameObject runePrefab;

    [Tooltip("所有手牌展開的總寬度（世界單位），卡片數量多時會重疊")]
    [SerializeField] private float _spreadWidth = 400f;

    private readonly List<GameObject> _cards = new List<GameObject>();
    private bool _subscribed;
    private bool _started;

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
        if (_subscribed)
        {
            if (GameFacade.Instance != null)
                GameFacade.Instance.OnHandUpdated -= HandleHandUpdated;
            _subscribed = false;
        }
    }

    private void TrySubscribe()
    {
        if (!_subscribed && GameFacade.Instance != null)
        {
            GameFacade.Instance.OnHandUpdated += HandleHandUpdated;
            _subscribed = true;
            Debug.Log("[RuneOnHandUI] 訂閱 OnHandUpdated");
        }
        else if (!_subscribed && _started)
        {
            Debug.LogWarning("[RuneOnHandUI] TrySubscribe 失敗：GameFacade.Instance 為 null");
        }
    }

    private void HandleHandUpdated(IReadOnlyList<RuneData> hand)
    {
        Debug.Log($"[RuneOnHandUI] OnHandUpdated → {hand?.Count ?? 0} 張");
        Apply(hand);
    }

    private void Refresh()
    {
        if (GameFacade.Instance != null)
            Apply(GameFacade.Instance.Hand);
    }

    // =========================================================
    // Apply
    // =========================================================

    private void Apply(IReadOnlyList<RuneData> hand)
    {
        // 清除舊卡；RuneCardUI.OnDestroy 會自動取消 OnSelectionChanged 訂閱
        foreach (var card in _cards)
            if (card != null) Destroy(card);
        _cards.Clear();

        // 手牌索引全部重新分配，舊的 SelectedIndex 已無意義，必須清除
        HandSelectionState.Deselect();

        if (hand == null || hand.Count == 0) return;

        if (runePrefab == null)
        {
            Debug.LogError("[RuneOnHandUI] runePrefab 未設定，無法 Instantiate");
            return;
        }

        // 依手牌順序生成卡片，index 與 HandSelectionState 的 SelectedIndex 對應
        for (int i = 0; i < hand.Count; i++)
        {
            var go = Instantiate(runePrefab, transform);
            _cards.Add(go);
            FillCard(go, hand[i]);

            // 附加點擊互動元件（掛在 Icon 子物件上，因其有 Image + RaycastTarget）
            SetupClickHandler(go, i, hand[i]);
        }

        LayoutCards();
    }

    // =========================================================
    // 排列
    // =========================================================

    private void LayoutCards()
    {
        int count = _cards.Count;
        if (count == 0) return;

        for (int i = 0; i < count; i++)
        {
            float x = count == 1
                ? 0f
                : -_spreadWidth / 2f + i * (_spreadWidth / (count - 1));

            var rt = _cards[i].GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = new Vector2(x, 0f);
            else
                _cards[i].transform.localPosition = new Vector3(x, 0f, 0f);

            // 右側卡排在前面（sibling index 越大越晚渲染 = 在上層）
            _cards[i].transform.SetSiblingIndex(i);
        }
    }

    // =========================================================
    // 填入資料
    // =========================================================

    private void FillCard(GameObject card, RuneData data)
    {
        SetText(card, "Title", data.runeName);
        SetText(card, "EffectText", data.GetEffectSummary());
        SetIcon(card, "Icon", data.icon);
    }

    private void SetText(GameObject root, string childName, string value)
    {
        var t = root.transform.Find(childName);
        if (t == null)
        {
            Debug.LogWarning($"[RuneOnHandUI] 找不到子物件 '{childName}'");
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
            Debug.LogWarning($"[RuneOnHandUI] 找不到子物件 '{childName}'");
            return;
        }
        var img = t.GetComponent<Image>();
        if (img != null) img.sprite = sprite;
    }

    // =========================================================
    // 點擊互動設定
    // =========================================================

    private void SetupClickHandler(GameObject card, int index, RuneData rune)
    {
        // Rune prefab 根物件是 Transform（沒有 Image），EventSystem 無法對它射線偵測。
        // Icon 子物件有 Image + RaycastTarget = true，掛在這裡才能接收點擊。
        // RuneCardUI 內部保有 card.transform 參照，動畫作用於整張卡而非只有 Icon。
        var iconTransform = card.transform.Find("Icon");
        if (iconTransform == null)
        {
            Debug.LogWarning("[RuneOnHandUI] 找不到 Icon 子物件，無法設定點擊互動");
            return;
        }

        // GetComponent 防止重複 AddComponent（理論上新 Instantiate 不會有，保險起見）
        var cardUI = iconTransform.GetComponent<RuneCardUI>();
        if (cardUI == null)
            cardUI = iconTransform.gameObject.AddComponent<RuneCardUI>();

        cardUI.Setup(index, rune, card.transform);
    }
}

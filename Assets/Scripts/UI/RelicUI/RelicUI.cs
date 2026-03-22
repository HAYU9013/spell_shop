using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RelicUI : MonoBehaviour
{
    [SerializeField] private GameObject relicPrefab;

    private readonly List<Transform> _slots = new List<Transform>();
    private readonly List<GameObject> _instances = new List<GameObject>();
    private bool _subscribed;
    private bool _started;

    private void Awake()
    {
        // 收集所有名為 "Relic Place" 的直接子物件作為固定槽位
        foreach (Transform child in transform)
        {
            if (child.name == "Relic Place")
                _slots.Add(child);
        }
        Debug.Log($"[RelicUI] Awake — 找到 {_slots.Count} 個 Relic Place 槽位");
    }

    private void Start()
    {
        _started = true;
        TrySubscribe();
        Refresh();
        StartCoroutine(WaitForInitialRelics());
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
                GameFacade.Instance.OnRelicsChanged -= HandleRelicsChanged;
            _subscribed = false;
        }
    }

    private void TrySubscribe()
    {
        if (!_subscribed && GameFacade.Instance != null)
        {
            GameFacade.Instance.OnRelicsChanged += HandleRelicsChanged;
            _subscribed = true;
            Debug.Log("[RelicUI] 訂閱 OnRelicsChanged");
        }
        else if (!_subscribed && _started)
        {
            Debug.LogWarning("[RelicUI] TrySubscribe 失敗：GameFacade.Instance 為 null");
        }
    }

    // 輪詢等待 Relic 初始化（OnRelicsChanged 在遊戲開始時不會主動 fire）
    private IEnumerator WaitForInitialRelics()
    {
        while (GameFacade.Instance == null)
            yield return null;

        while (GameFacade.Instance.Relics == null || GameFacade.Instance.Relics.Count == 0)
            yield return null;

        Debug.Log($"[RelicUI] 初始 Relic 已就緒，執行 Refresh");
        Refresh();
    }

    private void HandleRelicsChanged(IReadOnlyList<RelicSnapshot> relics)
    {
        Debug.Log($"[RelicUI] OnRelicsChanged → {relics?.Count ?? 0} 個 Relic");
        Apply(relics);
    }

    private void Refresh()
    {
        if (GameFacade.Instance != null)
            Apply(GameFacade.Instance.Relics);
    }

    // =========================================================
    // Apply
    // =========================================================

    private void Apply(IReadOnlyList<RelicSnapshot> relics)
    {
        // 清除所有槽位內的舊 instance
        foreach (var inst in _instances)
            if (inst != null) Destroy(inst);
        _instances.Clear();

        if (relics == null || relics.Count == 0) return;

        if (relicPrefab == null)
        {
            Debug.LogError("[RelicUI] relicPrefab 未設定，無法 Instantiate");
            return;
        }

        if (relics.Count > _slots.Count)
            Debug.LogWarning($"[RelicUI] Relic 數量 ({relics.Count}) 超過槽位數 ({_slots.Count})，多餘的不會顯示");

        int displayCount = Mathf.Min(relics.Count, _slots.Count);
        for (int i = 0; i < displayCount; i++)
        {
            var go = Instantiate(relicPrefab, _slots[i]);
            go.transform.localPosition = Vector3.zero;
            _instances.Add(go);
            FillCard(go, relics[i]);
        }
    }

    // =========================================================
    // 填入資料
    // =========================================================

    private void FillCard(GameObject card, RelicSnapshot data)
    {
        SetText(card, "Title", data.RelicName);

        string effectText = data.IsSatisfied ? data.SatisfiedEffectText : data.UnsatisfiedEffectText;
        SetText(card, "EffectText", effectText);

        SetIcon(card, "Image", data.Icon);

        var trigger = card.GetComponent<HoverTooltipTrigger>() ?? card.AddComponent<HoverTooltipTrigger>();
        trigger.Setup(data.RelicName, data.Description);
    }

    private void SetText(GameObject root, string childName, string value)
    {
        var t = root.transform.Find(childName);
        if (t == null)
        {
            Debug.LogWarning($"[RelicUI] 找不到子物件 '{childName}'");
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
            Debug.LogWarning($"[RelicUI] 找不到子物件 '{childName}'");
            return;
        }
        var sr = t.GetComponent<SpriteRenderer>();
        if (sr != null) { sr.sprite = sprite; return; }

        var img = t.GetComponent<UnityEngine.UI.Image>();
        if (img != null) img.sprite = sprite;
    }
}

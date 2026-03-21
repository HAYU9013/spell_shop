using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CustomerUI : MonoBehaviour
{
    [SerializeField] private GameObject customerPrefab;

    private GameObject _instance;
    private bool _subscribed;
    private bool _started;

    private void Awake()
    {
        Debug.Log($"[CustomerUI] Awake — GameFacade.Instance={(GameFacade.Instance != null ? "OK" : "null")}");
    }

    private void Start()
    {
        _started = true;
        TrySubscribe();

        // 同步抓一次（若 Phase 1 已在 GameManager.Start() 中同步執行完）
        Refresh();

        // 非同步輪詢（若 Phase 1 尚未執行）
        StartCoroutine(WaitForFirstCustomer());

        Debug.Log($"[CustomerUI] Start — subscribed={_subscribed}, IsPresent={GameFacade.Instance?.CurrentCustomer.IsPresent}");
    }

    private IEnumerator WaitForFirstCustomer()
    {
        int frame = 0;
        while (GameFacade.Instance == null)
        {
            Debug.Log($"[CustomerUI] WaitForFirstCustomer frame{frame}: GameFacade null");
            frame++;
            yield return null;
        }

        while (!GameFacade.Instance.CurrentCustomer.IsPresent)
        {
            if (frame <= 10)
                Debug.Log($"[CustomerUI] WaitForFirstCustomer frame{frame}: IsPresent=False");
            frame++;
            yield return null;
        }

        Debug.Log($"[CustomerUI] WaitForFirstCustomer frame{frame}: 顧客到來，執行 Refresh");
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
                GameFacade.Instance.OnCustomerChanged -= HandleCustomerChanged;
            _subscribed = false;
        }
    }

    private void TrySubscribe()
    {
        if (!_subscribed && GameFacade.Instance != null)
        {
            GameFacade.Instance.OnCustomerChanged += HandleCustomerChanged;
            _subscribed = true;
            Debug.Log("[CustomerUI] 訂閱 OnCustomerChanged");
        }
        else if (!_subscribed && _started)
        {
            Debug.LogWarning("[CustomerUI] TrySubscribe 失敗：GameFacade.Instance 為 null");
        }
    }

    private void HandleCustomerChanged(CustomerSnapshot snapshot)
    {
        Debug.Log($"[CustomerUI] HandleCustomerChanged → IsPresent={snapshot.IsPresent}" +
                  (snapshot.IsPresent ? $", Name={snapshot.Name}" : ""));
        Apply(snapshot);
    }

    private void Refresh()
    {
        if (GameFacade.Instance == null) return;
        var snapshot = GameFacade.Instance.CurrentCustomer;
        Debug.Log($"[CustomerUI] Refresh → IsPresent={snapshot.IsPresent}");
        Apply(snapshot);
    }

    private void Apply(CustomerSnapshot snapshot)
    {
        if (!snapshot.IsPresent)
        {
            if (_instance != null)
            {
                Debug.Log("[CustomerUI] 顧客離開，移除 prefab instance");
                Destroy(_instance);
                _instance = null;
            }
            return;
        }

        if (_instance == null)
        {
            if (customerPrefab == null)
            {
                Debug.LogError("[CustomerUI] customerPrefab 未設定，無法 Instantiate");
                return;
            }
            _instance = Instantiate(customerPrefab, transform);
            Debug.Log($"[CustomerUI] Instantiate 顧客 prefab → {snapshot.Name}");
        }

        SetText(_instance, "name", snapshot.Name);
        SetText(_instance, "flavorText", snapshot.FlavorText);
        SetPortrait(_instance, "portrait", snapshot.Portrait);
    }

    private void SetText(GameObject root, string childName, string value)
    {
        var t = root.transform.Find(childName);
        if (t == null)
        {
            Debug.LogWarning($"[CustomerUI] 找不到子物件 '{childName}'");
            return;
        }
        var tmp = t.GetComponent<TextMeshProUGUI>();
        if (tmp != null) tmp.text = value;
    }

    private void SetPortrait(GameObject root, string childName, Sprite sprite)
    {
        var t = root.transform.Find(childName);
        if (t == null)
        {
            Debug.LogWarning($"[CustomerUI] 找不到子物件 '{childName}'");
            return;
        }

        var img = t.GetComponent<Image>();
        if (img != null) { img.sprite = sprite; return; }

        var sr = t.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sprite = sprite;
    }
}

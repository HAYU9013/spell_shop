using TMPro;
using UnityEngine;

public class CustomerPatienceUI : MonoBehaviour
{
    [SerializeField] private TMP_Text patienceText;

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
        }
        else if (!_subscribed && _started)
        {
            Debug.LogWarning("[CustomerPatienceUI] TrySubscribe 失敗：GameFacade.Instance 為 null");
        }
    }

    private void HandleCustomerChanged(CustomerSnapshot snapshot)
    {
        Apply(snapshot);
    }

    private void Refresh()
    {
        if (GameFacade.Instance == null) return;
        Apply(GameFacade.Instance.CurrentCustomer);
    }

    private void Apply(CustomerSnapshot snapshot)
    {
        if (patienceText == null) return;
        patienceText.text = snapshot.IsPresent
            ? $"耐心: {snapshot.CurrentPatience}/{snapshot.MaxPatience}"
            : "-/-";
    }
}

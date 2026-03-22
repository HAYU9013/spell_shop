using TMPro;
using UnityEngine;

/// <summary>
/// 顯示當前顧客的敘述文字（FlavorText）。
/// </summary>
public class CustomerRequirementUI : MonoBehaviour
{
    [SerializeField] private TMP_Text requirementText;

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
            {
                GameFacade.Instance.OnCustomerChanged -= HandleCustomerChanged;
            }
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
            Debug.LogWarning("[CustomerFlavorTextUI] TrySubscribe 失敗：GameFacade.Instance 為 null");
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
        if (requirementText == null) return;
        requirementText.text = snapshot.IsPresent ? snapshot.FlavorText : "";
    }
}

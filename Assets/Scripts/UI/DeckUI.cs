using UnityEngine;
using UnityEngine.UI;

public class DeckUI : MonoBehaviour
{
    [SerializeField] private Button submitButton;

    private bool _subscribed;
    private bool _started;

    private void Start()
    {
        _started = true;
        submitButton.onClick.AddListener(OnSubmitClicked);
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
        }
        else if (!_subscribed && _started)
        {
            Debug.LogWarning("[DeckUI] TrySubscribe 失敗：GameFacade.Instance 為 null");
        }
    }

    private void HandleWorkbenchChanged(WorkbenchSnapshot snapshot)
    {
        SetSubmittable(snapshot.CanSubmit);
    }

    private void Refresh()
    {
        if (GameFacade.Instance == null) return;
        SetSubmittable(GameFacade.Instance.Workbench.CanSubmit);
    }

    private void SetSubmittable(bool canSubmit)
    {
        if (submitButton != null)
            submitButton.interactable = canSubmit;
    }

    private void OnSubmitClicked()
    {
        GameFacade.Instance?.SubmitScroll();
    }
}

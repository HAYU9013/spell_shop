using TMPro;
using UnityEngine;

public class UpdateBrightness : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private bool _subscribed;

    private void Awake()
    {
        foreach (Transform child in transform)
        {
            _text = child.GetComponent<TextMeshProUGUI>();
            if (_text != null) break;
        }
    }

    private void Start()
    {
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
                GameFacade.Instance.OnEnvironmentChanged -= HandleEnvironmentChanged;
            _subscribed = false;
        }
    }

    private void TrySubscribe()
    {
        if (!_subscribed && GameFacade.Instance != null)
        {
            GameFacade.Instance.OnEnvironmentChanged += HandleEnvironmentChanged;
            _subscribed = true;
        }
    }

    private void HandleEnvironmentChanged(EnvironmentData data)
    {
        SetText(data.brightness);
    }

    private void Refresh()
    {
        if (GameFacade.Instance != null)
            SetText(GameFacade.Instance.Environment.brightness);
    }

    private void SetText(int value)
    {
        if (_text != null)
            _text.text = value.ToString();
    }
}

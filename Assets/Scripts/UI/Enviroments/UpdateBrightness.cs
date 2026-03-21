using DG.Tweening;
using TMPro;
using UnityEngine;

public class UpdateBrightness : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private bool _subscribed;
    private int _displayValue;

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
        if (_text == null) return;
        _text.DOKill();
        DOTween.To(() => _displayValue, x => { _displayValue = x; _text.text = x.ToString(); }, value, 0.4f)
            .SetEase(Ease.OutQuad).SetLink(gameObject);
        _text.DOColor(Color.yellow, 0.1f)
            .OnComplete(() => _text.DOColor(Color.white, 0.25f))
            .SetLink(gameObject);
    }
}

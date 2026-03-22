using TMPro;
using UnityEngine;

public class DeckCountUI : MonoBehaviour
{
    [SerializeField] private TMP_Text drawPileText;
    [SerializeField] private TMP_Text discardPileText;

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
                GameFacade.Instance.OnDeckCountChanged -= HandleDeckCountChanged;
            _subscribed = false;
        }
    }

    private void TrySubscribe()
    {
        if (!_subscribed && GameFacade.Instance != null)
        {
            GameFacade.Instance.OnDeckCountChanged += HandleDeckCountChanged;
            _subscribed = true;
        }
        else if (!_subscribed && _started)
        {
            Debug.LogWarning("[DeckCountUI] TrySubscribe 失敗：GameFacade.Instance 為 null");
        }
    }

    private void HandleDeckCountChanged(int drawPile, int discardPile)
    {
        SetTexts(drawPile, discardPile);
    }

    private void Refresh()
    {
        if (GameFacade.Instance == null) return;
        SetTexts(GameFacade.Instance.DrawPileCount, GameFacade.Instance.DiscardPileCount);
    }

    private void SetTexts(int drawPile, int discardPile)
    {
        if (drawPileText != null)
            drawPileText.text = drawPile.ToString();

        if (discardPileText != null)
            discardPileText.text = discardPile.ToString();
    }
}

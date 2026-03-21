using UnityEngine;
using DG.Tweening;

public class OpenDeskUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject deskUI;
    [SerializeField] private float fadeDuration = 0.3f;

    private RectTransform _deskRT;
    private float _targetY;

    private void Awake()
    {
        _deskRT = deskUI != null ? deskUI.GetComponent<RectTransform>() : null;
        if (_deskRT != null)
            _targetY = _deskRT.anchoredPosition.y;
    }

    /// <summary>
    /// 切換 DeskUI 顯示狀態
    /// </summary>
    public void ToggleDeskUI()
    {
        if (deskUI == null) return;

        if (!deskUI.activeSelf)
            Open();
        else
            Close();
    }

    private void Open()
    {
        _deskRT.DOKill();
        deskUI.SetActive(true);

        float height = _deskRT.rect.height;
        _deskRT.anchoredPosition = new Vector2(_deskRT.anchoredPosition.x, _targetY - height);
        _deskRT.DOAnchorPosY(_targetY, fadeDuration).SetEase(Ease.OutCubic).SetLink(deskUI);
    }

    public void Close()
    {
        _deskRT.DOKill();

        float height = _deskRT.rect.height;
        _deskRT.DOAnchorPosY(_targetY - height, fadeDuration * 0.75f)
            .SetEase(Ease.InCubic)
            .SetLink(deskUI)
            .OnComplete(() =>
            {
                deskUI.SetActive(false);
                _deskRT.anchoredPosition = new Vector2(_deskRT.anchoredPosition.x, _targetY);
            });
    }
}
using TMPro;
using UnityEngine;

/// <summary>
/// 通知 UI — 只顯示最新一條訊息，像新聞跑馬燈。
/// 新訊息進來時覆蓋舊的，停留後自動淡出。
///
/// Inspector 只需指定一個 TextMeshProUGUI。
/// </summary>
public class NotificationUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;


    private void Awake()
    {
        if (_text != null)
            _text.text = "無";
    }

    private void Start()
    {
        if (GameFacade.Instance != null)
            GameFacade.Instance.OnNotification += ShowNotification;
        else
            Debug.LogWarning("[NotificationUI] GameFacade.Instance 為 null，無法訂閱");
    }

    private void OnDestroy()
    {
        if (GameFacade.Instance != null)
            GameFacade.Instance.OnNotification -= ShowNotification;
    }

    private void ShowNotification(string message)
    {
        if (_text == null) return;

        _text.text = message;
    }
}

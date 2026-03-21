using UnityEngine;
using DG.Tweening; // 預留 DOTween 命名空間

public class OpenDeskUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject deskUI;
    [SerializeField] private float fadeDuration = 0.3f;

    /// <summary>
    /// 切換 DeskUI 顯示狀態
    /// </summary>
    public void ToggleDeskUI()
    {
        if (deskUI == null) return;

        bool isActive = !deskUI.activeSelf;

        if (isActive)
        {
            Open();
        }
        else
        {
            Close();
        }
    }

    private void Open()
    {
        deskUI.SetActive(true);
        // 未來 DOTween 動畫可寫在此處，例如：
        deskUI.transform.DOScale(1, fadeDuration).From(0);
    }

    private void Close()
    {
        // 若要等待動畫結束再 SetActive(false)，可使用 DOTween 回呼
        deskUI.SetActive(false);
    }
}
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 掛在有 RaycastTarget 的 UI 物件上。
/// 滑鼠進入時顯示 TooltipUI，離開或物件關閉時隱藏。
///
/// 使用方式：
///   var trigger = go.AddComponent&lt;HoverTooltipTrigger&gt;();
///   trigger.Setup("符文名稱", "效果說明");
/// </summary>
public class HoverTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private string _title;
    private string _description;

    public void Setup(string title, string description)
    {
        _title       = title;
        _description = description;
    }

    public void OnPointerEnter(PointerEventData _)
    {
        if (TooltipUI.Instance != null)
            TooltipUI.Instance.Show(_title, _description);
    }

    public void OnPointerExit(PointerEventData _)
    {
        if (TooltipUI.Instance != null)
            TooltipUI.Instance.Hide();
    }

    private void OnDisable()
    {
        if (TooltipUI.Instance != null)
            TooltipUI.Instance.Hide();
    }
}

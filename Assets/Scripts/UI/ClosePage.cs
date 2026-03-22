
using UnityEngine;
using UnityEngine.UI;

public class ClosePage : MonoBehaviour
{
    [Header("設定要關閉的頁面")]
    public GameObject targetPage;

    /// <summary>
    /// 供 Button 組件的 OnClick() 事件呼叫
    /// </summary>
    public void DeactivatePage()
    {
        if (targetPage != null)
        {
            targetPage.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Target Page 未指派，請檢查 Inspector 設置。");
        }
    }
}
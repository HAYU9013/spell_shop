using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 全域 Tooltip 面板，跟隨滑鼠顯示標題與說明。
///
/// 【Unity 設定步驟】
///   1. 在 Canvas 最上層新增 GameObject "TooltipUI"，掛上此腳本
///   2. 在其下建立子 Panel（帶 Image 背景），指定給 _panel
///   3. Panel 下建立 "Title"（TMP_Text）→ _titleText
///   4. Panel 下建立 "Description"（TMP_Text）→ _descriptionText
///   5. Panel 加上 ContentSizeFitter（Horizontal + Vertical = Preferred Size）讓寬高自適應
/// </summary>
public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    [SerializeField] private RectTransform _panel;
    [SerializeField] private TMP_Text      _titleText;
    [SerializeField] private TMP_Text      _descriptionText;

    [Tooltip("相對於滑鼠的偏移（local space）")]
    [SerializeField] private Vector2 _offset = new Vector2(12f, -12f);

    private Canvas        _canvas;
    private RectTransform _canvasRect;

    // =========================================================
    // 生命週期
    // =========================================================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _canvas     = GetComponentInParent<Canvas>();
        _canvasRect = _canvas != null ? _canvas.GetComponent<RectTransform>() : null;

        // 確保 Panel 不攔截滑鼠射線，否則 Tooltip 會遮住觸發物件造成閃爍
        if (_panel != null)
        {
            var cg = _panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = _panel.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable   = false;
        }

        Hide();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (_panel != null && _panel.gameObject.activeSelf)
            PositionAtMouse();
    }

    // =========================================================
    // 公開 API
    // =========================================================

    public void Show(string title, string description)
    {
        if (_titleText       != null) _titleText.text       = title;
        if (_descriptionText != null) _descriptionText.text = description;
        if (_panel           != null) _panel.gameObject.SetActive(true);
        PositionAtMouse();
    }

    public void Hide()
    {
        if (_panel != null) _panel.gameObject.SetActive(false);
    }

    // =========================================================
    // 位置計算
    // =========================================================

    private void PositionAtMouse()
    {
        if (_canvasRect == null || _panel == null) return;

        var camera = _canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            Input.mousePosition,
            camera,
            out Vector2 localPoint
        );

        _panel.anchoredPosition = localPoint + _offset;
    }
}

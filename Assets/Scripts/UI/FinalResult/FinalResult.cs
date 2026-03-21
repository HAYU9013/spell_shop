using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// 遊戲結束畫面。
/// - LevelClear（正常結束）：啟用 YouWin 子物件，並將 Score 文字更新為當前業績。
/// - GameOver（失敗）      ：啟用 YouDie 子物件。
///
/// 在 Inspector 中指定：
///   _youWin  — You Win 子物件
///   _youDie  — You Die 子物件
///   _scoreText — Score 的 TextMeshProUGUI（位於 YouWin 子物件內）
/// </summary>
public class FinalResult : MonoBehaviour
{
    [Header("子物件")]
    [SerializeField] private GameObject _youWin;
    [SerializeField] private GameObject _youDie;

    [Header("業績文字（YouWin 內）")]
    [SerializeField] private TextMeshProUGUI _scoreText;

    private void Start()
    {
        if (_youWin != null) _youWin.SetActive(false);
        if (_youDie != null) _youDie.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelClear += HandleLevelClear;
            GameManager.Instance.OnGameOver   += HandleGameOver;
        }
        else
        {
            Debug.LogWarning("[FinalResult] GameManager.Instance 為 null，無法訂閱結束事件");
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelClear -= HandleLevelClear;
            GameManager.Instance.OnGameOver   -= HandleGameOver;
        }
    }

    private void HandleLevelClear()
    {
        if (_youDie != null) _youDie.SetActive(false);

        if (_youWin != null)
        {
            _youWin.SetActive(true);
            _youWin.transform.localScale = Vector3.zero;
            _youWin.transform.DOScale(1f, 0.6f).SetEase(Ease.OutElastic).SetLink(_youWin);
        }

        if (_scoreText != null && EnvironmentManager.Instance != null)
        {
            int target = EnvironmentManager.Instance.Score;
            _scoreText.text = "0";
            DOTween.To(() => 0, x => _scoreText.text = x.ToString(), target, 1.2f)
                .SetEase(Ease.OutQuad)
                .SetDelay(0.5f)
                .SetLink(_youWin);
        }
    }

    private void HandleGameOver(string reason)
    {
        if (_youWin != null) _youWin.SetActive(false);

        if (_youDie != null)
        {
            var cg = _youDie.GetComponent<CanvasGroup>() ?? _youDie.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            _youDie.transform.localScale = Vector3.one * 1.2f;
            _youDie.SetActive(true);
            cg.DOFade(1f, 0.6f).SetEase(Ease.OutQuad).SetLink(_youDie);
            _youDie.transform.DOScale(1f, 0.6f).SetEase(Ease.OutQuad).SetLink(_youDie);
        }
    }
}

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
        if (_youWin != null) _youWin.SetActive(true);

        if (_scoreText != null && EnvironmentManager.Instance != null)
            _scoreText.text = EnvironmentManager.Instance.Score.ToString();
    }

    private void HandleGameOver(string reason)
    {
        if (_youWin != null) _youWin.SetActive(false);
        if (_youDie != null) _youDie.SetActive(true);
    }
}

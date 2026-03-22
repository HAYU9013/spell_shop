using TMPro;
using UnityEngine;

/// <summary>
/// 掛在單一遺物的 UI GameObject 上。
/// 在 Inspector 填入 relicName（對應 RelicData.relicName）與文字元件，
/// 自動顯示「已累積不滿意 / 上限」。
/// </summary>
public class RelicDissatisfiedUI : MonoBehaviour
{
    [SerializeField] private TMP_Text dissatisfiedText;
    [SerializeField] private RelicData relicData;

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
                GameFacade.Instance.OnRelicsChanged -= HandleRelicsChanged;
            _subscribed = false;
        }
    }

    private void TrySubscribe()
    {
        if (!_subscribed && GameFacade.Instance != null)
        {
            GameFacade.Instance.OnRelicsChanged += HandleRelicsChanged;
            _subscribed = true;
        }
        else if (!_subscribed && _started)
        {
            Debug.LogWarning("[RelicDissatisfiedUI] TrySubscribe 失敗：GameFacade.Instance 為 null");
        }
    }

    private void HandleRelicsChanged(System.Collections.Generic.IReadOnlyList<RelicSnapshot> relics)
    {
        Apply(relics);
    }

    private void Refresh()
    {
        if (GameFacade.Instance == null) return;
        Apply(GameFacade.Instance.Relics);
    }

    private void Apply(System.Collections.Generic.IReadOnlyList<RelicSnapshot> relics)
    {
        if (dissatisfiedText == null) return;

        if (relics == null || relicData == null) { dissatisfiedText.text = "-/-"; return; }

        foreach (var r in relics)
        {
            if (r.RelicName != relicData.relicName) continue;
            dissatisfiedText.text = $"{r.DissatisfiedCount}/{r.DissatisfiedLimit}";
            return;
        }

        dissatisfiedText.text = "-/-";
    }
}

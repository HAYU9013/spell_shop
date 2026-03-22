using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 環境數值資料結構。
/// 保存亮度、水分、溫度三項環境屬性與業績。
/// 環境數值在回合之間持續保留，不會自動重置。
/// 業績初始值 30，降至 0 → Game Over，同時作為批發商貨幣。
/// </summary>
[System.Serializable]
public class EnvironmentData
{
    // ----- 邊界常數 -----
    public const int MIN_VALUE = 0;
    public const int MAX_VALUE = 20;

    // ----- 初始值 -----
    public const int INITIAL_BRIGHTNESS = 5;
    public const int INITIAL_MOISTURE = 5;
    public const int INITIAL_TEMPERATURE = 5;
    public const int INITIAL_SCORE = 0;

    // ----- 環境屬性 -----
    [Range(MIN_VALUE, MAX_VALUE)] public int brightness;
    [Range(MIN_VALUE, MAX_VALUE)] public int moisture;
    [Range(MIN_VALUE, MAX_VALUE)] public int temperature;

    // ----- 業績 -----
    public int score;

    // =========================================================
    // 建構子
    // =========================================================

    public EnvironmentData()
    {
        brightness = INITIAL_BRIGHTNESS;
        moisture = INITIAL_MOISTURE;
        temperature = INITIAL_TEMPERATURE;
        score = INITIAL_SCORE;
    }

    // =========================================================
    // 讀取 / 寫入
    // =========================================================

    /// <summary>取得指定屬性的當前數值</summary>
    public int GetValue(EnvAttribute attr)
    {
        switch (attr)
        {
            case EnvAttribute.Brightness: return brightness;
            case EnvAttribute.Moisture: return moisture;
            case EnvAttribute.Temperature: return temperature;
            default:
                Debug.LogWarning($"[EnvironmentData] GetValue: unknown attribute {attr}");
                return 0;
        }
    }

    /// <summary>
    /// 設定指定屬性的數值，自動 Clamp 在 MIN_VALUE ~ MAX_VALUE。
    /// 回傳是否因 Clamp 而觸碰邊界（用於極端事件判定）。
    /// </summary>
    public bool SetValue(EnvAttribute attr, int value)
    {
        int clamped = Mathf.Clamp(value, MIN_VALUE, MAX_VALUE);
        bool hitBoundary = (value <= MIN_VALUE || value >= MAX_VALUE);

        switch (attr)
        {
            case EnvAttribute.Brightness: brightness = clamped; break;
            case EnvAttribute.Moisture: moisture = clamped; break;
            case EnvAttribute.Temperature: temperature = clamped; break;
            default:
                Debug.LogWarning($"[EnvironmentData] SetValue: unknown attribute {attr}");
                break;
        }

        return hitBoundary;
    }

    /// <summary>
    /// 對指定屬性增減 delta，自動 Clamp。
    /// 回傳是否觸碰邊界（用於極端事件判定）。
    /// </summary>
    public bool ApplyDelta(EnvAttribute attr, int delta)
    {
        int current = GetValue(attr);
        return SetValue(attr, current + delta);
    }

    /// <summary>修改業績（允許負數傳入，但結果 clamp 在 0 以上）</summary>
    public void ModifyScore(int delta)
    {
        score = Mathf.Max(0, score + delta);
    }

    // =========================================================
    // 拷貝 / 差異
    // =========================================================

    /// <summary>建立深拷貝，用於快照與預覽</summary>
    public EnvironmentData Clone()
    {
        return new EnvironmentData
        {
            brightness = this.brightness,
            moisture = this.moisture,
            temperature = this.temperature,
            score = this.score
        };
    }

    /// <summary>計算 from → to 之間每項屬性的差異</summary>
    public static EnvironmentDelta Diff(EnvironmentData from, EnvironmentData to)
    {
        return new EnvironmentDelta
        {
            brightnessDelta = to.brightness - from.brightness,
            moistureDelta = to.moisture - from.moisture,
            temperatureDelta = to.temperature - from.temperature
        };
    }

    // =========================================================
    // 工具
    // =========================================================

    public override string ToString()
    {
        return $"Brightness:{brightness} Moisture:{moisture} Temperature:{temperature} Score:{score}";
    }
}

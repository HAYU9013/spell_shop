using System;
using UnityEngine;

// ============================================================
// 全域列舉與共用資料結構
// ============================================================

// ----- 環境屬性 -----

public enum EnvAttribute
{
    Brightness, // 亮度  0~20
    Moisture,   // 水分  0~20
    Temperature // 溫度  0~20
}

// ----- 極端事件（數值觸碰邊界時一次性觸發） -----

public enum ExtremeEvent
{
    None,
    Darkness,  // 黑暗降臨：亮度 = 0，所有遺物不滿意計數 +1
    Flood,     // 大洪水  ：水分 = 20，水分額外 +5
    Overheat,  // 高溫爆發：溫度 = 20，業績 -10
    Overcold   // 極寒侵襲：溫度 = 0，業績 -10
}

// ----- 稀有度 -----

public enum Rarity
{
    Common,    // 普通
    Rare,      // 稀有
    Legendary  // 傳說
}

// ----- 符文類型 -----

public enum RuneType
{
    Cycle,      // 循環型：使用後進棄牌堆，牌庫抽完洗牌重來
    Consumable  // 消耗型：使用後永久移除
}

// ----- 符文標籤（Flags，支援多標籤） -----

[Flags]
public enum RuneTag
{
    None    = 0,
    Water   = 1 << 0, // 水系：水分↑、溫度↓
    Fire    = 1 << 1, // 火系：溫度↑、水分↓
    Light   = 1 << 2, // 光系：亮度↑
    Dark    = 1 << 3, // 暗系：亮度↓
    Wind    = 1 << 4, // 風系：多項數值小幅調整
    Neutral = 1 << 5  // 中性：複合效果，無標籤加成
}

// ----- 比較運算子（顧客需求條件用） -----

public enum CompareOperator
{
    GreaterEqual, // >=
    LessEqual,    // <=
    Equal,        // ==
    InRange       // min <= value <= max
}

// ----- 顧客需求類型 -----

public enum RequirementType
{
    Absolute,       // 絕對值條件：環境值達到指定數值
    RelativeChange, // 相對變化條件：自顧客到來後累積變化量達標
    Stability,      // 環境穩定條件：條件連續 N 回合成立
    TagPreference   // 標籤偏好條件：送出卷軸中指定標籤符文達數量
}

// ----- 遺物條件邏輯 -----

public enum ConditionLogic
{
    And, // 所有條件皆需成立
    Or   // 任一條件成立即可
}

// ----- 遺物懲罰類型 -----

public enum RelicPunishment
{
    EnvironmentShock, // 環境劇烈波動（強制設定某屬性）
    ScoreReset,       // 業績清零
    PlayerDeath       // Game Over
}

// ----- 卷軸修飾規則 -----

public enum ScrollModifierType
{
    DirectAdd,    // 效果直接疊加
    MultiplyAll,  // 所有效果 ×N
    PositiveOnly, // 只計算正值效果
    TagMultiply,  // 特定標籤效果 ×N
    Average,      // 所有效果取平均（保留小數）
    Invert        // 正負反轉
}

// ----- 遊戲模式 -----

public enum GameMode
{
    Level,   // 關卡模式：固定顧客隊列，清空通關
    Endless  // 無限模式：顧客無限生成，撐到 Game Over
}

// ----- 獎勵類型 -----

public enum RewardType
{
    RandomRune,   // 隨機符文（依稀有度權重）
    RandomScroll, // 隨機卷軸
    Relic,        // 遺物（玩家可選擇接受或拒絕）
    Score         // 業績加成
}

// ============================================================
// 共用資料結構
// ============================================================

/// <summary>
/// 單一環境效果條目，例如「水分 +2」或「溫度 -1」
/// </summary>
[Serializable]
public struct RuneEffect
{
    public EnvAttribute attribute; // 影響的環境屬性
    public int value;              // 變化量（正=增加，負=減少）
}

/// <summary>
/// 兩個環境快照之間的差異，用於相對變化條件判定
/// </summary>
[Serializable]
public struct EnvironmentDelta
{
    public int brightnessDelta;
    public int moistureDelta;
    public int temperatureDelta;

    public int GetDelta(EnvAttribute attr)
    {
        switch (attr)
        {
            case EnvAttribute.Brightness:   return brightnessDelta;
            case EnvAttribute.Moisture:     return moistureDelta;
            case EnvAttribute.Temperature:  return temperatureDelta;
            default:                        return 0;
        }
    }

    public override string ToString()
    {
        return $"Brightness:{brightnessDelta:+0;-0;0} " +
               $"Moisture:{moistureDelta:+0;-0;0} " +
               $"Temperature:{temperatureDelta:+0;-0;0}";
    }
}

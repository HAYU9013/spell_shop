# Spell Shop — 程式架構設計文件

---

## 1. 技術環境

| 項目       | 內容                    |
| ---------- | ----------------------- |
| 引擎       | Unity (C#)              |
| 動畫套件   | DOTween                 |
| 架構模式   | MVC + ScriptableObject  |
| 資料序列化 | ScriptableObject + JSON |

---

## 2. 檔案結構

```
Assets/
├── Scripts/
│   ├── Core/                        # 遊戲核心系統
│   │   ├── GameManager.cs           # 遊戲主管理器（Singleton，協調所有子系統）
│   │   ├── TurnManager.cs           # 回合流程管理（八階段狀態機）
│   │   ├── LevelManager.cs          # 關卡管理（顧客隊列、通關判定）
│   │   ├── GameState.cs             # 遊戲狀態資料容器
│   │   └── GameEventChannel.cs      # 全域事件頻道（觀察者模式）
│   │
│   ├── Environment/                 # 環境系統
│   │   ├── EnvironmentManager.cs    # 環境數值管理（持續保留，不重置）
│   │   ├── EnvironmentData.cs       # 環境數值資料結構
│   │   └── ExtremeEventHandler.cs   # 極端事件處理器（一次性觸發）
│   │
│   ├── Rune/                        # 符文系統
│   │   ├── RuneData.cs              # 符文 ScriptableObject 定義
│   │   ├── RuneInstance.cs          # 符文運行時實例
│   │   ├── RuneEffect.cs           # 符文效果資料結構
│   │   ├── SpecialEffect/           # 符文特殊效果子系統
│   │   │   ├── ISpecialEffect.cs    # 特殊效果介面
│   │   │   └── RemoveRelicEffect.cs # 移除遺物效果（驅逐石）
│   │   └── RuneDatabase.cs         # 符文資料庫 ScriptableObject
│   │
│   ├── Scroll/                      # 卷軸系統
│   │   ├── ScrollData.cs            # 卷軸 ScriptableObject 定義
│   │   ├── ScrollInstance.cs        # 卷軸運行時實例
│   │   ├── ScrollModifier.cs        # 卷軸修飾規則（策略模式）
│   │   ├── ScrollProcessor.cs       # 卷軸結算處理器（含標籤連鎖計算）
│   │   └── TagChainCalculator.cs    # 標籤連鎖加成計算器
│   │
│   ├── Customer/                    # 顧客系統
│   │   ├── CustomerData.cs          # 顧客 ScriptableObject 定義
│   │   ├── CustomerInstance.cs      # 顧客運行時實例（追蹤到來時環境快照）
│   │   ├── CustomerManager.cs       # 顧客管理（單一顧客服務模式）
│   │   ├── Requirements/            # 顧客需求子系統
│   │   │   ├── IRequirement.cs      # 需求介面
│   │   │   ├── AbsoluteRequirement.cs       # 絕對值條件
│   │   │   ├── RelativeChangeRequirement.cs # 相對變化條件（基於顧客到來時快照）
│   │   │   ├── CompoundRequirement.cs       # 複合條件
│   │   │   ├── StabilityRequirement.cs      # 穩定條件
│   │   │   └── TagPreferenceRequirement.cs  # 標籤偏好條件
│   │   └── CustomerPool.cs          # 顧客池（權重隨機抽取）
│   │
│   ├── Relic/                       # 遺物系統
│   │   ├── RelicData.cs             # 遺物 ScriptableObject 定義（含標籤偏好）
│   │   ├── RelicInstance.cs         # 遺物運行時實例（不滿意計數達上限後不重置）
│   │   ├── RelicManager.cs          # 遺物管理器（支援排序、動態增減）
│   │   └── RelicPool.cs             # 遺物池
│   │
│   ├── Deck/                        # 牌庫系統
│   │   ├── DeckManager.cs           # 牌庫管理器（抽牌、棄牌、洗牌、消耗型移除）
│   │   ├── HandManager.cs           # 手牌管理器（每回合全棄重抽，無上限）
│   │   └── ScrollInventory.cs       # 卷軸庫存管理（消耗品）
│   │
│   ├── Wholesaler/                  # 批發商系統
│   │   ├── WholesalerManager.cs     # 批發商管理器
│   │   ├── WholesalerData.cs        # 批發商商品資料
│   │   └── WholesalerUI.cs          # 批發商介面（關卡間展示）
│   │
│   ├── Reward/                      # 獎勵系統
│   │   ├── RewardGenerator.cs       # 獎勵隨機生成器
│   │   ├── RewardEntry.cs           # 獎勵條目定義
│   │   └── RewardPool.cs            # 獎勵池（依稀有度權重）
│   │
│   ├── UI/                          # UI 系統
│   │   ├── Panels/
│   │   │   ├── EnvironmentPanel.cs  # 環境數值顯示面板
│   │   │   ├── CustomerPanel.cs     # 顧客資訊面板（含需求追蹤進度）
│   │   │   ├── RelicPanel.cs        # 遺物狀態面板（支援拖曳排序）
│   │   │   ├── WorkbenchPanel.cs    # 卷軸工作台面板（含效果預覽與標籤連鎖提示）
│   │   │   ├── HandPanel.cs         # 手牌區面板
│   │   │   ├── ScorePanel.cs        # 業績面板
│   │   │   ├── WholesalerPanel.cs   # 批發商面板
│   │   │   └── GameOverPanel.cs     # 遊戲結束面板
│   │   ├── Components/
│   │   │   ├── RuneCardUI.cs        # 符文卡牌 UI 元件（顯示類型與標籤）
│   │   │   ├── ScrollSlotUI.cs      # 卷軸槽位 UI 元件
│   │   │   ├── RelicIconUI.cs       # 遺物圖示 UI 元件（含不滿意計數進度條）
│   │   │   ├── CustomerPortraitUI.cs # 顧客頭像 UI 元件
│   │   │   ├── RequirementTrackerUI.cs # 需求追蹤進度 UI（穩定條件、累積條件）
│   │   │   └── TooltipUI.cs         # 通用提示框
│   │   ├── DragDrop/
│   │   │   ├── DraggableRune.cs     # 符文拖曳行為
│   │   │   ├── DraggableRelic.cs    # 遺物拖曳排序行為
│   │   │   └── DropZone.cs          # 放置區域行為
│   │   └── UIManager.cs             # UI 總管理器
│   │
│   ├── Animation/                   # 動畫演出
│   │   ├── CardAnimator.cs          # 卡牌動畫控制
│   │   ├── EnvironmentVFX.cs        # 環境特效（極端事件演出）
│   │   └── TurnTransition.cs        # 回合轉場動畫
│   │
│   └── Utils/                       # 工具類
│       ├── WeightedRandom.cs        # 加權隨機工具
│       ├── ObjectPool.cs            # 通用物件池
│       └── Constants.cs             # 全域常數定義
│
├── Data/                            # ScriptableObject 資料資產
│   ├── Runes/                       # 符文資料檔
│   ├── Scrolls/                     # 卷軸資料檔
│   ├── Customers/                   # 顧客資料檔
│   ├── Relics/                      # 遺物資料檔
│   └── Config/
│       ├── GameConfig.asset         # 遊戲全域設定
│       └── LevelConfig.asset        # 關卡難度設定
│
├── Prefabs/                         # 預製物
│   ├── UI/
│   ├── Cards/
│   └── Effects/
│
├── Plugins/
│   └── Demigiant/DOTween/           # DOTween 套件
│
└── Scenes/
    ├── MainMenu.unity
    ├── GameScene.unity
    └── WholesalerScene.unity        # 批發商場景（或作為 GameScene 的子狀態）
```

---

## 3. 核心資料結構設計

### 3.1 列舉定義

```csharp
// 環境屬性類型
public enum EnvAttribute
{
    Brightness, // 亮度
    Moisture,   // 水分
    Temperature // 溫度
}

// 稀有度
public enum Rarity
{
    Common,    // 普通
    Rare,      // 稀有
    Legendary  // 傳說
}

// 符文類型
public enum RuneType
{
    Cycle,    // 循環型：使用後進棄牌堆，牌庫抽完洗牌重來
    Consumable // 消耗型：使用後永久移除
}

// 符文標籤（使用 Flags 支援多標籤）
[Flags]
public enum RuneTag
{
    None    = 0,
    Water   = 1 << 0,  // 水系：水分↑、溫度↓
    Fire    = 1 << 1,  // 火系：溫度↑、水分↓
    Light   = 1 << 2,  // 光系：亮度↑
    Dark    = 1 << 3,  // 暗系：亮度↓
    Wind    = 1 << 4,  // 風系：多項數值小幅調整
    Neutral = 1 << 5   // 中性：複合效果，無標籤加成
}

// 遊戲階段（回合內八階段）
public enum TurnPhase
{
    CustomerArrival,   // 1. 若無等待顧客 → 新顧客到來
    RelicTrigger,      // 2. 遺物效果結算
    DrawCards,         // 3. 棄掉上回手牌，重新抽取
    PlayerAction,      // 4. 玩家行動（選一個卷軸送出）
    EnvironmentResolve,// 5. 環境數值結算 + 極端事件檢查
    CustomerJudge,     // 6. 顧客需求判定
    PatienceCheck,     // 7. 顧客耐心判定
    RelicUpdate        // 8. 遺物不滿意計數更新
}

// 比較運算子（用於需求條件）
public enum CompareOperator
{
    GreaterEqual,   // >=
    LessEqual,      // <=
    Equal,          // ==
    InRange         // 介於兩值之間
}

// 卷軸修飾類型
public enum ScrollModifierType
{
    DirectAdd,       // 效果直接疊加
    MultiplyAll,     // 所有效果 ×N
    PositiveOnly,    // 只計算正值效果
    TagMultiply,     // 特定標籤效果 ×N
    Average,         // 所有效果取平均（保留小數）
    Invert           // 正負反轉
}

// 極端事件類型（一次性觸發）
public enum ExtremeEvent
{
    None,
    Darkness,   // 黑暗降臨（亮度 = 0）：所有遺物不滿意計數 +1
    Flood,      // 大洪水（水分 = 20）：水分 +5
    Overheat,   // 高溫爆發（溫度 = 20）：業績 -10
    Overcold    // 極寒侵襲（溫度 = 0）：業績 -10
}

// 顧客需求類型
public enum RequirementType
{
    Absolute,       // 絕對值條件：環境值達到指定數值
    RelativeChange, // 相對變化條件：自顧客到來後累積變化量達標
    Stability,      // 環境穩定條件：條件連續 N 回合成立
    TagPreference   // 標籤偏好條件：送出卷軸中指定標籤符文達數量
}

// 遺物懲罰類型
public enum RelicPunishment
{
    EnvironmentShock,  // 環境劇烈波動（如水分強制設為 10）
    ScoreReset,        // 業績清零
    PlayerDeath        // Game Over
}

// 獎勵類型
public enum RewardType
{
    RandomRune,     // 隨機符文（依稀有度權重）
    RandomScroll,   // 隨機卷軸
    Relic,          // 遺物（玩家可選擇接受或拒絕）
    Score           // 業績加成
}

// 批發商商品類型
public enum WholesalerItemType
{
    CommonRune,     // 普通符文（3 選 1）
    RareRune,       // 稀有符文（2 選 1）
    LegendaryRune,  // 傳說符文（1 張）
    BasicScroll,    // 基礎卷軸
    AdvancedScroll, // 進階卷軸（2 選 1）
    Relic,          // 遺物（可選擇接受或拒絕）
    RemoveRune      // 移除牌庫中一張符文
}
```

### 3.2 環境資料

```csharp
/// <summary>
/// 環境數值資料結構，保存三項環境屬性與業績。
/// 環境數值在回合之間持續保留，不會重置。
/// </summary>
[System.Serializable]
public class EnvironmentData
{
    public const int MIN_VALUE = 0;
    public const int MAX_VALUE = 20;
    public const int INITIAL_BRIGHTNESS = 5;
    public const int INITIAL_MOISTURE = 5;
    public const int INITIAL_TEMPERATURE = 5;
    public const int INITIAL_SCORE = 30;

    [Range(MIN_VALUE, MAX_VALUE)] public int brightness;
    [Range(MIN_VALUE, MAX_VALUE)] public int moisture;
    [Range(MIN_VALUE, MAX_VALUE)] public int temperature;
    public int score; // 降至 0 → Game Over；同時作為批發商貨幣

    /// <summary>取得指定屬性的數值</summary>
    public int GetValue(EnvAttribute attr);

    /// <summary>設定指定屬性的數值（自動 Clamp 在 0~20）</summary>
    public void SetValue(EnvAttribute attr, int value);

    /// <summary>對指定屬性增減數值，回傳是否觸碰邊界（用於極端事件檢查）</summary>
    public bool ApplyDelta(EnvAttribute attr, int delta);

    /// <summary>建立深拷貝（用於預覽、快照）</summary>
    public EnvironmentData Clone();

    /// <summary>計算兩個環境狀態之間的差異</summary>
    public static EnvironmentDelta Diff(EnvironmentData from, EnvironmentData to);
}

/// <summary>
/// 環境數值差異，用於相對變化條件判定
/// </summary>
[System.Serializable]
public struct EnvironmentDelta
{
    public int brightnessDelta;
    public int moistureDelta;
    public int temperatureDelta;

    public int GetDelta(EnvAttribute attr);
}
```

### 3.3 符文效果

```csharp
/// <summary>
/// 單一環境效果條目，例如「水分 +2」
/// </summary>
[System.Serializable]
public struct RuneEffect
{
    public EnvAttribute attribute; // 影響的環境屬性
    public int value;              // 數值變化量（正=增加，負=減少）
}
```

### 3.4 符文特殊效果介面

```csharp
/// <summary>
/// 符文特殊效果介面（如驅逐石的「移除一個遺物」）
/// 與普通的環境數值效果分開處理
/// </summary>
public interface ISpecialEffect
{
    /// <summary>執行特殊效果</summary>
    void Execute(GameState state);

    /// <summary>取得效果描述文字</summary>
    string GetDescription();
}

/// <summary>
/// 移除遺物效果（驅逐石使用）
/// 執行時讓玩家選擇一個遺物移除
/// </summary>
public class RemoveRelicEffect : ISpecialEffect
{
    public void Execute(GameState state);
    public string GetDescription() => "移除一個遺物（玩家選擇）";
}
```

### 3.5 符文資料（ScriptableObject）

```csharp
/// <summary>
/// 符文的靜態資料定義
/// </summary>
[CreateAssetMenu(fileName = "NewRune", menuName = "SpellShop/Rune")]
public class RuneData : ScriptableObject
{
    public string runeName;
    public Sprite icon;
    [TextArea] public string description;
    public RuneType runeType;              // 循環型 or 消耗型
    public Rarity rarity;
    public RuneTag tags;                   // 支援多標籤（Flags）
    public List<RuneEffect> effects;       // 環境數值效果列表

    [Header("特殊效果（可選）")]
    public bool hasSpecialEffect;
    public SpecialEffectType specialEffectType; // 序列化用的列舉
}

public enum SpecialEffectType
{
    None,
    RemoveRelic  // 移除一個遺物
}
```

### 3.6 符文運行時實例

```csharp
/// <summary>
/// 符文的運行時包裝，追蹤該張符文在牌庫/手牌/棄牌堆中的狀態
/// </summary>
public class RuneInstance
{
    public RuneData Data { get; private set; }
    public string UniqueId { get; private set; }

    /// <summary>是否為消耗型（使用後永久移除）</summary>
    public bool IsConsumable => Data.runeType == RuneType.Consumable;

    public RuneInstance(RuneData data)
    {
        Data = data;
        UniqueId = System.Guid.NewGuid().ToString();
    }
}
```

### 3.7 卷軸資料（ScriptableObject）

```csharp
/// <summary>
/// 卷軸的靜態資料定義。卷軸為消耗品，使用後永久移除。
/// </summary>
[CreateAssetMenu(fileName = "NewScroll", menuName = "SpellShop/Scroll")]
public class ScrollData : ScriptableObject
{
    public string scrollName;
    public Sprite icon;
    [TextArea] public string description;
    public Rarity rarity;
    public int slotCount;                        // 符文槽數量（必須放滿才能送出）
    public ScrollModifierType modifierType;      // 修飾規則類型
    public float modifierValue;                  // 修飾參數（如倍率 2.0f）
    public RuneTag tagRestriction;               // 標籤限制（None = 無限制）
    public RuneTag tagMultiplyTarget;            // TagMultiply 模式下的目標標籤
}
```

### 3.8 卷軸運行時實例

```csharp
/// <summary>
/// 卷軸的運行時實例，追蹤槽位中已放入的符文。
/// 一個回合只能送出一個卷軸。
/// </summary>
public class ScrollInstance
{
    public ScrollData Data { get; private set; }
    public RuneInstance[] Slots { get; private set; }
    public string UniqueId { get; private set; }

    public ScrollInstance(ScrollData data);

    /// <summary>是否所有槽位都已放入符文</summary>
    public bool IsFull { get; }

    /// <summary>嘗試將符文放入下一個空槽位（檢查標籤限制）</summary>
    public bool TryPlaceRune(RuneInstance rune);

    /// <summary>從指定槽位移除符文</summary>
    public RuneInstance RemoveRuneAt(int slotIndex);

    /// <summary>檢查符文是否符合卷軸的標籤限制</summary>
    public bool CanAcceptRune(RuneInstance rune);

    /// <summary>清空所有槽位</summary>
    public void ClearSlots();

    /// <summary>取得槽位中各標籤的出現次數（用於連鎖計算）</summary>
    public Dictionary<RuneTag, int> GetTagCounts();
}
```

### 3.9 顧客資料（ScriptableObject）

```csharp
/// <summary>
/// 顧客的靜態資料定義。
/// 同一時間只有一位顧客，舊顧客離開後新顧客立即登場。
/// </summary>
[CreateAssetMenu(fileName = "NewCustomer", menuName = "SpellShop/Customer")]
public class CustomerData : ScriptableObject
{
    public string customerName;
    public Sprite portrait;
    [TextArea] public string dialogue;

    [Header("需求")]
    public List<RequirementData> requirements;     // 需求條件列表（AND 邏輯）

    [Header("耐心")]
    public int maxPatience;                        // 最大等待回合數

    [Header("獎勵")]
    public RewardPoolConfig rewardPool;            // 隨機獎勵池設定
    public int scoreReward;                        // 業績加成
    public int scorePenalty;                       // 憤怒離開時的業績扣除

    [Header("難度標記")]
    public int minDifficulty;                      // 最低出現難度（用於難度曲線）
}
```

### 3.10 需求條件資料

```csharp
/// <summary>
/// 顧客需求條件的序列化資料。
/// 相對變化條件的計算基準為「顧客到來時的環境數值」。
/// </summary>
[System.Serializable]
public class RequirementData
{
    public RequirementType type;
    public EnvAttribute attribute;         // 目標屬性
    public CompareOperator compareOp;      // 比較方式
    public int targetValue;                // 目標值
    public int targetValueMax;             // InRange 上界
    public int requiredDelta;              // 相對變化條件：累積變化量
    public int stabilityTurns;             // 穩定條件：需持續的回合數

    [Header("標籤偏好條件")]
    public RuneTag requiredTag;            // 要求的標籤
    public int requiredTagCount;           // 要求的數量（如「至少 2 張水系」）
}

/// <summary>
/// 獎勵池設定，定義顧客獎勵的隨機規則
/// </summary>
[System.Serializable]
public class RewardPoolConfig
{
    public List<RewardPoolEntry> entries;   // 可能的獎勵條目
}

[System.Serializable]
public class RewardPoolEntry
{
    public RewardType type;
    public Rarity maxRarity;               // 隨機符文/卷軸的最高稀有度
    public int count;                      // 數量
    public float weight;                   // 權重（用於隨機抽取）
    public RelicData specificRelic;         // 若為遺物獎勵，指定遺物（null 則隨機）
}
```

### 3.11 獎勵條目

```csharp
/// <summary>
/// 獎勵生成結果，已確定的具體獎勵內容
/// </summary>
public class RewardResult
{
    public RewardType type;
    public RuneData runeReward;            // 具體符文
    public ScrollData scrollReward;        // 具體卷軸
    public RelicData relicReward;          // 具體遺物（玩家可選擇接受或拒絕）
    public int scoreAmount;                // 業績數量
}
```

### 3.12 顧客運行時實例

```csharp
/// <summary>
/// 顧客的運行時實例。
/// 追蹤到來時的環境快照（用於相對變化條件）、耐心、穩定條件進度。
/// </summary>
public class CustomerInstance
{
    public CustomerData Data { get; private set; }
    public int CurrentPatience { get; private set; }

    /// <summary>顧客到來時的環境快照（相對變化條件的基準）</summary>
    public EnvironmentData ArrivalSnapshot { get; private set; }

    /// <summary>穩定條件的連續滿足回合追蹤</summary>
    public Dictionary<int, int> StabilityProgress { get; private set; }

    /// <summary>相對變化條件的累積變化量追蹤</summary>
    public Dictionary<int, int> AccumulatedDeltas { get; private set; }

    public CustomerInstance(CustomerData data, EnvironmentData currentEnv);

    /// <summary>
    /// 檢查需求是否滿足。
    /// 標籤偏好條件需要傳入本回合送出的卷軸資訊。
    /// </summary>
    public bool CheckRequirements(EnvironmentData currentEnv, ScrollInstance submittedScroll);

    /// <summary>更新累積變化量（每回合環境結算後呼叫）</summary>
    public void UpdateAccumulatedDeltas(EnvironmentData currentEnv);

    /// <summary>更新穩定條件進度</summary>
    public void UpdateStabilityProgress(EnvironmentData currentEnv);

    /// <summary>耐心遞減，回傳是否仍有耐心</summary>
    public bool TickPatience();

    public bool IsOutOfPatience => CurrentPatience <= 0;
}
```

### 3.13 遺物資料（ScriptableObject）

```csharp
/// <summary>
/// 遺物的靜態資料定義。
/// 遺物透過顧客獎勵或批發商取得，只能透過特殊符文/卷軸移除。
/// 玩家取得遺物時可選擇接受或拒絕。
/// </summary>
[CreateAssetMenu(fileName = "NewRelic", menuName = "SpellShop/Relic")]
public class RelicData : ScriptableObject
{
    public string relicName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("滿意條件")]
    public List<RequirementData> satisfactionConditions;

    [Header("標籤偏好效果（可選）")]
    public bool hasTagPreference;
    public RuneTag preferredTag;                      // 偏好的標籤
    public List<RuneEffect> tagPreferenceEffects;     // 使用該標籤時的額外效果
    public int tagPreferenceScoreBonus;               // 使用該標籤時的額外業績

    [Header("滿意效果")]
    public List<RuneEffect> satisfiedEffects;         // 每回合正向效果
    public int satisfiedScoreBonus;                   // 每回合業績加成
    public bool satisfiedDrawExtraRune;               // 是否額外抽牌

    [Header("不滿意效果")]
    public List<RuneEffect> unsatisfiedEffects;       // 每回合副作用

    [Header("懲罰機制")]
    public int maxUnsatisfiedTurns;                   // 不滿意容忍上限
    public RelicPunishment punishment;                // 達上限後的懲罰類型
    public List<RuneEffect> punishmentEffects;        // 懲罰效果（如水分強制設為 10）
}
```

### 3.14 遺物運行時實例

```csharp
/// <summary>
/// 遺物的運行時實例。
/// 不滿意計數規則：
///   - 不滿意 → 計數 +1
///   - 滿意 → 計數立刻歸零
///   - 計數達上限 → 觸發懲罰，計數不重置，後續每回合繼續懲罰直到再次滿意
/// </summary>
public class RelicInstance
{
    public RelicData Data { get; private set; }
    public int UnsatisfiedCount { get; private set; }
    public bool IsSatisfied { get; private set; }
    public int DisplayOrder { get; set; } // 玩家排列的順序（影響結算順序）

    public RelicInstance(RelicData data);

    /// <summary>根據當前環境判斷滿意狀態並回傳效果</summary>
    public RelicTickResult Tick(EnvironmentData env);

    /// <summary>處理標籤偏好觸發（玩家送出卷軸時呼叫）</summary>
    public RelicTagResult CheckTagPreference(ScrollInstance scroll);

    /// <summary>是否已達到或超過懲罰觸發門檻</summary>
    public bool IsPunishmentActive => UnsatisfiedCount >= Data.maxUnsatisfiedTurns;
}

/// <summary>遺物每回合結算結果</summary>
public struct RelicTickResult
{
    public bool isSatisfied;
    public List<RuneEffect> effectsToApply;       // 滿意或不滿意效果
    public int scoreChange;
    public bool drawExtraRune;
    public bool punishmentTriggered;              // 本回合是否觸發懲罰
    public RelicPunishment punishmentType;
    public List<RuneEffect> punishmentEffects;
}

/// <summary>遺物標籤偏好觸發結果</summary>
public struct RelicTagResult
{
    public bool triggered;                        // 是否有觸發
    public List<RuneEffect> bonusEffects;          // 額外效果
    public int bonusScore;                         // 額外業績
}
```

### 3.15 遊戲狀態

```csharp
/// <summary>
/// 遊戲全局狀態容器
/// </summary>
public class GameState
{
    public EnvironmentData Environment { get; set; }
    public EnvironmentData PreviousEnvironment { get; set; }    // 上回合環境快照
    public int CurrentTurn { get; set; }                        // 當前回合數
    public int CurrentLevel { get; set; }                       // 當前關卡
    public TurnPhase CurrentPhase { get; set; }
    public bool IsGameOver { get; set; }
    public string GameOverReason { get; set; }

    // 關卡相關
    public List<CustomerData> CustomerQueue { get; set; }       // 當前關卡的顧客隊列
    public int CustomersRemaining => CustomerQueue?.Count ?? 0;

    // 歷史紀錄
    public List<EnvironmentData> EnvironmentHistory { get; set; } // 環境歷史（穩定條件用）
}
```

### 3.16 批發商資料

```csharp
/// <summary>
/// 批發商商品條目
/// </summary>
[System.Serializable]
public class WholesalerItem
{
    public WholesalerItemType type;
    public int cost;                        // 業績成本

    // 隨機生成的具體選項（遊戲運行時填入）
    public List<RuneData> runeChoices;      // 符文選項
    public List<ScrollData> scrollChoices;  // 卷軸選項
    public List<RelicData> relicChoices;    // 遺物選項
    public bool isPurchased;                // 是否已購買
}

/// <summary>
/// 批發商商品價格設定
/// </summary>
[CreateAssetMenu(fileName = "WholesalerConfig", menuName = "SpellShop/WholesalerConfig")]
public class WholesalerConfig : ScriptableObject
{
    public int commonRuneCost = 5;
    public int rareRuneCost = 15;
    public int legendaryRuneCost = 35;
    public int basicScrollCost = 8;
    public int advancedScrollCost = 20;
    public int relicCost = 25;
    public int removeRuneCost = 10;
}
```

---

## 4. 繼承與介面設計

### 4.1 顧客需求介面（策略模式）

```csharp
/// <summary>
/// 顧客需求條件的判定介面
/// </summary>
public interface IRequirement
{
    /// <summary>檢查需求是否滿足</summary>
    /// <param name="context">需求判定的上下文資料</param>
    bool IsSatisfied(RequirementContext context);

    /// <summary>取得需求的文字描述（顯示於 UI）</summary>
    string GetDescription();

    /// <summary>取得當前進度描述（穩定條件、累積條件用）</summary>
    string GetProgressText(RequirementContext context);
}

/// <summary>
/// 需求判定上下文，封裝所有判定所需資料
/// </summary>
public struct RequirementContext
{
    public EnvironmentData currentEnv;              // 當前環境
    public EnvironmentData arrivalSnapshot;          // 顧客到來時的環境快照
    public List<EnvironmentData> envHistory;         // 環境歷史紀錄
    public ScrollInstance submittedScroll;            // 本回合送出的卷軸
    public Dictionary<int, int> accumulatedDeltas;   // 累積變化量
    public Dictionary<int, int> stabilityProgress;   // 穩定條件進度
}

/// <summary>絕對值條件：如 溫度 >= 8</summary>
public class AbsoluteRequirement : IRequirement
{
    private EnvAttribute _attribute;
    private CompareOperator _op;
    private int _target;
    private int _targetMax; // InRange 用

    public AbsoluteRequirement(RequirementData data);
    public bool IsSatisfied(RequirementContext context);
    public string GetDescription();
    public string GetProgressText(RequirementContext context);
}

/// <summary>
/// 相對變化條件：如「顧客來到後水分累積減少 >= 3」。
/// 基準為顧客到來時的環境快照，每回合累積計算。
/// </summary>
public class RelativeChangeRequirement : IRequirement
{
    private EnvAttribute _attribute;
    private int _requiredDelta; // 正值表示累積增加，負值表示累積減少

    public RelativeChangeRequirement(RequirementData data);
    public bool IsSatisfied(RequirementContext context);
    public string GetDescription();
    public string GetProgressText(RequirementContext context);
}

/// <summary>穩定條件：如 連續 1 回合溫度維持在 4~6 之間</summary>
public class StabilityRequirement : IRequirement
{
    private EnvAttribute _attribute;
    private int _min, _max;
    private int _requiredTurns;

    public StabilityRequirement(RequirementData data);
    public bool IsSatisfied(RequirementContext context);
    public string GetDescription();
    public string GetProgressText(RequirementContext context); // 顯示「連續1回合：0/1」
}

/// <summary>
/// 標籤偏好條件：如「卷軸中至少 2 張水系符文」。
/// 檢查本回合送出的卷軸內符文標籤。
/// </summary>
public class TagPreferenceRequirement : IRequirement
{
    private RuneTag _requiredTag;
    private int _requiredCount;

    public TagPreferenceRequirement(RequirementData data);
    public bool IsSatisfied(RequirementContext context);
    public string GetDescription();
    public string GetProgressText(RequirementContext context);
}

/// <summary>複合條件：多個需求的 AND 組合</summary>
public class CompoundRequirement : IRequirement
{
    private List<IRequirement> _subRequirements;

    public CompoundRequirement(List<RequirementData> dataList);
    public bool IsSatisfied(RequirementContext context);
    public string GetDescription();
    public string GetProgressText(RequirementContext context);
}
```

### 4.2 需求工廠

```csharp
public static class RequirementFactory
{
    public static IRequirement Create(RequirementData data)
    {
        return data.type switch
        {
            RequirementType.Absolute       => new AbsoluteRequirement(data),
            RequirementType.RelativeChange => new RelativeChangeRequirement(data),
            RequirementType.Stability      => new StabilityRequirement(data),
            RequirementType.TagPreference  => new TagPreferenceRequirement(data),
            _ => throw new System.ArgumentException($"Unknown requirement type: {data.type}")
        };
    }
}
```

### 4.3 卷軸修飾器（策略模式）

```csharp
/// <summary>
/// 卷軸修飾規則介面，負責將槽位中的符文效果轉換為最終環境變化量
/// </summary>
public interface IScrollModifier
{
    /// <summary>計算此卷軸的最終效果列表</summary>
    List<RuneEffect> Calculate(RuneInstance[] slots, ScrollData scrollData);

    /// <summary>取得修飾規則的文字描述</summary>
    string GetDescription();
}

/// <summary>直接疊加：所有符文效果直接相加</summary>
public class DirectAddModifier : IScrollModifier { ... }

/// <summary>全體倍率：所有效果乘以 N</summary>
public class MultiplyAllModifier : IScrollModifier { ... }

/// <summary>僅正值：過濾掉所有負數效果</summary>
public class PositiveOnlyModifier : IScrollModifier { ... }

/// <summary>標籤倍率：特定標籤的符文效果乘以 N</summary>
public class TagMultiplyModifier : IScrollModifier { ... }

/// <summary>平均化：所有效果取平均後套用（保留小數，最終四捨五入）</summary>
public class AverageModifier : IScrollModifier { ... }

/// <summary>反轉：所有效果正負反轉（仍可觸發極端事件）</summary>
public class InvertModifier : IScrollModifier { ... }
```

### 4.4 修飾器工廠

```csharp
public static class ScrollModifierFactory
{
    public static IScrollModifier Create(ScrollModifierType type)
    {
        return type switch
        {
            ScrollModifierType.DirectAdd    => new DirectAddModifier(),
            ScrollModifierType.MultiplyAll  => new MultiplyAllModifier(),
            ScrollModifierType.PositiveOnly => new PositiveOnlyModifier(),
            ScrollModifierType.TagMultiply  => new TagMultiplyModifier(),
            ScrollModifierType.Average      => new AverageModifier(),
            ScrollModifierType.Invert       => new InvertModifier(),
            _ => throw new System.ArgumentException($"Unknown modifier type: {type}")
        };
    }
}
```

### 4.5 特殊效果工廠

```csharp
public static class SpecialEffectFactory
{
    public static ISpecialEffect Create(SpecialEffectType type)
    {
        return type switch
        {
            SpecialEffectType.RemoveRelic => new RemoveRelicEffect(),
            SpecialEffectType.None       => null,
            _ => null
        };
    }
}
```

---

## 5. 管理器設計（Manager 層）

### 5.1 GameManager — 遊戲主管理器

```csharp
/// <summary>
/// 遊戲主管理器，Singleton 入口，協調所有子系統
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 子系統引用
    [SerializeField] private GameConfig _config;
    private TurnManager _turnManager;
    private LevelManager _levelManager;
    private EnvironmentManager _environmentManager;
    private DeckManager _deckManager;
    private HandManager _handManager;
    private ScrollInventory _scrollInventory;
    private CustomerManager _customerManager;
    private RelicManager _relicManager;
    private WholesalerManager _wholesalerManager;
    private UIManager _uiManager;
    private GameState _gameState;

    // 生命週期
    private void Awake();                    // Singleton 初始化
    public void StartNewGame();              // 開始新遊戲（初始牌庫 + 第一關）
    public void EndGame(string reason);      // 結束遊戲（業績歸零或遺物即死）

    /// <summary>檢查業績是否歸零，觸發 Game Over</summary>
    public void CheckGameOver();

    // 外部存取
    public GameState State => _gameState;
    public GameConfig Config => _config;
}
```

### 5.2 TurnManager — 回合流程管理

```csharp
/// <summary>
/// 管理回合的八個階段。
/// 一個回合只能送出一個卷軸。
/// 服務一位顧客可橫跨多個回合。
/// </summary>
public class TurnManager
{
    private GameState _state;
    private ScrollInstance _submittedScroll; // 本回合送出的卷軸

    // 事件
    public event System.Action<TurnPhase> OnPhaseChanged;
    public event System.Action<int> OnTurnStarted;
    public event System.Action<int> OnTurnEnded;

    /// <summary>開始新回合，依序執行八個階段</summary>
    public async void StartTurn();

    // 各階段方法
    private void Phase_CustomerArrival();     // 若無等待顧客 → 下一位顧客登場
    private void Phase_RelicTrigger();        // 遺物效果結算（依玩家排列順序）
    private void Phase_DrawCards();           // 棄掉上回所有手牌，重新抽取
    private void Phase_WaitPlayerAction();    // 等待玩家選卷軸 + 放符文 + 送出
    private void Phase_EnvironmentResolve();  // 環境結算 + 極端事件檢查
    private void Phase_CustomerJudge();       // 顧客需求判定（傳入卷軸資訊）
    private void Phase_PatienceCheck();       // 耐心判定
    private void Phase_RelicUpdate();         // 遺物不滿意計數更新

    /// <summary>玩家按下送出按鈕時呼叫</summary>
    public void OnPlayerSubmit(ScrollInstance submittedScroll);
}
```

### 5.3 LevelManager — 關卡管理

```csharp
/// <summary>
/// 管理關卡進程：顧客隊列生成、通關判定、批發商過渡
/// </summary>
public class LevelManager
{
    private int _currentLevel;
    private List<CustomerData> _customerQueue;
    private int _queueIndex;

    // 事件
    public event System.Action<int> OnLevelStarted;
    public event System.Action<int> OnLevelCleared; // 顧客隊列清空
    public event System.Action OnEnterWholesaler;

    /// <summary>開始新關卡（生成顧客隊列）</summary>
    public void StartLevel(int level);

    /// <summary>取得下一位顧客，若隊列為空則觸發通關</summary>
    public CustomerData GetNextCustomer();

    /// <summary>目前關卡是否還有顧客</summary>
    public bool HasMoreCustomers { get; }

    /// <summary>顧客離開後呼叫（無論滿足或憤怒）</summary>
    public void OnCustomerDeparted();

    /// <summary>進入下一關（批發商結束後呼叫）</summary>
    public void AdvanceToNextLevel();

    /// <summary>取得當前難度等級（影響顧客池篩選與隊列長度）</summary>
    public int CurrentDifficulty { get; }

    public int CurrentLevel => _currentLevel;
    public int CustomersRemaining => _customerQueue.Count - _queueIndex;
}
```

### 5.4 EnvironmentManager — 環境管理

```csharp
/// <summary>
/// 管理環境數值。環境數值在回合之間持續保留，不重置。
/// 極端事件為一次性觸發，該回合結算後即解除。
/// </summary>
public class EnvironmentManager
{
    private EnvironmentData _current;
    private EnvironmentData _previous;
    private List<EnvironmentData> _history;
    private ExtremeEventHandler _extremeHandler;

    // 事件
    public event System.Action<EnvAttribute, int, int> OnValueChanged; // 屬性, 舊值, 新值
    public event System.Action<int> OnScoreChanged;                    // 新業績值
    public event System.Action<ExtremeEvent> OnExtremeEventTriggered;   // 極端事件觸發

    /// <summary>套用一組效果到環境上</summary>
    public void ApplyEffects(List<RuneEffect> effects);

    /// <summary>修改業績</summary>
    public void ModifyScore(int delta);

    /// <summary>快照當前狀態到 previous，並加入歷史</summary>
    public void SnapshotCurrent();

    /// <summary>取得當前環境的唯讀拷貝</summary>
    public EnvironmentData GetCurrentSnapshot();

    /// <summary>取得歷史紀錄</summary>
    public List<EnvironmentData> GetHistory();

    /// <summary>
    /// 檢查極端事件（數值觸碰邊界 0 或 20 時觸發一次性事件）。
    /// 黑暗降臨：所有遺物不滿意計數 +1
    /// 大洪水：水分 +5
    /// 高溫爆發：業績 -10
    /// 極寒侵襲：業績 -10
    /// </summary>
    public List<ExtremeEvent> CheckAndTriggerExtremeEvents();

    /// <summary>檢查業績是否歸零</summary>
    public bool IsScoreZero => _current.score <= 0;

    public EnvironmentData Current => _current;
    public EnvironmentData Previous => _previous;
}
```

### 5.5 DeckManager — 牌庫管理

```csharp
/// <summary>
/// 管理符文牌庫。
/// 循環型符文：使用後進棄牌堆，牌庫抽完洗牌重來。
/// 消耗型符文：使用後永久移除。
/// </summary>
public class DeckManager
{
    private List<RuneInstance> _drawPile;      // 抽牌堆
    private List<RuneInstance> _discardPile;   // 棄牌堆
    private List<RuneInstance> _exhaustPile;   // 消耗堆（消耗型符文用完後放此）

    // 事件
    public event System.Action<int> OnDrawPileChanged;
    public event System.Action<int> OnDiscardPileChanged;

    /// <summary>初始化牌庫（水珠×3、小燭×3、蒸氣×2）</summary>
    public void Initialize(List<RuneData> startingRunes);

    /// <summary>從抽牌堆抽取指定數量的符文（不足時先洗牌）</summary>
    public List<RuneInstance> Draw(int count);

    /// <summary>將循環型符文放入棄牌堆</summary>
    public void Discard(RuneInstance rune);

    /// <summary>將消耗型符文放入消耗堆（永久移除）</summary>
    public void Exhaust(RuneInstance rune);

    /// <summary>將棄牌堆洗入抽牌堆（消耗型不會回來）</summary>
    public void Reshuffle();

    /// <summary>將新符文加入牌庫（顧客獎勵/批發商購買）</summary>
    public void AddToDeck(RuneData runeData);

    /// <summary>從牌庫中永久移除一張符文（批發商花費業績）</summary>
    public void RemoveFromDeck(RuneInstance rune);

    public int DrawPileCount { get; }
    public int DiscardPileCount { get; }
    public int ExhaustPileCount { get; }
}
```

### 5.6 HandManager — 手牌管理

```csharp
/// <summary>
/// 管理玩家手牌。每回合全棄重抽，無手牌上限。
/// </summary>
public class HandManager
{
    private List<RuneInstance> _hand;

    public event System.Action<RuneInstance> OnRuneAdded;
    public event System.Action<RuneInstance> OnRuneRemoved;
    public event System.Action OnHandCleared;

    /// <summary>加入手牌</summary>
    public void AddToHand(RuneInstance rune);

    /// <summary>從手牌移除（放入卷軸時呼叫）</summary>
    public void RemoveFromHand(RuneInstance rune);

    /// <summary>
    /// 回合開始時丟棄所有手牌。
    /// 循環型 → 棄牌堆；消耗型已使用 → 消耗堆（由呼叫方處理）
    /// </summary>
    public List<RuneInstance> DiscardAll();

    public IReadOnlyList<RuneInstance> Hand { get; }
    public int Count { get; }
}
```

### 5.7 ScrollInventory — 卷軸庫存

```csharp
/// <summary>
/// 管理玩家持有的卷軸（消耗品，使用後永久移除）。
/// 初始庫存：基礎卷軸 ×3。
/// 沒有卷軸可用時玩家無法行動。
/// </summary>
public class ScrollInventory
{
    private List<ScrollInstance> _scrolls;

    public event System.Action<ScrollInstance> OnScrollAdded;
    public event System.Action<ScrollInstance> OnScrollConsumed;
    public event System.Action OnScrollsEmpty; // 沒有卷軸可用

    /// <summary>初始化（基礎卷軸 ×3）</summary>
    public void Initialize(List<ScrollData> startingScrolls);

    /// <summary>加入新卷軸（顧客獎勵/批發商購買）</summary>
    public void AddScroll(ScrollData data);

    /// <summary>使用卷軸（從庫存移除）</summary>
    public void ConsumeScroll(ScrollInstance scroll);

    /// <summary>是否還有可用卷軸</summary>
    public bool HasScrolls => _scrolls.Count > 0;

    public IReadOnlyList<ScrollInstance> Scrolls { get; }
}
```

### 5.8 CustomerManager — 顧客管理

```csharp
/// <summary>
/// 管理顧客。同一時間只有一位顧客。
/// 舊顧客離開（滿足或憤怒）後新顧客立即登場。
/// </summary>
public class CustomerManager
{
    private CustomerInstance _currentCustomer;
    private LevelManager _levelManager;

    public event System.Action<CustomerInstance> OnCustomerArrived;
    public event System.Action<CustomerInstance, List<RewardResult>> OnCustomerSatisfied;
    public event System.Action<CustomerInstance, int> OnCustomerAngryLeft; // 業績扣除數

    /// <summary>迎接下一位顧客（若有）</summary>
    public CustomerInstance WelcomeNextCustomer(EnvironmentData currentEnv);

    /// <summary>
    /// 檢查當前顧客需求是否滿足。
    /// 需傳入卷軸資訊（標籤偏好條件需要）。
    /// </summary>
    public bool CheckCurrentCustomer(EnvironmentData env, ScrollInstance submittedScroll);

    /// <summary>更新顧客的累積追蹤資料（每回合環境結算後）</summary>
    public void UpdateCustomerTracking(EnvironmentData env);

    /// <summary>耐心遞減</summary>
    public void TickPatience();

    /// <summary>生成隨機獎勵</summary>
    public List<RewardResult> GenerateRewards();

    /// <summary>是否有當前顧客</summary>
    public bool HasCustomer => _currentCustomer != null;

    public CustomerInstance CurrentCustomer { get; }
}
```

### 5.9 RelicManager — 遺物管理

```csharp
/// <summary>
/// 管理所有遺物。
/// 遺物不在初始配置中，遊戲途中透過顧客或批發商取得。
/// 玩家可自由排列順序（影響結算順序）。
/// 只能透過特殊符文（驅逐石）或卷軸移除。
/// </summary>
public class RelicManager
{
    private List<RelicInstance> _relics; // 依 DisplayOrder 排序

    public event System.Action<RelicInstance, RelicTickResult> OnRelicTicked;
    public event System.Action<RelicInstance> OnRelicPunishment;
    public event System.Action<RelicInstance> OnRelicAdded;
    public event System.Action<RelicInstance> OnRelicRemoved;
    public event System.Action OnRelicOrderChanged;

    /// <summary>新增遺物（玩家接受後呼叫）</summary>
    public void AddRelic(RelicData data);

    /// <summary>移除遺物（驅逐石效果）</summary>
    public void RemoveRelic(RelicInstance relic);

    /// <summary>重新排列遺物順序（玩家拖曳後呼叫）</summary>
    public void ReorderRelics(List<RelicInstance> newOrder);

    /// <summary>觸發所有遺物的回合效果（依排列順序結算）</summary>
    public List<RelicTickResult> TickAll(EnvironmentData env);

    /// <summary>檢查所有遺物的標籤偏好觸發</summary>
    public List<RelicTagResult> CheckAllTagPreferences(ScrollInstance scroll);

    /// <summary>極端事件「黑暗降臨」：所有遺物不滿意計數 +1</summary>
    public void ForceIncrementAllUnsatisfied();

    public IReadOnlyList<RelicInstance> Relics { get; }
    public int Count => _relics.Count;
}
```

### 5.10 WholesalerManager — 批發商管理

```csharp
/// <summary>
/// 批發商系統，每關通關後出現。
/// 玩家消耗業績購買符文/卷軸/遺物，或花業績移除牌庫中的符文。
/// 商品隨機展示，不能重複刷新。
/// </summary>
public class WholesalerManager
{
    private WholesalerConfig _config;
    private List<WholesalerItem> _currentItems;

    public event System.Action<List<WholesalerItem>> OnShopOpened;
    public event System.Action<WholesalerItem> OnItemPurchased;
    public event System.Action OnShopClosed;

    /// <summary>生成隨機商品列表</summary>
    public void GenerateShop(int level);

    /// <summary>購買商品（扣除業績，發放物品）</summary>
    public bool TryPurchase(WholesalerItem item, int choiceIndex);

    /// <summary>檢查是否買得起</summary>
    public bool CanAfford(WholesalerItem item);

    /// <summary>關閉批發商，進入下一關</summary>
    public void CloseShop();

    public IReadOnlyList<WholesalerItem> CurrentItems { get; }
}
```

---

## 6. 卷軸結算處理器

```csharp
/// <summary>
/// 負責計算卷軸送出後的最終環境效果。
/// 流程：基礎效果 → 修飾規則 → 標籤連鎖加成 → 最終效果
/// </summary>
public class ScrollProcessor
{
    /// <summary>
    /// 計算卷軸的最終效果
    /// 1. 收集所有槽位中符文的效果
    /// 2. 根據卷軸的修飾規則計算
    /// 3. 計算標籤連鎖加成
    /// 4. 回傳合併後的效果列表
    /// </summary>
    public static List<RuneEffect> Process(ScrollInstance scroll)
    {
        // 修飾規則處理
        IScrollModifier modifier = ScrollModifierFactory.Create(scroll.Data.modifierType);
        var effects = modifier.Calculate(scroll.Slots, scroll.Data);

        // 標籤連鎖加成
        var chainBonus = TagChainCalculator.Calculate(scroll);
        effects.AddRange(chainBonus);

        return MergeEffects(effects);
    }

    /// <summary>預覽效果（不實際套用，UI 顯示 delta 用）</summary>
    public static EnvironmentData Preview(ScrollInstance scroll, EnvironmentData currentEnv)
    {
        var preview = currentEnv.Clone();
        var effects = Process(scroll);
        foreach (var effect in effects)
            preview.ApplyDelta(effect.attribute, effect.value);
        return preview;
    }

    /// <summary>合併同屬性的效果</summary>
    private static List<RuneEffect> MergeEffects(List<RuneEffect> effects);

    /// <summary>收集卷軸中所有消耗型符文（結算後需移除）</summary>
    public static List<RuneInstance> GetConsumableRunes(ScrollInstance scroll);

    /// <summary>收集卷軸中的特殊效果</summary>
    public static List<ISpecialEffect> GetSpecialEffects(ScrollInstance scroll);
}
```

### 6.1 標籤連鎖計算器

```csharp
/// <summary>
/// 計算同一卷軸中相同標籤的連鎖加成。
/// 2 張相同標籤 → 該標籤主要屬性效果合計 +1
/// 3 張相同標籤 → +2
/// 以此類推。
/// </summary>
public static class TagChainCalculator
{
    /// <summary>計算連鎖加成效果</summary>
    public static List<RuneEffect> Calculate(ScrollInstance scroll);

    /// <summary>取得標籤的主要影響屬性（用於加成）</summary>
    private static EnvAttribute GetPrimaryAttribute(RuneTag tag);

    /// <summary>取得連鎖加成文字描述（UI 顯示用）</summary>
    public static string GetChainDescription(ScrollInstance scroll);
}
```

---

## 7. 事件系統

```csharp
/// <summary>
/// 通用事件頻道（無參數），用於跨系統通訊
/// </summary>
[CreateAssetMenu(menuName = "SpellShop/Events/GameEventChannel")]
public class GameEventChannel : ScriptableObject
{
    private System.Action _listeners;

    public void Register(System.Action listener)   => _listeners += listener;
    public void Unregister(System.Action listener) => _listeners -= listener;
    public void Raise()                            => _listeners?.Invoke();
}

/// <summary>
/// 泛型事件頻道（帶參數）
/// </summary>
public abstract class GameEventChannel<T> : ScriptableObject
{
    private System.Action<T> _listeners;

    public void Register(System.Action<T> listener)   => _listeners += listener;
    public void Unregister(System.Action<T> listener) => _listeners -= listener;
    public void Raise(T value)                        => _listeners?.Invoke(value);
}

// 具體事件頻道
[CreateAssetMenu(menuName = "SpellShop/Events/IntEventChannel")]
public class IntEventChannel : GameEventChannel<int> { }

[CreateAssetMenu(menuName = "SpellShop/Events/TurnPhaseEventChannel")]
public class TurnPhaseEventChannel : GameEventChannel<TurnPhase> { }
```

---

## 8. 全域設定

```csharp
/// <summary>
/// 遊戲全域設定
/// </summary>
[CreateAssetMenu(menuName = "SpellShop/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("環境初始值")]
    public int initialBrightness = 5;
    public int initialMoisture = 5;
    public int initialTemperature = 5;
    public int initialScore = 30;             // 業績初始值，同時也是生命線

    [Header("手牌設定")]
    public int drawPerTurn = 5;               // 每回合抽牌數（無手牌上限）

    [Header("初始牌庫")]
    public List<RuneData> startingRunes;      // 水珠×3、小燭×3、蒸氣×2
    public List<ScrollData> startingScrolls;  // 基礎卷軸×3

    [Header("標籤連鎖")]
    public int chainBonusPerExtraTag = 1;     // 每多 1 張同標籤的額外加成值
}

/// <summary>
/// 關卡難度設定
/// </summary>
[CreateAssetMenu(menuName = "SpellShop/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    [Header("隊列長度")]
    public int baseQueueLength = 3;           // 第一關顧客數
    public int queueGrowthPerLevel = 1;       // 每關增加的顧客數

    [Header("難度遞增")]
    public float patienceReductionPerLevel = 0.1f; // 每關耐心縮減比例
    public int complexRequirementMinLevel = 3;     // 複合條件最早出現的關卡
    public int tagPreferenceMinLevel = 5;          // 標籤偏好條件最早出現的關卡
}
```

---

## 9. 核心流程時序圖

### 9.1 回合流程

```
StartTurn()
    │
    ├─ Phase 1: CustomerArrival
    │   └─ 若無當前顧客 → CustomerManager.WelcomeNextCustomer(env)
    │       ├─ 記錄 ArrivalSnapshot（相對變化基準）
    │       ├─ 若隊列為空 → LevelManager.OnLevelCleared() → 進入批發商
    │       └─ Event: OnCustomerArrived
    │
    ├─ Phase 2: RelicTrigger（依玩家排列順序結算）
    │   └─ RelicManager.TickAll(env)
    │       ├─ 每個 Relic（按 DisplayOrder）→ RelicInstance.Tick(env)
    │       │   ├─ 滿意 → 計數歸零，ApplyEffects(satisfiedEffects)
    │       │   └─ 不滿意 → 計數 +1，ApplyEffects(unsatisfiedEffects)
    │       │       └─ 計數 >= 上限 → 觸發懲罰（計數不重置，後續每回合繼續懲罰）
    │       └─ Event: OnRelicTicked / OnRelicPunishment
    │
    ├─ Phase 3: DrawCards
    │   ├─ HandManager.DiscardAll() → 所有手牌送回棄牌堆
    │   │   └─ 消耗型符文已使用的 → DeckManager.Exhaust()
    │   └─ DeckManager.Draw(config.drawPerTurn)
    │       └─ HandManager.AddToHand(runes)
    │
    ├─ Phase 4: PlayerAction [等待玩家操作]
    │   │
    │   │  玩家操作:
    │   │  1. ScrollInventory 選擇一個卷軸 → ScrollInstance
    │   │  2. 拖曳 RuneInstance → ScrollInstance.TryPlaceRune()
    │   │     └─ 檢查標籤限制
    │   │  3. ScrollProcessor.Preview() → UI 顯示預覽 + 標籤連鎖提示
    │   │  4. 按下「送出」 → TurnManager.OnPlayerSubmit()
    │   │
    │   └─ 等待 OnPlayerSubmit 信號
    │
    ├─ Phase 5: EnvironmentResolve
    │   ├─ EnvironmentManager.SnapshotCurrent()
    │   ├─ ScrollProcessor.Process(submittedScroll) → 最終效果
    │   ├─ EnvironmentManager.ApplyEffects(finalEffects)
    │   ├─ ScrollProcessor.GetConsumableRunes() → DeckManager.Exhaust()
    │   ├─ ScrollProcessor.GetSpecialEffects() → 執行特殊效果
    │   ├─ RelicManager.CheckAllTagPreferences(scroll) → 遺物標籤加成
    │   ├─ EnvironmentManager.CheckAndTriggerExtremeEvents()
    │   │   ├─ Darkness → RelicManager.ForceIncrementAllUnsatisfied()
    │   │   ├─ Flood → env.moisture += 5
    │   │   ├─ Overheat → env.score -= 10
    │   │   └─ Overcold → env.score -= 10
    │   ├─ ScrollInventory.ConsumeScroll(submittedScroll)
    │   └─ CustomerManager.UpdateCustomerTracking(env)
    │
    ├─ Phase 6: CustomerJudge
    │   └─ CustomerManager.CheckCurrentCustomer(env, submittedScroll)
    │       ├─ 滿足 → GenerateRewards() → Event: OnCustomerSatisfied
    │       │         └─ 獎勵含遺物 → 提示玩家接受/拒絕
    │       └─ 未滿足 → 進入 Phase 7
    │
    ├─ Phase 7: PatienceCheck
    │   └─ CustomerManager.TickPatience()
    │       ├─ 耐心 > 0 → 等待下一回合
    │       └─ 耐心 = 0 → 業績 -N → Event: OnCustomerAngryLeft
    │                       └─ 顧客離開 → 迎接下一位
    │
    └─ Phase 8: RelicUpdate
        └─ GameManager.CheckGameOver()
            ├─ 業績 <= 0 → EndGame("破產")
            └─ 遺物即死 → EndGame("遺物懲罰")
```

### 9.2 關卡間流程

```
顧客隊列清空
      │
      ├─ LevelManager.OnLevelCleared
      │
      ├─ 進入批發商
      │   ├─ WholesalerManager.GenerateShop(level)
      │   ├─ 玩家購買（消耗業績）
      │   │   ├─ 普通符文 3 選 1（5 業績）
      │   │   ├─ 稀有符文 2 選 1（15 業績）
      │   │   ├─ 傳說符文 1 張（35 業績）
      │   │   ├─ 基礎卷軸（8 業績）
      │   │   ├─ 進階卷軸 2 選 1（20 業績）
      │   │   ├─ 遺物 2 選 1（25 業績，可拒絕）
      │   │   └─ 移除符文（10 業績）
      │   └─ WholesalerManager.CloseShop()
      │
      └─ LevelManager.AdvanceToNextLevel()
          └─ 難度遞增：隊列更長、耐心更短、複合條件出現
```

---

## 10. UI 資料流

```
                   Model (Data)                     View (UI)
                   ───────────                      ─────────
EnvironmentManager ──OnValueChanged───────────────→ EnvironmentPanel
                    OnScoreChanged                   └─ 數字、進度條、極端事件提示
                    OnExtremeEventTriggered

DeckManager ────────OnDrawPileChanged─────────────→ ScorePanel
                    OnDiscardPileChanged              └─ 抽牌堆/棄牌堆/消耗堆數量

HandManager ────────OnRuneAdded───────────────────→ HandPanel
                    OnRuneRemoved                     └─ 生成/銷毀 RuneCardUI
                    OnHandCleared                          （顯示類型：循環/消耗、標籤圖示）

ScrollInventory ────OnScrollAdded─────────────────→ WorkbenchPanel
                    OnScrollConsumed                  ├─ 卷軸選擇下拉（含標籤限制提示）
                    OnScrollsEmpty                    ├─ 槽位 ScrollSlotUI
                                                      ├─ 效果預覽（delta 數值）
                                                      └─ 標籤連鎖加成提示

CustomerManager ────OnCustomerArrived─────────────→ CustomerPanel
                    OnCustomerSatisfied               ├─ 頭像、對話
                    OnCustomerAngryLeft               ├─ 需求文字 + RequirementTrackerUI
                                                      │   └─ 穩定進度「0/1」、累積進度「-2/-3」
                                                      └─ 耐心倒數

RelicManager ───────OnRelicTicked─────────────────→ RelicPanel
                    OnRelicPunishment                 ├─ 遺物圖示（可拖曳排序）
                    OnRelicAdded                      ├─ 滿意/不滿意狀態 ✓/✗
                    OnRelicRemoved                    ├─ 不滿意計數進度條 (2/4)
                    OnRelicOrderChanged               └─ 標籤偏好提示

LevelManager ───────OnLevelStarted────────────────→ ScorePanel
                    OnLevelCleared                    └─ 關卡數、剩餘顧客數

WholesalerManager ──OnShopOpened──────────────────→ WholesalerPanel
                    OnItemPurchased                   ├─ 商品列表（含價格）
                    OnShopClosed                      ├─ 選擇介面（N 選 1）
                                                      └─ 遺物接受/拒絕按鈕
```

---

## 11. 設計模式總覽

| 設計模式                | 應用位置                                        | 目的                                             |
| ----------------------- | ----------------------------------------------- | ------------------------------------------------ |
| Singleton               | GameManager                                     | 唯一入口，協調全部子系統                         |
| Strategy (策略)         | IRequirement, IScrollModifier, ISpecialEffect   | 需求判定、卷軸修飾、特殊效果封裝為可替換策略     |
| Factory (工廠)          | RequirementFactory, ScrollModifierFactory, SpecialEffectFactory | 根據資料建立對應策略實例       |
| Observer (觀察者)       | GameEventChannel, C# event                      | 解耦系統間通訊                                   |
| Data-Driven (資料驅動)  | ScriptableObject (各種 Data)                    | 符文、卷軸、顧客、遺物等資料與程式碼分離         |
| Object Pool (物件池)    | ObjectPool\<T\>                                 | 重複使用 UI 元件，減少 GC 壓力                   |
| State Machine (狀態機)  | TurnManager (TurnPhase)                         | 管理回合的八個階段順序與轉換                     |
| MVC                     | Manager(C) + Data(M) + Panel(V)                 | 資料、邏輯、顯示三層分離                         |
| Context Object          | RequirementContext                              | 封裝需求判定所需的完整上下文                     |

---

## 12. 開發優先序對應

### Phase 1 — 核心機制原型

| 優先序 | 類別                                          | 說明                         |
| ------ | --------------------------------------------- | ---------------------------- |
| 1      | `EnvironmentData`, `EnvAttribute`, `RuneEffect` | 環境數值基礎               |
| 2      | `RuneData`, `RuneInstance`, `RuneType`        | 符文資料（含循環/消耗型）    |
| 3      | `ScrollData`, `ScrollInstance`                | 卷軸資料                     |
| 4      | `IScrollModifier` + `DirectAddModifier`       | 最基本的卷軸修飾             |
| 5      | `ScrollProcessor`                             | 卷軸結算（不含連鎖）         |
| 6      | `EnvironmentManager`                          | 環境管理                     |
| 7      | `DeckManager`, `HandManager`                  | 牌庫（全棄重抽）與手牌       |
| 8      | `ScrollInventory`                             | 卷軸庫存                     |
| 9      | `IRequirement` + `AbsoluteRequirement`        | 最基本的需求判定             |
| 10     | `CustomerData`, `CustomerInstance`            | 顧客資料                     |
| 11     | `CustomerManager`                             | 顧客管理（單一顧客模式）     |
| 12     | `TurnManager`                                 | 回合流程（一回合一卷軸）     |
| 13     | `GameState`, `GameManager`, `GameConfig`      | 遊戲主框架                   |
| 14     | UI 面板（基礎版）                              | 最小可玩 UI                  |

### Phase 2 — 遊戲循環完整化

| 類別                                           | 說明                               |
| ---------------------------------------------- | ---------------------------------- |
| `RelicData`, `RelicInstance`, `RelicManager`   | 遺物系統（動態增減、排序、不重置） |
| `ExtremeEventHandler`                          | 極端事件（一次性觸發）             |
| `LevelManager`                                | 關卡管理（隊列制）                 |
| `WholesalerManager`, `WholesalerConfig`        | 批發商系統（基礎版固定商品）       |
| 顧客耐心、業績增減                              | 完整的分數循環                     |

### Phase 3 — 系統深度

| 類別                                           | 說明                               |
| ---------------------------------------------- | ---------------------------------- |
| `TagChainCalculator`                           | 標籤連鎖加成                       |
| `TagPreferenceRequirement`                     | 顧客標籤偏好需求                   |
| 遺物標籤偏好效果                                | 遺物對標籤的反應                   |
| `RelativeChangeRequirement`, `StabilityRequirement` | 完整需求類型                 |
| `ISpecialEffect`, `RemoveRelicEffect`          | 消耗型符文特殊效果（驅逐石）       |
| 其餘 `IScrollModifier` 實作                    | 完整卷軸修飾                       |
| 批發商完整版（隨機商品池）                       | 完整批發商                         |

### Phase 4 — 內容擴充

| 內容                  | 數量          |
| --------------------- | ------------- |
| 符文                  | 25+ 種        |
| 卷軸                  | 12+ 種        |
| 遺物                  | 12+ 種        |
| 顧客類型              | 15+ 種        |
| 極端事件完整效果       | 4 種          |

### Phase 5 — 打磨與平衡

| 項目           |
| -------------- |
| 難度曲線調整   |
| UI 美術        |
| 音效與演出     |
| 分數排行榜     |

---

_本文件版本：v0.2_
_最後更新：2026-03-21_

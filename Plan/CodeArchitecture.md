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
│   │   ├── GameManager.cs           # 遊戲主管理器（遊戲循環控制）
│   │   ├── TurnManager.cs           # 回合流程管理
│   │   ├── GameState.cs             # 遊戲狀態資料容器
│   │   └── GameEventChannel.cs      # 全域事件頻道（觀察者模式）
│   │
│   ├── Environment/                 # 環境系統
│   │   ├── EnvironmentManager.cs    # 環境數值管理
│   │   ├── EnvironmentData.cs       # 環境數值資料結構
│   │   └── ExtremeStateHandler.cs   # 極端狀態處理器
│   │
│   ├── Rune/                        # 符文系統
│   │   ├── RuneData.cs              # 符文 ScriptableObject 定義
│   │   ├── RuneInstance.cs          # 符文運行時實例
│   │   ├── RuneEffect.cs           # 符文效果資料結構
│   │   └── RuneDatabase.cs         # 符文資料庫 ScriptableObject
│   │
│   ├── Scroll/                      # 卷軸系統
│   │   ├── ScrollData.cs            # 卷軸 ScriptableObject 定義
│   │   ├── ScrollInstance.cs        # 卷軸運行時實例
│   │   ├── ScrollModifier.cs        # 卷軸修飾規則（策略模式）
│   │   └── ScrollProcessor.cs       # 卷軸結算處理器
│   │
│   ├── Customer/                    # 顧客系統
│   │   ├── CustomerData.cs          # 顧客 ScriptableObject 定義
│   │   ├── CustomerInstance.cs      # 顧客運行時實例
│   │   ├── CustomerManager.cs       # 顧客生成與管理
│   │   ├── Requirements/            # 顧客需求子系統
│   │   │   ├── IRequirement.cs      # 需求介面
│   │   │   ├── AbsoluteRequirement.cs       # 絕對值條件
│   │   │   ├── RelativeChangeRequirement.cs # 相對變化條件
│   │   │   ├── CompoundRequirement.cs       # 複合條件
│   │   │   └── StabilityRequirement.cs      # 穩定條件
│   │   └── CustomerPool.cs          # 顧客池（權重隨機抽取）
│   │
│   ├── Relic/                       # 遺物系統
│   │   ├── RelicData.cs             # 遺物 ScriptableObject 定義
│   │   ├── RelicInstance.cs         # 遺物運行時實例
│   │   ├── RelicManager.cs          # 遺物管理器
│   │   └── RelicPool.cs             # 遺物池
│   │
│   ├── Deck/                        # 牌庫系統
│   │   ├── DeckManager.cs           # 牌庫管理器（抽牌、棄牌、洗牌）
│   │   ├── HandManager.cs           # 手牌管理器
│   │   └── ScrollInventory.cs       # 卷軸庫存管理
│   │
│   ├── UI/                          # UI 系統
│   │   ├── Panels/
│   │   │   ├── EnvironmentPanel.cs  # 環境數值顯示面板
│   │   │   ├── CustomerPanel.cs     # 顧客資訊面板
│   │   │   ├── RelicPanel.cs        # 遺物狀態面板
│   │   │   ├── WorkbenchPanel.cs    # 卷軸工作台面板
│   │   │   ├── HandPanel.cs         # 手牌區面板
│   │   │   └── ScorePanel.cs        # 業績面板
│   │   ├── Components/
│   │   │   ├── RuneCardUI.cs        # 符文卡牌 UI 元件
│   │   │   ├── ScrollSlotUI.cs      # 卷軸槽位 UI 元件
│   │   │   ├── RelicIconUI.cs       # 遺物圖示 UI 元件
│   │   │   ├── CustomerPortraitUI.cs # 顧客頭像 UI 元件
│   │   │   └── TooltipUI.cs         # 通用提示框
│   │   ├── DragDrop/
│   │   │   ├── DraggableRune.cs     # 符文拖曳行為
│   │   │   └── DropZone.cs          # 放置區域行為
│   │   └── UIManager.cs             # UI 總管理器
│   │
│   ├── Animation/                   # 動畫演出
│   │   ├── CardAnimator.cs          # 卡牌動畫控制
│   │   ├── EnvironmentVFX.cs        # 環境特效
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
│       └── GameConfig.asset         # 遊戲全域設定
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
    └── GameScene.unity
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

// 符文標籤（使用 Flags 支援多標籤）
[Flags]
public enum RuneTag
{
    None    = 0,
    Water   = 1 << 0,  // 水系
    Fire    = 1 << 1,  // 火系
    Light   = 1 << 2,  // 光系
    Dark    = 1 << 3,  // 暗系
    Neutral = 1 << 4   // 中性
}

// 遊戲階段
public enum TurnPhase
{
    CustomerArrival,   // 1. 新顧客到來
    RelicTrigger,      // 2. 遺物效果觸發
    DrawCards,         // 3. 手牌補充
    PlayerAction,      // 4. 玩家行動階段
    EnvironmentResolve,// 5. 環境結算
    CustomerJudge,     // 6. 顧客需求判定
    PatienceCheck,     // 7. 耐心判定
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
    Average,         // 所有效果取平均
    Invert           // 正負反轉
}

// 極端環境狀態
public enum ExtremeState
{
    None,
    Darkness,   // 黑暗（亮度 = 0）
    Flood,      // 大洪水（水分 = 20）
    Overheat    // 高溫（溫度 = 20）
}
```

### 3.2 環境資料

```csharp
/// <summary>
/// 環境數值資料結構，保存當前三項環境屬性與業績
/// </summary>
[System.Serializable]
public class EnvironmentData
{
    public const int MIN_VALUE = 0;
    public const int MAX_VALUE = 20;

    [Range(MIN_VALUE, MAX_VALUE)] public int brightness;
    [Range(MIN_VALUE, MAX_VALUE)] public int moisture;
    [Range(MIN_VALUE, MAX_VALUE)] public int temperature;
    public int score;

    /// <summary>取得指定屬性的數值</summary>
    public int GetValue(EnvAttribute attr);

    /// <summary>設定指定屬性的數值（自動 Clamp 在合法範圍內）</summary>
    public void SetValue(EnvAttribute attr, int value);

    /// <summary>對指定屬性增減數值</summary>
    public void ApplyDelta(EnvAttribute attr, int delta);

    /// <summary>建立當前狀態的深拷貝（用於預覽）</summary>
    public EnvironmentData Clone();
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

### 3.4 符文資料（ScriptableObject）

```csharp
/// <summary>
/// 符文的靜態資料定義，作為 ScriptableObject 存於 Assets/Data/Runes/
/// </summary>
[CreateAssetMenu(fileName = "NewRune", menuName = "SpellShop/Rune")]
public class RuneData : ScriptableObject
{
    public string runeName;
    public Sprite icon;
    [TextArea] public string description;
    public Rarity rarity;
    public RuneTag tags;
    public List<RuneEffect> effects;
}
```

### 3.5 符文運行時實例

```csharp
/// <summary>
/// 符文的運行時包裝，追蹤該張符文在牌庫/手牌/棄牌堆中的狀態
/// </summary>
public class RuneInstance
{
    public RuneData Data { get; private set; }
    public string UniqueId { get; private set; } // 每張符文的唯一識別碼

    public RuneInstance(RuneData data)
    {
        Data = data;
        UniqueId = System.Guid.NewGuid().ToString();
    }
}
```

### 3.6 卷軸資料（ScriptableObject）

```csharp
/// <summary>
/// 卷軸的靜態資料定義
/// </summary>
[CreateAssetMenu(fileName = "NewScroll", menuName = "SpellShop/Scroll")]
public class ScrollData : ScriptableObject
{
    public string scrollName;
    public Sprite icon;
    [TextArea] public string description;
    public Rarity rarity;
    public int slotCount;                        // 符文槽數量（必須放滿）
    public ScrollModifierType modifierType;      // 修飾規則類型
    public float modifierValue;                  // 修飾參數（如倍率）
    public RuneTag tagRestriction;               // 標籤限制（None = 無限制）
    public RuneTag tagMultiplyTarget;            // TagMultiply 模式下的目標標籤
}
```

### 3.7 卷軸運行時實例

```csharp
/// <summary>
/// 卷軸的運行時實例，追蹤槽位中已放入的符文
/// </summary>
public class ScrollInstance
{
    public ScrollData Data { get; private set; }
    public RuneInstance[] Slots { get; private set; }
    public string UniqueId { get; private set; }

    public ScrollInstance(ScrollData data);

    /// <summary>是否所有槽位都已放入符文</summary>
    public bool IsFull { get; }

    /// <summary>嘗試將符文放入下一個空槽位，回傳是否成功</summary>
    public bool TryPlaceRune(RuneInstance rune);

    /// <summary>從指定槽位移除符文</summary>
    public RuneInstance RemoveRuneAt(int slotIndex);

    /// <summary>檢查符文是否符合卷軸的標籤限制</summary>
    public bool CanAcceptRune(RuneInstance rune);

    /// <summary>清空所有槽位</summary>
    public void ClearSlots();
}
```

### 3.8 顧客資料（ScriptableObject）

```csharp
/// <summary>
/// 顧客的靜態資料定義
/// </summary>
[CreateAssetMenu(fileName = "NewCustomer", menuName = "SpellShop/Customer")]
public class CustomerData : ScriptableObject
{
    public string customerName;
    public Sprite portrait;
    [TextArea] public string dialogue;             // 特殊對話
    public int maxPatience;                        // 最大等待耐心（回合數）
    public List<RequirementData> requirements;     // 需求條件列表
    public List<RewardEntry> rewards;              // 獎勵列表
    public int scoreReward;                        // 業績加成
    public int scorePenalty;                       // 憤怒離開時的業績扣除
}
```

### 3.9 需求條件資料

```csharp
/// <summary>
/// 顧客需求條件的序列化資料
/// </summary>
[System.Serializable]
public class RequirementData
{
    public RequirementType type;
    public EnvAttribute attribute;
    public CompareOperator compareOp;
    public int targetValue;
    public int targetValueMax;         // InRange 時使用的上界
    public int requiredDelta;          // 相對變化條件的變化量
    public int stabilityTurns;         // 穩定條件所需維持回合數
}

public enum RequirementType
{
    Absolute,       // 絕對值條件
    RelativeChange, // 相對變化條件
    Stability       // 環境穩定條件
}
```

### 3.10 獎勵條目

```csharp
/// <summary>
/// 獎勵條目，定義顧客滿足後給予的獎勵
/// </summary>
[System.Serializable]
public class RewardEntry
{
    public RewardType type;
    public RuneData runeReward;     // 指定符文（null 則隨機）
    public ScrollData scrollReward; // 指定卷軸
    public Rarity randomRarity;    // 隨機獎勵的稀有度篩選
    public int count;               // 數量
}

public enum RewardType
{
    SpecificRune,
    RandomRune,
    SpecificScroll,
    RandomScroll,
    Score
}
```

### 3.11 顧客運行時實例

```csharp
/// <summary>
/// 顧客的運行時實例，追蹤耐心與需求滿足狀態
/// </summary>
public class CustomerInstance
{
    public CustomerData Data { get; private set; }
    public int CurrentPatience { get; private set; }

    public CustomerInstance(CustomerData data);

    /// <summary>檢查當前環境是否滿足所有需求</summary>
    public bool CheckRequirements(EnvironmentData env, EnvironmentData prevEnv);

    /// <summary>耐心遞減，回傳是否仍有耐心</summary>
    public bool TickPatience();

    /// <summary>是否已沒有耐心</summary>
    public bool IsOutOfPatience { get; }
}
```

### 3.12 遺物資料（ScriptableObject）

```csharp
/// <summary>
/// 遺物的靜態資料定義
/// </summary>
[CreateAssetMenu(fileName = "NewRelic", menuName = "SpellShop/Relic")]
public class RelicData : ScriptableObject
{
    public string relicName;
    public Sprite icon;
    [TextArea] public string description;

    // 滿意條件
    public List<RequirementData> satisfactionConditions;

    // 滿意效果
    public List<RuneEffect> satisfiedEffects;         // 每回合正向效果
    public int satisfiedScoreBonus;                   // 每回合業績加成
    public bool satisfiedDrawExtraRune;               // 是否額外抽牌

    // 不滿意效果
    public List<RuneEffect> unsatisfiedEffects;       // 每回合副作用

    // 懲罰機制
    public int maxUnsatisfiedTurns;                   // 不滿意容忍上限
    public RelicPunishment punishment;                // 達上限後的懲罰類型
    public List<RuneEffect> punishmentEffects;        // 懲罰效果
}

public enum RelicPunishment
{
    EnvironmentShock,  // 環境劇烈波動
    ScoreReset,        // 業績清零
    PlayerDeath        // 玩家死亡
}
```

### 3.13 遺物運行時實例

```csharp
/// <summary>
/// 遺物的運行時實例，追蹤不滿意計數
/// </summary>
public class RelicInstance
{
    public RelicData Data { get; private set; }
    public int UnsatisfiedCount { get; private set; }
    public bool IsSatisfied { get; private set; }

    public RelicInstance(RelicData data);

    /// <summary>根據當前環境判斷滿意狀態並回傳效果</summary>
    public RelicTickResult Tick(EnvironmentData env);

    /// <summary>是否已達到懲罰觸發門檻</summary>
    public bool IsPunishmentTriggered { get; }

    /// <summary>重置不滿意計數（滿意時歸零）</summary>
    public void ResetUnsatisfiedCount();
}

/// <summary>遺物每回合結算結果</summary>
public struct RelicTickResult
{
    public bool isSatisfied;
    public List<RuneEffect> effectsToApply;
    public int scoreChange;
    public bool drawExtraRune;
    public bool punishmentTriggered;
    public RelicPunishment punishmentType;
    public List<RuneEffect> punishmentEffects;
}
```

### 3.14 遊戲狀態

```csharp
/// <summary>
/// 遊戲全局狀態容器
/// </summary>
public class GameState
{
    public EnvironmentData Environment { get; set; }
    public EnvironmentData PreviousEnvironment { get; set; } // 上回合環境快照
    public int CurrentTurn { get; set; }
    public TurnPhase CurrentPhase { get; set; }
    public bool IsGameOver { get; set; }
    public string GameOverReason { get; set; }
    public List<ExtremeState> ActiveExtremeStates { get; set; }
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
    /// <summary>檢查當前環境是否滿足此需求</summary>
    /// <param name="current">當前環境數據</param>
    /// <param name="previous">上回合環境數據（用於相對變化判定）</param>
    /// <param name="history">歷史環境紀錄（用於穩定條件判定）</param>
    bool IsSatisfied(EnvironmentData current, EnvironmentData previous, List<EnvironmentData> history);

    /// <summary>取得需求的文字描述（顯示於 UI）</summary>
    string GetDescription();
}

/// <summary>絕對值條件：如 溫度 >= 8</summary>
public class AbsoluteRequirement : IRequirement
{
    private EnvAttribute _attribute;
    private CompareOperator _op;
    private int _target;
    private int _targetMax; // InRange 用

    public AbsoluteRequirement(RequirementData data);
    public bool IsSatisfied(EnvironmentData current, EnvironmentData previous, List<EnvironmentData> history);
    public string GetDescription();
}

/// <summary>相對變化條件：如 本回合水分減少 >= 3</summary>
public class RelativeChangeRequirement : IRequirement
{
    private EnvAttribute _attribute;
    private int _requiredDelta;

    public RelativeChangeRequirement(RequirementData data);
    public bool IsSatisfied(EnvironmentData current, EnvironmentData previous, List<EnvironmentData> history);
    public string GetDescription();
}

/// <summary>穩定條件：如 連續 2 回合溫度維持在 4~6 之間</summary>
public class StabilityRequirement : IRequirement
{
    private EnvAttribute _attribute;
    private int _min, _max;
    private int _requiredTurns;

    public StabilityRequirement(RequirementData data);
    public bool IsSatisfied(EnvironmentData current, EnvironmentData previous, List<EnvironmentData> history);
    public string GetDescription();
}

/// <summary>複合條件：多個需求的 AND 組合</summary>
public class CompoundRequirement : IRequirement
{
    private List<IRequirement> _subRequirements;

    public CompoundRequirement(List<RequirementData> dataList);
    public bool IsSatisfied(EnvironmentData current, EnvironmentData previous, List<EnvironmentData> history);
    public string GetDescription();
}
```

### 4.2 需求工廠

```csharp
/// <summary>
/// 根據 RequirementData 建立對應的 IRequirement 實例
/// </summary>
public static class RequirementFactory
{
    public static IRequirement Create(RequirementData data)
    {
        return data.type switch
        {
            RequirementType.Absolute       => new AbsoluteRequirement(data),
            RequirementType.RelativeChange => new RelativeChangeRequirement(data),
            RequirementType.Stability      => new StabilityRequirement(data),
            _ => throw new System.ArgumentException($"Unknown requirement type: {data.type}")
        };
    }
}
```

### 4.3 卷軸修飾器（策略模式）

```csharp
/// <summary>
/// 卷軸修飾規則的介面，負責將槽位中的符文效果轉換為最終環境變化量
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

/// <summary>平均化：所有效果取平均後套用</summary>
public class AverageModifier : IScrollModifier { ... }

/// <summary>反轉：所有效果正負反轉</summary>
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

---

## 5. 管理器設計（Manager 層）

### 5.1 GameManager — 遊戲主管理器

```csharp
/// <summary>
/// 遊戲主管理器，作為唯一的 Singleton 入口，協調所有子系統
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // 子系統引用
    [SerializeField] private GameConfig _config;
    private TurnManager _turnManager;
    private EnvironmentManager _environmentManager;
    private DeckManager _deckManager;
    private HandManager _handManager;
    private ScrollInventory _scrollInventory;
    private CustomerManager _customerManager;
    private RelicManager _relicManager;
    private UIManager _uiManager;
    private GameState _gameState;

    // 生命週期
    private void Awake();        // Singleton 初始化
    public void StartNewGame();  // 開始新遊戲
    public void EndGame(string reason); // 結束遊戲

    // 外部存取
    public GameState State => _gameState;
    public GameConfig Config => _config;
}
```

### 5.2 TurnManager — 回合流程管理

```csharp
/// <summary>
/// 管理回合的八個階段，依序執行並透過事件通知 UI
/// </summary>
public class TurnManager
{
    private GameState _state;

    // 事件
    public event System.Action<TurnPhase> OnPhaseChanged;
    public event System.Action<int> OnTurnStarted;
    public event System.Action<int> OnTurnEnded;

    /// <summary>開始新回合，依序執行八個階段</summary>
    public async void StartTurn();

    /// <summary>
    /// 各階段執行方法（依序呼叫）
    /// 階段 4 (PlayerAction) 會暫停等待玩家操作完成
    /// </summary>
    private void Phase_CustomerArrival();
    private void Phase_RelicTrigger();
    private void Phase_DrawCards();
    private void Phase_WaitPlayerAction();    // 等待玩家送出卷軸
    private void Phase_EnvironmentResolve();
    private void Phase_CustomerJudge();
    private void Phase_PatienceCheck();
    private void Phase_RelicUpdate();

    /// <summary>玩家按下送出按鈕時呼叫，結束行動階段</summary>
    public void OnPlayerSubmit(ScrollInstance submittedScroll);
}
```

### 5.3 EnvironmentManager — 環境管理

```csharp
/// <summary>
/// 管理環境數值的變化、極端狀態偵測與歷史紀錄
/// </summary>
public class EnvironmentManager
{
    private EnvironmentData _current;
    private EnvironmentData _previous;
    private List<EnvironmentData> _history;  // 歷史紀錄（用於穩定條件判定）
    private ExtremeStateHandler _extremeHandler;

    // 事件
    public event System.Action<EnvAttribute, int, int> OnValueChanged; // 屬性, 舊值, 新值
    public event System.Action<ExtremeState> OnExtremeStateEntered;
    public event System.Action<ExtremeState> OnExtremeStateExited;

    /// <summary>套用一組效果到環境上</summary>
    public void ApplyEffects(List<RuneEffect> effects);

    /// <summary>在回合結算前快照當前狀態到 previous</summary>
    public void SnapshotCurrent();

    /// <summary>取得當前環境的唯讀拷貝</summary>
    public EnvironmentData GetCurrentSnapshot();

    /// <summary>取得環境歷史紀錄</summary>
    public List<EnvironmentData> GetHistory();

    /// <summary>檢查並處理極端狀態</summary>
    public void CheckExtremeStates();
}
```

### 5.4 DeckManager — 牌庫管理

```csharp
/// <summary>
/// 管理符文牌庫的抽牌、棄牌、洗牌循環
/// </summary>
public class DeckManager
{
    private List<RuneInstance> _drawPile;    // 抽牌堆
    private List<RuneInstance> _discardPile; // 棄牌堆

    // 事件
    public event System.Action<int> OnDrawPileChanged;   // 剩餘張數
    public event System.Action<int> OnDiscardPileChanged;

    /// <summary>初始化牌庫（傳入所有符文資料）</summary>
    public void Initialize(List<RuneData> startingRunes);

    /// <summary>從抽牌堆抽取指定數量的符文</summary>
    public List<RuneInstance> Draw(int count);

    /// <summary>將符文放入棄牌堆</summary>
    public void Discard(RuneInstance rune);

    /// <summary>將棄牌堆洗入抽牌堆</summary>
    public void Reshuffle();

    /// <summary>將新符文加入牌庫（顧客獎勵）</summary>
    public void AddToDeck(RuneData runeData);

    /// <summary>從牌庫中永久移除一張符文</summary>
    public void RemoveFromDeck(RuneInstance rune);

    public int DrawPileCount { get; }
    public int DiscardPileCount { get; }
}
```

### 5.5 HandManager — 手牌管理

```csharp
/// <summary>
/// 管理玩家手牌
/// </summary>
public class HandManager
{
    private List<RuneInstance> _hand;
    private int _maxHandSize;

    public event System.Action<RuneInstance> OnRuneAdded;
    public event System.Action<RuneInstance> OnRuneRemoved;

    /// <summary>加入手牌</summary>
    public void AddToHand(RuneInstance rune);

    /// <summary>從手牌移除（放入卷軸或棄牌）</summary>
    public void RemoveFromHand(RuneInstance rune);

    /// <summary>回合結束時棄掉所有手牌</summary>
    public void DiscardAll(DeckManager deck);

    public IReadOnlyList<RuneInstance> Hand { get; }
    public int Count { get; }
}
```

### 5.6 ScrollInventory — 卷軸庫存

```csharp
/// <summary>
/// 管理玩家持有的卷軸（消耗品）
/// </summary>
public class ScrollInventory
{
    private List<ScrollInstance> _scrolls;

    public event System.Action<ScrollInstance> OnScrollAdded;
    public event System.Action<ScrollInstance> OnScrollUsed;

    /// <summary>加入新卷軸</summary>
    public void AddScroll(ScrollData data);

    /// <summary>使用卷軸（從庫存移除）</summary>
    public void ConsumeScroll(ScrollInstance scroll);

    public IReadOnlyList<ScrollInstance> Scrolls { get; }
}
```

### 5.7 CustomerManager — 顧客管理

```csharp
/// <summary>
/// 管理顧客的生成、排隊與需求判定
/// </summary>
public class CustomerManager
{
    private CustomerPool _pool;
    private CustomerInstance _currentCustomer;
    private int _difficulty; // 隨回合數遞增

    public event System.Action<CustomerInstance> OnCustomerArrived;
    public event System.Action<CustomerInstance> OnCustomerSatisfied;
    public event System.Action<CustomerInstance> OnCustomerLeft; // 憤怒離開

    /// <summary>生成新顧客</summary>
    public CustomerInstance SpawnCustomer();

    /// <summary>檢查當前顧客的需求是否被滿足</summary>
    public bool CheckCurrentCustomer(EnvironmentData env, EnvironmentData prevEnv, List<EnvironmentData> history);

    /// <summary>處理顧客耐心遞減</summary>
    public void TickPatience();

    /// <summary>發放顧客獎勵</summary>
    public List<RewardEntry> ClaimRewards();

    public CustomerInstance CurrentCustomer { get; }
}
```

### 5.8 RelicManager — 遺物管理

```csharp
/// <summary>
/// 管理所有遺物的狀態更新與效果觸發
/// </summary>
public class RelicManager
{
    private List<RelicInstance> _relics;
    private RelicPool _pool;

    public event System.Action<RelicInstance, RelicTickResult> OnRelicTicked;
    public event System.Action<RelicInstance> OnRelicPunishment;

    /// <summary>初始化遺物（隨機選取 1~3 個）</summary>
    public void Initialize(int count);

    /// <summary>觸發所有遺物的回合效果</summary>
    public List<RelicTickResult> TickAll(EnvironmentData env);

    public IReadOnlyList<RelicInstance> Relics { get; }
}
```

---

## 6. 卷軸結算處理器

```csharp
/// <summary>
/// 負責計算卷軸送出後的最終環境效果
/// </summary>
public class ScrollProcessor
{
    /// <summary>
    /// 計算卷軸的最終效果
    /// 1. 收集所有槽位中符文的效果
    /// 2. 根據卷軸的修飾規則計算最終數值
    /// 3. 回傳合併後的效果列表
    /// </summary>
    public static List<RuneEffect> Process(ScrollInstance scroll)
    {
        IScrollModifier modifier = ScrollModifierFactory.Create(scroll.Data.modifierType);
        return modifier.Calculate(scroll.Slots, scroll.Data);
    }

    /// <summary>
    /// 預覽效果（不實際套用，僅供 UI 顯示 delta）
    /// </summary>
    public static EnvironmentData Preview(ScrollInstance scroll, EnvironmentData currentEnv)
    {
        var preview = currentEnv.Clone();
        var effects = Process(scroll);
        foreach (var effect in effects)
        {
            preview.ApplyDelta(effect.attribute, effect.value);
        }
        return preview;
    }
}
```

---

## 7. 事件系統

採用 ScriptableObject-based 事件頻道，解耦系統間的通訊。

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
/// 遊戲全域設定 ScriptableObject
/// </summary>
[CreateAssetMenu(menuName = "SpellShop/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("環境初始值")]
    public int initialBrightness = 5;
    public int initialMoisture = 5;
    public int initialTemperature = 5;

    [Header("手牌設定")]
    public int drawPerTurn = 5;        // 每回合抽牌數
    public int maxHandSize = 10;       // 手牌上限

    [Header("遺物設定")]
    public int minRelics = 1;          // 每局最少遺物數
    public int maxRelics = 3;          // 每局最多遺物數

    [Header("難度設定")]
    public int difficultyScaleTurn = 5; // 每 N 回合難度提升一級

    [Header("初始牌庫")]
    public List<RuneData> startingRunes;   // 初始符文
    public List<ScrollData> startingScrolls; // 初始卷軸
}
```

---

## 9. 核心流程時序圖

```
StartTurn()
    │
    ├─ Phase 1: CustomerArrival
    │   └─ CustomerManager.SpawnCustomer()
    │       └─ Event: OnCustomerArrived
    │
    ├─ Phase 2: RelicTrigger
    │   └─ RelicManager.TickAll(env)
    │       ├─ 每個 Relic → RelicInstance.Tick(env)
    │       │   ├─ 滿意 → ApplyEffects(satisfiedEffects)
    │       │   └─ 不滿意 → ApplyEffects(unsatisfiedEffects)
    │       └─ Event: OnRelicTicked / OnRelicPunishment
    │
    ├─ Phase 3: DrawCards
    │   └─ DeckManager.Draw(config.drawPerTurn)
    │       └─ HandManager.AddToHand(runes)
    │           └─ Event: OnRuneAdded (每張)
    │
    ├─ Phase 4: PlayerAction [等待玩家操作]
    │   │
    │   │  玩家操作:
    │   │  1. ScrollInventory 選擇卷軸 → ScrollInstance
    │   │  2. 拖曳 RuneInstance → ScrollInstance.TryPlaceRune()
    │   │  3. ScrollProcessor.Preview() → UI 顯示預覽
    │   │  4. 按下送出 → TurnManager.OnPlayerSubmit()
    │   │
    │   └─ 等待 OnPlayerSubmit 信號
    │
    ├─ Phase 5: EnvironmentResolve
    │   ├─ EnvironmentManager.SnapshotCurrent()
    │   ├─ ScrollProcessor.Process(submittedScroll)
    │   ├─ EnvironmentManager.ApplyEffects(finalEffects)
    │   ├─ EnvironmentManager.CheckExtremeStates()
    │   └─ ScrollInventory.ConsumeScroll(submittedScroll)
    │
    ├─ Phase 6: CustomerJudge
    │   └─ CustomerManager.CheckCurrentCustomer(env, prevEnv, history)
    │       ├─ 滿足 → ClaimRewards() → Event: OnCustomerSatisfied
    │       └─ 未滿足 → (進入 Phase 7)
    │
    ├─ Phase 7: PatienceCheck
    │   └─ CustomerManager.TickPatience()
    │       ├─ 耐心 > 0 → 繼續等待
    │       └─ 耐心 = 0 → 扣除業績 → Event: OnCustomerLeft
    │
    └─ Phase 8: RelicUpdate
        └─ 更新遺物不滿意計數
            ├─ 未達上限 → 繼續
            └─ 達上限 → 觸發懲罰
                ├─ PlayerDeath → GameManager.EndGame()
                ├─ ScoreReset → score = 0
                └─ EnvironmentShock → ApplyEffects()
```

---

## 10. UI 資料流

```
                   Model (Data)                    View (UI)
                   ───────────                     ─────────
EnvironmentManager ──OnValueChanged──────────────→ EnvironmentPanel
                                                   └─ 更新數字、進度條、極端狀態圖示

DeckManager ────────OnDrawPileChanged────────────→ ScorePanel
                    OnDiscardPileChanged             └─ 顯示抽牌堆/棄牌堆數量

HandManager ────────OnRuneAdded──────────────────→ HandPanel
                    OnRuneRemoved                    └─ 生成/銷毀 RuneCardUI

ScrollInventory ────OnScrollAdded────────────────→ WorkbenchPanel
                    OnScrollUsed                     ├─ 更新卷軸選擇下拉
                                                     └─ 更新槽位 ScrollSlotUI

CustomerManager ────OnCustomerArrived────────────→ CustomerPanel
                    OnCustomerSatisfied              ├─ 顯示顧客頭像、對話
                    OnCustomerLeft                   └─ 顯示需求、耐心倒數

RelicManager ───────OnRelicTicked────────────────→ RelicPanel
                    OnRelicPunishment                ├─ 顯示遺物圖示
                                                     └─ 滿意/不滿意狀態指示
```

---

## 11. 設計模式總覽

| 設計模式                | 應用位置                               | 目的                                       |
| ----------------------- | -------------------------------------- | ------------------------------------------ |
| Singleton               | GameManager                            | 唯一入口，協調全部子系統                   |
| Strategy (策略)         | IRequirement, IScrollModifier          | 將不同的需求判定和卷軸修飾規則封裝為可替換策略 |
| Factory (工廠)          | RequirementFactory, ScrollModifierFactory | 根據資料建立對應的策略實例                 |
| Observer (觀察者)       | GameEventChannel, C# event             | 解耦系統間通訊                             |
| Data-Driven (資料驅動)  | ScriptableObject (各種 Data)           | 符文、卷軸、顧客、遺物等資料與程式碼分離   |
| Object Pool (物件池)    | ObjectPool<T>                          | 重複使用 UI 元件，減少 GC 壓力             |
| State Machine (狀態機)  | TurnManager (TurnPhase)                | 管理回合的八個階段順序與轉換               |
| MVC                     | Manager(C) + Data(M) + Panel(V)        | 資料、邏輯、顯示三層分離                   |

---

## 12. 開發優先序對應

### Phase 1（MVP 核心機制）需實作的類別

| 優先序 | 類別                                   | 說明               |
| ------ | -------------------------------------- | ------------------ |
| 1      | `EnvironmentData`, `EnvAttribute`      | 環境數值基礎       |
| 2      | `RuneData`, `RuneEffect`, `RuneInstance` | 符文資料與實例     |
| 3      | `ScrollData`, `ScrollInstance`         | 卷軸資料與實例     |
| 4      | `IScrollModifier` + `DirectAddModifier` | 最基本的卷軸修飾   |
| 5      | `ScrollProcessor`                      | 卷軸結算           |
| 6      | `EnvironmentManager`                   | 環境管理           |
| 7      | `DeckManager`, `HandManager`           | 牌庫與手牌         |
| 8      | `ScrollInventory`                      | 卷軸庫存           |
| 9      | `IRequirement` + `AbsoluteRequirement` | 最基本的需求判定   |
| 10     | `CustomerData`, `CustomerInstance`     | 顧客資料與實例     |
| 11     | `CustomerManager`                      | 顧客管理           |
| 12     | `TurnManager`                          | 回合流程           |
| 13     | `GameState`, `GameManager`, `GameConfig` | 遊戲主框架       |
| 14     | UI 面板（基礎版）                       | 最小可玩 UI        |

### Phase 2 追加

| 類別                                      | 說明           |
| ----------------------------------------- | -------------- |
| `RelicData`, `RelicInstance`, `RelicManager` | 遺物系統完整化 |
| `ExtremeStateHandler`                      | 極端狀態       |
| 其餘 `IRequirement` 實作                   | 完整需求類型   |
| 其餘 `IScrollModifier` 實作                | 完整卷軸修飾   |
| `CustomerPool`, `RelicPool`                | 隨機池         |

---

_本文件版本：v0.1_
_最後更新：2026-03-21_

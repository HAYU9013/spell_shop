# 咒語店 — 已實作內容

> 最後更新：2026-03-21

---

## 實作進度總覽

| 系統 | 狀態 | 備註 |
| ---- | ---- | ---- |
| 全域定義（Enum / Struct） | ✅ 完成 | |
| 環境系統 | ✅ 完成 | 含極端事件 |
| 回合狀態機 | ✅ 框架完成 | Phase 1/2/6/7 已接線，3/4/5 待牌庫/卷軸 |
| 符文（Rune） | ✅ 完成 | ScriptableObject + 6 張預設資產 |
| 卷軸（Scroll） | ✅ 完成 | ScriptableObject + 3 張預設資產 |
| 遺物（Relic） | ✅ 完成 | ScriptableObject + 2 個預設資產，Phase 2 接線 |
| 顧客（Customer） | ✅ 完成 | 三種需求類型，Phase 1/5/6/7 接線 |
| 牌庫 / 手牌 | ❌ 待實作 | DeckManager、HandManager |
| 卷軸結算 | ❌ 待實作 | ScrollProcessor、WorkbenchManager |
| GameManager | ❌ 待實作 | Game Over / Level Clear |
| UI | ❌ 待實作 | 所有面板 |

---

## 檔案結構

```
Assets/
├── Editor/
│   ├── RuneAssetCreator.cs       SpellShop → Create Default Runes
│   ├── ScrollAssetCreator.cs     SpellShop → Create Default Scrolls
│   ├── RelicAssetCreator.cs      SpellShop → Create Default Relics
│   └── CustomerAssetCreator.cs   SpellShop → Create Default Customers
│
├── Scripts/
│   ├── Core/
│   │   └── GameDefinitions.cs    全域 Enum + 共用 Struct
│   │
│   ├── Environment/
│   │   ├── EnvironmentData.cs
│   │   ├── ExtremeEventHandler.cs
│   │   └── EnvironmentManager.cs
│   │
│   ├── Rune/
│   │   ├── RuneData.cs           ScriptableObject
│   │   └── RuneInstance.cs       執行時包裝
│   │
│   ├── Scroll/
│   │   ├── ScrollData.cs         ScriptableObject
│   │   └── ScrollInstance.cs     執行時包裝
│   │
│   ├── Relic/
│   │   ├── RelicData.cs          ScriptableObject（含 RelicCondition）
│   │   ├── RelicInstance.cs      執行時狀態 + Tick()
│   │   └── RelicManager.cs       Singleton MonoBehaviour
│   │
│   ├── Customer/
│   │   ├── RequirementData.cs    序列化需求資料
│   │   ├── IRequirement.cs       介面 + RequirementContext
│   │   ├── RequirementFactory.cs 工廠
│   │   ├── Requirements/
│   │   │   ├── AbsoluteRequirement.cs
│   │   │   ├── RelativeChangeRequirement.cs
│   │   │   └── StabilityRequirement.cs
│   │   ├── CustomerData.cs       ScriptableObject
│   │   ├── CustomerInstance.cs   執行時狀態
│   │   └── CustomerManager.cs    Singleton MonoBehaviour
│   │
│   └── Mangagers/
│       ├── TurnManager.cs
│       └── TurnState/
│           ├── IGameState.cs
│           ├── CustomerArrivalState.cs   ✅ 接線
│           ├── RelicTriggerState.cs      ✅ 接線
│           ├── DrawCardsState.cs         ⏳ TODO（等 DeckManager）
│           ├── PlayerActionState.cs      ✅ 阻塞 / Submit
│           ├── EnvironmentResolveState.cs ⏳ TODO（等 ScrollProcessor）
│           ├── CustomerJudgeState.cs     ✅ 接線
│           ├── PatienceCheckState.cs     ✅ 接線
│           └── RelicUpdateState.cs       ✅ 呼叫 OnTurnEnd
│
└── Data/                          ScriptableObject 資產存放
    ├── Runes/         （執行 Create Default Runes 後生成）
    ├── Scrolls/       （執行 Create Default Scrolls 後生成）
    ├── Relics/        （執行 Create Default Relics 後生成）
    └── Customers/     （執行 Create Default Customers 後生成）
```

---

## 各系統說明

### Core / GameDefinitions.cs

所有跨系統共用的 Enum 與 Struct，集中於單一檔案。

**Enum**

| 名稱 | 值 |
| ---- | -- |
| `EnvAttribute` | Brightness, Moisture, Temperature |
| `ExtremeEvent` | None, Darkness, Flood, Overheat, Overcold |
| `Rarity` | Common, Rare, Legendary |
| `RuneType` | Cycle, Consumable |
| `RuneTag` [Flags] | None, Water, Fire, Light, Dark, Wind, Neutral |
| `CompareOperator` | GreaterEqual, LessEqual, Equal, InRange |
| `RequirementType` | Absolute, RelativeChange, Stability, TagPreference |
| `RelicPunishment` | EnvironmentShock, ScoreReset, PlayerDeath |
| `ScrollModifierType` | DirectAdd, MultiplyAll, PositiveOnly, TagMultiply, Average, Invert |
| `RewardType` | RandomRune, RandomScroll, Relic, Score |

**Struct**

| 名稱 | 欄位 |
| ---- | ---- |
| `RuneEffect` | EnvAttribute attribute, int value |
| `EnvironmentDelta` | brightnessDelta, moistureDelta, temperatureDelta + GetDelta(attr) |

---

### Environment 系統

#### EnvironmentData

純資料類別（`[Serializable]`，非 MonoBehaviour）。

| 常數 | 值 |
| ---- | -- |
| MIN_VALUE | 0 |
| MAX_VALUE | 20 |
| INITIAL_BRIGHTNESS | 5 |
| INITIAL_MOISTURE | 5 |
| INITIAL_TEMPERATURE | 5 |
| INITIAL_SCORE | 30 |

**主要方法**

| 方法 | 說明 |
| ---- | ---- |
| `GetValue(attr)` | 取得屬性值 |
| `SetValue(attr, value)` | 設定並 Clamp，回傳是否觸碰邊界 |
| `ApplyDelta(attr, delta)` | 增減並 Clamp，回傳是否觸碰邊界 |
| `ModifyScore(delta)` | 業績增減（下限 0） |
| `Clone()` | 深拷貝 |
| `static Diff(from, to)` | 回傳 EnvironmentDelta |

#### ExtremeEventHandler

非 MonoBehaviour，由 EnvironmentManager 持有。

| 事件 / 方法 | 說明 |
| ---- | ---- |
| `event OnDarknessTriggered` | 黑暗降臨時觸發，RelicManager 訂閱 |
| `Process(env, hitBoundaryFlags)` | 檢查邊界並套用對應效果，回傳 `List<ExtremeEvent>` |

**極端事件效果**

| 事件 | 觸發條件 | 效果 |
| ---- | -------- | ---- |
| Darkness | 亮度 = 0 | 所有遺物不滿意計數 +1 |
| Flood | 水分 = 20 | 水分 +5（可連鎖，最多 10 次） |
| Overheat | 溫度 = 20 | 業績 -10 |
| Overcold | 溫度 = 0 | 業績 -10 |

#### EnvironmentManager

Singleton MonoBehaviour。

| 事件 | 觸發時機 |
| ---- | -------- |
| `OnValueChanged(attr, val)` | 任何屬性改變 |
| `OnScoreChanged(val)` | 業績改變 |
| `OnExtremeEventTriggered(list)` | 極端事件觸發 |
| `OnDarknessTriggered` | 黑暗降臨（轉發給 RelicManager） |

| 方法 | 說明 |
| ---- | ---- |
| `ApplyEffects(effects)` | 套用效果列表並自動觸發極端事件檢查 |
| `ForceSetValue(attr, value)` | 強制設定（遺物懲罰用） |
| `ModifyScore(delta)` | 業績增減 |
| `SnapshotCurrent()` | 回傳當前環境深拷貝 |
| `DiffFromSnapshot(snapshot)` | 計算累積變化量 |
| `ResetToInitial()` | 重置為初始值 |

**ContextMenu**：`Debug_PrintEnvState`、`Debug_TriggerFlood`、`Debug_TriggerDarkness`、`Debug_ScoreMinus10`

---

### 回合狀態機

#### TurnManager

狀態機核心，驅動 8 個 Phase 的 Coroutine 鏈。

**回合流程**

```
Turn Start
  │
  ▼
Phase 1  CustomerArrival    → 迎接顧客（若無）
  │
  ▼
Phase 2  RelicTrigger       → 遺物效果結算
  │
  ▼
Phase 3  DrawCards          → 棄手牌、重新抽牌 ⏳
  │
  ▼
Phase 4  PlayerAction       → 等待玩家送出（阻塞）
  │
  ▼
Phase 5  EnvironmentResolve → 卷軸/符文效果結算 ⏳ + 更新顧客追蹤
  │
  ▼
Phase 6  CustomerJudge      → 顧客需求判定
  │
  ▼
Phase 7  PatienceCheck      → 耐心扣除（已滿足則跳過）
  │
  ▼
Phase 8  RelicUpdate        → Game Over 檢查 ⏳ → Turn End
  │
  ▼
Turn End → 自動開始下一回合
```

**ContextMenu**：`Debug_StartGame`、`Debug_GoToNextState`、`Debug_SkipToPlayerAction`、`Debug_PrintCurrentPhase`

#### 各 State 接線狀況

| Phase | State | 接線狀態 |
| ----- | ----- | -------- |
| 1 | CustomerArrivalState | ✅ `CustomerManager.SpawnNextCustomer()` |
| 2 | RelicTriggerState | ✅ `RelicManager.TickAll()` |
| 3 | DrawCardsState | ⏳ TODO：DeckManager |
| 4 | PlayerActionState | ✅ 阻塞等待 Submit |
| 5 | EnvironmentResolveState | ⏳ TODO：ScrollProcessor；✅ `CustomerManager.UpdateCurrentTracking()` |
| 6 | CustomerJudgeState | ✅ `CustomerManager.CheckCurrentCustomer()` |
| 7 | PatienceCheckState | ✅ `CustomerManager.TickPatience()` |
| 8 | RelicUpdateState | ✅ `TurnManager.OnTurnEnd()` |

---

### Rune 系統

#### RuneData（ScriptableObject）

| 欄位 | 類型 | 說明 |
| ---- | ---- | ---- |
| runeName | string | 顯示名稱 |
| description | string | 效果說明 |
| icon | Sprite | 卡牌圖示 |
| rarity | Rarity | Common / Rare / Legendary |
| runeType | RuneType | Cycle（循環）/ Consumable（消耗） |
| tags | RuneTag | 可多選標籤 |
| effects | RuneEffect[] | 環境效果列表 |

**6 張預設符文**（`SpellShop → Create Default Runes`）

| 符文 | 效果 | 類型 | 標籤 |
| ---- | ---- | ---- | ---- |
| 水珠 | 水分 +2 | Cycle | Water |
| 小燭 | 亮度 +1、溫度 +1 | Cycle | Fire + Light |
| 寒冰 | 溫度 -3、水分 +1 | Cycle | Water |
| 黑霧 | 亮度 -2、水分 +1 | Cycle | Dark |
| 蒸氣 | 溫度 -1、水分 -1、亮度 +1 | Cycle | Wind |
| 太陽核 | 亮度 +5、溫度 +3、水分 -2 | **Consumable** | Light + Fire |

#### RuneInstance

執行時包裝，追蹤 Consumable 是否已用盡。
- `Exhaust()` → 標記消耗型為已用盡
- `IsAvailable` → 是否可放入卷軸

---

### Scroll 系統

#### ScrollData（ScriptableObject）

| 欄位 | 類型 | 說明 |
| ---- | ---- | ---- |
| scrollName | string | 顯示名稱 |
| slotCount | int (1~6) | 符文槽數量 |
| modifierType | ScrollModifierType | 結算規則 |
| multiplier | float | 倍率（MultiplyAll / TagMultiply 用） |
| tagFilter | RuneTag | 標籤篩選（TagMultiply 用） |

**3 張預設卷軸**（`SpellShop → Create Default Scrolls`）

| 卷軸 | 槽數 | 規則 |
| ---- | ---- | ---- |
| 基礎卷軸 | 2 | DirectAdd |
| 放大卷軸 | 3 | MultiplyAll ×2 |
| 純化卷軸 | 2 | PositiveOnly（忽略負值） |

#### ScrollInstance

執行時包裝，帶唯一 `InstanceId`（區分庫存中同款卷軸）。

---

### Relic 系統

#### RelicData（ScriptableObject）

包含 `RelicCondition`（序列化的滿意條件判斷）。

| 欄位群 | 欄位 |
| ------ | ---- |
| 基本 | relicName, description, icon, displayOrder |
| 滿意條件 | `RelicCondition`（attribute, compareOp, targetValue, rangeMin, rangeMax） |
| 滿意效果 | satisfiedEnvEffects[], satisfiedScoreChange |
| 不滿意效果 | unsatisfiedEnvEffects[], unsatisfiedScoreChange |
| 懲罰 | unsatisfiedLimit, punishmentType, shockAttribute, shockValue, shockIsForceSet |

**2 個預設遺物**（`SpellShop → Create Default Relics`）

| 遺物 | 滿意條件 | 滿意效果 | 不滿意效果 | 上限 | 懲罰 |
| ---- | -------- | -------- | ---------- | ---- | ---- |
| 日晷 | 亮度 >= 7 | 亮度 +1/回合 | 亮度 -1/回合 | 4 回合 | 亮度 delta -5 |
| 苔蘚石 | 水分 4~8 | 業績 +5/回合 | 無 | 3 回合 | 水分強制設為 10 |

#### RelicInstance + RelicTickResult

`Tick(EnvironmentData)` 回傳 `RelicTickResult`：
- 滿意 → 計數歸零，回傳滿意效果
- 不滿意 → 計數 +1，回傳不滿意效果 + 是否達懲罰上限
- 計數達上限後不重置，每回合繼續觸發懲罰直到再次滿意
- `ForceIncrementUnsatisfied()` — 黑暗降臨時呼叫

#### RelicManager（Singleton MonoBehaviour）

| 方法 | 說明 |
| ---- | ---- |
| `InitRelics(list)` | 初始化，依 displayOrder 排序 |
| `AddRelic(data)` | 動態新增（顧客獎勵） |
| `TickAll()` | Phase 2 呼叫，結算所有遺物 |

- 自動訂閱 `EnvironmentManager.OnDarknessTriggered` → `ForceIncrementAllUnsatisfied()`
- **ContextMenu**：`Debug_TickAll`、`Debug_PrintAllRelics`、`Debug_ForceIncrementAll`

---

### Customer 系統

#### 需求架構

```
RequirementData  (序列化資料，存於 CustomerData)
       │
       ▼
RequirementFactory.Create(data)
       │
       ├─→ AbsoluteRequirement       當前值符合條件
       ├─→ RelativeChangeRequirement 顧客到來後累積變化達門檻
       └─→ StabilityRequirement      條件連續成立達指定回合數
```

**RequirementContext**（每次判定時傳入）

| 欄位 | 說明 |
| ---- | ---- |
| CurrentEnv | 當前環境 |
| ArrivalSnapshot | 顧客到來時快照（RelativeChange 基準） |
| AccumulatedDelta | 到來至今累積 delta |
| StabilityCount | 此需求的連續計數 |

**RelativeChange 符號規則**：`requiredDelta >= 0` → 需增加達標；`requiredDelta < 0` → 需減少達標。

#### CustomerData（ScriptableObject）

| 欄位 | 類型 |
| ---- | ---- |
| customerName | string |
| requirements | List\<RequirementData\> |
| maxPatience | int |
| scoreReward | int |
| scorePenalty | int |
| rewardType | RewardType |

**5 位預設顧客**（`SpellShop → Create Default Customers`）

| 顧客 | 需求 | 條件 | 耐心 | 獎勵 | 懲罰 |
| ---- | ---- | ---- | ---- | ---- | ---- |
| 落湯雞旅人 | Absolute | 溫度 >= 8 | 2 | +10 | -5 |
| 感冒農夫 | RelativeChange | 水分累積減少 >= 3 | 3 | +8 | -4 |
| 夜盲商人 | Absolute | 亮度 >= 10 | 1 | +20 | -8 |
| 冰魔法師 | Absolute × 2 | 溫度 <= 2 且 亮度 >= 5 | 4 | +15 | -6 |
| 養花老人 | Stability | 溫度 4~6 連續 1 回合 | 3 | +12 | -5 |

#### CustomerInstance

執行時狀態追蹤：
- `ArrivalSnapshot` — 到來時環境快照
- `AccumulatedDelta` — 由 `UpdateTracking()` 每回合更新
- `StabilityCounters[]` — 每條 Stability 需求的連續計數

| 方法 | 呼叫時機 |
| ---- | -------- |
| `UpdateTracking(env)` | Phase 5（環境結算後） |
| `CheckRequirements(env)` | Phase 6 |
| `TickPatience()` | Phase 7（需求未滿足時） |
| `GetProgressTexts(env)` | UI 顯示用 |

#### CustomerManager（Singleton MonoBehaviour）

| 方法 | Phase |
| ---- | ----- |
| `SpawnNextCustomer()` | 1 |
| `UpdateCurrentTracking()` | 5 |
| `CheckCurrentCustomer()` | 6（滿足則直接發獎勵、移除顧客） |
| `TickPatience()` | 7 |

- `InitLevelMode(queue)` — 固定隊列，清空觸發 `OnQueueEmpty`
- `InitEndlessMode(pool)` — 隨機抽取，永不結束
- **ContextMenu**：`Debug_PrintCurrentCustomer`、`Debug_ForceSpawnNext`、`Debug_ForceCheckRequirements`

---

## 待實作（依優先順序）

### 下一步：牌庫 + 卷軸結算（接通 Phase 3 / 4 / 5）

| 檔案 | 說明 |
| ---- | ---- |
| `DeckManager.cs` | drawPile / discardPile，DrawHand、Exhaust、洗牌 |
| `HandManager.cs` | 當前手牌，AddToHand、DiscardAll |
| `ScrollProcessor.cs` | 核心結算邏輯，6 種 ScrollModifierType |
| `WorkbenchManager.cs` | 卷軸選擇、符文槽位、Submit 觸發 |
| `ScrollInventory.cs` | 玩家持有的卷軸庫存 |

### 之後：GameManager + UI

| 檔案 | 說明 |
| ---- | ---- |
| `GameManager.cs` | Game Over / Level Clear，`GameMode` 切換 |
| 各面板 UI | 環境面板、手牌區、工作台、顧客面板、遺物面板 |

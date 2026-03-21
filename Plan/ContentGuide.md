# 內容新增指南 — 企劃手冊

> 本文件說明如何在不改程式碼的情況下，透過 Unity Inspector 新增遊戲內容。
> 所有內容均以 **ScriptableObject** 存在 Assets 中，右鍵建立後填欄位即可生效。

---

## 目錄

1. [環境系統速覽](#0-環境系統速覽)
2. [符文 Rune](#1-符文-rune)
3. [卷軸 Scroll](#2-卷軸-scroll)
4. [顧客 Customer](#3-顧客-customer)
5. [遺物 Relic](#4-遺物-relic)
6. [獎勵系統 Reward](#5-獎勵系統-reward)
7. [實際範例庫](#6-實際範例庫)
8. [設計注意事項](#7-設計注意事項)

---

## 0. 環境系統速覽

遊戲世界由三個環境數值組成，所有機制都圍繞著它們運作：

| 屬性 | 名稱 | 範圍 | 碰邊界時觸發 |
|------|------|------|-------------|
| `Brightness` | 亮度 | 0 ~ 20 | **黑暗降臨**（亮度跌到 0）：所有遺物不滿意計數 +1 |
| `Moisture`   | 水分 | 0 ~ 20 | **大洪水**（水分超過 20）：水分額外 +5 |
| `Temperature`| 溫度 | 0 ~ 20 | **高溫爆發**（溫度超過 20）：業績 -10<br>**極寒侵襲**（溫度跌到 0）：業績 -10 |

> **設計提示**：環境數值的中間值（約 8~12）通常是最「安全」的狀態。
> 設計需求時讓玩家把數值推向極端，但又不要太超過，這樣會產生張力。

---

## 1. 符文 Rune

**作用**：符文是玩家手牌中的「材料」，放入卷軸槽位後由卷軸決定如何計算效果。

### 建立方式

```
Assets 任意資料夾 → 右鍵 → Create → SpellShop → Rune
```

### Inspector 欄位說明

#### 基本資料

| 欄位 | 型別 | 說明 |
|------|------|------|
| `runeName` | string | 顯示名稱，例如「水球符文」 |
| `description` | string | 卡牌說明文字（玩家看得到） |
| `icon` | Sprite | 卡牌圖示（可先用 Placeholder） |

#### 分類

| 欄位 | 選項 | 說明 |
|------|------|------|
| `rarity` | Common / Rare / Legendary | 稀有度，影響獎勵池的抽取機率 |
| `runeType` | **Cycle** / Consumable | Cycle = 用後進棄牌堆（可重複用）<br>Consumable = 用後永久消失 |
| `tags` | 可多選 Flag | 影響卷軸的標籤加成，詳見下表 |

#### 標籤（`RuneTag`）一覽

| 標籤 | 主題 | 搭配效果建議 |
|------|------|-------------|
| `Water` | 水系 | 水分↑、溫度↓ |
| `Fire`  | 火系 | 溫度↑、水分↓ |
| `Light` | 光系 | 亮度↑ |
| `Dark`  | 暗系 | 亮度↓ |
| `Wind`  | 風系 | 多項小幅調整 |
| `Neutral`| 中性 | 複合效果，不受標籤加成 |

> 可以同時選多個標籤，例如「水Light符文」= Water + Light，讓卷軸 TagMultiply 能匹配更多組合。

#### 環境效果（`effects[]`）

每筆效果是一個 `attribute + value` 對，代表「對哪個環境屬性改變多少」。

```
範例：
  [0] attribute = Moisture,    value = +2   → 水分 +2
  [1] attribute = Temperature, value = -1   → 溫度 -1
```

> **value 可以是負數**（例如 -3 代表減少）。
> 一張符文可以有多個效果，填多筆即可。
> 不需要效果的「空符文」把 `effects` 留空即可（某些卷軸設計可能有用）。

---

### 符文設計心法

| 定位 | 特徵建議 |
|------|----------|
| 主力符文（Common） | 單屬性±1~3，效果穩定，Cycle |
| 組合符文（Rare） | 雙屬性（一正一負），或單屬性±4~5，Cycle |
| 爆發符文（Legendary / Consumable） | 大幅單屬性±6~10，或跨三屬性，Consumable |
| 平衡符文 | 正負抵消（水分+3 溫度-3），搭配特定卷軸才有用 |

---

## 2. 卷軸 Scroll

**作用**：卷軸決定「符文怎麼計算」。玩家每回合選一張卷軸，把手牌符文填入槽位後送出。

### 建立方式

```
Assets 任意資料夾 → 右鍵 → Create → SpellShop → Scroll
```

### Inspector 欄位說明

| 欄位 | 型別 | 說明 |
|------|------|------|
| `scrollName` | string | 顯示名稱 |
| `description` | string | 說明文字 |
| `icon` | Sprite | 圖示 |
| `slotCount` | int（1~6） | 符文槽位數量，決定一次要放幾張符文 |
| `modifierType` | 下表 | 結算規則 |
| `multiplier` | float | 倍率（MultiplyAll / TagMultiply 用） |
| `tagFilter` | RuneTag | 標籤篩選（TagMultiply 用） |

### 修飾規則（`modifierType`）詳解

#### `DirectAdd` — 直接加總
最基本的卷軸，把所有符文的效果直接相加。

```
範例：槽位 = 2
  符文A：水分 +2
  符文B：水分 +1, 溫度 -1
  結果：水分 +3, 溫度 -1
```

#### `MultiplyAll` — 全效果倍率
把所有符文效果加總後，整體乘以 `multiplier`（四捨五入）。

```
範例：槽位 = 2, multiplier = 2.0
  符文A：水分 +2
  符文B：溫度 +1
  結果：水分 +4, 溫度 +2  （×2）
```

> 負數 multiplier 等同 Invert（效果反轉），一般不建議。
> 推薦 multiplier 範圍：1.5 ~ 3.0

#### `PositiveOnly` — 只取正值
忽略所有負值效果，只計算正的部分。

```
範例：槽位 = 2
  符文A：水分 +3, 溫度 -2
  符文B：水分 -1, 亮度 +1
  結果：水分 +3, 亮度 +1   （負值全部忽略）
```

> 搭配「帶負效果的強力符文」使用，讓玩家安心放副作用符文。

#### `TagMultiply` — 標籤倍率
指定 `tagFilter` 標籤的符文效果乘以 `multiplier`，其餘符文效果 DirectAdd。

```
範例：槽位 = 3, tagFilter = Water, multiplier = 2.0
  符文A (Water)：水分 +2, 溫度 -1  → ×2 → 水分 +4, 溫度 -2
  符文B (Fire) ：溫度 +2             → ×1 → 溫度 +2
  符文C (Water)：水分 +1             → ×2 → 水分 +2
  結果：水分 +6, 溫度 +0
```

#### `Average` — 效果取平均
所有符文效果加總後除以符文數量（小數四捨五入）。

```
範例：槽位 = 3
  符文A：水分 +6
  符文B：水分 +0
  符文C：水分 +3
  結果：水分 +3  （(6+0+3)÷3 = 3）
```

> 讓高強度符文不會破壞平衡，適合「槽位多但輸出穩定」的設計。

#### `Invert` — 效果反轉
所有效果正負對調。

```
範例：槽位 = 2
  符文A：水分 +2, 溫度 -1
  符文B：亮度 +3
  結果：水分 -2, 溫度 +1, 亮度 -3
```

> 進階設計：讓玩家用「降低某屬性的符文」搭配 Invert 卷軸來提升另一屬性。

---

### 卷軸設計心法

| 定位 | 建議設計 |
|------|----------|
| 入門卷軸 | DirectAdd, slotCount = 2~3 |
| 爆發卷軸 | MultiplyAll × 2.0, slotCount = 2 |
| 穩定卷軸 | Average, slotCount = 4 |
| 主題卷軸 | TagMultiply（Water×2）, slotCount = 3 |
| 謎題卷軸 | Invert, slotCount = 2（玩家要反向思考） |
| 容錯卷軸 | PositiveOnly, slotCount = 4（可以放壞符文） |

---

## 3. 顧客 Customer

**作用**：顧客帶著需求來店裡，玩家在耐心耗盡前達成需求可獲得業績獎勵。

### 建立方式

```
Assets 任意資料夾 → 右鍵 → Create → SpellShop → Customer
```

### Inspector 欄位說明

#### 基本資料

| 欄位 | 說明 |
|------|------|
| `customerName` | 顯示名稱，例如「迷途旅人」 |
| `flavorText` | 對話文字（增加代入感），例如「我需要一個溫暖的地方...」 |
| `portrait` | 顯示立繪 Sprite |

#### 耐心

| 欄位 | 說明 |
|------|------|
| `maxPatience` | 最多幾回合（1~10）。耗盡後顧客憤怒離去，扣 scorePenalty |

#### 獎懲

| 欄位 | 說明 |
|------|------|
| `scoreReward` | 需求達成時給予的業績（建議 5~30） |
| `scorePenalty` | 耐心耗盡時扣除的業績（填正值，例如 5，套用時為 -5） |
| `bonusRewards` | 額外獎勵（符文/卷軸），詳見[獎勵系統](#5-獎勵系統-reward) |

---

### 需求（`requirements[]`）

每個顧客可以有多個需求，**所有需求同時滿足**才算通關。需求有三種類型：

---

#### 類型 A：`Absolute` — 環境絕對值

「當前環境數值滿足某條件」

| 欄位 | 說明 |
|------|------|
| `attribute` | 要判定哪個屬性（亮度/水分/溫度） |
| `compareOp` | 比較方式（見下表） |
| `targetValue` | 目標數值 |
| `targetValueMax` | 範圍上限（只有 InRange 用到） |

| compareOp | 說明 | 範例 |
|-----------|------|------|
| `GreaterEqual` | ≥ 某值 | 「溫度 >= 10」→ 溫度至少要 10 |
| `LessEqual` | ≤ 某值 | 「水分 <= 5」→ 水分不能超過 5 |
| `Equal` | = 某值 | 「亮度 == 15」→ 亮度剛好要 15（很難，謹慎使用） |
| `InRange` | 在範圍內 | 「溫度 8~12」→ 溫度要在 8 到 12 之間 |

```
InRange 範例填法：
  type = Absolute
  attribute = Temperature
  compareOp = InRange
  targetValue = 8       ← 下限
  targetValueMax = 12   ← 上限
```

---

#### 類型 B：`RelativeChange` — 累積變化量

「自顧客到來後，某屬性累積增加/減少了多少」

| 欄位 | 說明 |
|------|------|
| `attribute` | 要追蹤哪個屬性 |
| `requiredDelta` | 需要的累積變化量（正值 = 累積增加；負值 = 累積減少） |

```
範例：讓水分從顧客來時累積增加 5 以上
  type = RelativeChange
  attribute = Moisture
  requiredDelta = 5   ← 正值，需要增加

範例：讓溫度從顧客來時累積減少 3 以上
  type = RelativeChange
  attribute = Temperature
  requiredDelta = -3  ← 負值，需要減少
```

> **注意**：這是從顧客到來那一刻的環境值開始算差值，不是從遊戲開始算。
> 讓玩家感覺「我需要推著這個屬性朝某個方向走」，比較有動態感。

---

#### 類型 C：`Stability` — 連續維持條件

「某屬性需要連續 N 回合都滿足某條件」

| 欄位 | 說明 |
|------|------|
| `attribute` | 要維持哪個屬性 |
| `compareOp` | 比較方式（同 Absolute） |
| `targetValue` | 目標數值 |
| `targetValueMax` | 範圍上限（InRange 用） |
| `stabilityTurns` | 需要連續幾回合成立 |

```
範例：溫度需維持在 8~12 之間，連續 2 回合
  type = Stability
  attribute = Temperature
  compareOp = InRange
  targetValue = 8
  targetValueMax = 12
  stabilityTurns = 2
```

> **注意**：條件只要有一回合不成立，連續計數就歸零重算。
> stabilityTurns = 1 等同 Absolute（每回合判定一次），設 2 以上才有「維持」意涵。

---

### 顧客設計心法

| 難度 | 建議設計 |
|------|----------|
| 簡單客人 | 1 個 Absolute 需求，寬鬆範圍（InRange 4~16），maxPatience = 4~5 |
| 普通客人 | 2 個需求（Absolute + RelativeChange），maxPatience = 3 |
| 困難客人 | 3 個需求或含 Stability，maxPatience = 2 |
| BOSS 客人 | 需求衝突（需要高溫同時需要高水分），maxPatience = 5~7 |

**需求衝突的張力範例：**
- 「水分 >= 14」+ 「溫度 <= 6」→ 水/火對立，需要玩家精確操作
- 「亮度 == 10」→ 極難的精確需求，搭配高報酬
- 「溫度維持 8~12，連續 3 回合」→ 需要玩家規劃多回合

---

## 4. 遺物 Relic

**作用**：遺物是「常駐 Buff/Debuff」，每回合 Phase 2 自動判定，給予獎勵或懲罰。
玩家需要在滿足遺物條件的同時兼顧顧客需求，形成長期策略壓力。

### 建立方式

```
Assets 任意資料夾 → 右鍵 → Create → SpellShop → Relic
```

### Inspector 欄位說明

#### 基本資料

| 欄位 | 說明 |
|------|------|
| `relicName` | 顯示名稱 |
| `description` | 說明文字 |
| `icon` | 圖示 Sprite |
| `displayOrder` | 結算順序（數字小的先算），建議從 0 開始依序填 |

---

#### 滿意條件（`satisfiedCondition`）

每回合判定當前環境是否符合條件，決定這回合遺物是「滿意」還是「不滿意」。

| 欄位 | 說明 |
|------|------|
| `attribute` | 要判定的屬性（亮度/水分/溫度） |
| `compareOp` | GreaterEqual / LessEqual / Equal / InRange |
| `targetValue` | 目標數值 |
| `rangeMin` | 範圍下限（InRange 用） |
| `rangeMax` | 範圍上限（InRange 用） |

```
InRange 範例：
  attribute = Brightness
  compareOp = InRange
  rangeMin = 5
  rangeMax = 15
  → 亮度在 5~15 之間即為「滿意」
```

---

#### 滿意效果

條件成立時，**每回合**套用：

| 欄位 | 說明 |
|------|------|
| `satisfiedEnvEffects[]` | 對環境屬性的影響（可多條，可正可負） |
| `satisfiedScoreChange` | 業績變化（正=加分，負=扣分） |
| `satisfiedRewards` | 額外獎勵（符文/卷軸），詳見獎勵系統 |

---

#### 不滿意效果

條件不成立時，**每回合**套用，且計數 +1：

| 欄位 | 說明 |
|------|------|
| `unsatisfiedEnvEffects[]` | 副作用（可留空） |
| `unsatisfiedScoreChange` | 業績扣分（通常填負值） |

---

#### 懲罰設定

不滿意計數達到 `unsatisfiedLimit` 時觸發（**計數不重置，之後每回合繼續觸發**）：

| 欄位 | 說明 |
|------|------|
| `unsatisfiedLimit` | 容忍上限（1~10 回合） |
| `punishmentType` | 懲罰類型（見下表） |

| 懲罰類型 | 說明 | 額外欄位 |
|----------|------|----------|
| `EnvironmentShock` | 強制改變某屬性 | `shockAttribute`（哪個屬性）<br>`shockValue`（改變量或目標值）<br>`shockIsForceSet`（true=強制設為此值，false=加減 delta） |
| `ScoreReset` | 業績直接歸零 | — |
| `PlayerDeath` | 直接 Game Over | — |

```
EnvironmentShock 範例（強制設定）：
  punishmentType = EnvironmentShock
  shockAttribute = Brightness
  shockValue = 0
  shockIsForceSet = true
  → 觸發後亮度強制變為 0（黑暗降臨）

EnvironmentShock 範例（delta）：
  punishmentType = EnvironmentShock
  shockAttribute = Temperature
  shockValue = -8
  shockIsForceSet = false
  → 觸發後溫度 -8
```

---

### 遺物設計心法

| 類型 | 設計建議 |
|------|----------|
| 純 Buff 遺物 | 條件容易達成（InRange 大範圍）<br>滿意 → 業績 +3~5 或環境小幅調整 |
| 風險/報酬遺物 | 條件難（精確值 or 極端值）<br>滿意 → 大業績 +10~20<br>不滿意 → 業績 -5 |
| 壓力遺物 | 條件中等，懲罰嚴重（ScoreReset 或 Death） |
| 環境塑形遺物 | 滿意 → 自動推某屬性<br>不滿意 → 推反方向 |

**條件與懲罰搭配建議：**

```
輕量遺物（新手友善）：
  滿意：亮度 >= 5（容易達成）
  滿意效果：業績 +3
  不滿意效果：業績 -0
  unsatisfiedLimit = 5（5 回合才懲罰）
  punishmentType = EnvironmentShock（亮度 -5，輕微）

重型遺物（進階）：
  滿意：溫度 InRange 9~11（窄範圍）
  滿意效果：業績 +10
  不滿意效果：業績 -5
  unsatisfiedLimit = 2（2 回合就懲罰）
  punishmentType = ScoreReset（業績歸零）
```

---

## 5. 獎勵系統 Reward

`RewardEntry` 可以加在：
- `CustomerData.bonusRewards`：顧客需求達成時一次性給予
- `RelicData.satisfiedRewards`：遺物每回合滿意時給予（小心平衡）

### 類型說明

#### `Score` — 直接給業績

| 欄位 | 說明 |
|------|------|
| `scoreAmount` | 給幾點業績 |

> 建議直接用 `CustomerData.scoreReward` 欄位，這個留給特殊附加用途。

#### `RandomRune` — 給符文

| 欄位 | 說明 |
|------|------|
| `runePool[]` | 符文獎勵池，從中隨機選取 |
| `runeCount` | 給幾張（不超過 runePool 大小） |

```
範例：3 選 1 隨機給一張水系符文
  type = RandomRune
  runePool = [水球符文, 大波浪符文, 暴雨符文]
  runeCount = 1
```

#### `RandomScroll` — 給卷軸

| 欄位 | 說明 |
|------|------|
| `scrollPool[]` | 卷軸獎勵池 |
| `scrollCount` | 給幾張 |

---

## 6. 實際範例庫

### 符文範例

| 名稱 | runeType | tags | effects |
|------|----------|------|---------|
| 水球符文 | Cycle | Water | 水分 +2, 溫度 -1 |
| 火焰符文 | Cycle | Fire | 溫度 +2, 水分 -1 |
| 光芒符文 | Cycle | Light | 亮度 +2 |
| 暗影符文 | Cycle | Dark | 亮度 -2 |
| 清風符文 | Cycle | Wind | 亮度 +1, 水分 +1, 溫度 -1 |
| 烈焰符文 | Consumable | Fire | 溫度 +5, 水分 -3 |（消耗型，強力）
| 虛空符文 | Cycle | Neutral | （無效果）|（某些卷軸有用）
| 雙水符文 | Rare | Water | 水分 +3 |
| 冰霜符文 | Rare | Water+Dark | 水分 +2, 溫度 -3, 亮度 -1 |

---

### 卷軸範例

| 名稱 | slotCount | modifierType | 其他 |
|------|-----------|--------------|------|
| 基礎術式 | 2 | DirectAdd | — |
| 強化術式 | 2 | MultiplyAll | multiplier=2.0 |
| 水系奧義 | 3 | TagMultiply | tagFilter=Water, multiplier=2.0 |
| 均衡術式 | 4 | Average | — |
| 純淨術式 | 3 | PositiveOnly | — |
| 逆轉術式 | 2 | Invert | — |

---

### 顧客範例

**「旅行商人」**（簡單）
```
customerName = 旅行商人
flavorText = 長途跋涉，需要溫暖的休憩之所。
maxPatience = 4
scoreReward = 10
scorePenalty = 3
需求：
  [0] Absolute — 溫度 >= 10
```

**「冰雪精靈」**（普通）
```
customerName = 冰雪精靈
flavorText = 炎熱讓我感到不適，請幫我調節氣候。
maxPatience = 3
scoreReward = 18
scorePenalty = 6
需求：
  [0] Absolute — 溫度 <= 7
  [1] RelativeChange — 溫度累積減少 >= -4（requiredDelta = -4）
```

**「光之祭司」**（困難）
```
customerName = 光之祭司
flavorText = 光明與穩定，方能完成儀式。
maxPatience = 5
scoreReward = 30
scorePenalty = 10
需求：
  [0] Absolute — 亮度 >= 12
  [1] Stability — 亮度 InRange 10~20，連續 3 回合
  [2] RelativeChange — 亮度累積增加 >= 6
```

---

### 遺物範例

**「苔蘚寶石」**（輕量 Buff）
```
relicName = 苔蘚寶石
滿意條件：水分 >= 8
滿意效果：業績 +3
不滿意效果：業績 -0
unsatisfiedLimit = 5
punishmentType = EnvironmentShock（水分 -3, delta）
```

**「冥火燭台」**（高風險高回報）
```
relicName = 冥火燭台
滿意條件：溫度 InRange 9~11
滿意效果：業績 +10
不滿意效果：業績 -5
unsatisfiedLimit = 2
punishmentType = ScoreReset
```

**「黑夜面具」**（環境塑形）
```
relicName = 黑夜面具
滿意條件：亮度 <= 5
滿意效果：亮度 -1（每回合繼續壓低）, 業績 +5
不滿意效果：亮度 +2（往反方向推）
unsatisfiedLimit = 4
punishmentType = EnvironmentShock（亮度強制設為 0）
```

---

## 7. 設計注意事項

### ✅ 建議

- **新符文先在 Common 測試**，確認數值影響後再升稀有度
- **遺物滿意條件盡量和顧客需求有時衝突、時配合**的關係，製造策略決策點
- **RequiredDelta 用正或負要想清楚**：正值 = 需要累積增加，負值 = 需要累積減少
- **InRange 優先於 Equal**：Equal 幾乎不可能精確達成，只在特殊挑戰關使用
- **Stability 的 stabilityTurns 建議不超過 3**：回合太長玩家會感覺遙遙無期

### ⚠️ 注意

- **環境數值範圍是 0~20**，超出範圍的效果會被 Clamp，設計時不要假設能到 21+
- **極端事件是自動觸發的**：亮度碰 0 = 黑暗降臨（遺物不滿意計數 +1），這會讓壓低亮度的遺物更難維持
- **Consumable 符文用後即失去**，獎勵獲得的消耗型符文要讓玩家覺得值得
- **遺物懲罰達到 limit 後不重置**，之後每回合都會繼續觸發，PlayerDeath 遺物要謹慎

### 🔢 數值平衡參考

| 指標 | 建議範圍 |
|------|----------|
| 普通顧客業績獎勵 | 8 ~ 15 |
| 困難顧客業績獎勵 | 20 ~ 40 |
| 遺物每回合業績 | ±3 ~ ±10 |
| 符文單屬性效果 | ±1 ~ ±5（Common）, ±4 ~ ±8（Rare+） |
| 顧客耐心 | 2~5 回合（簡單）, 1~3 回合（困難） |
| 遺物容忍上限 | 3~5（輕量）, 1~2（重型） |

---

*建立日期：2026-03-21*
*版本：v1.0*

# Relic 擴充計畫

日期：2026-03-22

---

## 一、設計內容總表

### 遺物（Relic）

| 遺物 | 滿足效果 | 觸發條件 | 副作用（懲罰） |
|------|----------|----------|----------------|
| 小黃花 | 每回合亮度 +1 | 亮度 > 3 **且** 水分 > 3 | 未達成 2 回合 → 玩家死亡 + 給予枯萎 ×2 |
| 煤油燈 | 每回合亮度 +、溫度 + | 溫度 > 4 **且** 水分 < 4 | 亮度 -5、溫度 -5（重新滿足條件後自動恢復） |
| 食人花 | 每回合給予生長 ×2 | 亮度 > 5 **且** 水分 > 3 | 未達成 4 回合 → 玩家死亡 |
| 能量水晶 | 每回合變化一張牌 | 溫度 > 5 **且** 亮度 > 4 | 未達成 → 效果暫停（無額外懲罰） |
| 環境燈 | 三屬性各往 5 移動 1 | 亮度 == 5 **且** 水分 == 5 **且** 溫度 == 5 | 未達成 3 回合 → 三屬性強制設為 10 |
| 一碗白色粉末 | 每回合水分 -、溫度 - | 水分 < 7 | 未達成 1 回合 → 失去此遺物 |
| 52Hz的金魚草 | 每回合水分 +2 | 水分 > 6（即 ≥ 7） | 未達成 2 回合 → 水分歸零 |

### 環境效果（極端值觸發）

| 效果名 | 觸發條件 | 具體效果 |
|--------|----------|----------|
| 晃眼 | 亮度 = 10 | 趕走當前顧客 |
| 失明 | 亮度 = 0 | 失去視野（UI 遮罩） |
| 燥熱 | 溫度 = 10 | 捨棄手牌中隨機 3 張 |
| 寒凍 | 溫度 = 0 | 本回合少抽 2 張貼紙 |
| 過潮 | 水分 = 10 | 往牌組塞入 5 張「濕潤」 |
| 乾燥 | 水分 = 0 | 往牌組塞入 5 張「枯萎」 |

### 卷軸效果（Scroll Effects）

| 效果名 | 說明 |
|--------|------|
| 單格 | 必須且只能放入 1 張貼紙 |
| 雙格 | 必須且只能放入 2 張貼紙 |
| 三格 | 必須且只能放入 3 張貼紙 |
| 光封印 | 該卷軸送出後不影響亮度數值 |
| 水封印 | 該卷軸送出後不影響水分數值 |
| 熱封印 | 該卷軸送出後不影響溫度數值 |

---

## 二、功能可行性分析

### 遺物

| 遺物 | 效果 | 條件 | 副作用 | 整體 |
|------|------|------|--------|------|
| **52Hz金魚草** | ✅ satisfiedEnvEffects | ✅ GreaterEqual 7 | ✅ EnvironmentShock force 0 | **全部可做** |
| **小黃花** | ✅ | ❌ 需多條件 | ✅ PlayerDeath / ❌ 懲罰給 Rune | **缺 2 項** |
| **煤油燈** | ✅ | ❌ 需多條件 | ❌ 懲罰多屬性 shock / ✅ 恢復已有 | **缺 2 項** |
| **食人花** | ✅（需建生長 Rune 資產） | ❌ 需多條件 | ✅ PlayerDeath | **缺 1 項** |
| **能量水晶** | ❌ 全新邏輯 | ❌ 需多條件 | ✅ 預設行為（不滿足=不給效果） | **缺 2 項** |
| **環境燈** | ❌ 動態往目標移動 | ❌ 三條件 AND | ❌ 懲罰多屬性 | **缺 3 項** |
| **一碗白色粉末** | ✅ | ✅ LessEqual 6 | ❌ 新懲罰類型 DiscardSelf | **缺 1 項** |

### 環境效果

| 效果 | 事件觸發 | 具體效果 |
|------|----------|----------|
| 失明（亮度=0） | ⚠️ OnDarknessTriggered 已有 | ❌ 「失去視野」UI 效果未做 |
| 其他 5 種 | ❌ 需新增閾值事件 | ❌ 各自需要新邏輯 |

### 卷軸效果

| 效果 | 狀態 |
|------|------|
| 單格 / 雙格 / 三格 | ✅ ScrollData.slotCount 已完整支援 |
| 光 / 水 / 熱封印 | ❌ ScrollProcessor 需新增屬性過濾 |

---

## 三、需要新實作的功能

### 小改動（改現有類別，影響範圍小）

| 功能 | 改動位置 | 說明 |
|------|----------|------|
| Relic 多條件 AND/OR | `RelicData.cs`、`RelicInstance.cs` | `satisfiedCondition` → `List<RelicCondition>` + `ConditionLogic` enum；新增 `EvaluateAll()` 靜態方法 |
| Relic 懲罰：失去自身 | `GameDefinitions.cs`、`RelicManager.cs` | 新增 `PunishmentType.DiscardSelf`，ApplyPunishment 呼叫 DiscardRelic |
| Relic 懲罰：給予 Rune | `GameDefinitions.cs`、`RelicData.cs`、`RelicManager.cs` | 新增 `PunishmentType.GrantRunes`，RelicData 加欄位 `punishmentRunes: List<RewardEntry>`，RelicManager → DeckManager.AddRune |
| Relic 懲罰：多屬性 shock | `RelicData.cs`、`RelicManager.cs` | 將 `shockAttribute/shockValue/shockIsForceSet` 改為 `punishmentShocks: RuneEffect[]`（或同結構新 struct），ApplyPunishment 迭代處理 |
| 卷軸封印效果 | `ScrollData.cs`、`ScrollProcessor.cs` | ScrollData 加 `EnvAttribute sealedAttributes`（Flags enum），ScrollProcessor 過濾對應屬性輸出 |

### 中改動（需新系統或跨多個類別）

| 功能 | 改動位置 | 說明 |
|------|----------|------|
| 動態「往目標值移動」效果 | `RelicData.cs`、`RelicManager.cs`、`EnvironmentManager.cs` | 新增 `RelicEffectType.MoveToward(target, step)`，RelicManager 讀取當前環境值計算方向後呼叫 ApplyDelta |
| 環境極端值觸發系統 | `EnvironmentManager.cs` | 擴充現有 OnDarknessTriggered 邏輯，新增 6 個閾值事件：`OnBrightnessMax`、`OnBrightnessMin`、`OnTemperatureMax`、`OnTemperatureMin`、`OnMoistureMax`、`OnMoistureMin` |
| 燥熱：捨棄隨機手牌 3 張 | `DeckManager.cs`、訂閱 OnTemperatureMax | 新增 `DiscardRandomFromHand(int count)` |
| 寒凍：本回合少抽 2 張 | `DeckManager.cs`、訂閱 OnTemperatureMin | 新增 `_drawReductionThisTurn` 欄位，DrawCards 時套用 |
| 過潮/乾燥：塞牌入牌組 | `DeckManager.cs`、訂閱 OnMoistureMax/Min | `AddRune()` 已有，需批次呼叫並傳入指定 RuneData（濕潤/枯萎 Rune 資產需建立） |

### 大改動（全新設計，影響範圍廣）

| 功能 | 改動位置 | 說明 |
|------|----------|------|
| 每回合變化一張牌（能量水晶） | `DeckManager.cs`、`RuneData.cs`、`RelicManager.cs` | 需先定義「變化」的語義（隨機替換/升級/轉換為另一張），再實作對應邏輯；目前完全未有相關基礎設施 |
| 失去視野（失明） | UI 層、`EnvironmentManager` 事件 | 訂閱 OnBrightnessMin，對玩家工作台/環境面板加遮罩或模糊效果；需與 UI 設計師確認視覺方案 |
| 趕走顧客（晃眼） | `CustomerManager.cs`（或等效類別）、訂閱 OnBrightnessMax | 需確認強制結束顧客時的評分規則（算失敗？算中性？），再實作 ForceLeave 方法 |

---

## 四、建議實作順序

```
Phase 1 — Relic 多條件（解鎖大部分遺物設計）
  └─ RelicData 改為 List<RelicCondition> + ConditionLogic
  └─ RelicInstance.Tick() 更新
  └─ GameFacade 顯示文字更新

Phase 2 — 環境極端值觸發系統（後續所有環境效果的基礎建設）
  └─ EnvironmentManager 擴充 6 個閾值事件

Phase 3 — Relic 懲罰擴充
  └─ 多屬性 shock（煤油燈、環境燈）
  └─ 給予 Rune（小黃花）
  └─ 失去自身（一碗白色粉末）

Phase 4 — 卷軸封印效果
  └─ ScrollData + ScrollProcessor 過濾邏輯

Phase 5 — 環境效果具體行為
  └─ 燥熱、寒凍、過潮、乾燥（需 DeckManager 新方法）
  └─ 建立濕潤、枯萎 Rune 資產

Phase 6 — 複雜功能（最後處理）
  └─ 趕走顧客（晃眼）
  └─ 失去視野（失明）
  └─ 每回合變化一張牌（能量水晶）— 需先定義語義
  └─ 動態往目標移動效果（環境燈）
```

---

## 五、新增 Rune 資產需求

以下 Rune 需要建立 ScriptableObject 資產：

| Rune 名稱 | 效果 | 用途 |
|-----------|------|------|
| 枯萎 | 水分 - | 小黃花懲罰、乾燥環境效果塞入牌組 |
| 生長 | 水分 +（或複合效果） | 食人花每回合給予 |
| 濕潤 | 水分 + | 過潮環境效果塞入牌組 |

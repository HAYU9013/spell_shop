# 咒語店 — 24H Gamejam Todolist

> **人員**：Coder ×1、Art ×1
> **引擎**：Unity + DOTween
> **目標**：可完整遊玩的 MVP，以「能跑完一局」為唯一標準

---

## MVP 砍掉的功能（時間不夠，全部後移）

| 砍掉 | 原因 |
|------|------|
| 批發商系統 | 需要額外場景與 UI |
| 標籤連鎖加成 | 可用最基本標籤實作即可 |
| 拖曳放符文 | 改為「點選符文 → 自動填入下一空槽」 |
| 動畫演出 | 只留數值跳動的 DOTween tween，無過場動畫 |

## MVP 保留的進階功能

| 保留 | 說明 |
|------|------|
| 遺物系統 | 2 個遺物（日晷、苔蘚石），含不滿意計數與懲罰 |
| 相對變化 / 穩定性需求 | 完整實作三種需求類型（絕對值、相對變化、穩定性） |
| 主選單 | 簡易主選單，提供「關卡模式」與「無限模式」兩種入口 |

---

## 時間軸總覽

```
H00 ━━ 專案建立 + 分工確認
H01 ━━ 資料層（Enum / SO 定義）           [Coder]
         UI 草稿確認 + 美術風格定調         [Art]
H03 ━━ 環境系統 + 牌庫系統               [Coder]
         背景 + 環境面板 UI               [Art]
H06 ━━ 卷軸結算 + 工作台邏輯             [Coder]
         符文卡牌 + 工作台 UI             [Art]
H09 ━━ 顧客系統（含三種需求）+ 回合流程   [Coder]
         顧客立繪 + 顧客面板 UI           [Art]  ← 🔴 中期同步
H12 ━━ 遺物系統                         [Coder]
         遺物 UI + 遺物圖示              [Art]
H14 ━━ 主選單 + UI 接線 + 完整跑通一局    [Both]
H17 ━━ 內容填充（符文/卷軸/顧客/遺物資料） [Both]
H19 ━━ 遊戲結束 + 基礎修蟲               [Both]  ← 🔴 功能凍結
H21 ━━ 視覺潤色 + 音效                   [Art / Coder 輔助]
H23 ━━ 最終測試 + 打包                   [Both]
H24 ━━ 交件
```

---

## H00 — 專案建立（0:00 ~ 1:00）

### Coder
- [ ] 建立 Unity 專案，確認 Unity 版本
- [ ] 匯入 DOTween
- [ ] 建立資料夾結構（Scripts / Data / Prefabs / Scenes）
- [ ] 建立 `GameScene.unity`，佈置 Canvas（Screen Space - Overlay）

### Art
- [ ] 確認視覺風格（手繪 / 像素 / 扁平）
- [ ] 畫出主畫面 UI 草稿（參考 GameDescript.md §9.1 配置）
- [ ] 定義色板（背景色、主色、強調色）

### 🔴 同步確認
- UI 分區位置（環境面板、顧客區、工作台、手牌區）的尺寸與座標對齊

---

## H01~H03 — 資料層（1:00 ~ 3:00）

### Coder
- [ ] `Constants.cs` — 數值常數（初始業績30、環境0~20）
- [ ] 定義所有 Enum（`EnvAttribute` / `Rarity` / `RuneType` / `RuneTag` / `ScrollModifierType` / `TurnPhase` / `RequirementType` / `CompareOperator` / `RelicPunishment`）
- [ ] `RuneData.cs` — ScriptableObject，欄位：名稱、效果列表、稀有度、標籤、RuneType（循環/消耗）
- [ ] `RuneEffect.cs` — `[EnvAttribute attr, int delta]` 資料結構
- [ ] `ScrollData.cs` — ScriptableObject，欄位：名稱、槽數、修飾類型、標籤限制
- [ ] `RequirementData.cs` — 需求資料結構（type、attribute、compareOp、targetValue、targetValueMax、requiredDelta、stabilityTurns）
- [ ] `CustomerData.cs` — ScriptableObject，欄位：名稱、`List<RequirementData>` 需求列表、耐心、業績獎勵、業績懲罰
- [ ] `RelicData.cs` — ScriptableObject，欄位：名稱、滿意條件、滿意效果、不滿意效果、不滿意容忍上限、懲罰類型、懲罰效果

### Art
- [ ] 繪製背景底圖（店內場景）
- [ ] 設計環境數值 icon（亮度、水分、溫度、業績的圖示）
- [ ] 設計空白卡牌框架（符文卡框）
- [ ] 設計遺物圖示 2 張（日晷、苔蘚石）

### 🔴 同步確認
- `RuneData` 的欄位是否滿足美術卡牌需要顯示的資訊（名稱、圖示欄位）
- `RelicData` 圖示尺寸與遺物區版面對齊

---

## H03~H06 — 環境 + 牌庫系統（3:00 ~ 6:00）

### Coder
- [ ] `EnvironmentData.cs` — `GetValue / SetValue / ApplyDelta / Clone()`
- [ ] `EnvironmentManager.cs`
  - [ ] 持有 `EnvironmentData` 單例
  - [ ] `ApplyEffects(List<RuneEffect>)` — 套用效果並檢查邊界
  - [ ] `CheckExtremeEvent()` — 邊界觸發後執行一次性懲罰（業績-10 等）
- [ ] `DeckManager.cs`
  - [ ] `List<RuneData> drawPile / discardPile`
  - [ ] `DrawHand(int count)` — 抽牌，牌庫空了自動洗牌（消耗型不回收）
  - [ ] `AddRune(RuneData)` — 顧客獎勵時新增符文
- [ ] `ScrollInventory.cs` — `List<ScrollData>`，`UseScroll(ScrollData)` 移除一張

### Art
- [ ] 實作環境面板 UI Prefab（四個數值 Text + Icon）
- [ ] 製作業績數值的跳動動畫測試（DOTween `DOCounter`）

---

## H06~H10 — 卷軸結算 + 工作台（6:00 ~ 10:00）

### Coder
- [ ] `ScrollProcessor.cs` — 核心結算邏輯
  - [ ] `DirectAdd`：效果直接疊加
  - [ ] `MultiplyAll`：所有 delta × 倍率
  - [ ] `PositiveOnly`：delta < 0 的效果忽略
  - [ ] `Average`：所有 delta 加總 / 數量（保留 float，最後套用 int）
  - [ ] `Invert`：所有 delta 正負反轉
- [ ] `WorkbenchManager.cs`
  - [ ] `SelectScroll(ScrollData)` — 設定當前卷軸，清空槽位
  - [ ] `PlaceRune(RuneData)` — 點選手牌符文後填入下一個空槽
  - [ ] `RemoveRune(int slotIndex)` — 取消放置
  - [ ] `CanSubmit()` — 槽位全滿才回傳 true
  - [ ] `Submit()` → 呼叫 `ScrollProcessor` → 呼叫 `EnvironmentManager.ApplyEffects`

### Art
- [ ] 工作台面板 Prefab（卷軸選擇按鈕列、符文槽位 ×4、送出按鈕）
- [ ] 符文槽位元件（空槽 / 已放入 兩個狀態圖示）
- [ ] 卷軸選擇 UI（Button 列，顯示卷軸名稱與槽數）
- [ ] 繪製 5~6 張符文圖示（水珠、小燭、寒冰、黑霧、蒸氣 + 1 備用）

---

## H09~H12 — 顧客系統（含三種需求）+ 回合流程（9:00 ~ 12:00）

### 🔴 中期同步（9:00，30 分鐘）
- Coder 展示目前 WorkbenchManager.Submit() 是否能正確更新環境數值
- Art 確認卡牌框 + 工作台 Prefab 是否可以掛上 Coder 的腳本
- 確認剩餘工時，必要時再次砍功能

### Coder
- [ ] `IRequirement.cs` — 需求介面 `IsSatisfied(RequirementContext)`
- [ ] `AbsoluteRequirement.cs` — 絕對值條件判定（如 溫度 >= 8）
- [ ] `RelativeChangeRequirement.cs` — 相對變化條件判定（基於顧客到來時的環境快照，累積計算）
- [ ] `StabilityRequirement.cs` — 穩定條件判定（連續 N 回合維持範圍）
- [ ] `RequirementFactory.cs` — 根據 `RequirementData.type` 建立對應實例
- [ ] `CustomerInstance.cs`
  - [ ] 持有 `CustomerData` + 當前耐心計數
  - [ ] `ArrivalSnapshot` — 顧客到來時的環境快照
  - [ ] `AccumulatedDeltas` — 累積變化量追蹤（相對變化需求用）
  - [ ] `StabilityProgress` — 穩定條件進度追蹤
  - [ ] `CheckRequirements(RequirementContext)` — 全部需求滿足回傳 true
  - [ ] `UpdateTracking(EnvironmentData)` — 每回合更新累積 / 穩定追蹤
  - [ ] `TickPatience()` — 耐心 -1，回傳是否耐心耗盡
- [ ] `CustomerManager.cs`
  - [ ] 持有顧客隊列 `Queue<CustomerData>`
  - [ ] `SpawnNextCustomer(EnvironmentData)` — 從隊列取出，建立 Instance 並記錄 ArrivalSnapshot
  - [ ] `IsQueueEmpty()` — 隊列空 = 關卡通關
- [ ] `TurnManager.cs` — 驅動完整 8 階段回合
  ```
  CustomerArrival → RelicTrigger → DrawCards → PlayerAction（等待 Submit）
  → EnvironmentResolve → CustomerJudge → PatienceCheck → RelicUpdate → (下一回合)
  ```
- [ ] `GameManager.cs`
  - [ ] 監聽 `score <= 0` → 觸發 Game Over
  - [ ] 監聽 `IsQueueEmpty` → 觸發 Level Clear
  - [ ] 支援兩種遊戲模式（關卡模式 / 無限模式）

### Art
- [ ] 繪製顧客立繪 4 張（落湯雞旅人、感冒農夫、夜盲商人、冰魔法師）
- [ ] 顧客面板 Prefab（立繪、名稱、需求文字、耐心進度條、需求追蹤進度）
- [ ] 手牌區 Prefab（水平排列的卡牌容器）
- [ ] 符文卡牌 Prefab（圖示 + 名稱 + 效果文字 + 類型標記）

---

## H12~H14 — 遺物系統 + 主選單（12:00 ~ 14:00）

### Coder
- [ ] `RelicInstance.cs`
  - [ ] 持有 `RelicData` + 當前不滿意計數
  - [ ] `Tick(EnvironmentData)` — 判定滿意/不滿意，回傳效果
  - [ ] 滿意 → 計數歸零；不滿意 → 計數 +1
  - [ ] 計數 >= 上限 → 觸發懲罰（計數不重置，後續每回合繼續懲罰直到再次滿意）
- [ ] `RelicManager.cs`
  - [ ] 持有 `List<RelicInstance>`
  - [ ] `TickAll(EnvironmentData)` — 依序結算所有遺物
  - [ ] 與 `TurnManager` Phase 2 (RelicTrigger) 及 Phase 8 (RelicUpdate) 整合
- [ ] 主選單場景 `MainMenu.unity`
  - [ ] 「關卡模式」按鈕 → 進入遊戲（有固定顧客隊列，清空通關）
  - [ ] 「無限模式」按鈕 → 進入遊戲（顧客無限生成，撐到 Game Over）
  - [ ] `GameMode` 列舉（`Level` / `Endless`），`GameManager` 根據模式決定行為

### Art
- [ ] 遺物面板 Prefab（遺物圖示列表 + 滿意/不滿意狀態 + 不滿意計數文字）
- [ ] 主選單畫面（標題「Spell Shop」+ 兩個模式按鈕 + 簡單背景）

---

## H14~H17 — UI 全接線 + 跑通一局（14:00 ~ 17:00）

> 這個時段兩人主要合作，Art 負責 Prefab 接線，Coder 負責邏輯綁定

### Coder + Art（合作）
- [ ] 主選單 → 選擇模式 → 進入 GameScene
- [ ] 將 `EnvironmentManager` 資料綁定到 `EnvironmentPanel` 的 Text
- [ ] 將 `DeckManager.DrawHand()` 的結果渲染到 `HandPanel`（動態生成卡牌 Prefab）
- [ ] 點選手牌卡牌 → 呼叫 `WorkbenchManager.PlaceRune()`，更新槽位 UI
- [ ] 卷軸選擇按鈕 → 呼叫 `WorkbenchManager.SelectScroll()`
- [ ] 送出按鈕 → `WorkbenchManager.Submit()` → 更新環境面板
- [ ] `CustomerManager` 渲染當前顧客到 `CustomerPanel`（含需求追蹤進度文字）
- [ ] `RelicManager` 渲染遺物到 `RelicPanel`（含滿意/不滿意狀態與計數）
- [ ] 顧客需求判定結果 → 顯示滿足/失敗提示（簡單 Text 閃爍即可）
- [ ] 遺物懲罰觸發時顯示警告提示
- [ ] 業績數值更新觸發 DOTween 數字跳動

### 驗收標準（H17 前必須達成）
- [ ] 主選單可選「關卡模式」或「無限模式」進入遊戲
- [ ] 能抽到手牌
- [ ] 能選卷軸 + 放符文 + 點送出
- [ ] 環境數值正確更新
- [ ] 顧客三種需求類型（絕對值、相對變化、穩定性）都能正確判定
- [ ] 遺物滿意/不滿意效果正確套用，不滿意計數正確遞增/歸零
- [ ] 業績會增減
- [ ] Score = 0 出現 Game Over 文字
- [ ] 關卡模式：隊列清空 → Level Clear
- [ ] 無限模式：顧客持續生成直到 Game Over

---

## H17~H19 — 內容填充（17:00 ~ 19:00）

> 遊戲邏輯已通，現在填入實際的資料資產

### Coder
- [ ] 建立符文 ScriptableObject 資產（6 種）
  - 水珠（水分+2，循環）
  - 小燭（亮度+1 溫度+1，循環）
  - 寒冰（溫度-3 水分+1，循環）
  - 黑霧（亮度-2 水分+1，循環）
  - 蒸氣（溫度-1 水分-1 亮度+1，循環）
  - 太陽核（亮度+5 溫度+3 水分-2，消耗）
- [ ] 建立卷軸 ScriptableObject 資產（3 種）
  - 基礎卷軸（槽2，DirectAdd）
  - 放大卷軸（槽3，MultiplyAll ×2）
  - 純化卷軸（槽2，PositiveOnly）
- [ ] 建立顧客 ScriptableObject 資產（5 位，含不同需求類型）
  - 落湯雞旅人（絕對值：溫度>=8，耐心2，業績+10）
  - 感冒農夫（相對變化：顧客來到後水分累積減少>=3，耐心3，業績+8）
  - 夜盲商人（絕對值：亮度>=10，耐心1，業績+20）
  - 冰魔法師（複合絕對值：溫度<=2 且 亮度>=5，耐心4，業績+15）
  - 養花老人（穩定條件：連續1回合溫度維持在4~6，耐心3，業績+12）
- [ ] 建立遺物 ScriptableObject 資產（2 個）
  - 日晷（滿意：亮度>=7，滿意效果：亮度+1/回合，不滿意：亮度-1/回合，上限4回合，懲罰：亮度-5）
  - 苔蘚石（滿意：水分4~8，滿意效果：業績+5/回合，不滿意：無，上限3回合，懲罰：水分強制設為10）
- [ ] 設定初始牌庫（水珠×3、小燭×3、蒸氣×2）
- [ ] 設定初始卷軸庫存（基礎卷軸×3）
- [ ] 設定關卡模式顧客隊列（5 位顧客）
- [ ] 設定無限模式顧客池（從所有顧客中隨機抽取）
- [ ] 設定初始遺物（日晷固定裝備，苔蘚石在第3位顧客後出現）

### Art
- [ ] 為 6 張符文補上對應圖示（可使用簡單幾何圖形代替）
- [ ] 業績面板最終樣式
- [ ] 顧客需求文字格式確認（含三種類型的顯示格式）
  - 絕對值：「溫度 >= 8」
  - 相對變化：「水分累積減少 >= 3（目前：-1）」
  - 穩定條件：「溫度維持 4~6（連續：0/1 回合）」

---

## H19~H21 — 遊戲結束 + 修蟲（19:00 ~ 21:00）

### 🔴 功能凍結（19:00）
> 19:00 後不新增功能，只修 Bug

### Coder
- [ ] Game Over 畫面（業績歸零或遺物即死時出現，顯示最終業績 + 通過回合數，「回主選單」按鈕）
- [ ] Level Clear 畫面（關卡模式：顧客隊列清空時出現，顯示通關，「回主選單」按鈕）
- [ ] 「回主選單」 → 回到主選單場景
- [ ] 修正：槽位全滿前送出按鈕不可點擊（灰色）
- [ ] 修正：牌庫空了不崩潰（邊界保護）
- [ ] 修正：顧客隊列空了不崩潰（關卡模式通關 / 無限模式繼續生成）
- [ ] 修正：遺物懲罰觸發 Game Over 時流程正確結束
- [ ] 修正：相對變化累積值在顧客切換時正確重置

### Art
- [ ] Game Over 畫面美術
- [ ] Level Clear 畫面美術
- [ ] 確認所有面板在 1920×1080 與 1280×720 下不爆版

---

## H21~H23 — 視覺潤色 + 音效（21:00 ~ 23:00）

### Art
- [ ] 調整字體（確保中文顯示正常）
- [ ] 符文卡牌 Hover 高亮效果
- [ ] 送出按鈕點擊特效（簡單的縮放 DOTween）
- [ ] 顧客滿意 / 憤怒的表情圖示

### Coder
- [ ] 加入 1~2 個音效（送出卷軸音、顧客離開音）— 若無音效素材則跳過
- [ ] 環境數值超出邊界時螢幕閃紅（`ScreenFlash` DOTween sequence）
- [ ] 業績變動的 DOTween 數字跳動（正值綠色 +N，負值紅色 -N）

---

## H23~H24 — 最終測試 + 打包（23:00 ~ 24:00）

### Both
- [ ] 完整跑一局，確認以下流程無當機
  - [ ] 主選單 → 選擇關卡模式 → 進入遊戲
  - [ ] 主選單 → 選擇無限模式 → 進入遊戲
  - [ ] 抽牌 → 選卷軸 → 放符文 → 送出
  - [ ] 顧客滿足 → 獎勵 → 下一位顧客
  - [ ] 顧客憤怒離開 → 業績扣除
  - [ ] 遺物滿意 → 正向效果套用
  - [ ] 遺物不滿意 → 副作用套用 + 計數增加
  - [ ] 遺物懲罰觸發 → 效果正確（亮度-5 / 水分強制設為10）
  - [ ] 相對變化需求正確累積判定
  - [ ] 穩定性需求正確追蹤連續回合
  - [ ] 業績到 0 → Game Over
  - [ ] 關卡模式隊列清空 → Level Clear
  - [ ] 無限模式持續生成顧客直到 Game Over
- [ ] 確認 Build 設定（平台 Windows / WebGL）
- [ ] 打包 Build
- [ ] 測試 Build 版本可正常啟動
- [ ] 準備 Gamejam 說明文字（一段話介紹玩法）

---

## 緊急降級方案

> 如果到 H17 還無法跑通一局，按以下順序繼續砍

| 優先砍除 | 替代方案 |
|---------|---------|
| 穩定性需求 | 只保留絕對值 + 相對變化條件 |
| 相對變化需求 | 只保留絕對值條件 |
| 遺物系統 | 移除遺物，TurnManager 跳過 Phase 2 和 Phase 8 |
| 多種卷軸 | 只留「基礎卷軸」（DirectAdd），卷軸選擇 UI 移除 |
| 消耗型符文 | 所有符文都是循環型 |
| 主選單 | 砍掉主選單，遊戲直接進入無限模式 |
| 顧客耐心 | 顧客無限等待，直到玩家送出滿足需求的卷軸 |

---

## 補充：分工原則

- **Coder** 完成腳本後，**先寫假資料（hardcode）測試通過**，再等 Art 交付 SO 資料資產
- **Art** 製作 UI Prefab 時，**留好對應的欄位名稱**供 Coder 直接 `GetComponent` 綁定
- 任何 Blocker（等待對方才能繼續）立刻開口，不要等

---

*Todolist 版本：v1.1*
*最後更新：2026-03-21*

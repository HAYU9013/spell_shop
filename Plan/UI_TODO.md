# UI TODO List — 讓遊戲可以玩

> 目標：在現有 Core / Manager / GameFacade 全部到位的前提下，
> 用最少的 UI 實作讓一局遊戲跑完（選卷軸 → 放符文 → 送出 → 看顧客反應 → 迴圈直到 GameOver/LevelClear）。
>
> API 唯一入口：`GameFacade.Instance`（詳見 `UI_entry_point.md`）
> 所有 UI 腳本放在 `Assets/Scripts/UI/`

---

## 優先級說明

| 標籤 | 意義 |
|------|------|
| 🔴 **P0** | 沒有它遊戲根本無法啟動或操作 |
| 🟠 **P1** | 可以玩但看不到資訊，體驗殘缺 |
| 🟡 **P2** | 有它才算完整一局 |
| 🟢 **P3** | 視覺回饋 / 演出，可最後補 |

---

## PHASE 0 — 測試資料與場景 Bootstrap（前置作業）

> 沒有 ScriptableObject 資料，Manager.Init() 全部是空的，遊戲根本不動。

- [ ] 🔴 **建立測試用 ScriptableObject 資料**
  - [ ] `RuneData` × 5 種（至少覆蓋水/火/光/暗各一）
    - 欄位：`runeName`, `icon`, `runeType`(Cycle/Consumable), `tag`, `effects[]`
  - [ ] `ScrollData` × 2 種（至少一個 DirectAdd、一個 MultiplyAll）
    - 欄位：`scrollName`, `slotCount`, `modifierType`, `multiplier`
  - [ ] `CustomerData` × 3 筆（含各式 RequirementType 各一）
    - 欄位：`customerName`, `maxPatience`, `requirements[]`
  - [ ] `RelicData` × 1 筆（punishmentType = ScoreReset 最安全）

- [ ] 🔴 **GameBootstrapper.cs** — 掛在場景，Start() 中以測試資料呼叫 StartGame
  ```
  路徑：Assets/Scripts/UI/GameBootstrapper.cs
  職責：
    [SerializeField] List<RuneData>     startingDeck;
    [SerializeField] List<ScrollData>   startingScrolls;
    [SerializeField] List<CustomerData> customerQueue;
    [SerializeField] List<RelicData>    startingRelics;
    [SerializeField] GameMode           gameMode = GameMode.Level;

    void Start() => GameManager.Instance.StartGame(gameMode);
    // ★ GameManager.StartGame() 內部已接受 RuneDataList 等參數，
    //   或改為讓 GameManager 從 Bootstrapper 取得 — 依現有 StartGame 簽章調整
  ```

- [ ] 🔴 **Unity 場景 GameObject 架構建立**（在 GameScene.unity 中手動建）

  ```
  [Canvas] (Screen Space - Overlay)
    ├─ [TopBar]
    │    ├─ ScorePanel
    │    └─ PhaseLabel
    ├─ [LeftPanel]
    │    └─ EnvironmentPanel
    ├─ [CenterPanel]
    │    ├─ CustomerPanel
    │    └─ WorkbenchPanel
    ├─ [RightPanel]
    │    └─ RelicPanel
    ├─ [BottomPanel]
    │    └─ HandPanel
    └─ [Overlay]          ← 最上層，Game Over / Level Clear / 極端事件
         ├─ GameOverPanel   (預設 inactive)
         ├─ LevelClearPanel (預設 inactive)
         └─ ExtremeEventFlash (預設 inactive)

  [GameSystems]           ← 非 UI，場景必要的 Manager GameObject
    ├─ GameManager        (+ GameFacade)
    ├─ TurnManager
    ├─ EnvironmentManager
    ├─ DeckManager
    ├─ WorkbenchManager
    ├─ ScrollInventory
    ├─ CustomerManager
    ├─ RelicManager
    └─ GameBootstrapper
  ```

---

## PHASE 1 — 最小可操作迴圈（P0）

> 完成後玩家可以：看到環境值、選卷軸、放符文、按送出、重複。

### 1-A  EnvironmentPanel.cs
```
路徑：Assets/Scripts/UI/EnvironmentPanel.cs
訂閱：OnEnvironmentChanged
顯示：
  - 亮度 (Brightness) 數值 Text
  - 水分 (Moisture)   數值 Text
  - 溫度 (Temperature)數值 Text
  （各屬性上下限 0~20，顯示數字即可）
```
- [ ] 🔴 建立腳本
- [ ] 🔴 在 Inspector 連結 3 個 TMP_Text 欄位
- [ ] 🔴 OnEnable 訂閱 / OnDisable 取消

---

### 1-B  WorkbenchPanel.cs + 子元件
```
路徑：Assets/Scripts/UI/WorkbenchPanel.cs
訂閱：OnWorkbenchChanged
```

- [ ] 🔴 **ScrollSelectorRow** — 動態生成卷軸選擇按鈕
  - 從 `WorkbenchSnapshot.AvailableScrolls` 建立按鈕列
  - 點擊 → `GameFacade.Instance.SelectScroll(scroll)`
  - 已選中的按鈕顯示 highlight（例如改顏色）

- [ ] 🔴 **SlotUI 陣列** — 顯示每個符文槽
  - 依 `WorkbenchSnapshot.SlotCount` 動態生成（或固定最大 4 個）
  - 槽位有符文：顯示符文名稱（或圖示）
  - 空槽：顯示「空」或灰色底
  - 點擊已填符文 → `GameFacade.Instance.RemoveRuneAt(slotIndex)` 退回手牌

- [ ] 🔴 **送出按鈕**
  - `interactable = WorkbenchSnapshot.CanSubmit`
  - 點擊 → `GameFacade.Instance.SubmitScroll()`

- [ ] 🔴 在 Inspector 連結按鈕 / 容器

---

### 1-C  HandPanel.cs + RuneCardUI.cs
```
路徑：Assets/Scripts/UI/HandPanel.cs
      Assets/Scripts/UI/RuneCardUI.cs
訂閱：OnHandUpdated
```

- [ ] 🔴 **HandPanel** — 每次收到 OnHandUpdated 清空重建子物件
  ```csharp
  void RefreshHand(IReadOnlyList<RuneData> hand)
  {
      foreach (Transform t in cardContainer) Destroy(t.gameObject);
      foreach (var rune in hand)
      {
          var card = Instantiate(runeCardPrefab, cardContainer);
          card.GetComponent<RuneCardUI>().Setup(rune, OnCardClicked);
      }
  }
  void OnCardClicked(RuneData rune) => GameFacade.Instance.PlaceRune(rune);
  ```

- [ ] 🔴 **RuneCardUI prefab** — 最小版本
  - TMP_Text：符文名稱
  - Button：點擊呼叫 OnCardClicked
  - （可選）TMP_Text：效果描述

- [ ] 🔴 `[SerializeField] GameObject runeCardPrefab` 連結 prefab
- [ ] 🔴 `[SerializeField] Transform cardContainer` 連結 Layout Group

---

## PHASE 2 — 讓玩家看到顧客與勝負（P1）

> 完成後玩家知道自己在幫誰、滿足了沒、何時結束。

### 2-A  CustomerPanel.cs
```
路徑：Assets/Scripts/UI/CustomerPanel.cs
訂閱：OnCustomerChanged, OnRequirementProgressUpdated
```

- [ ] 🟠 顯示顧客名稱（TMP_Text）
- [ ] 🟠 耐心進度條（Slider 或自製 Bar，`CurrentPatience / MaxPatience`）
- [ ] 🟠 需求列表 — 動態生成 RequirementRowUI
  - 每行顯示 `RequirementSnapshot.DisplayText`
  - 已滿足：文字轉綠色 / 加 ✓ 圖示
  - 未滿足：白色
- [ ] 🟠 無顧客時 `gameObject.SetActive(false)`（隊列空時）
- [ ] 🟠 顯示「排隊中：N 位」（`QueueRemaining`）

---

### 2-B  ScorePanel.cs
```
路徑：Assets/Scripts/UI/ScorePanel.cs
訂閱：OnScoreChanged
```

- [ ] 🟠 顯示當前業績數字（TMP_Text）
- [ ] 🟠 收到 delta 時，數字直接更新即可（動畫 P3 補）

---

### 2-C  GameOverPanel.cs
```
路徑：Assets/Scripts/UI/GameOverPanel.cs
訂閱：OnGameOver（在 GameFacade 的 Start 或 OnEnable 訂閱）
```

- [ ] 🟠 預設 `SetActive(false)`
- [ ] 🟠 收到 OnGameOver → `SetActive(true)` + 顯示最終業績 + 回合數
- [ ] 🟠 「再玩一次」按鈕 → `SceneManager.LoadScene(現在場景)` 重置

---

### 2-D  LevelClearPanel.cs
```
路徑：Assets/Scripts/UI/LevelClearPanel.cs
訂閱：OnLevelClear
```

- [ ] 🟠 預設 `SetActive(false)`
- [ ] 🟠 收到 OnLevelClear → `SetActive(true)` + 顯示通關文字 + 最終業績
- [ ] 🟠 「繼續」按鈕（目前點了重開即可）

---

## PHASE 3 — 遺物面板與回合鎖定（P1~P2）

### 3-A  RelicPanel.cs + RelicIconUI.cs
```
路徑：Assets/Scripts/UI/RelicPanel.cs
      Assets/Scripts/UI/RelicIconUI.cs
訂閱：OnRelicsChanged
```

- [ ] 🟡 **RelicPanel** — 收到 OnRelicsChanged 清空重建
- [ ] 🟡 **RelicIconUI prefab**
  - 遺物名稱（TMP_Text）
  - 滿意/不滿意底色（IsSatisfied → 綠/紅）
  - 不滿意計數 `DissatisfiedCount / DissatisfiedLimit`（例：`2/3`）
  - Tooltip（滑過顯示 SatisfiedEffectText / UnsatisfiedEffectText / PunishmentText）

---

### 3-B  PhaseLabel.cs — 回合階段顯示與 UI 鎖定
```
路徑：Assets/Scripts/UI/PhaseLabel.cs
訂閱：OnPhaseChanged
```

- [ ] 🟡 顯示目前回合階段文字（例：`PlayerAction` 時顯示「輪到你了」）
- [ ] 🟡 **UI 互動鎖定**：非 `PlayerAction` 階段時
  - WorkbenchPanel 的送出按鈕 `interactable = false`
  - HandPanel 的符文卡牌 `interactable = false`
  - （否則在 AI 處理階段玩家點擊會出問題）
  ```csharp
  void OnPhase(TurnPhase phase)
  {
      bool isPlayerTurn = phase == TurnPhase.PlayerAction;
      workbenchPanel.SetInteractable(isPlayerTurn);
      handPanel.SetInteractable(isPlayerTurn);
      phaseText.text = GetPhaseDisplayName(phase);
  }
  ```

---

## PHASE 4 — 視覺回饋演出（P2~P3）

### 4-A  WorkbenchPreviewText — 送出前環境預覽
```
隸屬：WorkbenchPanel 的一部分
訂閱：OnWorkbenchChanged（或 OnEnvironmentChanged）
```

- [ ] 🟡 在送出按鈕旁顯示：`亮度 +2  水分 +0  溫度 -1`
  ```csharp
  var delta = GameFacade.Instance.WorkbenchPreviewDelta;
  previewText.text = $"亮度 {Fmt(delta.brightnessDelta)}  " +
                     $"水分 {Fmt(delta.moistureDelta)}  " +
                     $"溫度 {Fmt(delta.temperatureDelta)}";
  string Fmt(int d) => d == 0 ? "--" : (d > 0 ? $"+{d}" : $"{d}");
  ```

---

### 4-B  ExtremeEventFlash.cs — 全畫面警告演出
```
路徑：Assets/Scripts/UI/ExtremeEventFlash.cs
訂閱：OnExtremeEventTriggered
```

- [ ] 🟢 收到極端事件 → 短暫顯示警告文字（例：「黑暗降臨！」）
  - 對應 ExtremeEvent 枚舉：Darkness / Flood / Overheat / Overcold
  - 可用 DOTween `DOFade` 做淡入淡出（或 Coroutine 手動做）

---

### 4-C  ScorePanel 跳字動畫
- [ ] 🟢 `OnScoreChanged(newScore, delta)` 中顯示浮字 `+N` / `-N`
  - 生成 WorldSpace 或 ScreenSpace 浮字 prefab，向上飄出後消失
  - 正分綠色、負分紅色

---

### 4-D  EnvironmentPanel — 數值邊界視覺警告
- [ ] 🟢 亮度/水分/溫度接近 0 或 20 時，Text 顏色轉紅（threshold 可設 2 以內）

---

## PHASE 5 — 完整性收尾

- [ ] 🟢 **TurnCounter** — 顯示目前第幾回合（`TurnManager.Instance.CurrentTurn`）
- [ ] 🟢 **DeckCounter** — 顯示牌庫剩餘張數、棄牌堆張數
  - 訂閱 `DeckManager.OnDeckChanged`（目前 GameFacade 尚未暴露，可直接讀 DeckManager.Instance 或擴充 Facade）
- [ ] 🟢 **符文 Tooltip** — 滑過符文卡牌顯示完整效果列表
- [ ] 🟢 **顧客立繪** — `CustomerSnapshot.Portrait` Sprite 顯示

---

## 實作順序建議

```
Week 1  PHASE 0  → 測試資料 + Bootstrapper + 場景架構
Week 1  PHASE 1  → EnvironmentPanel + WorkbenchPanel + HandPanel
                   ★ 此時已可操作一回合
Week 2  PHASE 2  → CustomerPanel + ScorePanel + GameOverPanel + LevelClearPanel
                   ★ 此時一局可以跑完
Week 2  PHASE 3  → RelicPanel + PhaseLabel（UI 鎖定）
                   ★ 此時機制完整，可以真正測試平衡
Week 3  PHASE 4+ → 演出、動畫、Tooltip 等
```

---

## 腳本命名規範

| 用途 | 命名規則 | 範例 |
|------|----------|------|
| 面板（Panel）| `{系統}Panel.cs` | `CustomerPanel.cs` |
| 單一元素 UI | `{元素}UI.cs` | `RuneCardUI.cs`, `RelicIconUI.cs` |
| 訂閱 Facade 事件 | `OnEnable/OnDisable` | — |
| Refresh 方法 | 接受 Snapshot 型別 | `Refresh(WorkbenchSnapshot wb)` |
| 全域腳本 | 功能描述 | `GameBootstrapper.cs`, `PhaseLabel.cs` |

---

## 注意事項（開發時）

1. **所有面板只透過 `GameFacade.Instance`**，不直接引用任何 Manager
2. **OnEnable 訂閱、OnDisable 取消**，Refresh 方法要冪等
3. **送出按鈕 interactable** 依 `Workbench.CanSubmit` 且只在 `PlayerAction` 階段開放
4. **CustomerPanel** 需同時訂閱 `OnCustomerChanged`（顧客切換）和
   `OnRequirementProgressUpdated`（環境變動後即時刷新需求進度）
5. **GameOverPanel / LevelClearPanel** 在 `GameFacade.Start()` 或自身 `OnEnable` 訂閱，
   不要在 `OnEnable` 訂閱後立刻被 `SetActive(false)` 造成訂閱失效

---

*建立日期：2026-03-21*

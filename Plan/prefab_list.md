# Prefab 清單

> **Prefab vs 場景物件的判斷原則**
> - **需要 Prefab**：執行時動態 Instantiate（數量不固定）
> - **直接放場景**：數量固定、只會出現一次的 GameObject
>
> 存放路徑建議：`Assets/Prefabs/UI/` 和 `Assets/Prefabs/System/`

---

## 優先級

| 標籤 | 意義 |
|------|------|
| 🔴 P0 | 遊戲基本操作必須 |
| 🟠 P1 | 資訊顯示必須 |
| 🟡 P2 | 完整體驗必須 |
| 🟢 P3 | 視覺演出，可後補 |

---

## 一、UI Runtime Prefabs（執行時動態生成）

這些需要真正做成 `.prefab` 檔，由對應腳本 `Instantiate` 建立。

---

### 🔴 P0 — RuneCardUI
```
路徑：Assets/Prefabs/UI/RuneCardUI.prefab
由誰建立：HandPanel.cs（每次 OnHandUpdated 時重建）
```

**用途**：手牌中的每一張符文卡牌。

**必要 Components：**
- `RectTransform`
- `Button`（點擊 → 呼叫 GameFacade.PlaceRune）
- `RuneCardUI.cs`（自製腳本，接收 RuneData 渲染）

**子物件結構：**
```
RuneCardUI (Button)
  ├─ Icon          (Image)           ← rune.icon
  ├─ RuneName      (TMP_Text)        ← rune.runeName
  ├─ EffectText    (TMP_Text)        ← rune.GetEffectSummary()
  └─ TypeBadge     (Image)           ← Cycle/Consumable 標記（可選）
```

**Inspector 需連結：**
- `Image icon`
- `TMP_Text nameText`
- `TMP_Text effectText`

---

### 🔴 P0 — ScrollButtonUI
```
路徑：Assets/Prefabs/UI/ScrollButtonUI.prefab
由誰建立：WorkbenchPanel.cs（每次 OnWorkbenchChanged 更新卷軸列時）
```

**用途**：工作台上方的卷軸選擇按鈕，每個庫存卷軸一個。

**必要 Components：**
- `RectTransform`
- `Button`（點擊 → GameFacade.SelectScroll）
- `ScrollButtonUI.cs`

**子物件結構：**
```
ScrollButtonUI (Button)
  ├─ Icon           (Image)           ← scroll.icon
  ├─ ScrollName     (TMP_Text)        ← scroll.scrollName
  ├─ ModifierDesc   (TMP_Text)        ← scroll.GetModifierDescription()
  └─ SelectedFrame  (Image)           ← 選中時顯示（平時 inactive）
```

---

### 🔴 P0 — SlotUI
```
路徑：Assets/Prefabs/UI/SlotUI.prefab
由誰建立：WorkbenchPanel.cs（選擇卷軸後依 SlotCount 動態生成）
```

**用途**：工作台的每個符文槽位，點擊可退回手牌。

**必要 Components：**
- `RectTransform`
- `Button`（點擊 → GameFacade.RemoveRuneAt(slotIndex)）
- `SlotUI.cs`

**子物件結構：**
```
SlotUI (Button)
  ├─ EmptyState     (GameObject)      ← 空槽時顯示
  │    └─ EmptyIcon  (Image)          ← 加號或虛線框
  └─ FilledState    (GameObject)      ← 有符文時顯示
       ├─ RuneIcon   (Image)          ← rune.icon
       └─ RuneName   (TMP_Text)       ← rune.runeName
```

**腳本邏輯：**
```
void Setup(RuneData rune, int index)
  → rune != null → 顯示 FilledState，隱藏 EmptyState
  → rune == null → 顯示 EmptyState，隱藏 FilledState
  → 記住 slotIndex，Button.onClick → GameFacade.RemoveRuneAt(index)
```

---

### 🟠 P1 — RequirementRowUI
```
路徑：Assets/Prefabs/UI/RequirementRowUI.prefab
由誰建立：CustomerPanel.cs（每次顧客更換時重建）
```

**用途**：顧客需求面板中每一行需求顯示。

**必要 Components：**
- `RectTransform`
- `RequirementRowUI.cs`（選用，也可直接在 CustomerPanel 中操作子物件）

**子物件結構：**
```
RequirementRowUI
  ├─ CheckIcon      (Image)           ← 滿足時顯示 ✓（可 SetActive）
  ├─ CrossIcon      (Image)           ← 未滿足時顯示 ✗（可 SetActive）
  └─ RequirementText (TMP_Text)       ← RequirementSnapshot.DisplayText
```

**顏色規則：**
- `IsSatisfied = true`  → `RequirementText` 顏色改綠
- `IsSatisfied = false` → `RequirementText` 顏色維持白

---

### 🟡 P2 — RelicIconUI
```
路徑：Assets/Prefabs/UI/RelicIconUI.prefab
由誰建立：RelicPanel.cs（每次 OnRelicsChanged 時重建）
```

**用途**：遺物面板中每個遺物的圖示卡片。

**必要 Components：**
- `RectTransform`
- `RelicIconUI.cs`

**子物件結構：**
```
RelicIconUI
  ├─ Background     (Image)           ← 滿意=綠框, 不滿意=紅框
  ├─ Icon           (Image)           ← relic.Icon
  ├─ RelicName      (TMP_Text)        ← relic.RelicName
  ├─ CounterText    (TMP_Text)        ← "2 / 3"（DissatisfiedCount / Limit）
  └─ TooltipTrigger (EventTrigger)    ← 滑入顯示 Tooltip（P3）
```

**狀態規則：**
- `IsSatisfied = true`  → Background 綠色
- `IsSatisfied = false` → Background 紅色，CounterText 顯示計數

---

### 🟢 P3 — FloatingTextUI
```
路徑：Assets/Prefabs/UI/FloatingTextUI.prefab
由誰建立：ScorePanel.cs（每次 OnScoreChanged 時生成）
```

**用途**：業績變動時的浮字動畫（+5 / -10 往上飄出後消失）。

**必要 Components：**
- `RectTransform`（Anchored Position 由腳本設定）
- `TMP_Text`（顯示 "+5" 或 "-10"）
- `FloatingTextUI.cs`（播完動畫後自我 Destroy）

**子物件結構：**
```
FloatingTextUI
  └─ FloatText      (TMP_Text)        ← "+5" 綠色 / "-10" 紅色
```

**動畫方式（Coroutine 版，不需 DOTween）：**
```
IEnumerator PlayAndDestroy()
  → 0.5s 內 Y 軸上移 50px，Alpha 從 1→0
  → Destroy(gameObject)
```

---

### 🟢 P3 — TooltipUI
```
路徑：Assets/Prefabs/UI/TooltipUI.prefab
由誰建立：TooltipManager.cs（全域單例），任何需要 Tooltip 的地方呼叫
```

**用途**：滑鼠滑入符文卡、遺物圖示時顯示的說明視窗。

**必要 Components：**
- `RectTransform`（動態跟隨滑鼠位置）
- `CanvasGroup`（Alpha 控制顯示/隱藏）

**子物件結構：**
```
TooltipUI
  ├─ Background     (Image)           ← 半透明黑底
  ├─ TitleText      (TMP_Text)        ← 物件名稱
  ├─ BodyText       (TMP_Text)        ← 說明文字
  └─ ContentSizeFitter               ← 讓背景自動配合文字大小
```

---

## 二、場景內固定 GameObject（不是 Prefab，直接放在場景中）

這些物件在場景裡只有一個，**建立好後直接掛在 Hierarchy**，不需要做成 Prefab 檔。

### Canvas 層級結構

```
[Canvas]  Screen Space - Overlay, Sort Order = 0
│
├── [TopBar]
│     ├── ScorePanel             ← ScorePanel.cs
│     │     ├── ScoreLabel       (TMP_Text)  "業績"
│     │     └── ScoreValue       (TMP_Text)  數字
│     └── PhaseLabel             ← PhaseLabel.cs
│           └── PhaseText        (TMP_Text)
│
├── [LeftPanel]
│     └── EnvironmentPanel       ← EnvironmentPanel.cs
│           ├── BrightnessRow
│           │     ├── Label      (TMP_Text)  "亮度"
│           │     └── Value      (TMP_Text)
│           ├── MoistureRow
│           │     ├── Label      (TMP_Text)  "水分"
│           │     └── Value      (TMP_Text)
│           └── TemperatureRow
│                 ├── Label      (TMP_Text)  "溫度"
│                 └── Value      (TMP_Text)
│
├── [CenterPanel]
│     ├── CustomerPanel          ← CustomerPanel.cs
│     │     ├── Portrait         (Image)         顧客立繪
│     │     ├── NameText         (TMP_Text)
│     │     ├── FlavorText       (TMP_Text)
│     │     ├── PatienceBar      (Slider)
│     │     ├── PatienceText     (TMP_Text)      "3 / 5"
│     │     ├── QueueText        (TMP_Text)      "等待中：2 位"
│     │     └── RequirementContainer (VerticalLayoutGroup)
│     │           └── [RequirementRowUI × N]   ← 動態生成
│     │
│     └── WorkbenchPanel         ← WorkbenchPanel.cs
│           ├── ScrollSelectorRow (HorizontalLayoutGroup)
│           │     └── [ScrollButtonUI × N]     ← 動態生成
│           ├── SlotRow          (HorizontalLayoutGroup)
│           │     └── [SlotUI × N]             ← 動態生成
│           ├── PreviewText      (TMP_Text)      "亮度 +2  水分 +0  溫度 -1"
│           └── SubmitButton     (Button)        送出
│
├── [RightPanel]
│     └── RelicPanel             ← RelicPanel.cs
│           └── RelicContainer   (VerticalLayoutGroup)
│                 └── [RelicIconUI × N]        ← 動態生成
│
├── [BottomPanel]
│     └── HandPanel              ← HandPanel.cs
│           └── CardContainer    (HorizontalLayoutGroup)
│                 └── [RuneCardUI × N]         ← 動態生成
│
└── [Overlay]   Sort Order = 10（最上層）
      ├── GameOverPanel          ← GameOverPanel.cs（預設 inactive）
      │     ├── Title            (TMP_Text)      "遊戲結束"
      │     ├── FinalScoreText   (TMP_Text)
      │     ├── TurnsText        (TMP_Text)
      │     └── RestartButton    (Button)
      │
      ├── LevelClearPanel        ← LevelClearPanel.cs（預設 inactive）
      │     ├── Title            (TMP_Text)      "通關！"
      │     ├── FinalScoreText   (TMP_Text)
      │     └── ContinueButton   (Button)
      │
      └── ExtremeEventFlash      ← ExtremeEventFlash.cs（預設 inactive）
            ├── Background       (Image)         全畫面半透明色塊
            └── EventText        (TMP_Text)      "黑暗降臨！"
```

---

## 三、System GameObject（非 UI，場景中的管理器）

放在場景中一個名為 `[GameSystems]` 的空 GameObject 下，純掛腳本，無視覺元件。

```
[GameSystems]
  ├── GameManager                ← GameMAnager.cs
  │                                 GameFacade.cs（同掛一個 GO）
  ├── TurnManager                ← TurnManager.cs
  ├── EnvironmentManager         ← EnvironmentManager.cs
  ├── DeckManager                ← DeckManager.cs
  ├── WorkbenchManager           ← WorkbenchManager.cs
  ├── ScrollInventory            ← ScrollInventory.cs
  ├── CustomerManager            ← CustomerManager.cs
  ├── RelicManager               ← RelicManager.cs
  └── GameBootstrapper           ← GameBootstrapper.cs
        [SerializeField] startingDeck      → 拖入 RuneData × N
        [SerializeField] startingScrolls   → 拖入 ScrollData × N
        [SerializeField] customerQueue     → 拖入 CustomerData × N
        [SerializeField] startingRelics    → 拖入 RelicData × N
```

---

## 四、總覽表

| Prefab 名稱 | 類型 | 路徑 | 建立者腳本 | 優先級 |
|-------------|------|------|-----------|--------|
| `RuneCardUI` | UI Prefab | `Prefabs/UI/` | HandPanel | 🔴 P0 |
| `ScrollButtonUI` | UI Prefab | `Prefabs/UI/` | WorkbenchPanel | 🔴 P0 |
| `SlotUI` | UI Prefab | `Prefabs/UI/` | WorkbenchPanel | 🔴 P0 |
| `RequirementRowUI` | UI Prefab | `Prefabs/UI/` | CustomerPanel | 🟠 P1 |
| `RelicIconUI` | UI Prefab | `Prefabs/UI/` | RelicPanel | 🟡 P2 |
| `FloatingTextUI` | UI Prefab | `Prefabs/UI/` | ScorePanel | 🟢 P3 |
| `TooltipUI` | UI Prefab | `Prefabs/UI/` | TooltipManager | 🟢 P3 |
| `ScorePanel` | 場景物件 | GameScene Hierarchy | — | 🔴 P0 |
| `EnvironmentPanel` | 場景物件 | GameScene Hierarchy | — | 🔴 P0 |
| `WorkbenchPanel` | 場景物件 | GameScene Hierarchy | — | 🔴 P0 |
| `HandPanel` | 場景物件 | GameScene Hierarchy | — | 🔴 P0 |
| `CustomerPanel` | 場景物件 | GameScene Hierarchy | — | 🟠 P1 |
| `RelicPanel` | 場景物件 | GameScene Hierarchy | — | 🟡 P2 |
| `PhaseLabel` | 場景物件 | GameScene Hierarchy | — | 🟡 P2 |
| `GameOverPanel` | 場景物件 | GameScene Hierarchy | — | 🟠 P1 |
| `LevelClearPanel` | 場景物件 | GameScene Hierarchy | — | 🟠 P1 |
| `ExtremeEventFlash` | 場景物件 | GameScene Hierarchy | — | 🟢 P3 |

---

## 五、製作順序建議

```
Step 1  SlotUI + ScrollButtonUI + RuneCardUI   ← 先讓工作台和手牌可以動
Step 2  RequirementRowUI                        ← 顧客面板補齊
Step 3  RelicIconUI                             ← 遺物面板
Step 4  FloatingTextUI + TooltipUI              ← 演出最後補
```

---

*建立日期：2026-03-21*

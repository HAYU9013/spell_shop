# UI Entry Point — 串接說明文件

---

## 設計原則

UI 層**不直接引用任何 Manager**，一律透過 `GameFacade` 這個單一入口：

```
[EnvironmentPanel]  ──┐
[CustomerPanel]     ──┤                          ┌── EnvironmentManager
[HandPanel]         ──┤  →  GameFacade (單例)  ──┤── CustomerManager
[WorkbenchPanel]    ──┤                          ├── DeckManager
[RelicPanel]        ──┤                          ├── WorkbenchManager
[ScorePanel]        ──┘                          └── RelicManager
```

好處：Manager 內部改動不影響 UI；UI 腳本不需要 `[SerializeField]` 一堆 Manager 參考。

---

## GameFacade.cs 概覽

```csharp
/// <summary>
/// 所有 UI 腳本的唯一資料與操作入口。
/// 掛在場景中的 GameManager GameObject 上。
/// </summary>
public class GameFacade : MonoBehaviour
{
    public static GameFacade Instance { get; private set; }

    // ── 取得資料（只讀快照） ──────────────────────────────
    // 環境
    // 工作台
    // 手牌
    // 顧客
    // 遺物
    // 回合狀態

    // ── 觸發操作 ──────────────────────────────────────────
    // 選卷軸 / 放符文 / 取消符文 / 送出

    // ── 訂閱事件 ──────────────────────────────────────────
    // 環境更新 / 手牌更新 / 顧客更新 / 遺物更新
    // 極端事件 / Game Over / Level Clear / 回合階段切換
}
```

---

## 一、取得資料（Read API）

UI 腳本在 `Start()` 或事件回呼中呼叫以下屬性取得快照，然後渲染到畫面上。

### 1.1 環境數值

```csharp
// 完整環境快照（亮度、水分、溫度）
EnvironmentData env = GameFacade.Instance.Environment;
int brightness   = env.brightness;
int moisture     = env.moisture;
int temperature  = env.temperature;

// 業績
int score = GameFacade.Instance.Score;

// 工作台「送出後」的預覽 delta（送出前即時更新，供 UI 顯示預覽）
EnvironmentDelta preview = GameFacade.Instance.WorkbenchPreviewDelta;
// preview.brightnessDelta / .moistureDelta / .temperatureDelta
```

### 1.2 工作台狀態

```csharp
WorkbenchSnapshot wb = GameFacade.Instance.Workbench;

wb.SelectedScroll          // ScrollData，目前選中的卷軸（null = 尚未選擇）
wb.SlotCount               // int，槽位總數
wb.PlacedRunes             // RuneData[]，每個槽放的符文（null = 空槽）
wb.CanSubmit               // bool，槽位全滿才為 true
wb.AvailableScrolls        // List<ScrollData>，目前庫存中可用的卷軸清單
```

```csharp
// WorkbenchSnapshot 定義
public struct WorkbenchSnapshot
{
    public ScrollData        SelectedScroll;
    public int               SlotCount;
    public RuneData[]        PlacedRunes;       // 長度 = SlotCount
    public bool              CanSubmit;
    public List<ScrollData>  AvailableScrolls;
}
```

### 1.3 手牌

```csharp
// 目前手上的符文列表（每回合補充後更新）
IReadOnlyList<RuneData> hand = GameFacade.Instance.Hand;

foreach (var rune in hand)
{
    rune.runeName       // string
    rune.icon           // Sprite
    rune.description    // string
    rune.runeType       // RuneType（循環/消耗）
    rune.tag            // RuneTag（水系/火系...）
    rune.effects        // List<RuneEffect>
}
```

### 1.4 顧客資訊

```csharp
CustomerSnapshot cs = GameFacade.Instance.CurrentCustomer;

cs.IsPresent           // bool，目前是否有顧客（隊列空時為 false）
cs.Name                // string
cs.Portrait            // Sprite
cs.CurrentPatience     // int，剩餘耐心
cs.MaxPatience         // int，最大耐心
cs.Requirements        // List<RequirementSnapshot>，需求列表
cs.QueueRemaining      // int，隊列中還剩幾位顧客
```

```csharp
// 每筆需求的快照（含進度）
public struct RequirementSnapshot
{
    public string   DisplayText;   // 已格式化的需求文字，直接塞進 Text
                                   // 例："溫度 >= 8（目前：6）"
                                   //     "水分累積減少 >= 3（已減：-1）"
                                   //     "溫度維持 4~6（連續：0/1 回合）"
    public bool     IsSatisfied;   // 是否已達成（可用於顯示綠色✓）
    public float    Progress;      // 0.0 ~ 1.0，供進度條使用（選用）
}
```

### 1.5 遺物狀態

```csharp
IReadOnlyList<RelicSnapshot> relics = GameFacade.Instance.Relics;

foreach (var r in relics)
{
    r.RelicName            // string
    r.Icon                 // Sprite
    r.IsSatisfied          // bool
    r.DissatisfiedCount    // int，目前不滿意計數
    r.DissatisfiedLimit    // int，懲罰上限
    r.SatisfiedEffectText  // string，滿意效果描述（例："每回合亮度 +1"）
    r.UnsatisfiedEffectText// string，不滿意副作用描述
    r.PunishmentText       // string，懲罰描述（例："亮度 -5"）
}
```

### 1.6 回合階段（選用，進階 UI 用）

```csharp
TurnPhase phase = GameFacade.Instance.CurrentPhase;
// 可用於在 PlayerAction 以外的階段鎖定 UI 互動
```

---

## 二、觸發操作（Action API）

UI 腳本在玩家點擊時呼叫以下方法。

### 2.1 工作台操作

```csharp
// 選擇一個卷軸（從 Workbench.AvailableScrolls 中選）
GameFacade.Instance.SelectScroll(ScrollData scroll);

// 點選手牌中的一張符文 → 填入下一個空槽
// 若槽位已全滿，回傳 false 不執行
bool ok = GameFacade.Instance.PlaceRune(RuneData rune);

// 點選工作台槽位 → 移除該槽的符文（退回手牌）
GameFacade.Instance.RemoveRuneAt(int slotIndex);

// 送出卷軸（CanSubmit 為 false 時呼叫無效）
GameFacade.Instance.SubmitScroll();
```

> **UI 注意**：送出按鈕應每幀（或在 `OnWorkbenchChanged` 事件中）檢查
> `GameFacade.Instance.Workbench.CanSubmit` 來決定是否 interactable。

---

## 三、訂閱事件（Event API）

UI 在 `OnEnable()` 訂閱、`OnDisable()` 取消訂閱。事件觸發時重新讀取 Read API 並刷新畫面。

```csharp
// 事件定義（GameFacade 上的 static/instance event）

// 環境數值有任何變動（含業績）
event Action<EnvironmentData>           OnEnvironmentChanged;

// 業績單獨變動（帶 delta，用於 +N / -N 跳字動畫）
event Action<int /*newScore*/, int /*delta*/> OnScoreChanged;

// 工作台狀態變動（選卷軸、放符文、取消符文）
event Action<WorkbenchSnapshot>         OnWorkbenchChanged;

// 手牌更新（每回合補充後）
event Action<IReadOnlyList<RuneData>>   OnHandUpdated;

// 顧客切換（新顧客到來 or 顧客離開）
event Action<CustomerSnapshot>          OnCustomerChanged;

// 顧客需求進度更新（每次送出卷軸後，追蹤數值改變）
event Action<CustomerSnapshot>          OnRequirementProgressUpdated;

// 遺物狀態更新（每回合結算後）
event Action<IReadOnlyList<RelicSnapshot>> OnRelicsChanged;

// 極端事件觸發（用於演出閃爍、警告 UI）
event Action<ExtremeEvent>              OnExtremeEventTriggered;

// 回合階段切換（用於鎖定/解鎖 UI 互動）
event Action<TurnPhase>                 OnPhaseChanged;

// 遊戲結束
event Action<int /*finalScore*/, int /*turnsCleared*/> OnGameOver;

// 關卡通關（關卡模式）
event Action                            OnLevelClear;
```

---

## 四、各面板串接範例

### EnvironmentPanel.cs

```csharp
void OnEnable()
{
    GameFacade.Instance.OnEnvironmentChanged += Refresh;
    Refresh(GameFacade.Instance.Environment);
}

void OnDisable()
{
    GameFacade.Instance.OnEnvironmentChanged -= Refresh;
}

void Refresh(EnvironmentData env)
{
    brightnessText.text  = env.brightness.ToString();
    moistureText.text    = env.moisture.ToString();
    temperatureText.text = env.temperature.ToString();
}
```

### ScorePanel.cs

```csharp
void OnEnable()
{
    GameFacade.Instance.OnScoreChanged += OnScore;
}

void OnDisable()
{
    GameFacade.Instance.OnScoreChanged -= OnScore;
}

void OnScore(int newScore, int delta)
{
    // 數字跳動
    scoreText.DOCounter(int.Parse(scoreText.text), newScore, 0.4f);

    // 跳出 +N / -N 浮字
    string sign = delta > 0 ? "+" : "";
    ShowFloatingText($"{sign}{delta}", delta > 0 ? Color.green : Color.red);
}
```

### WorkbenchPanel.cs

```csharp
void OnEnable()
{
    GameFacade.Instance.OnWorkbenchChanged += RefreshWorkbench;
    GameFacade.Instance.OnEnvironmentChanged += RefreshPreview;
    RefreshWorkbench(GameFacade.Instance.Workbench);
}

void RefreshWorkbench(WorkbenchSnapshot wb)
{
    // 重繪卷軸選擇按鈕列
    RebuildScrollButtons(wb.AvailableScrolls, wb.SelectedScroll);

    // 重繪符文槽位
    for (int i = 0; i < slotUIs.Length; i++)
        slotUIs[i].SetRune(wb.PlacedRunes[i]); // null = 顯示空槽

    // 送出按鈕可用性
    submitButton.interactable = wb.CanSubmit;
}

void RefreshPreview(EnvironmentData _)
{
    var delta = GameFacade.Instance.WorkbenchPreviewDelta;
    previewText.text =
        $"亮度 {FmtDelta(delta.brightnessDelta)}  " +
        $"水分 {FmtDelta(delta.moistureDelta)}  "  +
        $"溫度 {FmtDelta(delta.temperatureDelta)}";
}

// 玩家點選卷軸按鈕
public void OnScrollButtonClicked(ScrollData scroll)
    => GameFacade.Instance.SelectScroll(scroll);

// 玩家點選送出
public void OnSubmitClicked()
    => GameFacade.Instance.SubmitScroll();

string FmtDelta(int d) => d == 0 ? "--" : (d > 0 ? $"+{d}" : $"{d}");
```

### HandPanel.cs

```csharp
void OnEnable()
{
    GameFacade.Instance.OnHandUpdated += RefreshHand;
    RefreshHand(GameFacade.Instance.Hand);
}

void RefreshHand(IReadOnlyList<RuneData> hand)
{
    // 清空舊卡牌
    foreach (Transform t in cardContainer) Destroy(t.gameObject);

    // 生成新卡牌
    foreach (var rune in hand)
    {
        var card = Instantiate(runeCardPrefab, cardContainer);
        card.GetComponent<RuneCardUI>().Setup(rune);
    }
}

// RuneCardUI 點擊時呼叫（由 RuneCardUI 自身觸發）
public void OnRuneCardClicked(RuneData rune)
    => GameFacade.Instance.PlaceRune(rune);
```

### CustomerPanel.cs

```csharp
void OnEnable()
{
    GameFacade.Instance.OnCustomerChanged           += RefreshCustomer;
    GameFacade.Instance.OnRequirementProgressUpdated += RefreshRequirements;
    RefreshCustomer(GameFacade.Instance.CurrentCustomer);
}

void RefreshCustomer(CustomerSnapshot cs)
{
    if (!cs.IsPresent) { gameObject.SetActive(false); return; }
    gameObject.SetActive(true);

    nameText.text    = cs.Name;
    portrait.sprite  = cs.Portrait;
    patienceBar.value = (float)cs.CurrentPatience / cs.MaxPatience;
    queueText.text   = $"等待中：{cs.QueueRemaining} 位";

    RefreshRequirements(cs);
}

void RefreshRequirements(CustomerSnapshot cs)
{
    foreach (Transform t in requirementContainer) Destroy(t.gameObject);

    foreach (var req in cs.Requirements)
    {
        var row = Instantiate(requirementRowPrefab, requirementContainer);
        row.GetComponentInChildren<Text>().text  = req.DisplayText;
        row.GetComponentInChildren<Image>().color = req.IsSatisfied
            ? Color.green : Color.white;
    }
}
```

### RelicPanel.cs

```csharp
void OnEnable()
{
    GameFacade.Instance.OnRelicsChanged += RefreshRelics;
    RefreshRelics(GameFacade.Instance.Relics);
}

void RefreshRelics(IReadOnlyList<RelicSnapshot> relics)
{
    foreach (Transform t in relicContainer) Destroy(t.gameObject);

    foreach (var r in relics)
    {
        var icon = Instantiate(relicIconPrefab, relicContainer);
        icon.GetComponent<RelicIconUI>().Setup(r);
        // RelicIconUI 自行處理滿意/不滿意顏色、計數文字、Tooltip
    }
}
```

---

## 五、事件流完整時序圖

```
玩家點擊符文卡牌
      │
      ▼
HandPanel.OnRuneCardClicked(rune)
      │
      ▼
GameFacade.PlaceRune(rune)
      │  → WorkbenchManager.PlaceRune()
      │  → 計算 PreviewDelta
      │
      ▼
GameFacade fires: OnWorkbenchChanged(WorkbenchSnapshot)
      │
      ├──▶ WorkbenchPanel.RefreshWorkbench()   ← 槽位 UI 更新、送出按鈕狀態
      └──▶ WorkbenchPanel.RefreshPreview()     ← 預覽 delta 文字更新

────────────────────────────────────────────
玩家點擊送出按鈕
      │
      ▼
WorkbenchPanel.OnSubmitClicked()
      │
      ▼
GameFacade.SubmitScroll()
      │  → TurnManager 推進至 EnvironmentResolve
      │  → EnvironmentManager.ApplyEffects()
      │  → ExtremeEvent 檢查
      │  → CustomerJudge
      │  → PatienceCheck
      │  → RelicUpdate
      │  → DrawCards（下回合）
      │
      ▼  (依序 fire)
OnScoreChanged        → ScorePanel 跳字
OnEnvironmentChanged  → EnvironmentPanel、WorkbenchPanel（清空預覽）
OnWorkbenchChanged    → WorkbenchPanel（清空槽位）
OnHandUpdated         → HandPanel（新手牌）
OnRequirementProgressUpdated → CustomerPanel（更新需求進度）
OnCustomerChanged     → CustomerPanel（顧客離開時切換）
OnRelicsChanged       → RelicPanel（遺物計數更新）
OnExtremeEventTriggered → 全畫面閃爍演出（若有）
OnGameOver / OnLevelClear → 顯示結束畫面
```

---

## 六、UI 腳本規則

1. **只讀不寫**：UI 腳本永遠只呼叫 Read API 或 Action API，**不直接修改任何 Manager 的欄位**
2. **OnEnable / OnDisable 成對**：訂閱與取消訂閱一定要成對，避免 null reference
3. **Refresh 要冪等**：同一份快照呼叫兩次 Refresh 結果相同，不累加副作用
4. **不快取 Snapshot**：每次事件觸發時重新從 `GameFacade.Instance` 讀取，不存舊快照

---

*本文件版本：v1.0*
*最後更新：2026-03-21*

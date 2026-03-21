# PlayerInteraction — 玩家點擊互動設計計劃

> 本文件描述**點擊操作的完整互動邏輯**，包含狀態機設計、各元件的點擊行為、
> 刪除操作、以及 Hover Tooltip 系統。
>
> 所有 Action 一律透過 `GameFacade.Instance`（詳見 `UI_entry_point.md`）。
> 腳本放在 `Assets/Scripts/UI/Interaction/`

---

## 零、DOTween 動畫設計原則

> 所有互動邏輯與動畫完全分離。互動腳本只負責「狀態切換 + 呼叫 Manager」，
> 動畫由各元件的 `AnimXxx()` 方法封裝，現在先留空（`// TODO: DOTween`），
> 後期填入 tween 而不需要改互動邏輯。

### 分離規則

```
點擊事件
    │
    ├─ 1. 呼叫邏輯（GameFacade / HandSelectionState）
    │
    └─ 2. 呼叫動畫方法 AnimXxx()
               │
               └─ 現在：直接設定最終狀態（SetActive / color / scale）
                  未來：DOTween 補間到最終狀態
```

### 各元件需要的動畫方法一覽

| 元件 | 動畫方法 | 觸發時機 | DOTween 建議 |
|------|---------|---------|-------------|
| `RuneCardUI` | `AnimSelect()` | 選中符文 | `DOScale(1.1f, 0.12f).SetEase(Ease.OutBack)` |
| `RuneCardUI` | `AnimDeselect()` | 取消選中 | `DOScale(1f, 0.1f)` |
| `RuneCardUI` | `AnimPlaced()` | 符文飛入槽 | `DOFade(0, 0.2f)` + 移位 tween |
| `RuneCardUI` | `AnimReturn()` | 符文退回手牌 | `DOFade(1, 0.15f)` + 淡入 |
| `SlotUI` | `AnimFill(runeData)` | 符文放入槽位 | `DOPunchScale(0.2f, 0.3f)` |
| `SlotUI` | `AnimEmpty()` | 符文退出槽位 | `DOShakePosition(0.15f, 3f)` |
| `ScrollButtonUI` | `AnimSelect()` | 卷軸選中 | `DOColor(goldColor, 0.15f)` |
| `ScrollButtonUI` | `AnimDeselect()` | 卷軸取消 | `DOColor(white, 0.15f)` |
| `SubmitButton` | `AnimAvailable()` | CanSubmit 變 true | `DOPunchScale(0.15f, 0.4f)` |
| `SubmitButton` | `AnimSubmit()` | 點擊送出 | `DOScale(0.9f, 0.05f).Then DOScale(1f, 0.1f)` |
| `TooltipPanel` | `AnimShow()` | Hover 進入 | `DOFade(1, 0.1f)` |
| `TooltipPanel` | `AnimHide()` | Hover 離開 | `DOFade(0, 0.08f)` |
| `RelicIconUI` | `AnimSatisfied()` | 遺物滿足 | `DOColor(green, 0.3f)` |
| `RelicIconUI` | `AnimUnsatisfied()` | 遺物不滿足 | `DOShakeRotation(0.2f, 10f)` |

### 動畫方法模板（以 RuneCardUI 為例）

```csharp
// ── 現在（無 DOTween）──────────────────────────────────
private void AnimSelect()
{
    transform.localScale = Vector3.one * 1.1f;   // 直接設定
    selectedBorder.enabled = true;
}

private void AnimDeselect()
{
    transform.localScale = Vector3.one;
    selectedBorder.enabled = false;
}

// ── 未來（填入 DOTween）────────────────────────────────
// private void AnimSelect()
// {
//     transform.DOKill();
//     transform.DOScale(1.1f, 0.12f).SetEase(Ease.OutBack);
//     selectedBorder.DOFade(1f, 0.1f);
// }
//
// private void AnimDeselect()
// {
//     transform.DOKill();
//     transform.DOScale(1f, 0.1f);
//     selectedBorder.DOFade(0f, 0.08f);
// }
```

### 重要注意事項

1. **互動邏輯不 await 動畫**：`PlaceRune()` 呼叫完立刻 `AnimPlaced()`，動畫非同步播放，邏輯不等待
2. **DOKill() 保護**：每個 AnimXxx 開頭呼叫 `transform.DOKill()` 防止上一個 tween 未完成被覆蓋
3. **送出後動畫**：`OnSubmitted` 事件觸發後播放演出，WorkbenchPanel 訂閱此事件做清空動畫
4. **UI 鎖定期間**：非 PlayerAction 階段取消所有進行中的 tween，直接設定最終狀態

---

## 一、全域互動狀態機（SelectionState）

整個 PlayerAction 階段存在一個「目前選中的符文」的全域狀態。
由 `HandSelectionState`（輕量 singleton MonoBehaviour 或 static class）維護。

```
┌─────────────────────────────────────────────────────┐
│                  互動狀態機                          │
│                                                     │
│  [None]  ←────────────────────────────────────┐    │
│    │                                           │    │
│    │ 點擊手牌符文                               │    │
│    ▼                                           │    │
│  [RuneSelected]  ──── 再點一次同一張符文 ───────┘    │
│    │                                           │    │
│    │ 點擊空槽位                 點擊已佔用槽位   │    │
│    ▼                               ▼           │    │
│  PlaceRune()              RemoveRune() ────────┘    │
│  → 回到 [None]            → 回到 [None]              │
│                                                     │
│  點擊新卷軸（任何時候）→ SelectScroll() → [None]      │
└─────────────────────────────────────────────────────┘
```

### HandSelectionState.cs

```csharp
// Assets/Scripts/UI/Interaction/HandSelectionState.cs

public static class HandSelectionState
{
    public static RuneInstance SelectedRune { get; private set; }
    public static event System.Action OnSelectionChanged;

    public static void Select(RuneInstance rune)
    {
        SelectedRune = rune;
        OnSelectionChanged?.Invoke();
    }

    public static void Deselect()
    {
        SelectedRune = null;
        OnSelectionChanged?.Invoke();
    }

    public static bool IsSelected(RuneInstance rune)
        => SelectedRune != null && SelectedRune == rune;
}
```

> **為何用 RuneInstance 而非 RuneData？**
> 手牌中可能有多張相同的 RuneData（同名符文），
> 用 RuneInstance 才能精確識別是哪一張。

---

## 二、手牌符文卡（RuneCardUI）

**腳本：** `Assets/Scripts/UI/RuneCardUI.cs`
**Prefab：** `Assets/Prefabs/UI/RuneCard.prefab`

### Inspector 欄位
```
[Header("Display")]
[SerializeField] TMP_Text nameText
[SerializeField] Image    icon
[SerializeField] TMP_Text typeLabel      // "循環" or "消耗"
[SerializeField] Image    selectedBorder // 選中時顯示的邊框 / 高亮

[Header("Data")]
RuneInstance _rune  ← 由 HandPanel.Setup() 注入
```

### 點擊邏輯

```csharp
// IPointerClickHandler 實作
public void OnPointerClick(PointerEventData _)
{
    // 非 PlayerAction 階段：鎖定
    if (!IsInteractable) return;

    if (HandSelectionState.IsSelected(_rune))
    {
        // 再次點擊同一張 → 取消選中
        HandSelectionState.Deselect();
        // ↑ 會觸發 OnSelectionChanged → RefreshHighlight → AnimDeselect()
    }
    else
    {
        // 選中此符文
        HandSelectionState.Select(_rune);
        // ↑ 會觸發 OnSelectionChanged → RefreshHighlight → AnimSelect()
    }
}
```

### 選中視覺效果

```csharp
void Awake()
{
    HandSelectionState.OnSelectionChanged += RefreshHighlight;
}

void OnDestroy()
{
    HandSelectionState.OnSelectionChanged -= RefreshHighlight;
}

void RefreshHighlight()
{
    if (HandSelectionState.IsSelected(_rune))
        AnimSelect();
    else
        AnimDeselect();
}

// ── 動畫方法（DOTween 預留點）──────────────────
private void AnimSelect()
{
    transform.localScale = Vector3.one * 1.1f;   // TODO: DOTween DOScale(1.1f, 0.12f).SetEase(Ease.OutBack)
    selectedBorder.enabled = true;               // TODO: DOTween selectedBorder.DOFade(1f, 0.1f)
}

private void AnimDeselect()
{
    transform.localScale = Vector3.one;          // TODO: DOTween DOScale(1f, 0.1f)
    selectedBorder.enabled = false;              // TODO: DOTween selectedBorder.DOFade(0f, 0.08f)
}

// 符文被放入槽位時（由 HandPanel 在 OnHandChanged 後對消失的卡牌呼叫）
private void AnimPlaced()
{
    gameObject.SetActive(false);                 // TODO: DOTween DOFade(0, 0.2f).OnComplete(SetInactive)
}

// 符文從槽位退回手牌時（由 HandPanel 在 OnHandChanged 後對新出現的卡牌呼叫）
private void AnimReturn()
{
    // 直接顯示                                  // TODO: DOTween DOFade from 0→1, 0.15f
}
```

### 互動鎖定

```csharp
public bool IsInteractable { get; set; } = true;
// 由 PhaseLabel 在非 PlayerAction 時設為 false
```

---

## 三、卷軸槽位（SlotUI）

**腳本：** `Assets/Scripts/UI/SlotUI.cs`
**Prefab：** `Assets/Prefabs/UI/Slot.prefab`

### Inspector 欄位
```
[SerializeField] Image    slotBackground  // 空槽：灰底；有符文：亮底
[SerializeField] Image    runeIcon
[SerializeField] TMP_Text runeNameText
[SerializeField] GameObject emptyIndicator  // "空" 提示，有符文時隱藏
int _slotIndex  ← 由 WorkbenchPanel 注入
```

### 點擊邏輯

```csharp
public void OnPointerClick(PointerEventData _)
{
    if (!IsInteractable) return;

    var wb = GameFacade.Instance.Workbench;
    bool hasRune = wb.PlacedRunes[_slotIndex] != null;

    if (hasRune)
    {
        // 點擊已有符文的槽位 → 退回手牌，清除選中
        GameFacade.Instance.RemoveRuneAt(_slotIndex);
        HandSelectionState.Deselect();
    }
    else if (HandSelectionState.SelectedRune != null)
    {
        // 有選中符文 + 空槽 → 放入
        bool ok = GameFacade.Instance.PlaceRune(HandSelectionState.SelectedRune);
        if (ok) HandSelectionState.Deselect();
    }
    // else：空槽 + 無選中 → 無操作
}
```

### 點擊邏輯

```csharp
public void OnPointerClick(PointerEventData _)
{
    if (!IsInteractable) return;

    var wb = GameFacade.Instance.Workbench;
    bool hasRune = wb.PlacedRunes[_slotIndex] != null;

    if (hasRune)
    {
        GameFacade.Instance.RemoveRuneAt(_slotIndex);
        HandSelectionState.Deselect();
        AnimEmpty();   // ← 動畫預留點
    }
    else if (HandSelectionState.SelectedRune != null)
    {
        bool ok = GameFacade.Instance.PlaceRune(HandSelectionState.SelectedRune);
        if (ok)
        {
            HandSelectionState.Deselect();
            AnimFill();   // ← 動畫預留點
        }
    }
}
```

### 顯示刷新

```csharp
// 由 WorkbenchPanel 在 OnWorkbenchChanged 時呼叫
// 注意：動畫（AnimFill/AnimEmpty）由點擊事件觸發，Refresh 只負責同步最終狀態
public void Refresh(RuneData runeInSlot)
{
    bool filled = runeInSlot != null;
    emptyIndicator.SetActive(!filled);
    runeIcon.enabled     = filled;
    runeNameText.enabled = filled;

    if (filled)
    {
        runeIcon.sprite    = runeInSlot.icon;
        runeNameText.text  = runeInSlot.runeName;
        slotBackground.color = Color.white;
    }
    else
    {
        slotBackground.color = new Color(0.6f, 0.6f, 0.6f);
    }
}

// ── 動畫方法（DOTween 預留點）──────────────────
private void AnimFill()
{
    transform.localScale = Vector3.one;             // TODO: DOTween DOPunchScale(Vector3.one * 0.2f, 0.3f)
}

private void AnimEmpty()
{
    // 無動作                                       // TODO: DOTween DOShakePosition(0.15f, strength: 3f)
}
```

---

## 四、卷軸選擇列（ScrollSelectorUI）

**腳本：** `Assets/Scripts/UI/ScrollSelectorUI.cs`
**子元件：** `ScrollButtonUI.cs`（每張卷軸一個按鈕）

### 卷軸按鈕點擊邏輯

```
流程：
  點擊卷軸按鈕
      │
      ├─ 已選中的同一個卷軸？
      │       ↓ 是
      │   （不做任何事，或可設計折疊/展開）
      │
      └─ 其他卷軸 / 未選中卷軸
              ↓
          GameFacade.Instance.SelectScroll(scroll)
              ↓
          WorkbenchManager 自動歸還所有槽位符文
          HandSelectionState.Deselect()  ← 同時清除手牌選中狀態
```

```csharp
// ScrollButtonUI.cs
public void OnPointerClick(PointerEventData _)
{
    if (!IsInteractable) return;

    var current = GameFacade.Instance.Workbench.SelectedScroll;
    if (current == _scroll) return;  // 已選中，略過

    GameFacade.Instance.SelectScroll(_scroll);
    HandSelectionState.Deselect();
}
```

### 高亮已選中的卷軸

```csharp
// ScrollButtonUI.Refresh() 由 WorkbenchPanel 在 OnWorkbenchChanged 後呼叫
public void Refresh(ScrollData selectedScroll)
{
    bool isSelected = selectedScroll == _scroll;
    if (isSelected) AnimSelect();
    else            AnimDeselect();
}

// ── 動畫方法（DOTween 預留點）──────────────────
private void AnimSelect()
{
    buttonBackground.color    = new Color(1f, 0.9f, 0.5f);   // TODO: DOTween DOColor(goldColor, 0.15f)
    selectedHighlight.enabled = true;
}

private void AnimDeselect()
{
    buttonBackground.color    = Color.white;                  // TODO: DOTween DOColor(white, 0.15f)
    selectedHighlight.enabled = false;
}
```

---

## 五、刪除卷軸按鈕（DiscardScrollButton）

每個 `ScrollButtonUI` prefab 上附帶一個刪除按鈕（小 ✕）。

```csharp
// ScrollButtonUI 中的刪除按鈕
[SerializeField] Button discardButton;

// OnEnable 時綁定
discardButton.onClick.AddListener(() =>
{
    // 若刪除的是當前選中的卷軸，先清空工作台
    var current = GameFacade.Instance.Workbench.SelectedScroll;
    if (current == _scroll)
        GameFacade.Instance.SelectScroll(null);  // 或設計 DeselectScroll() API

    GameFacade.Instance.DiscardScroll(_scroll);
});
```

> **GameFacade 需新增：**
> ```csharp
> public void DiscardScroll(ScrollData scroll)
>     => ScrollInventory.Instance.DiscardScroll(scroll);
> ```

---

## 六、刪除遺物按鈕（DiscardRelicButton）

每個 `RelicIconUI` prefab 上附帶一個刪除按鈕（需設計是否需要二次確認）。

```csharp
// RelicIconUI 中的刪除按鈕
[SerializeField] Button discardButton;

discardButton.onClick.AddListener(() =>
{
    // 可選：彈出確認對話框
    GameFacade.Instance.DiscardRelic(_relicData);
});
```

> **GameFacade 需新增：**
> ```csharp
> public void DiscardRelic(RelicData relic)
>     => RelicManager.Instance.RemoveRelic(relic);
> ```
>
> **RelicManager 需新增：**
> ```csharp
> public void RemoveRelic(RelicData data)
> {
>     _relics.RemoveAll(r => r.Data == data);
>     OnRelicsChanged?.Invoke(GetSnapshots());
> }
> ```

---

## 七、送出按鈕（SubmitButton）

**掛載在：** `WorkbenchPanel` 內

```csharp
// WorkbenchPanel.cs 片段

[SerializeField] Button submitButton;

// OnWorkbenchChanged 時刷新
void RefreshSubmitButton(WorkbenchSnapshot wb)
{
    submitButton.interactable = wb.CanSubmit && _isPlayerTurn;
}

// PhaseLabel 通知 PlayerAction 階段
public void SetPlayerTurn(bool isPlayerTurn)
{
    _isPlayerTurn = isPlayerTurn;
    var wb = GameFacade.Instance.Workbench;
    RefreshSubmitButton(wb);
}

// OnWorkbenchChanged 時刷新送出按鈕動畫
void RefreshSubmitButton(WorkbenchSnapshot wb)
{
    bool canSubmit = wb.CanSubmit && _isPlayerTurn;
    bool wasInteractable = submitButton.interactable;
    submitButton.interactable = canSubmit;

    // CanSubmit 剛變成 true 時：播放提示動畫
    if (canSubmit && !wasInteractable)
        AnimSubmitAvailable();
}

// 按鈕 OnClick
public void OnSubmitClicked()
{
    AnimSubmitClick();                           // ← 動畫預留點（先播動畫）
    GameFacade.Instance.SubmitScroll();
    HandSelectionState.Deselect();
}

// ── 動畫方法（DOTween 預留點）──────────────────
private void AnimSubmitAvailable()
{
    // 無動作                                   // TODO: DOTween submitButton.transform.DOPunchScale(...)
}

private void AnimSubmitClick()
{
    // 無動作                                   // TODO: DOTween DOScale(0.9f, 0.05f).Then().DOScale(1f, 0.1f)
}
```

---

## 八、Hover Tooltip 系統

### 架構

```
TooltipManager（Singleton）
    ├─ TooltipPanel（Canvas 最上層的 UI 面板）
    │    ├─ titleText : TMP_Text
    │    ├─ bodyText  : TMP_Text
    │    └─ iconImage : Image（可選）
    │
    └─ 每個可 hover 元件實作 ITooltipProvider 並使用
       TooltipTrigger.cs（IPointerEnterHandler / IPointerExitHandler）
```

### TooltipManager.cs

```csharp
// Assets/Scripts/UI/Tooltip/TooltipManager.cs
public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance { get; private set; }

    [SerializeField] GameObject tooltipPanel;
    [SerializeField] TMP_Text   titleText;
    [SerializeField] TMP_Text   bodyText;
    [SerializeField] Image      iconImage;
    [SerializeField] Vector2    offset = new Vector2(15, -15);

    void Awake() { Instance = this; Hide(); }

    public void Show(TooltipData data, Vector2 screenPos)
    {
        titleText.text = data.Title;
        bodyText.text  = data.Body;

        if (data.Icon != null) { iconImage.sprite = data.Icon; iconImage.enabled = true; }
        else                   { iconImage.enabled = false; }

        tooltipPanel.SetActive(true);
        PositionAt(screenPos);
        AnimShow();   // ← 動畫預留點
    }

    public void Hide()
    {
        AnimHide();   // ← 動畫預留點（未來 DOTween 結束後才 SetActive(false)）
        tooltipPanel.SetActive(false);
    }

    // ── 動畫方法（DOTween 預留點）──────────────────
    private void AnimShow()
    {
        // 直接顯示                             // TODO: DOTween canvasGroup.DOFade(1f, 0.1f) from 0
    }

    private void AnimHide()
    {
        // 直接隱藏                             // TODO: DOTween canvasGroup.DOFade(0f, 0.08f).OnComplete(Hide)
        //                                     //        （完成後才 SetActive(false)，需重構 Hide() 為非同步）
    }

    void Update()
    {
        // Tooltip 跟隨滑鼠
        if (tooltipPanel.activeSelf)
            PositionAt(Input.mousePosition);
    }

    void PositionAt(Vector2 screenPos)
    {
        var rt = tooltipPanel.GetComponent<RectTransform>();
        // 防止超出螢幕右側 / 下方
        Vector2 pos = screenPos + offset;
        float w = rt.rect.width, h = rt.rect.height;
        pos.x = Mathf.Min(pos.x, Screen.width  - w - 10);
        pos.y = Mathf.Max(pos.y, h + 10);
        rt.position = pos;
    }
}
```

### TooltipData 結構

```csharp
public struct TooltipData
{
    public string Title;
    public string Body;
    public Sprite Icon;   // 可為 null
}
```

### TooltipTrigger.cs（共用 Component）

```csharp
// Assets/Scripts/UI/Tooltip/TooltipTrigger.cs
// 掛在任何需要 tooltip 的 UI 元件上

public class TooltipTrigger : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    // 由外部（e.g. RuneCardUI.Setup()）注入
    public System.Func<TooltipData> DataProvider { get; set; }

    public void OnPointerEnter(PointerEventData e)
    {
        if (DataProvider == null) return;
        TooltipManager.Instance.Show(DataProvider(), e.position);
    }

    public void OnPointerExit(PointerEventData _)
        => TooltipManager.Instance.Hide();
}
```

---

## 九、各元件的 Tooltip 內容

### 9-A 符文卡（RuneData）

```csharp
// RuneCardUI.Setup() 中設定
_tooltipTrigger.DataProvider = () => new TooltipData
{
    Title = $"{rune.runeName}  [{RuneTypeLabel(rune.runeType)}]",
    Body  = BuildRuneBody(rune),
    Icon  = rune.icon
};

string BuildRuneBody(RuneData r)
{
    var sb = new System.Text.StringBuilder();
    sb.AppendLine(r.description);
    sb.AppendLine();
    foreach (var e in r.effects)
        sb.AppendLine($"  {AttributeName(e.attribute)}  {(e.value >= 0 ? "+" : "")}{e.value}");
    if (r.discardOtherRunes)
        sb.AppendLine("\n⚠ 使用後丟棄同卷軸其他符文");
    return sb.ToString();
}

string RuneTypeLabel(RuneType t) => t == RuneType.Cycle ? "循環" : "消耗";
```

**範例顯示：**
```
水滴符文  [循環]
────────────────
注入清涼水氣。

  水分  +3
  溫度  -1
```

---

### 9-B 卷軸按鈕（ScrollData）

```csharp
_tooltipTrigger.DataProvider = () => new TooltipData
{
    Title = scroll.scrollName,
    Body  = $"槽位：{scroll.slotCount}\n" +
            $"計算方式：{scroll.GetModifierDescription()}\n\n" +
            scroll.description,
    Icon  = scroll.icon
};
```

**範例顯示：**
```
增幅卷軸
────────────────
槽位：3
計算方式：所有效果 × 1.5

放大所有符文的力量。
```

---

### 9-C 卷軸槽位（SlotUI，有符文時）

```csharp
// SlotUI.Refresh() 時更新 TooltipTrigger
if (runeInSlot != null)
{
    _tooltipTrigger.DataProvider = () => new TooltipData
    {
        Title = runeInSlot.runeName,
        Body  = $"點擊退回手牌\n\n" + BuildRuneBody(runeInSlot),
        Icon  = runeInSlot.icon
    };
}
else
{
    _tooltipTrigger.DataProvider = HandSelectionState.SelectedRune != null
        ? () => new TooltipData { Title = "點擊放入", Body = "" }
        : null;
}
```

---

### 9-D 遺物圖示（RelicSnapshot）

```csharp
// RelicIconUI.Setup() 中設定
_tooltipTrigger.DataProvider = () => new TooltipData
{
    Title = relic.RelicName,
    Body  = $"【滿足條件】{BuildConditionText(relic)}\n" +
            $"滿足效果：{relic.SatisfiedEffectText}\n" +
            $"不滿足副作用：{relic.UnsatisfiedEffectText}\n\n" +
            $"⚠ 懲罰（{relic.DissatisfiedCount}/{relic.DissatisfiedLimit} 回合）：\n" +
            $"  {relic.PunishmentText}",
    Icon  = relic.Icon
};
```

**範例顯示：**
```
月光石
────────────────
【滿足條件】亮度 <= 5
滿足效果：每回合業績 +2
不滿足副作用：亮度 -1

⚠ 懲罰（1/3 回合）：
  業績清零
```

---

### 9-E 顧客需求列（RequirementSnapshot）

```csharp
// RequirementRowUI.Setup() 中設定
_tooltipTrigger.DataProvider = () => new TooltipData
{
    Title = req.IsSatisfied ? "✓ 已達成" : "✗ 未達成",
    Body  = req.DisplayText + "\n\n" +
            $"進度：{(req.Progress * 100):F0}%",
    Icon  = null
};
```

---

## 十、互動鎖定機制（非 PlayerAction 階段）

由 `PhaseLabel.cs`（或 `InteractionLockManager.cs`）統一控制。

```csharp
// 訂閱 OnPhaseChanged 後呼叫
void SetInteractable(bool isPlayerTurn)
{
    // 手牌
    foreach (var card in _handPanel.Cards)
        card.IsInteractable = isPlayerTurn;

    // 槽位
    foreach (var slot in _workbenchPanel.SlotUIs)
        slot.IsInteractable = isPlayerTurn;

    // 卷軸選擇按鈕
    foreach (var btn in _scrollSelector.Buttons)
        btn.IsInteractable = isPlayerTurn;

    // 送出按鈕（由 WorkbenchPanel 自己管理 CanSubmit）
    _workbenchPanel.SetPlayerTurn(isPlayerTurn);

    // 非 PlayerAction 時，清除選中狀態
    if (!isPlayerTurn) HandSelectionState.Deselect();
}
```

---

## 十一、腳本清單與建立順序

| 優先 | 腳本 | 路徑 | 職責 |
|------|------|------|------|
| 1 | `HandSelectionState.cs` | UI/Interaction/ | 全域選中符文狀態 |
| 2 | `RuneCardUI.cs` | UI/ | 手牌符文卡點擊、高亮 |
| 3 | `SlotUI.cs` | UI/ | 槽位放入/退回邏輯 |
| 4 | `ScrollButtonUI.cs` | UI/ | 卷軸選擇、刪除 |
| 5 | `TooltipManager.cs` | UI/Tooltip/ | Tooltip 顯示與定位 |
| 6 | `TooltipTrigger.cs` | UI/Tooltip/ | Hover 觸發 Component |
| 7 | `WorkbenchPanel.cs` | UI/ | 統籌工作台 UI（已在 TODO 中） |
| 8 | `HandPanel.cs` | UI/ | 統籌手牌 UI（已在 TODO 中） |

---

## 十二、GameFacade 需新增的 API（目前缺少）

下列 API 在現有 `GameFacade` 或 Manager 中尚未存在，需要補充：

```csharp
// GameFacade.cs 新增
public void DiscardScroll(ScrollData scroll)
    => ScrollInventory.Instance.DiscardScroll(scroll);

public void DiscardRelic(RelicData relic)
    => RelicManager.Instance.RemoveRelic(relic);

// 若需要清除卷軸選擇
public void DeselectScroll()
    => WorkbenchManager.Instance.SelectScroll(null);  // 需 WorkbenchManager 容許 null

// RelicManager.cs 新增
public void RemoveRelic(RelicData data) { ... }
```

---

*建立日期：2026-03-21*
*對應文件：UI_TODO.md、UI_entry_point.md*

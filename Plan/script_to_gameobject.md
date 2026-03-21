# 腳本 → GameObject 對應表

> 腳本分成三類：
> - **掛在 GameObject**：MonoBehaviour，需要在 Hierarchy 中指定物件
> - **ScriptableObject 資產**：存在 Assets 中的資料檔，不掛也不需要 GO
> - **純 C# / 靜態類別**：由其他腳本在程式中建立或呼叫，不需要 GO

---

## 一、需要掛在 GameObject 的腳本（MonoBehaviour）

### [GameSystems] 群組（空物件，放所有 Manager）

| 腳本檔案 | 掛在哪個 GameObject | 備注 |
|----------|-------------------|------|
| `GameMAnager.cs` | **GameManager** | Singleton，場景唯一 |
| `GameFacade.cs` | **GameManager**（同一個 GO） | 與 GameManager 共用同一個物件 |
| `TurnManager.cs` | **TurnManager** | Singleton |
| `EnvironmentManager.cs` | **EnvironmentManager** | Singleton |
| `DeckManager.cs` | **DeckManager** | Singleton |
| `WorkbenchManager.cs` | **WorkbenchManager** | Singleton |
| `ScrollInventory.cs` | **ScrollInventory** | Singleton |
| `CustomerManager.cs` | **CustomerManager** | Singleton |
| `RelicManager.cs` | **RelicManager** | Singleton |

**場景 Hierarchy 建法：**
```
[GameSystems]               ← 空 GameObject，純容器
  ├── GameManager            ← 掛 GameMAnager.cs + GameFacade.cs
  ├── TurnManager            ← 掛 TurnManager.cs
  ├── EnvironmentManager     ← 掛 EnvironmentManager.cs
  ├── DeckManager            ← 掛 DeckManager.cs
  ├── WorkbenchManager       ← 掛 WorkbenchManager.cs
  ├── ScrollInventory        ← 掛 ScrollInventory.cs
  ├── CustomerManager        ← 掛 CustomerManager.cs
  └── RelicManager           ← 掛 RelicManager.cs
```

---

### 捨棄 / 暫不使用

| 腳本檔案 | 狀態 | 原因 |
|----------|------|------|
| `MoveCard.cs`（SpriteMover） | ⚠️ 測試殘留，可刪除 | 只是 DOTween 測試，無遊戲用途 |

---

## 二、ScriptableObject 資料檔（存在 Assets，不掛不建 GO）

這些腳本定義「資料模板」，在 Unity Editor 中建立為 `.asset` 檔，
拖入 Inspector 或 GameBootstrapper 的清單中使用。

| 腳本檔案 | 建立方式 | 用在哪裡 |
|----------|----------|---------|
| `RuneData.cs` | 右鍵 → Create → SpellShop → Rune | GameBootstrapper.startingDeck |
| `ScrollData.cs` | 右鍵 → Create → SpellShop → Scroll | GameBootstrapper.startingScrolls |
| `CustomerData.cs` | 右鍵 → Create → SpellShop → Customer | GameBootstrapper.customerQueue |
| `RelicData.cs` | 右鍵 → Create → SpellShop → Relic | GameBootstrapper.startingRelics |

---

## 三、純 C# 類別（不需要 GameObject，由程式自動管理）

這些腳本由其他 MonoBehaviour 或 Manager 在執行期建立與呼叫，**不用掛在任何 GO**。

### 3-A 由 Manager 在執行時建立的實例類別

| 腳本檔案 | 由誰建立 | 說明 |
|----------|---------|------|
| `RuneInstance.cs` | `DeckManager.InitDeck()` | 包裝 RuneData，追蹤消耗狀態 |
| `CustomerInstance.cs` | `CustomerManager.SpawnNextCustomer()` | 包裝 CustomerData，追蹤耐心與進度 |
| `RelicInstance.cs` | `RelicManager.InitRelics()` | 包裝 RelicData，追蹤不滿意計數 |
| `ScrollInstance.cs` | （目前未使用，保留備用） | 包裝 ScrollData |
| `SubmitRecord.cs` | `WorkbenchManager.Submit()` | 記錄一次送出的卷軸+符文組合 |
| `EnvironmentData.cs` | `EnvironmentManager`（成員變數） | 儲存三個環境屬性數值 |
| `ExtremeEventHandler.cs` | `EnvironmentManager.Awake()` | 處理極端事件邏輯，由 EM 持有 |
| `RewardResult.cs`（在 RewardManager.cs 內） | `RewardManager.Grant()` | 記錄本次獎勵結果 |

### 3-B 需求判定類別（由 RequirementFactory 建立）

| 腳本檔案 | 由誰建立 | 說明 |
|----------|---------|------|
| `AbsoluteRequirement.cs` | `RequirementFactory.Create()` | 絕對值條件判定 |
| `RelativeChangeRequirement.cs` | `RequirementFactory.Create()` | 累積變化量條件判定 |
| `StabilityRequirement.cs` | `RequirementFactory.Create()` | 連續維持條件判定 |

### 3-C 回合狀態類別（由 TurnManager.InitializeStates() 建立）

| 腳本檔案 | 由誰建立 | 對應回合階段 |
|----------|---------|-------------|
| `CustomerArrivalState.cs` | `TurnManager` | Phase 1：顧客到來 |
| `RelicTriggerState.cs` | `TurnManager` | Phase 2：遺物結算 |
| `DrawCardsState.cs` | `TurnManager` | Phase 3：抽牌 |
| `PlayerActionState.cs` | `TurnManager` | Phase 4：玩家行動 |
| `EnvironmentResolveState.cs` | `TurnManager` | Phase 5：環境結算 |
| `CustomerJudgeState.cs` | `TurnManager` | Phase 6：顧客判定 |
| `PatienceCheckState.cs` | `TurnManager` | Phase 7：耐心扣減 |
| `RelicUpdateState.cs` | `TurnManager` | Phase 8：計分/回合結束 |

### 3-D 靜態工具類別（直接用類別名稱呼叫，無需任何 GO）

| 腳本檔案 | 呼叫者 | 說明 |
|----------|--------|------|
| `ScrollProcessor.cs` | `EnvironmentResolveState` | 計算符文效果，static class |
| `RequirementFactory.cs` | `CustomerInstance` 建構子 | 依型別建立需求物件，static class |
| `RewardManager.cs` | `CustomerManager`、`RelicManager` | 分發獎勵並觸發事件，static class |

### 3-E 純定義檔（枚舉、struct、interface）

| 腳本檔案 | 說明 |
|----------|------|
| `GameDefinitions.cs` | 所有枚舉（EnvAttribute / RuneType / RuneTag...）與共用 struct |
| `GameFacadeSnapshots.cs` | UI 用的快照 struct（WorkbenchSnapshot / CustomerSnapshot...） |
| `IGameState.cs` | 回合狀態介面（Enter/Execute/Exit） |
| `IRequirement.cs` | 需求介面 + RequirementContext struct |
| `RequirementData.cs` | `[Serializable]` 嵌入在 CustomerData.requirements 中 |
| `RewardEntry.cs` | `[Serializable]` 嵌入在 CustomerData / RelicData 的 rewards 中 |

---

## 四、一眼速查圖

```
場景 Hierarchy
│
├─ [GameSystems]
│    ├─ GameManager ──────── GameMAnager.cs
│    │                       GameFacade.cs          ← 兩個腳本掛同一個 GO
│    ├─ TurnManager ──────── TurnManager.cs
│    │                         └─ 建立 8 個 State 物件（不是 GO）
│    ├─ EnvironmentManager ── EnvironmentManager.cs
│    │                         └─ 建立 ExtremeEventHandler（不是 GO）
│    ├─ DeckManager ─────── DeckManager.cs
│    │                         └─ 建立 RuneInstance 列表（不是 GO）
│    ├─ WorkbenchManager ── WorkbenchManager.cs
│    │                         └─ 建立 SubmitRecord（不是 GO）
│    ├─ ScrollInventory ─── ScrollInventory.cs
│    ├─ CustomerManager ─── CustomerManager.cs
│    │                         └─ 建立 CustomerInstance（不是 GO）
│    │                              └─ RequirementFactory 建立 Requirement（不是 GO）
│    └─ RelicManager ────── RelicManager.cs
│                              └─ 建立 RelicInstance 列表（不是 GO）
│
└─ [Canvas]
     ├─ ... (各 Panel，掛 UI 腳本，UI_TODO.md 中定義)
     └─ ... (Prefab 動態生成，prefab_list.md 中定義)

Assets/ （不在 Hierarchy 中）
  ├─ RuneData.asset    ← RuneData.cs
  ├─ ScrollData.asset  ← ScrollData.cs
  ├─ CustomerData.asset← CustomerData.cs
  └─ RelicData.asset   ← RelicData.cs

純程式（不在 Hierarchy 也不在 Assets）
  ScrollProcessor  ← static，直接呼叫
  RequirementFactory ← static，直接呼叫
  RewardManager    ← static，直接呼叫
```

---

*建立日期：2026-03-21*

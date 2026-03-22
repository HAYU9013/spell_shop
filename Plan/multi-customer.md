# 多顧客並存設計計畫

## 現況

目前架構是**嚴格單一顧客**設計：

- `CustomerManager` 只有 `_currentCustomer`（單一實例）
- Phase 1 只在「無顧客」時才 Spawn 一位
- `GameFacade.CurrentCustomer` 回傳單一 `CustomerSnapshot`
- `CustomerUI` 只渲染一個顧客面板

---

## 核心概念

每回合可有 **N 位顧客**同時在場，共用同一份環境。
玩家每回合仍只送出一次配方，Phase 5~7 對所有顧客逐一結算。

新增設定：`maxActiveCustomers`（預設 2），控制同時在場上限。

---

## 回合流程變更

```
Phase 1: CustomerArrival
  while (activeCount < maxActiveCustomers && queue 有人)
      Spawn 一位

Phase 5: EnvironmentResolve
  foreach customer in activeCustomers
      UpdateTracking(currentEnv)          ← 原本只更新一位

Phase 6: CustomerJudge
  foreach customer in activeCustomers
      CheckRequirements → 滿足 → 獎勵 + 移出

Phase 7: PatienceCheck
  foreach customer in activeCustomers（未在 Phase 6 離開者）
      TickPatience → 耗盡 → 懲罰 + 移出
```

其餘 Phase（2 RelicTrigger、3 DrawCards、4 PlayerAction、8 RelicUpdate）不受影響。

---

## 需修改的檔案

### 1. `CustomerManager.cs` ★ 核心

| 現在 | 改成 |
|------|------|
| `CustomerInstance _currentCustomer` | `List<CustomerInstance> _activeCustomers` |
| `SpawnNextCustomer()` | `SpawnUntilFull()` — 填滿到 `maxActiveCustomers` |
| `CheckCurrentCustomer()` | `CheckAllCustomers()` — foreach，滿足者移出 |
| `TickPatience()` | `TickAllPatience()` — foreach，耗盡者移出 |
| `UpdateCurrentTracking()` | `UpdateAllTracking()` — foreach |

新增欄位：
```csharp
[SerializeField] private int maxActiveCustomers = 2;
public IReadOnlyList<CustomerInstance> ActiveCustomers => _activeCustomers;
```

---

### 2. `TurnState/CustomerArrivalState.cs` ★ 小改

```csharp
// 舊：SpawnNextCustomer()（只 Spawn 一位）
// 新：SpawnUntilFull()（迴圈到上限）
```

---

### 3. `TurnState/CustomerJudgeState.cs` ★ 小改

```csharp
// 舊：CheckCurrentCustomer()
// 新：CheckAllCustomers()
```

---

### 4. `TurnState/PatienceCheckState.cs` ★ 小改

```csharp
// 舊：TickPatience()
// 新：TickAllPatience()
```

---

### 5. `TurnState/EnvironmentResolveState.cs` ★ 小改

```csharp
// 舊：UpdateCurrentTracking()
// 新：UpdateAllTracking()
```

---

### 6. `GameFacadeSnapshots.cs` ★ 小改

```csharp
// CustomerSnapshot 結構本身保留不變
// 移除（或保留相容）：CustomerSnapshot CurrentCustomer
// 新增：
public IReadOnlyList<CustomerSnapshot> ActiveCustomers;
```

---

### 7. `GameFacade.cs` ★ 中改

| 現在 | 改成 |
|------|------|
| `CurrentCustomer` (single) | `ActiveCustomers` (`IReadOnlyList<CustomerSnapshot>`) |
| `OnCustomerChanged` (single) | `OnCustomersChanged` (傳整個列表) |
| `OnRequirementProgressUpdated` | 改為傳列表 |

內部 `BuildCurrentCustomerSnapshot()` 改為 `BuildActiveCustomerSnapshots()`。

---

### 8. `CustomerUI.cs` ★ 大改

- 現在：固定渲染一個顧客面板
- 改成：訂閱 `OnCustomersChanged`，動態生成/銷毀顧客卡片
- 架構參考 `RelicUI`（slot + prefab instantiate 模式）
- 需要一個 **CustomerCard prefab**，封裝單一顧客的顯示邏輯

---

## 改動幅度總覽

| 檔案 | 幅度 | 說明 |
|------|------|------|
| `CustomerManager.cs` | 大 | 核心邏輯全面複數化 |
| `CustomerArrivalState.cs` | 小 | 改呼叫 SpawnUntilFull |
| `CustomerJudgeState.cs` | 小 | 改呼叫 CheckAllCustomers |
| `PatienceCheckState.cs` | 小 | 改呼叫 TickAllPatience |
| `EnvironmentResolveState.cs` | 小 | 改呼叫 UpdateAllTracking |
| `GameFacade.cs` | 中 | API 改為列表 |
| `GameFacadeSnapshots.cs` | 小 | 加列表欄位 |
| `CustomerUI.cs` | 大 | 多卡片動態渲染 |

---

## 建議實作順序

1. `CustomerManager` — 複數化核心邏輯
2. 4 個 TurnState — 改呼叫新方法
3. `GameFacade` + `GameFacadeSnapshots` — 暴露列表 API
4. `CustomerUI` — 多卡片 UI 渲染

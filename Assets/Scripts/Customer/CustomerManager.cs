using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 顧客管理器（MonoBehaviour Singleton）。
///
/// 關卡模式：持有固定的 CustomerData 隊列，清空即通關。
/// 無限模式：從顧客池隨機抽取，永不耗盡。
///
/// 狀態機整合：
///   Phase 1  → SpawnNextCustomer()（若無當前顧客）
///   Phase 5  → UpdateCurrentTracking(env)
///   Phase 6  → CheckCurrentCustomer(env) → 滿足則 DismissWithReward()
///   Phase 7  → TickPatience() → 耐盡則 DismissWithPenalty()
/// </summary>
public class CustomerManager : MonoBehaviour
{
    // =========================================================
    // Singleton
    // =========================================================

    public static CustomerManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // =========================================================
    // 事件
    // =========================================================

    /// <summary>新顧客到來時觸發</summary>
    public event System.Action<CustomerInstance> OnCustomerArrived;

    /// <summary>顧客需求滿足、發放獎勵後觸發</summary>
    public event System.Action<CustomerInstance> OnCustomerSatisfied;

    /// <summary>顧客耐心耗盡憤怒離開後觸發</summary>
    public event System.Action<CustomerInstance> OnCustomerLeft;

    /// <summary>顧客耐心扣除後觸發（未耗盡時）</summary>
    public event System.Action<CustomerInstance> OnPatienceChanged;

    /// <summary>關卡模式隊列清空時觸發（通關）</summary>
    public event System.Action OnQueueEmpty;

    // =========================================================
    // 狀態
    // =========================================================

    private readonly Queue<CustomerData> _queue       = new Queue<CustomerData>();
    private readonly List<CustomerData>  _endlessPool = new List<CustomerData>();

    private CustomerInstance _currentCustomer;

    private bool _isEndlessMode = false;

    /// <summary>當前在店顧客（null = 無顧客）</summary>
    public CustomerInstance CurrentCustomer => _currentCustomer;

    /// <summary>是否有顧客正在等待</summary>
    public bool HasCustomer => _currentCustomer != null;

    // =========================================================
    // 初始化
    // =========================================================

    /// <summary>關卡模式：設定固定顧客隊列</summary>
    public void InitLevelMode(List<CustomerData> queue)
    {
        _queue.Clear();
        _endlessPool.Clear();
        _isEndlessMode = false;
        foreach (var c in queue)
            _queue.Enqueue(c);
        Debug.Log($"[CustomerManager] 關卡模式，隊列 {_queue.Count} 位顧客");
    }

    /// <summary>無限模式：設定隨機抽取池</summary>
    public void InitEndlessMode(List<CustomerData> pool)
    {
        _queue.Clear();
        _endlessPool.Clear();
        _endlessPool.AddRange(pool);
        _isEndlessMode = true;
        Debug.Log($"[CustomerManager] 無限模式，顧客池 {_endlessPool.Count} 種");
    }

    // =========================================================
    // Phase 1：迎接顧客
    // =========================================================

    /// <summary>
    /// 若目前無顧客，從隊列取出下一位並建立 Instance。
    /// 回傳 null 表示隊列已空（關卡模式通關）。
    /// </summary>
    public CustomerInstance SpawnNextCustomer()
    {
        if (_currentCustomer != null) return _currentCustomer;

        CustomerData next = GetNextCustomerData();
        if (next == null)
        {
            Debug.Log("[CustomerManager] 隊列已空 → 關卡通關");
            OnQueueEmpty?.Invoke();
            return null;
        }

        var snapshot = EnvironmentManager.Instance != null
            ? EnvironmentManager.Instance.SnapshotCurrent()
            : new EnvironmentData();

        _currentCustomer = new CustomerInstance(next, snapshot);
        Debug.Log($"[CustomerManager] 新顧客到來：{_currentCustomer}");
        OnCustomerArrived?.Invoke(_currentCustomer);
        return _currentCustomer;
    }

    // =========================================================
    // Phase 5：更新追蹤
    // =========================================================

    /// <summary>環境結算後呼叫，更新當前顧客的累積 delta 與穩定性計數</summary>
    public void UpdateCurrentTracking()
    {
        if (_currentCustomer == null) return;

        var envData = GetCurrentEnvData();
        _currentCustomer.UpdateTracking(envData);
    }

    // =========================================================
    // Phase 6：需求判定
    // =========================================================

    /// <summary>
    /// 判斷當前顧客是否滿足需求。
    /// 滿足 → 發放獎勵、業績 +N、顧客離開 → 回傳 true
    /// 未滿足 → 回傳 false，進入 Phase 7
    /// </summary>
    public bool CheckCurrentCustomer()
    {
        if (_currentCustomer == null) return false;

        var envData = GetCurrentEnvData();
        bool satisfied = _currentCustomer.CheckRequirements(envData);

        if (satisfied)
        {
            var customer = _currentCustomer;

            // 業績獎勵（固定值）
            Debug.Log($"[CustomerManager] {customer.Data.customerName} 需求滿足！業績 +{customer.Data.scoreReward}");
            EnvironmentManager.Instance?.ModifyScore(customer.Data.scoreReward);

            // 符文 / 卷軸等額外獎勵
            RewardManager.Grant(customer.Data.bonusRewards, customer.Data.customerName);

            OnCustomerSatisfied?.Invoke(customer);
            _currentCustomer = null;

            CheckQueueEmptyAfterDismiss();
        }

        return satisfied;
    }

    // =========================================================
    // Phase 7：耐心扣除
    // =========================================================

    /// <summary>
    /// 耐心 -1。耐心耗盡 → 業績 -N、顧客憤怒離開。
    /// Phase 6 已滿足的回合不應呼叫此方法。
    /// </summary>
    public void TickPatience()
    {
        if (_currentCustomer == null) return;

        bool exhausted = _currentCustomer.TickPatience();
        Debug.Log($"[CustomerManager] {_currentCustomer}");

        if (exhausted)
        {
            Debug.Log($"[CustomerManager] {_currentCustomer.Data.customerName} 耐心耗盡！業績 -{_currentCustomer.Data.scorePenalty}");
            EnvironmentManager.Instance?.ModifyScore(-_currentCustomer.Data.scorePenalty);
            OnCustomerLeft?.Invoke(_currentCustomer);
            _currentCustomer = null;

            CheckQueueEmptyAfterDismiss();
        }
        else
        {
            OnPatienceChanged?.Invoke(_currentCustomer);
        }
    }

    // =========================================================
    // 查詢
    // =========================================================

    public bool IsQueueEmpty => !_isEndlessMode && _queue.Count == 0 && _currentCustomer == null;

    /// <summary>
    /// 隊列中剩餘顧客數（不含當前顧客）。
    /// 無限模式回傳 -1，UI 可顯示「∞」。
    /// </summary>
    public int RemainingInQueue => _isEndlessMode ? -1 : _queue.Count;

    /// <summary>取得當前顧客需求進度文字（供 UI 顯示）</summary>
    public List<string> GetCurrentProgressTexts()
    {
        if (_currentCustomer == null) return new List<string>();
        var envData = GetCurrentEnvData();
        return _currentCustomer.GetProgressTexts(envData);
    }

    // =========================================================
    // 私有工具
    // =========================================================

    /// <summary>
    /// 顧客離開後立即檢查：關卡模式且隊列已空 → 觸發 OnQueueEmpty。
    /// </summary>
    private void CheckQueueEmptyAfterDismiss()
    {
        if (!_isEndlessMode && _queue.Count == 0)
        {
            Debug.Log("[CustomerManager] 最後一位顧客離開，隊列清空 → 關卡通關");
            OnQueueEmpty?.Invoke();
        }
    }

    private CustomerData GetNextCustomerData()
    {
        if (_isEndlessMode)
        {
            if (_endlessPool.Count == 0) return null;
            return _endlessPool[Random.Range(0, _endlessPool.Count)];
        }
        return _queue.Count > 0 ? _queue.Dequeue() : null;
    }

    private EnvironmentData GetCurrentEnvData()
    {
        if (EnvironmentManager.Instance == null) return new EnvironmentData();
        return EnvironmentManager.Instance.SnapshotCurrent();
    }

    // =========================================================
    // Debug
    // =========================================================

    [ContextMenu("Debug_PrintCurrentCustomer")]
    private void Debug_PrintCurrentCustomer()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        if (_currentCustomer == null) { Debug.Log("[CustomerManager] 目前無顧客"); return; }
        Debug.Log(_currentCustomer.ToString());
        foreach (var t in GetCurrentProgressTexts())
            Debug.Log($"  需求：{t}");
    }

    [ContextMenu("Debug_ForceSpawnNext")]
    private void Debug_ForceSpawnNext()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        _currentCustomer = null;
        SpawnNextCustomer();
    }

    [ContextMenu("Debug_ForceCheckRequirements")]
    private void Debug_ForceCheckRequirements()
    {
        if (!Application.isPlaying) { Debug.LogWarning("請在 Play Mode 下使用"); return; }
        bool result = CheckCurrentCustomer();
        Debug.Log($"[CustomerManager] 強制判定結果：{(result ? "滿足" : "未滿足")}");
    }
}

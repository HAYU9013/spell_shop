/// <summary>
/// 符文執行時實例。
/// 包裝一張 RuneData，追蹤消耗型符文是否已被用盡。
///
/// 生命週期：
///   - DeckManager 初始化時，每張 RuneData 建立一個 RuneInstance
///   - Cycle 符文：使用後進棄牌堆，牌庫抽完洗牌重來，IsExhausted 永遠為 false
///   - Consumable 符文：使用後 Exhaust()，IsExhausted = true，不再回到牌庫
/// </summary>
public class RuneInstance
{
    // =========================================================
    // 欄位
    // =========================================================

    public RuneData Data { get; private set; }

    /// <summary>是否已消耗（只對 Consumable 符文有意義）</summary>
    public bool IsExhausted { get; private set; }

    // =========================================================
    // 建構子
    // =========================================================

    public RuneInstance(RuneData data)
    {
        Data       = data;
        IsExhausted = false;
    }

    // =========================================================
    // 狀態
    // =========================================================

    /// <summary>這張符文是否還能被放入卷軸（未被消耗）</summary>
    public bool IsAvailable => !IsExhausted;

    /// <summary>
    /// 將消耗型符文標記為已用盡。
    /// Cycle 符文呼叫此方法無效（直接回棄牌堆由 DeckManager 處理）。
    /// </summary>
    public void Exhaust()
    {
        if (Data.runeType == RuneType.Consumable)
            IsExhausted = true;
    }

    // =========================================================
    // 工具
    // =========================================================

    public override string ToString()
    {
        string typeTag = Data.runeType == RuneType.Consumable ? "[消耗]" : "[循環]";
        string exhaustTag = IsExhausted ? " (已用盡)" : "";
        return $"{typeTag} {Data.runeName}{exhaustTag}";
    }
}

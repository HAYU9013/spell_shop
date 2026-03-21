/// <summary>
/// 卷軸執行時實例。
/// 包裝一張 ScrollData，代表玩家庫存中的一份卷軸。
///
/// 生命週期：
///   - ScrollInventory 初始化時，為每張 ScrollData 建立對應的 ScrollInstance
///   - 玩家送出後，ScrollInventory 從庫存中移除此 Instance
///   - 目前每張卷軸使用一次即移除（未來可擴充耐久度）
/// </summary>
public class ScrollInstance
{
    // =========================================================
    // 欄位
    // =========================================================

    public ScrollData Data { get; private set; }

    /// <summary>庫存中的唯一 ID（供 UI 區分相同 ScrollData 的多份卷軸）</summary>
    public int InstanceId { get; private set; }

    private static int _nextId = 0;

    // =========================================================
    // 建構子
    // =========================================================

    public ScrollInstance(ScrollData data)
    {
        Data       = data;
        InstanceId = _nextId++;
    }

    // =========================================================
    // 工具
    // =========================================================

    public override string ToString()
    {
        return $"[Scroll#{InstanceId}] {Data.scrollName} ({Data.modifierType}, 槽×{Data.slotCount})";
    }
}

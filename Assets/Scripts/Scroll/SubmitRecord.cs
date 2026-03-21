/// <summary>
/// 單次送出的歷史紀錄。
/// WorkbenchManager 每次 Submit() 建立一筆，存入歷史 List。
/// 頭（index 0）= 最新，尾（index N-1）= 最舊（由 WorkbenchManager 維護）
/// </summary>
public class SubmitRecord
{
    /// <summary>送出時的回合數</summary>
    public int TurnNumber { get; }

    /// <summary>使用的卷軸（不消耗，僅記錄參考）</summary>
    public ScrollData Scroll { get; }

    /// <summary>填入的符文資料（記錄 Data，不保留 Instance 狀態）</summary>
    public RuneData[] Runes { get; }

    public SubmitRecord(int turnNumber, ScrollData scroll, RuneData[] runes)
    {
        TurnNumber = turnNumber;
        Scroll     = scroll;
        Runes      = runes;
    }

    public override string ToString()
    {
        var runeNames = string.Join(", ", System.Array.ConvertAll(Runes, r => r.runeName));
        return $"[T{TurnNumber}] {Scroll.scrollName}（{Scroll.modifierType}）← [{runeNames}]";
    }
}

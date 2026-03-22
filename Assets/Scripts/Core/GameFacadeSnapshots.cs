using System.Collections.Generic;
using UnityEngine;

// ============================================================
// GameFacadeSnapshots.cs
// UI 層透過 GameFacade 取得的唯讀資料快照定義。
// 所有快照都是 struct（值型別），取得後直接使用，不用擔心外部修改。
// ============================================================

/// <summary>
/// 工作台狀態快照。
/// 供 WorkbenchPanel 渲染卷軸選擇、槽位狀態、預覽與送出按鈕。
/// </summary>
public struct WorkbenchSnapshot
{
    /// <summary>目前選中的卷軸（null = 尚未選擇）</summary>
    public ScrollData SelectedScroll;

    /// <summary>槽位總數（由 SelectedScroll.slotCount 決定；未選擇卷軸時為 0）</summary>
    public int SlotCount;

    /// <summary>
    /// 每個槽位放置的符文 Data（長度 = SlotCount）。
    /// null 元素 = 空槽。
    /// </summary>
    public RuneData[] PlacedRunes;

    /// <summary>是否所有槽位均已填滿，可以送出</summary>
    public bool CanSubmit;

    /// <summary>目前庫存中可用的卷軸清單（供卷軸選擇按鈕列渲染）</summary>
    public List<ScrollData> AvailableScrolls;
}

/// <summary>
/// 單一顧客需求的快照。
/// 供 CustomerPanel 渲染每一行需求文字與滿足狀態。
/// </summary>
public struct RequirementSnapshot
{
    /// <summary>
    /// 已格式化的需求進度文字，直接填入 UI Text，例如：
    ///   "溫度 >= 8（目前：6）"
    ///   "水分累積減少 >= 3（已減：-1）"
    ///   "溫度維持 4~6（連續：0/1 回合）"
    /// </summary>
    public string DisplayText;

    /// <summary>此條需求是否已達成（用於顯示綠色 ✓ 或改變文字顏色）</summary>
    public bool IsSatisfied;

    /// <summary>0.0 ~ 1.0 進度值，供需求進度條使用（選用）</summary>
    public float Progress;
}

/// <summary>
/// 顧客資訊快照。
/// 供 CustomerPanel 渲染立繪、名稱、需求列表、耐心進度條與隊列資訊。
/// </summary>
public struct CustomerSnapshot
{
    /// <summary>目前是否有顧客在店（false 時其餘欄位無意義）</summary>
    public bool IsPresent;

    public string Name;
    public string FlavorText;
    public Sprite Portrait;

    /// <summary>剩餘耐心回合數</summary>
    public int CurrentPatience;

    /// <summary>最大耐心（用於計算耐心進度條比例）</summary>
    public int MaxPatience;

    /// <summary>所有需求的快照列表（順序與 CustomerData.requirements 相同）</summary>
    public List<RequirementSnapshot> Requirements;

    /// <summary>
    /// 隊列中還剩幾位顧客（不含當前）。
    /// 無限模式回傳 -1，UI 可顯示「∞」。
    /// </summary>
    public int QueueRemaining;
}

/// <summary>
/// 遺物狀態快照。
/// 供 RelicPanel 渲染遺物圖示、滿意狀態、不滿意計數與效果描述。
/// </summary>
public struct RelicSnapshot
{
    public string RelicName;
    public string Description;
    public Sprite Icon;

    /// <summary>依當前環境即時計算的滿意狀態</summary>
    public bool IsSatisfied;

    /// <summary>目前不滿意計數</summary>
    public int DissatisfiedCount;

    /// <summary>觸發懲罰的不滿意容忍上限</summary>
    public int DissatisfiedLimit;

    /// <summary>滿意時的效果描述，例如："亮度 +1 / 業績 +5"</summary>
    public string SatisfiedEffectText;

    /// <summary>不滿意時的副作用描述，例如："亮度 -1"</summary>
    public string UnsatisfiedEffectText;

    /// <summary>達到上限時的懲罰描述，例如："亮度 -5" 或 "Game Over"</summary>
    public string PunishmentText;
}

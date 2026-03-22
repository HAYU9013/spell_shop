using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 編輯器工具：一鍵建立所有預設卷軸 ScriptableObject 資產。
/// 使用方式：Unity 選單列 → SpellShop → Create Default Scrolls
/// 資產輸出位置：Assets/Data/Scrolls/
///
/// 卷軸設計（對應設計表）：
///   槽位系列  — 單格 / 雙格 / 三格
///   封印系列  — 光封印（鎖亮度）/ 水封印（鎖水分）/ 熱封印（鎖溫度）
/// </summary>
public static class ScrollAssetCreator
{
    private const string OUTPUT_PATH = "Assets/Data/Scrolls";

    [MenuItem("SpellShop/Create Default Scrolls")]
    public static void CreateDefaultScrolls()
    {
        if (!AssetDatabase.IsValidFolder(OUTPUT_PATH))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Scrolls");
            Debug.Log($"[ScrollAssetCreator] 建立資料夾：{OUTPUT_PATH}");
        }

        // ── 槽位系列 ────────────────────────────────────────────
        // 效果直接加總，分別限制放 1 / 2 / 3 張貼紙。

        CreateScroll(
            fileName:    "Scroll_Single",
            scrollName:  "單格卷軸",
            description: "只能放入 1 張貼紙，效果直接生效。",
            slotCount:   1,
            modifier:    ScrollModifierType.DirectAdd,
            multiplier:  1f,
            tagFilter:   RuneTag.None,
            blocked:     EnvAttributeMask.None
        );

        CreateScroll(
            fileName:    "Scroll_Double",
            scrollName:  "雙格卷軸",
            description: "只能放入 2 張貼紙，效果直接加總生效。",
            slotCount:   2,
            modifier:    ScrollModifierType.DirectAdd,
            multiplier:  1f,
            tagFilter:   RuneTag.None,
            blocked:     EnvAttributeMask.None
        );

        CreateScroll(
            fileName:    "Scroll_Triple",
            scrollName:  "三格卷軸",
            description: "只能放入 3 張貼紙，效果直接加總生效。",
            slotCount:   3,
            modifier:    ScrollModifierType.DirectAdd,
            multiplier:  1f,
            tagFilter:   RuneTag.None,
            blocked:     EnvAttributeMask.None
        );

        // ── 封印系列 ────────────────────────────────────────────
        // 送出後，被封印的環境屬性完全不受本次符文影響。
        // 預設 2 槽，可搭配任意貼紙。

        CreateScroll(
            fileName:    "Scroll_SealLight",
            scrollName:  "光封印卷軸",
            description: "此卷軸送出後不影響亮度數值。\n其他屬性正常生效。",
            slotCount:   2,
            modifier:    ScrollModifierType.DirectAdd,
            multiplier:  1f,
            tagFilter:   RuneTag.None,
            blocked:     EnvAttributeMask.Brightness
        );

        CreateScroll(
            fileName:    "Scroll_SealWater",
            scrollName:  "水封印卷軸",
            description: "此卷軸送出後不影響水分數值。\n其他屬性正常生效。",
            slotCount:   2,
            modifier:    ScrollModifierType.DirectAdd,
            multiplier:  1f,
            tagFilter:   RuneTag.None,
            blocked:     EnvAttributeMask.Moisture
        );

        CreateScroll(
            fileName:    "Scroll_SealHeat",
            scrollName:  "熱封印卷軸",
            description: "此卷軸送出後不影響溫度數值。\n其他屬性正常生效。",
            slotCount:   2,
            modifier:    ScrollModifierType.DirectAdd,
            multiplier:  1f,
            tagFilter:   RuneTag.None,
            blocked:     EnvAttributeMask.Temperature
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[ScrollAssetCreator] 完成！6 張卷軸資產已建立於 {OUTPUT_PATH}");
        EditorUtility.DisplayDialog(
            "卷軸建立完成",
            $"6 張預設卷軸已建立於：\n{OUTPUT_PATH}",
            "OK"
        );
    }

    // =========================================================
    // 私有工具
    // =========================================================

    private static void CreateScroll(
        string             fileName,
        string             scrollName,
        string             description,
        int                slotCount,
        ScrollModifierType modifier,
        float              multiplier,
        RuneTag            tagFilter,
        EnvAttributeMask   blocked)
    {
        string assetPath = $"{OUTPUT_PATH}/{fileName}.asset";

        if (File.Exists(Path.Combine(Application.dataPath.Replace("Assets", ""), assetPath)))
        {
            Debug.Log($"[ScrollAssetCreator] 跳過（已存在）：{fileName}");
            return;
        }

        var scroll                  = ScriptableObject.CreateInstance<ScrollData>();
        scroll.scrollName           = scrollName;
        scroll.description          = description;
        scroll.slotCount            = slotCount;
        scroll.modifierType         = modifier;
        scroll.multiplier           = multiplier;
        scroll.tagFilter            = tagFilter;
        scroll.blockedAttributes    = blocked;

        AssetDatabase.CreateAsset(scroll, assetPath);
        Debug.Log($"[ScrollAssetCreator] 建立：{assetPath}");
    }
}

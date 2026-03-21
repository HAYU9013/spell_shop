using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 編輯器工具：一鍵建立 Todolist 中定義的 3 張基礎卷軸 ScriptableObject 資產。
/// 使用方式：Unity 選單列 → SpellShop → Create Default Scrolls
/// 資產輸出位置：Assets/Data/Scrolls/
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

        // 基礎卷軸 — 2 槽，效果直接加總
        CreateScroll(
            fileName:    "Scroll_Basic",
            scrollName:  "基礎卷軸",
            description: "最普通的卷軸，符文效果直接加總生效。",
            slotCount:   2,
            modifier:    ScrollModifierType.DirectAdd,
            multiplier:  1f,
            tagFilter:   RuneTag.None
        );

        // 放大卷軸 — 3 槽，所有效果 ×2
        CreateScroll(
            fileName:    "Scroll_Amplify",
            scrollName:  "放大卷軸",
            description: "強化的魔法卷軸，所有符文效果放大兩倍。",
            slotCount:   3,
            modifier:    ScrollModifierType.MultiplyAll,
            multiplier:  2f,
            tagFilter:   RuneTag.None
        );

        // 純化卷軸 — 2 槽，僅計算正值效果
        CreateScroll(
            fileName:    "Scroll_Purify",
            scrollName:  "純化卷軸",
            description: "過濾符文的負面效果，只讓有益的環境變化生效。",
            slotCount:   2,
            modifier:    ScrollModifierType.PositiveOnly,
            multiplier:  1f,
            tagFilter:   RuneTag.None
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[ScrollAssetCreator] 完成！3 張卷軸資產已建立於 {OUTPUT_PATH}");
        EditorUtility.DisplayDialog(
            "卷軸建立完成",
            $"3 張預設卷軸已建立於：\n{OUTPUT_PATH}",
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
        RuneTag            tagFilter)
    {
        string assetPath = $"{OUTPUT_PATH}/{fileName}.asset";

        if (File.Exists(Path.Combine(Application.dataPath.Replace("Assets", ""), assetPath)))
        {
            Debug.Log($"[ScrollAssetCreator] 跳過（已存在）：{fileName}");
            return;
        }

        var scroll          = ScriptableObject.CreateInstance<ScrollData>();
        scroll.scrollName   = scrollName;
        scroll.description  = description;
        scroll.slotCount    = slotCount;
        scroll.modifierType = modifier;
        scroll.multiplier   = multiplier;
        scroll.tagFilter    = tagFilter;

        AssetDatabase.CreateAsset(scroll, assetPath);
        Debug.Log($"[ScrollAssetCreator] 建立：{assetPath}");
    }
}

using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 編輯器工具：一鍵建立 Todolist 中定義的 6 張基礎符文 ScriptableObject 資產。
/// 使用方式：Unity 選單列 → SpellShop → Create Default Runes
/// 資產輸出位置：Assets/Data/Runes/
/// </summary>
public static class RuneAssetCreator
{
    private const string OUTPUT_PATH = "Assets/Data/Runes";

    [MenuItem("SpellShop/Create Default Runes")]
    public static void CreateDefaultRunes()
    {
        // 確保資料夾存在
        if (!AssetDatabase.IsValidFolder(OUTPUT_PATH))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Runes");
            Debug.Log($"[RuneAssetCreator] 建立資料夾：{OUTPUT_PATH}");
        }

        CreateRune(
            fileName:    "Rune_WaterDrop",
            runeName:    "水珠",
            description: "注入一股水氣，使環境更加濕潤。",
            rarity:      Rarity.Common,
            runeType:    RuneType.Cycle,
            tags:        RuneTag.Water,
            effects: new RuneEffect[]
            {
                new RuneEffect { attribute = EnvAttribute.Moisture, value = 2 }
            }
        );

        CreateRune(
            fileName:    "Rune_Candle",
            runeName:    "小燭",
            description: "點燃一支蠟燭，帶來微弱的光與暖意。",
            rarity:      Rarity.Common,
            runeType:    RuneType.Cycle,
            tags:        RuneTag.Fire | RuneTag.Light,
            effects: new RuneEffect[]
            {
                new RuneEffect { attribute = EnvAttribute.Brightness,   value =  1 },
                new RuneEffect { attribute = EnvAttribute.Temperature,  value =  1 }
            }
        );

        CreateRune(
            fileName:    "Rune_IceShard",
            runeName:    "寒冰",
            description: "凝結空氣中的水分，大幅降低溫度。",
            rarity:      Rarity.Common,
            runeType:    RuneType.Cycle,
            tags:        RuneTag.Water,
            effects: new RuneEffect[]
            {
                new RuneEffect { attribute = EnvAttribute.Temperature, value = -3 },
                new RuneEffect { attribute = EnvAttribute.Moisture,    value =  1 }
            }
        );

        CreateRune(
            fileName:    "Rune_DarkMist",
            runeName:    "黑霧",
            description: "散佈濃重的黑霧，遮蔽光線並帶來濕意。",
            rarity:      Rarity.Common,
            runeType:    RuneType.Cycle,
            tags:        RuneTag.Dark,
            effects: new RuneEffect[]
            {
                new RuneEffect { attribute = EnvAttribute.Brightness, value = -2 },
                new RuneEffect { attribute = EnvAttribute.Moisture,   value =  1 }
            }
        );

        CreateRune(
            fileName:    "Rune_Steam",
            runeName:    "蒸氣",
            description: "蒸發水分轉換為光，平衡三項環境數值。",
            rarity:      Rarity.Common,
            runeType:    RuneType.Cycle,
            tags:        RuneTag.Wind,
            effects: new RuneEffect[]
            {
                new RuneEffect { attribute = EnvAttribute.Temperature, value = -1 },
                new RuneEffect { attribute = EnvAttribute.Moisture,    value = -1 },
                new RuneEffect { attribute = EnvAttribute.Brightness,  value =  1 }
            }
        );

        CreateRune(
            fileName:    "Rune_SolarCore",
            runeName:    "太陽核",
            description: "釋放強烈的太陽能量，大幅提升光與熱，但蒸發水分。\n【消耗型】使用後永久移除。",
            rarity:      Rarity.Rare,
            runeType:    RuneType.Consumable,
            tags:        RuneTag.Light | RuneTag.Fire,
            effects: new RuneEffect[]
            {
                new RuneEffect { attribute = EnvAttribute.Brightness,   value =  5 },
                new RuneEffect { attribute = EnvAttribute.Temperature,  value =  3 },
                new RuneEffect { attribute = EnvAttribute.Moisture,     value = -2 }
            }
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[RuneAssetCreator] 完成！6 張符文資產已建立於 {OUTPUT_PATH}");
        EditorUtility.DisplayDialog(
            "符文建立完成",
            $"6 張預設符文已建立於：\n{OUTPUT_PATH}",
            "OK"
        );
    }

    // =========================================================
    // 私有工具
    // =========================================================

    private static void CreateRune(
        string      fileName,
        string      runeName,
        string      description,
        Rarity      rarity,
        RuneType    runeType,
        RuneTag     tags,
        RuneEffect[] effects)
    {
        string assetPath = $"{OUTPUT_PATH}/{fileName}.asset";

        // 若已存在則跳過（避免覆蓋手動修改的資料）
        if (File.Exists(Path.Combine(Application.dataPath.Replace("Assets", ""), assetPath)))
        {
            Debug.Log($"[RuneAssetCreator] 跳過（已存在）：{fileName}");
            return;
        }

        var rune         = ScriptableObject.CreateInstance<RuneData>();
        rune.runeName    = runeName;
        rune.description = description;
        rune.rarity      = rarity;
        rune.runeType    = runeType;
        rune.tags        = tags;
        rune.effects     = effects;

        AssetDatabase.CreateAsset(rune, assetPath);
        Debug.Log($"[RuneAssetCreator] 建立：{assetPath}");
    }
}

using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 編輯器工具：一鍵建立所有貼紙（符文）ScriptableObject 資產。
/// 使用方式：Unity 選單列 → SpellShop → Create Default Runes
/// 資產輸出位置：Assets/Data/Runes/
/// </summary>
public static class RuneAssetCreator
{
    private const string OUTPUT_PATH = "Assets/Data/Runes";

    [MenuItem("SpellShop/Create Default Runes")]
    public static void CreateDefaultRunes()
    {
        if (!AssetDatabase.IsValidFolder(OUTPUT_PATH))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Runes");
            Debug.Log($"[RuneAssetCreator] 建立資料夾：{OUTPUT_PATH}");
        }

        // ── 基礎單屬性 ────────────────────────────────────────

        CreateRune("Rune_Light",       "光",   "為環境注入一縷光亮。",               Rarity.Common, RuneTag.Light,
            Brightness(1));

        CreateRune("Rune_Dark",        "暗",   "遮蔽光線，使環境陷入昏暗。",         Rarity.Common, RuneTag.Dark,
            Brightness(-1));

        CreateRune("Rune_Water",       "水",   "滲入一股水氣，增加濕潤。",           Rarity.Common, RuneTag.Water,
            Moisture(1));

        CreateRune("Rune_Soil",        "土",   "吸收水分，使環境乾燥。",             Rarity.Common, RuneTag.Neutral,
            Moisture(-1));

        CreateRune("Rune_Heat",        "熱",   "釋放熱能，提升環境溫度。",           Rarity.Common, RuneTag.Fire,
            Temperature(1));

        CreateRune("Rune_Cold",        "冷",   "散佈寒意，降低環境溫度。",           Rarity.Common, RuneTag.Wind,
            Temperature(-1));

        // ── 雙屬性（複合） ────────────────────────────────────

        CreateRune("Rune_Growth",      "生長", "萬物萌芽，光與水同步上升。",         Rarity.Common, RuneTag.Light | RuneTag.Water,
            Brightness(1), Moisture(1));

        CreateRune("Rune_Wither",      "枯萎", "植物凋零，光與水同步流失。",         Rarity.Common, RuneTag.Dark,
            Brightness(-1), Moisture(-1));

        CreateRune("Rune_IceCold",     "冰冷", "寒冰凝結，帶來水氣卻帶走溫度。",   Rarity.Common, RuneTag.Water | RuneTag.Wind,
            Moisture(1), Temperature(-1));

        CreateRune("Rune_Ember",       "餘燼", "殘留的火星，同時照亮並加溫。",       Rarity.Common, RuneTag.Fire | RuneTag.Light,
            Brightness(1), Temperature(1));

        CreateRune("Rune_MoonEclipse", "月蝕", "月蝕籠罩，光與溫度同時下降。",       Rarity.Common, RuneTag.Dark | RuneTag.Wind,
            Brightness(-1), Temperature(-1));

        // ── 中等效果 ──────────────────────────────────────────

        CreateRune("Rune_Aurora",      "極光", "耀眼的極光，大幅提升亮度。",         Rarity.Common, RuneTag.Light,
            Brightness(2));

        CreateRune("Rune_HighTide",    "漲潮", "海浪湧來，水分大幅增加。",           Rarity.Common, RuneTag.Water,
            Moisture(2));

        CreateRune("Rune_SmallCold",   "小寒", "寒氣來襲，溫度明顯下降。",           Rarity.Common, RuneTag.Wind,
            Temperature(-2));

        CreateRune("Rune_WarmCurrent", "暖流", "暖流穿行，帶來水氣與溫暖。",         Rarity.Common, RuneTag.Water | RuneTag.Fire,
            Moisture(1), Temperature(2));

        CreateRune("Rune_Moisture",    "濕潤", "大量水氣湧入，環境極度潮濕。",       Rarity.Common, RuneTag.Water,
            Moisture(3));

        CreateRune("Rune_Glacier",     "冰河", "冰河流動，溫度急速下降。",           Rarity.Common, RuneTag.Wind,
            Temperature(-3));

        // ── 較強複合效果 ──────────────────────────────────────

        CreateRune("Rune_Scorched",    "曝曬", "烈日炙烤，大幅提升亮度並蒸發水分。", Rarity.Rare, RuneTag.Light | RuneTag.Fire,
            Brightness(3), Moisture(-3));

        CreateRune("Rune_Evaporate",   "蒸發", "水分迅速蒸發，溫度劇烈攀升。",       Rarity.Rare, RuneTag.Fire,
            Temperature(4), Moisture(-3));

        CreateRune("Rune_Blizzard",    "寒凍", "暴雪來襲，大幅降溫並補充水分。",     Rarity.Rare, RuneTag.Water | RuneTag.Wind,
            Temperature(-4), Moisture(3));

        CreateRune("Rune_Decay",       "腐敗", "腐敗蔓延，遮蔽光線卻增加濕氣。",     Rarity.Rare, RuneTag.Dark | RuneTag.Water,
            Brightness(-4), Moisture(2));

        CreateRune("Rune_GrainRain",   "穀雨", "春雨綿綿，遮光帶來大量水分。",       Rarity.Rare, RuneTag.Dark | RuneTag.Water,
            Brightness(-3), Moisture(4));

        CreateRune("Rune_Charred",     "焦黑", "燃燒後的焦黑，加溫並蒸乾水分。",     Rarity.Rare, RuneTag.Fire,
            Temperature(1), Moisture(-2));

        CreateRune("Rune_Scorching",   "熾焰", "烈焰燃燒，提升光與溫度，蒸發水分。", Rarity.Rare, RuneTag.Fire | RuneTag.Light,
            Brightness(1), Moisture(-1), Temperature(1));

        CreateRune("Rune_SolarEclipse","日蝕", "日蝕遮天，亮度與溫度同步下滑。",     Rarity.Rare, RuneTag.Dark | RuneTag.Wind,
            Brightness(-2), Temperature(-1));

        CreateRune("Rune_Eerie",       "靈異", "詭異的能量，遮蔽光線並降低溫度。",   Rarity.Rare, RuneTag.Dark | RuneTag.Wind,
            Brightness(-1), Temperature(-2));

        CreateRune("Rune_SiroccoWind", "焚風", "炎熱乾燥的焚風，大幅升溫並帶走水分。", Rarity.Rare, RuneTag.Fire,
            Moisture(-2), Temperature(2));

        // ── 特殊效果 ──────────────────────────────────────────

        CreateRune("Rune_Transform",   "變化", "將貼紙隨機變化成其他貼紙。\n【特殊】效果由遊戲邏輯處理。",
            Rarity.Rare, RuneTag.Neutral
            /* 無環境效果，特殊處理 */);

        CreateRune("Rune_Erase",       "消除", "消除卷軸上的一張貼紙。\n【特殊】效果由遊戲邏輯處理。",
            Rarity.Rare, RuneTag.Neutral
            /* 無環境效果，特殊處理 */);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[RuneAssetCreator] 完成！29 張貼紙資產已建立於 {OUTPUT_PATH}");
        EditorUtility.DisplayDialog(
            "貼紙建立完成",
            $"29 張貼紙（符文）已建立於：\n{OUTPUT_PATH}",
            "OK"
        );
    }

    // =========================================================
    // 快速建立輔助
    // =========================================================

    private static void CreateRune(
        string       fileName,
        string       runeName,
        string       description,
        Rarity       rarity,
        RuneTag      tags,
        params RuneEffect[] effects)
    {
        string assetPath = $"{OUTPUT_PATH}/{fileName}.asset";

        if (File.Exists(Path.Combine(Application.dataPath.Replace("Assets", ""), assetPath)))
        {
            Debug.Log($"[RuneAssetCreator] 跳過（已存在）：{fileName}");
            return;
        }

        var rune         = ScriptableObject.CreateInstance<RuneData>();
        rune.runeName    = runeName;
        rune.description = description;
        rune.rarity      = rarity;
        rune.runeType    = RuneType.Cycle;
        rune.tags        = tags;
        rune.effects     = effects;

        AssetDatabase.CreateAsset(rune, assetPath);
        Debug.Log($"[RuneAssetCreator] 建立：{assetPath}");
    }

    // =========================================================
    // RuneEffect 快速建構語法糖
    // =========================================================

    private static RuneEffect Brightness(int v)  => new RuneEffect { attribute = EnvAttribute.Brightness,   value = v };
    private static RuneEffect Moisture(int v)    => new RuneEffect { attribute = EnvAttribute.Moisture,     value = v };
    private static RuneEffect Temperature(int v) => new RuneEffect { attribute = EnvAttribute.Temperature,  value = v };
}

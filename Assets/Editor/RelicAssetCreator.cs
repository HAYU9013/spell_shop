using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 編輯器工具：一鍵建立 Todolist 中定義的 2 個基礎遺物 ScriptableObject 資產。
/// 使用方式：Unity 選單列 → SpellShop → Create Default Relics
/// 資產輸出位置：Assets/Data/Relics/
///
/// 日晷：滿意條件 亮度 >= 7
///        滿意效果：亮度 +1/回合
///        不滿意效果：亮度 -1/回合
///        懲罰上限：4 回合，懲罰：亮度 delta -5
///
/// 苔蘚石：滿意條件 水分 4~8
///          滿意效果：業績 +5/回合
///          不滿意效果：無
///          懲罰上限：3 回合，懲罰：水分強制設為 10
/// </summary>
public static class RelicAssetCreator
{
    private const string OUTPUT_PATH = "Assets/Data/Relics";

    [MenuItem("SpellShop/Create Default Relics")]
    public static void CreateDefaultRelics()
    {
        if (!AssetDatabase.IsValidFolder(OUTPUT_PATH))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Relics");
            Debug.Log($"[RelicAssetCreator] 建立資料夾：{OUTPUT_PATH}");
        }

        // ── 日晷 ──────────────────────────────────────────────
        var sundial = MakeRelic("Relic_Sundial");
        if (sundial != null)
        {
            sundial.relicName    = "日晷";
            sundial.description  = "古老的日晷，感知光線的存在。\n亮度充足時每回合提供光能，亮度不足時則吸走光能。";
            sundial.displayOrder = 0;

            sundial.satisfiedConditions = new List<RelicCondition>
            {
                new RelicCondition
                {
                    attribute   = EnvAttribute.Brightness,
                    compareOp   = CompareOperator.GreaterEqual,
                    targetValue = 7
                }
            };
            sundial.conditionLogic = ConditionLogic.And;

            sundial.satisfiedEnvEffects = new RuneEffect[]
            {
                new RuneEffect { attribute = EnvAttribute.Brightness, value = 1 }
            };
            sundial.satisfiedScoreChange = 0;

            sundial.unsatisfiedEnvEffects = new RuneEffect[]
            {
                new RuneEffect { attribute = EnvAttribute.Brightness, value = -1 }
            };
            sundial.unsatisfiedScoreChange = 0;

            sundial.unsatisfiedLimit  = 4;
            sundial.punishmentType    = RelicPunishment.EnvironmentShock;
            sundial.shockAttribute    = EnvAttribute.Brightness;
            sundial.shockValue        = -5;
            sundial.shockIsForceSet   = false; // delta：亮度 -5

            SaveRelic(sundial, "Relic_Sundial");
        }

        // ── 苔蘚石 ────────────────────────────────────────────
        var mossStone = MakeRelic("Relic_MossStone");
        if (mossStone != null)
        {
            mossStone.relicName    = "苔蘚石";
            mossStone.description  = "覆滿苔蘚的魔法石，在濕度適中的環境中緩慢釋放能量。\n水分失衡時將強制吸收大量水分。";
            mossStone.displayOrder = 1;

            mossStone.satisfiedConditions = new List<RelicCondition>
            {
                new RelicCondition
                {
                    attribute  = EnvAttribute.Moisture,
                    compareOp  = CompareOperator.InRange,
                    rangeMin   = 4,
                    rangeMax   = 8
                }
            };
            mossStone.conditionLogic = ConditionLogic.And;

            mossStone.satisfiedEnvEffects    = new RuneEffect[0]; // 無環境效果
            mossStone.satisfiedScoreChange   = 5;                 // 業績 +5

            mossStone.unsatisfiedEnvEffects  = new RuneEffect[0]; // 無不滿意環境效果
            mossStone.unsatisfiedScoreChange = 0;

            mossStone.unsatisfiedLimit  = 3;
            mossStone.punishmentType    = RelicPunishment.EnvironmentShock;
            mossStone.shockAttribute    = EnvAttribute.Moisture;
            mossStone.shockValue        = 10;
            mossStone.shockIsForceSet   = true; // 強制設定水分 = 10

            SaveRelic(mossStone, "Relic_MossStone");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[RelicAssetCreator] 完成！2 個遺物資產已建立於 {OUTPUT_PATH}");
        EditorUtility.DisplayDialog(
            "遺物建立完成",
            $"2 個預設遺物已建立於：\n{OUTPUT_PATH}",
            "OK"
        );
    }

    // =========================================================
    // 私有工具
    // =========================================================

    private static RelicData MakeRelic(string fileName)
    {
        string assetPath = $"{OUTPUT_PATH}/{fileName}.asset";
        if (File.Exists(Path.Combine(Application.dataPath.Replace("Assets", ""), assetPath)))
        {
            Debug.Log($"[RelicAssetCreator] 跳過（已存在）：{fileName}");
            return null;
        }
        return ScriptableObject.CreateInstance<RelicData>();
    }

    private static void SaveRelic(RelicData relic, string fileName)
    {
        string assetPath = $"{OUTPUT_PATH}/{fileName}.asset";
        AssetDatabase.CreateAsset(relic, assetPath);
        Debug.Log($"[RelicAssetCreator] 建立：{assetPath}");
    }
}

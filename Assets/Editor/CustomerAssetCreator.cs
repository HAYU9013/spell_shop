using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 編輯器工具：一鍵建立 5 位預設顧客 ScriptableObject 資產。
/// 使用方式：Unity 選單列 → SpellShop → Create Default Customers
/// 資產輸出位置：Assets/Data/Customers/
/// </summary>
public static class CustomerAssetCreator
{
    private const string OUTPUT_PATH = "Assets/Data/Customers";

    [MenuItem("SpellShop/Create Default Customers")]
    public static void CreateDefaultCustomers()
    {
        if (!AssetDatabase.IsValidFolder(OUTPUT_PATH))
        {
            AssetDatabase.CreateFolder("Assets/Data", "Customers");
            Debug.Log($"[CustomerAssetCreator] 建立資料夾：{OUTPUT_PATH}");
        }

        // ── 1. 落湯雞旅人 ── 絕對值：溫度 >= 8
        Create("Customer_WetTraveler", c =>
        {
            c.customerName = "落湯雞旅人";
            c.flavorText   = "渾身濕透，直打哆嗦。需要一個暖和的環境才願意掏錢。";
            c.maxPatience  = 2;
            c.scoreReward  = 10;
            c.scorePenalty = 5;
            c.requirements = new List<RequirementData>
            {
                new RequirementData
                {
                    type        = RequirementType.Absolute,
                    attribute   = EnvAttribute.Temperature,
                    compareOp   = CompareOperator.GreaterEqual,
                    targetValue = 8
                }
            };
        });

        // ── 2. 感冒農夫 ── 相對變化：水分累積減少 >= 3
        Create("Customer_SickFarmer", c =>
        {
            c.customerName = "感冒農夫";
            c.flavorText   = "鼻水直流，需要乾燥的空氣讓他好過一點。";
            c.maxPatience  = 3;
            c.scoreReward  = 8;
            c.scorePenalty = 4;
            c.requirements = new List<RequirementData>
            {
                new RequirementData
                {
                    type          = RequirementType.RelativeChange,
                    attribute     = EnvAttribute.Moisture,
                    requiredDelta = -3   // 累積減少 >= 3
                }
            };
        });

        // ── 3. 夜盲商人 ── 絕對值：亮度 >= 10
        Create("Customer_NightBlindMerchant", c =>
        {
            c.customerName = "夜盲商人";
            c.flavorText   = "夜盲症發作，在黑暗中甚麼都看不見。需要充足的光線才能交易。";
            c.maxPatience  = 1;
            c.scoreReward  = 20;
            c.scorePenalty = 8;
            c.requirements = new List<RequirementData>
            {
                new RequirementData
                {
                    type        = RequirementType.Absolute,
                    attribute   = EnvAttribute.Brightness,
                    compareOp   = CompareOperator.GreaterEqual,
                    targetValue = 10
                }
            };
        });

        // ── 4. 冰魔法師 ── 複合絕對值：溫度 <= 2 AND 亮度 >= 5
        Create("Customer_IceMage", c =>
        {
            c.customerName = "冰魔法師";
            c.flavorText   = "來自冰原，對高溫極度不適。需要寒冷且明亮的環境施展魔法。";
            c.maxPatience  = 4;
            c.scoreReward  = 15;
            c.scorePenalty = 6;
            c.requirements = new List<RequirementData>
            {
                new RequirementData
                {
                    type        = RequirementType.Absolute,
                    attribute   = EnvAttribute.Temperature,
                    compareOp   = CompareOperator.LessEqual,
                    targetValue = 2
                },
                new RequirementData
                {
                    type        = RequirementType.Absolute,
                    attribute   = EnvAttribute.Brightness,
                    compareOp   = CompareOperator.GreaterEqual,
                    targetValue = 5
                }
            };
        });

        // ── 5. 養花老人 ── 穩定性：溫度維持 4~6，連續 1 回合
        Create("Customer_FlowerGrower", c =>
        {
            c.customerName = "養花老人";
            c.flavorText   = "精心培育的珍稀花卉，對溫度極為敏感。環境必須穩定才能成交。";
            c.maxPatience  = 3;
            c.scoreReward  = 12;
            c.scorePenalty = 5;
            c.requirements = new List<RequirementData>
            {
                new RequirementData
                {
                    type           = RequirementType.Stability,
                    attribute      = EnvAttribute.Temperature,
                    compareOp      = CompareOperator.InRange,
                    targetValue    = 4,
                    targetValueMax = 6,
                    stabilityTurns = 1
                }
            };
        });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[CustomerAssetCreator] 完成！5 位顧客資產已建立於 {OUTPUT_PATH}");
        EditorUtility.DisplayDialog("顧客建立完成",
            $"5 位預設顧客已建立於：\n{OUTPUT_PATH}", "OK");
    }

    // =========================================================
    // 私有工具
    // =========================================================

    private static void Create(string fileName, System.Action<CustomerData> setup)
    {
        string assetPath = $"{OUTPUT_PATH}/{fileName}.asset";
        if (File.Exists(Path.Combine(Application.dataPath.Replace("Assets", ""), assetPath)))
        {
            Debug.Log($"[CustomerAssetCreator] 跳過（已存在）：{fileName}");
            return;
        }
        var data = ScriptableObject.CreateInstance<CustomerData>();
        setup(data);
        AssetDatabase.CreateAsset(data, assetPath);
        Debug.Log($"[CustomerAssetCreator] 建立：{assetPath}");
    }
}

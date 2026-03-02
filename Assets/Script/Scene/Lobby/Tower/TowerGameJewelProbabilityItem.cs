using UnityEngine;
using UnityEngine.UI;

public class TowerGameJewelProbabilityItem : MonoBehaviour
{
    [Header("UI Elements")]
    public Text typeText;
    public Text probabilityText;
    public Text valueText;

    public void UpdateUI(TowerWeaponJewelMetaData metaData, float probability)
    {
        if (typeText != null)
        {
            typeText.text = LocalizeManager.instance.GetString($"jewel.type.{metaData.type}");
        }
        if (probabilityText != null)
        {
            probabilityText.text = $"{probability:F2}%";
        }
        if (valueText != null)
        {
            if (probability <= 0)
            {
                valueText.text = "";
            }
            else
            {
                string minPct = GetJewelValueString(metaData.type, metaData.min);
                string maxPct = GetJewelValueString(metaData.type, metaData.max);
                valueText.text = $"{minPct} ~ {maxPct}";
            }
        }
    }

    private string GetJewelValueString(TowerJewelType type, float value)
    {
        if (type == TowerJewelType.None)
            return "";

        return type switch
        {
            TowerJewelType.CriDmg => $"{value * 100}%",
            TowerJewelType.CriRate or TowerJewelType.Avoid or TowerJewelType.DefBreak or TowerJewelType.DmgReduce or TowerJewelType.AtkMul or TowerJewelType.CriDmgMul => $"{value}%",
            _ => $"{value}",
        };
    }
}

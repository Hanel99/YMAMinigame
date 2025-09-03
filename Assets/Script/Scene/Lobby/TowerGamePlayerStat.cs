using UnityEngine;
using UnityEngine.UI;

public class TowerGamePlayerStat : MonoBehaviour
{
    public Text statName;
    public Text statLevel;
    public Text statValue;
    public Text requireCoin;
    public Button enchantButton;

    private TowerUserStatType statType;
    private int level;


    public void UpdateUI(TowerUserStatType type, int level)
    {
        statType = type;
        this.level = level;


        statName.text = LocalizeManager.instance.GetString($"Tower.StatType.{type}");
        statLevel.text = level.ToString();

        if (type == TowerUserStatType.criRate)
            statValue.text = $"{(GetValue<float>(type) * 100).ToString("F1")}%";
        else if (type == TowerUserStatType.criDmg)
            statValue.text = $"x{1 + (GetValue<float>(type) * 100).ToString("F1")}%";
        else
            statValue.text = GetValue<int>(type).ToString();

        requireCoin.text = level >= 100 ? "MAX" : GameResourceManager.instance.GetTowerUserLevelRequireCoin(level).ToString();
        enchantButton.interactable = level >= 100 || SaveDataManager.instance.playerData.coin < GameResourceManager.instance.GetTowerUserLevelRequireCoin(level);
    }

    public void OnClickEnchant()
    {
        level++;
        SaveDataManager.instance.SetTowerUserStatLevel(statType, level);
        UpdateUI(statType, level);
    }

    private T GetValue<T>(TowerUserStatType type)
    {
        return GameResourceManager.instance.GetTowerUserLevelStatValue<T>(type, level);
    }


}

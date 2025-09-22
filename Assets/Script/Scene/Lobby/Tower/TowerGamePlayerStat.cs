using UnityEngine;
using UnityEngine.UI;

public class TowerGamePlayerStat : MonoBehaviour
{
    public Text nameText;
    public Text levelText;
    public Text valueText;
    public Text requireCoinText;
    public Button enchantButton;

    private TowerUserStatType statType;
    private int statLevel;


    public void UpdateUIData(TowerUserStatType type, int statLevel)
    {
        statType = type;
        this.statLevel = statLevel;


        nameText.text = LocalizeManager.instance.GetString($"Tower.StatType.{type}");
        this.levelText.text = $"Lv.{statLevel}";

        if (type == TowerUserStatType.criRate)
            valueText.text = $"{(GetValue<float>(type) * 100).ToString("F1")}%";
        else if (type == TowerUserStatType.criDmg)
            valueText.text = $"x{((1 + GetValue<float>(type)) * 100).ToString("F1")}%";
        else
            valueText.text = GetValue<int>(type).ToString();

        requireCoinText.text = statLevel >= 100 ? "MAX" : GameResourceManager.instance.GetTowerUserLevelRequireCoin(statLevel).ToString();
        enchantButton.interactable = statLevel < 100 && SaveDataManager.instance.playerData.coin >= GameResourceManager.instance.GetTowerUserLevelRequireCoin(statLevel);
    }

    public void OnClickEnchant()
    {
        if (statLevel >= 100 || SaveDataManager.instance.playerData.coin < GameResourceManager.instance.GetTowerUserLevelRequireCoin(statLevel))
            return;

        SaveDataManager.instance.AddCoin(-GameResourceManager.instance.GetTowerUserLevelRequireCoin(statLevel), false);
        statLevel++;
        SaveDataManager.instance.SetTowerUserStatLevel(statType, statLevel);

#if DEV
        UpdateUIData(statType, statLevel);
#elif LIVE
        TowerGameWeaponEnchantPopup.instance.ShowEnchantResult(TowerGameResultType.stat, $"Lv.{statLevel - 1}", $"-> Lv.{statLevel}", () => UpdateUIData(statType, statLevel));
#endif
    }

    private T GetValue<T>(TowerUserStatType type)
    {
        return GameResourceManager.instance.GetTowerUserLevelStatValue<T>(type, statLevel);
    }


}

using UnityEngine;
using UnityEngine.UI;

public class TowerGameDetailStatPopup : PopupBase
{
    public static TowerGameDetailStatPopup instance { get; private set; }

    public Text playerAtk;
    public Text playerDef;
    public Text playerHp;
    public Text playerCriRate;
    public Text playerCriDmg;

    public Text weaponAtk;
    public Text weaponCriDmg;

    public Text bossAtk;
    public Text bossDef;
    public Text bossHp;
    public Text bossCriRate;
    public Text bossCriDmg;
    public Text bossRewardCoin;


    protected override void OnAwake()
    {
        instance = this;
    }


    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            UpdateUI();
            _OpenUI();
        }
        else
        {
            _CloseWindow();
        }
    }

    private void UpdateUI()
    {
        var level = SaveDataManager.instance.playerData.level;
        var floor = SaveDataManager.instance.playerData.towerFloor;
        var userStatData = SaveDataManager.instance.playerData.towerGameUserStatLevelData;
        var weaponStatData = SaveDataManager.instance.playerData.towerGameUserWeaponData;
        var weaponMetaData = GameResourceManager.instance.GetTowerWeaponLevelMetaData(weaponStatData.weaponLevel);
        var monsterMetaData = GameResourceManager.instance.GetTowerMonsterLevelMetaData(floor);

        playerAtk.text = GetValue<int>(TowerUserStatType.atk, userStatData.atkLevel).ToString();
        playerDef.text = GetValue<int>(TowerUserStatType.def, userStatData.defLevel).ToString();
        playerHp.text = GetValue<int>(TowerUserStatType.hp, userStatData.hpLevel).ToString();
        playerCriRate.text = $"{(GetValue<float>(TowerUserStatType.criRate, userStatData.criRateLevel) * 100).ToString("F1")}%";
        playerCriDmg.text = $"x{((1 + GetValue<float>(TowerUserStatType.criDmg, userStatData.criDmgLevel)) * 100).ToString("F1")}%";

        weaponAtk.text = weaponMetaData.atk.ToString();
        weaponCriDmg.text = $"x{((1 + weaponMetaData.criDmg) * 100).ToString("F1")}%";

        bossAtk.text = monsterMetaData.atk.ToString();
        bossDef.text = monsterMetaData.def.ToString();
        bossHp.text = monsterMetaData.hp.ToString();
        bossCriRate.text = $"{(monsterMetaData.criRate * 100).ToString("F1")}%";
        bossCriDmg.text = $"x{((1 + monsterMetaData.criDmg) * 100).ToString("F1")}%";
        bossRewardCoin.text = monsterMetaData.rewardCoin.ToString();
    }

    private T GetValue<T>(TowerUserStatType type, int level)
    {
        return GameResourceManager.instance.GetTowerUserLevelStatValue<T>(type, level);
    }
}

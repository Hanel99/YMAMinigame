using UnityEngine;
using UnityEngine.UI;

public class TowerGameDetailStatPopup : PopupBase
{
    public static TowerGameDetailStatPopup instance { get; private set; }
    public Text playerName;
    public Text bossName;

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
        playerName.text = SaveDataManager.instance.playerData.name;
        int bossNumber = TowerGamePopup.instance.GetBossNumber(SaveDataManager.instance.playerData.towerFloor);
        bossName.text = LocalizeManager.instance.GetString($"Tower.Boss.Name.{bossNumber.ToString("D2")}");

        var playerLevel = SaveDataManager.instance.playerData.level;
        var floor = SaveDataManager.instance.playerData.towerFloor;
        var userStatData = SaveDataManager.instance.playerData.towerGameUserStatLevelData;
        var weaponStatData = SaveDataManager.instance.playerData.towerGameUserWeaponData;
        var weaponMetaData = GameResourceManager.instance.GetTowerWeaponLevelMetaData(weaponStatData.weaponLevel);
        var bossMetaData = GameResourceManager.instance.GetTowerBossLevelMetaData(floor);

        playerAtk.text = $"{playerLevel + GetValue<int>(TowerUserStatType.atk, userStatData.atkLevel)}";
        playerDef.text = $"{playerLevel + GetValue<int>(TowerUserStatType.def, userStatData.defLevel)}";
        playerHp.text = $"{playerLevel + GetValue<int>(TowerUserStatType.hp, userStatData.hpLevel)}";
        playerCriRate.text = $"{(GetValue<float>(TowerUserStatType.criRate, userStatData.criRateLevel) * 100).ToString("F1")}%";
        playerCriDmg.text = $"x{((1 + GetValue<float>(TowerUserStatType.criDmg, userStatData.criDmgLevel)) * 100).ToString("F1")}%";

        weaponAtk.text = $"+ {weaponMetaData.atk}";
        weaponCriDmg.text = $"+ x{(weaponMetaData.criDmg * 100).ToString("F1")}%";

        bossAtk.text = bossMetaData.atk.ToString();
        bossDef.text = bossMetaData.def.ToString();
        bossHp.text = bossMetaData.hp.ToString();
        bossCriRate.text = $"{(bossMetaData.criRate * 100).ToString("F1")}%";
        bossCriDmg.text = $"x{((1 + bossMetaData.criDmg) * 100).ToString("F1")}%";
        bossRewardCoin.text = bossMetaData.rewardCoin.ToString();
    }

    private T GetValue<T>(TowerUserStatType type, int level)
    {
        return GameResourceManager.instance.GetTowerUserLevelStatValue<T>(type, level);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class TowerGameWeaponEnchantPopup : PopupBase
{
    public static TowerGameWeaponEnchantPopup instance { get; private set; }
    public GameObject statTab;
    public TowerGameWeaponStat weaponTab;
    public List<TowerGamePlayerStat> playerStatList;



    //private
    private bool isOnProcess = false;
    private bool isStatTab = true;

    private TowerGameUserStatLevelData towerGameUserStatLevelData;
    private TowerGameUserWeaponData towerGameUserWeaponData;


    protected override void OnAwake()
    {
        instance = this;
    }


    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            UpdateGameData();
            UpdateUI();
            _OpenUI();
            OnClickStatEnchantTab();
            isOnProcess = false;
        }
        else
        {
            _CloseWindow();
        }
    }

    private void UpdateUI()
    {

    }

    private void UpdateGameData()
    {
        var playerLevel = SaveDataManager.instance.playerData.level;
        var weaponMetaData = GameResourceManager.instance.GetTowerWeaponLevelMetaData(playerLevel);


        towerGameUserStatLevelData = SaveDataManager.instance.playerData.towerGameUserStatLevelData;
        towerGameUserWeaponData = SaveDataManager.instance.playerData.towerGameUserWeaponData;
    }


    public void OnClickStatEnchantTab()
    {
        for (TowerUserStatType type = TowerUserStatType.atk; type <= TowerUserStatType.criDmg; type++)
        {
            int statLevel = SaveDataManager.instance.playerData.towerGameUserStatLevelData.GetLevel(type);
            playerStatList[(int)type].UpdateUIData(type, statLevel);
        }

        SetTitle(LocalizeManager.instance.GetString("Tower.Enchant.StatTab"));
        isStatTab = true;
        statTab.SetActive(true);
        weaponTab.gameObject.SetActive(false);
    }

    public void OnClickWeaponEnchantTab()
    {
        SetTitle(LocalizeManager.instance.GetString("Tower.Enchant.WeaponTab"));
        isStatTab = false;
        statTab.SetActive(false);
        weaponTab.gameObject.SetActive(true);
    }
}
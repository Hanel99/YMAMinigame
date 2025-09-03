using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TowerGameWeaponEnchantPopup : PopupBase
{

    public static TowerGameWeaponEnchantPopup instance { get; private set; }
    public List<TowerGamePlayerStat> playerStatList;

    public GameObject statTab;
    public GameObject weaponTab;




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
        var level = SaveDataManager.instance.playerData.level;

        var weaponMetaData = GameResourceManager.instance.GetTowerWeaponLevelMetaData(level);


        towerGameUserStatLevelData = SaveDataManager.instance.playerData.towerGameUserStatLevelData;
        towerGameUserWeaponData = SaveDataManager.instance.playerData.towerGameUserWeaponData;

        for (TowerUserStatType type = TowerUserStatType.atk; type <= TowerUserStatType.criDmg; type++)
        {
            int statLevel = SaveDataManager.instance.playerData.towerGameUserStatLevelData.GetLevel(type);
            playerStatList[(int)type].UpdateUI(type, statLevel);
        }

        isStatTab = true;
        statTab.SetActive(true);
        weaponTab.SetActive(false);
    }


    public void OnClickStatEnchantTab()
    {
        if (isOpenCloseAnimationActing || isOnProcess) return;
        SetTitle(LocalizeManager.instance.GetString("Tower.Enchant.StatTab"));
        statTab.SetActive(true);
        weaponTab.SetActive(false);
    }

    public void OnClickWeaponEnchantTab()
    {
        if (isOpenCloseAnimationActing || isOnProcess) return;
        SetTitle(LocalizeManager.instance.GetString("Tower.Enchant.WeaponTab"));
        statTab.SetActive(false);
        weaponTab.SetActive(true);
    }
}
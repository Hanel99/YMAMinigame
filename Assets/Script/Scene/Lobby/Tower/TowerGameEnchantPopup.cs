using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerGameWeaponEnchantPopup : PopupBase
{
    public static TowerGameWeaponEnchantPopup instance { get; private set; }
    public GameObject statTab;
    public TowerGameWeaponStat weaponTab;
    public Text ownCoinText;
    public List<TowerGamePlayerStat> playerStatList;
    public TowerGameEnchantResult enchantResult;



    //private
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
        }
        else
        {
            _CloseWindow();
        }
    }

    public void UpdateUI()
    {
        ownCoinText.text = SaveDataManager.instance.playerData.coin.ToString();
    }

    private void UpdateGameData()
    {
        var playerLevel = SaveDataManager.instance.playerData.level;

        towerGameUserStatLevelData = SaveDataManager.instance.playerData.towerGameUserStatLevelData;
        towerGameUserWeaponData = SaveDataManager.instance.playerData.towerGameUserWeaponData;
        CloseEnchantResult();
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

        weaponTab.UpdateUIData(towerGameUserWeaponData.weaponLevel);
        isStatTab = false;
        statTab.SetActive(false);
        weaponTab.gameObject.SetActive(true);
    }

    public void ShowEnchantResult(TowerGameResultType type, string before, string after, Action UIRefreshAction = null)
    {
        enchantResult.gameObject.SetActive(true);
        enchantResult.ActResultAnimation(type, before, after, UIRefreshAction);
    }

    public void CloseEnchantResult()
    {
        enchantResult.gameObject.SetActive(false);
        UpdateUI();
    }
}
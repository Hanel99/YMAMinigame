using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;



public class TowerGameWeaponEnchantPopup : PopupBase
{
    public enum EnchantTabType
    {
        Stat,
        Weapon,
        Jewel
    }

    public static TowerGameWeaponEnchantPopup instance { get; private set; }
    public GameObject statTab;
    public TowerGameWeaponStat weaponTab;
    public TowerGameWeaponJewel weaponJewelTab;
    public GameObject jewelButton;
    public Text ownCoinText;
    public List<TowerGamePlayerStat> playerStatList;
    public TowerGameEnchantResult enchantResult;



    //private
    private EnchantTabType currentTab = EnchantTabType.Stat;

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
            currentTab = EnchantTabType.Stat;
            OnClickTab(currentTab, true);
        }
        else
        {
            _CloseWindow();
        }
    }

    public void UpdateUI()
    {
        ownCoinText.text = SaveDataManager.instance.playerData.coin.ToString("N0");
    }

    private void UpdateGameData()
    {
        var playerLevel = SaveDataManager.instance.playerData.level;

        towerGameUserStatLevelData = SaveDataManager.instance.playerData.towerGameUserStatLevelData;
        towerGameUserWeaponData = SaveDataManager.instance.playerData.towerGameUserWeaponData;
        CloseEnchantResult();
    }


    public void OnClickHowToPlay()
    {
        string typeStr = (currentTab) switch
        {
            EnchantTabType.Stat => "User",
            EnchantTabType.Weapon => "Weapon",
            EnchantTabType.Jewel => "Jewel",
            _ => ""
        };
        LobbyUIManager.instance.ShowHowToPlayPopup($"game.desc.Enchant.{typeStr}");
    }


    public void OnClickTab(EnchantTabType type, bool setForce = false)
    {
        if (!setForce && currentTab == type) return;

        currentTab = type;

        statTab.SetActive(false);
        weaponTab.gameObject.SetActive(false);
        weaponJewelTab.gameObject.SetActive(false);

        SetTitle(LocalizeManager.instance.GetString($"Tower.Enchant.{type}Tab"));

        switch (type)
        {
            case EnchantTabType.Stat:
                statTab.SetActive(true);
                for (TowerUserStatType statType = TowerUserStatType.Atk; statType <= TowerUserStatType.CriDmg; statType++)
                {
                    int statLevel = SaveDataManager.instance.playerData.towerGameUserStatLevelData.GetLevel(statType);
                    playerStatList[(int)statType].UpdateUIData(statType, statLevel);
                }
                break;
            case EnchantTabType.Weapon:
                weaponTab.gameObject.SetActive(true);
                weaponTab.UpdateUIData(towerGameUserWeaponData.weaponLevel);
                weaponTab.HideParticle();
                break;
            case EnchantTabType.Jewel:
                weaponJewelTab.gameObject.SetActive(true);
                weaponJewelTab.UpdateUIData();
                break;
        }
    }

    // 버튼 할당을 위해...
    public void OnClickStatEnchantTab()
    {
        OnClickTab(EnchantTabType.Stat);
    }

    public void OnClickWeaponEnchantTab()
    {
        if (!CheckUnlockCondition(StaticGameData.unlockWeaponFloor)) return;

        OnClickTab(EnchantTabType.Weapon);
    }

    public void OnClickWeaponJewelTab()
    {
        if (!CheckUnlockCondition(StaticGameData.unlockJewelFloor[0])) return;

        OnClickTab(EnchantTabType.Jewel);
    }

    public void OnClickJewelProbabilityButton()
    {
        LobbyUIManager.instance.ShowTowerJewelProbabilityPopup(weaponJewelTab.CurrentGrade);
    }

    public void ShowEnchantResult(TowerGameResultType type, string before, string after, Action UIRefreshAction = null)
    {
        isActBackKey = false;
        enchantResult.gameObject.SetActive(true);
        enchantResult.ActResultAnimation(type, before, after, UIRefreshAction).Forget();
    }

    public void CloseEnchantResult()
    {
        isActBackKey = true;
        enchantResult.gameObject.SetActive(false);
        UpdateUI();
    }

    private bool CheckUnlockCondition(int unlockFloor)
    {
        if (SaveDataManager.instance.playerData.towerFloor < unlockFloor)
        {
            LobbyUIManager.instance.ShowCommonPopup("미개방 컨텐츠", $"하넬을 {unlockFloor}명 쓰러트린 뒤 부터 사용할 수 있습니다.", true, true, false);
            return false;
        }
        return true;
    }
}
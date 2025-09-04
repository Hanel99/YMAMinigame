using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TowerGamePopup : PopupBase
{
    public static TowerGamePopup instance { get; private set; }


    public Text bossTitle;
    public Image bossImage;
    public Text bossDesc;



    //private
    private bool isOnProcess = false;

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
        bossTitle.text = "";
        bossImage.sprite = null;
        bossImage.SetNativeSize();

        bossDesc.text = "";
    }

    private void UpdateGameData()
    {
        var level = SaveDataManager.instance.playerData.level;
        var userMetaData = GameResourceManager.instance.GetTowerUserLevelMetaData(level);
        var weaponMetaData = GameResourceManager.instance.GetTowerWeaponLevelMetaData(level);


        towerGameUserStatLevelData = SaveDataManager.instance.playerData.towerGameUserStatLevelData;
        towerGameUserWeaponData = SaveDataManager.instance.playerData.towerGameUserWeaponData;
    }

    public void OnClickStartCombat()
    {
        if (isOpenCloseAnimationActing || isOnProcess) return;

        isOnProcess = true;



    }


    public void OnClickShowEnchantPopup()
    {
        if (isOpenCloseAnimationActing || isOnProcess) return;

        LobbyUIManager.instance.ShowPopup<TowerGameWeaponEnchantPopup>();
    }
}

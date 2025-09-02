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

    private TowerGameUserStatData towerGameUserStatData;
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
        towerGameUserStatData = SaveDataManager.instance.playerData.towerGameUserStatData;
        towerGameUserWeaponData = SaveDataManager.instance.playerData.towerGameUserWeaponData;
    }

    public void OnClickProbability()
    {
        if (isOpenCloseAnimationActing) return;

        // LobbyUIManager.instance.ShowPopup<GachaProbabilityPopup>();
    }

    public void OnClickStartCombat()
    {
        if (isOnProcess) return;

        isOnProcess = true;



    }
}

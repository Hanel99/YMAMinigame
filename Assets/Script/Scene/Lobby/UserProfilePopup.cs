using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserProfilePopup : PopupBase
{
    public static UserProfilePopup instance { get; private set; }

    public Text userMid;
    public Text userName;
    public Text userMaster;
    public Text userCoin;
    public Text userMilage;
    public Text userCardCount;


    protected override void OnAwake()
    {
        instance = this;
    }


    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            UpdateDataText();
            _OpenUI();
        }
        else
        {
            _CloseWindow();
        }
    }


    public void UpdateDataText()
    {
        userMid.text = SaveDataManager.instance.playerData.mid.ToString("D4");
        userName.text = SaveDataManager.instance.playerData.name.ToString();
        userMaster.text = LocalizeManager.instance.GetString($"master.name.{SaveDataManager.instance.playerData.master}");
        userCoin.text = SaveDataManager.instance.playerData.coin.ToString();
        userMilage.text = SaveDataManager.instance.playerData.mileage.ToString();
        userCardCount.text = $"{SaveDataManager.instance.playerData.ownCardList.Count} / {ResourceManager.instance.GetTotalCardCount()}";
    }

    public void OnClickPlayerDataSettingButton()
    {
        if (isOpenCloseAnimationActing) return;

        HLLogger.Log("@@@ playerDataSettingBtn Action");
        LobbyUIManager.instance.ShowPlayerDataSettingPopup();
    }
}

using UnityEngine;

public class CheatPopup : PopupBase
{
    public static CheatPopup instance { get; private set; }



    protected override void OnAwake()
    {
        instance = this;
    }

    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            _OpenUI();
        }
        else
        {
            _CloseWindow();
        }
    }



    public void OnClickLevelSelectButton()
    {
        SaveDataManager.instance.playerData.level = 1;
        SaveDataManager.instance.playerData.exp = 0;
    }

}

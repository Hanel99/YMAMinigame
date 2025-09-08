using UnityEngine;
using UnityEngine.UI;

public class HowToPlayPopup : PopupBase
{
    public static HowToPlayPopup instance { get; private set; }
    public Text titleText;


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


    public void ShowPopup(string descKey)
    {
        titleText.text = LocalizeManager.instance.GetString(descKey);
        ShowPopup();
    }
}

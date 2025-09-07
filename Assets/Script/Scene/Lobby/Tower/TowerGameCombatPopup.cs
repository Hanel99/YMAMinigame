using UnityEngine;
using UnityEngine.UI;

public class TowerGameCombatPopup : PopupBase
{
    public static TowerGameCombatPopup instance { get; private set; }


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

    public void UpdateUI()
    {


    }


}

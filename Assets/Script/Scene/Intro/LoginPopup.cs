using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LoginPopup : PopupBase
{
    public static LoginPopup instance { get; private set; }

    public InputField idInputField;
    public InputField pwInputField;
    public Toggle autoLogin;



    protected override void OnAwake()
    {
        instance = this;
    }

    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            idInputField.text = "";
            pwInputField.text = "";
            autoLogin.isOn = false;

            _OpenUI();
        }
        else
        {
            _CloseWindow();
        }
    }


    public void OnClickLogin()
    {
        //login process


    }

    public void OnClickShowRegisterPopup()
    {
        IntroUIManager.instance.ShowRegisterPopup();
    }
}

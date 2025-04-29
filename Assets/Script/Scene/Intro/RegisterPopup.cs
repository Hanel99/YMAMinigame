using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPopup : PopupBase
{
    public static RegisterPopup instance { get; private set; }

    public InputField idInputField;
    public InputField pwInputField;


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


    public void OnClickRegister()
    {
        if (idInputField.text.Length < 2)
        {
            IntroUIManager.instance.ShowCommonPopup("오류", "ID는 3자 이상 입력해주세요.", true, true, false, null, null);
            return;
        }
        if (pwInputField.text.Length < 3)
        {
            IntroUIManager.instance.ShowCommonPopup("오류", "비밀번호는 4자 이상 입력해주세요.", true, true, false, null, null);
            return;
        }


        // reguster process


    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RegisterPopup : PopupBase
{
    public static RegisterPopup instance { get; private set; }

    public InputField idInputField;
    public InputField pwInputField;
    public Text processText;
    public Button registerButton;

    private bool isConnecting = false;


    protected override void OnAwake()
    {
        instance = this;
    }


    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            UIInit();
            _OpenUI();
        }
        else
        {
            isConnecting = true;
            _CloseWindow();
        }
    }

    private void UIInit()
    {
        idInputField.text = "";
        pwInputField.text = "";
        registerButton.enabled = true;
        processText.text = "";
    }


    public void OnClickRegister()
    {
        if (isConnecting)
            return;


        if (idInputField.text.Length < 3)
        {
            processText.text = "ID는 3자 이상 입력해주세요.";
            return;
        }
        if (pwInputField.text.Length < 6)
        {
            processText.text = "비밀번호는 6자 이상 입력해주세요.";
            return;
        }


        // reguster process

        isConnecting = true;
        registerButton.enabled = false;
        processText.text = "회원가입 진행 중...";
        PlayFabManager.instance.IntroUserRegisterProcess(idInputField.text, pwInputField.text, () =>
        {
            CommonPopup popup = null;
            popup = IntroUIManager.instance.ShowCommonPopup("성공", "회원가입이 완료되었습니다.", false, true, false, null, () =>
            {
                isConnecting = false;
                popup?.ShowPopup(false);
                ShowPopup(false);
            });
        }, (error) =>
        {
            isConnecting = false;
            registerButton.enabled = true;
            processText.text = "";

            var text = PlayFabManager.instance.GetPlayFabErrorText(error.Error, error.GenerateErrorReport());
            IntroUIManager.instance.ShowCommonPopup("오류", $"회원가입에 실패했습니다.\n\n{text}", false, true, false, null, null);
        });

    }
}

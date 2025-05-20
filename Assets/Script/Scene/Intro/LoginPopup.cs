using UnityEngine.UI;

public class LoginPopup : PopupBase
{
    public static LoginPopup instance { get; private set; }

    public InputField idInputField;
    public InputField pwInputField;
    public Toggle autoLogin;
    private bool isConnecting = false;



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
            isConnecting = true;
            _CloseWindow();
        }
    }


    public void OnClickLogin()
    {
        if (isConnecting)
            return;

        if (idInputField.text.Length < 3)
        {
            IntroUIManager.instance.ShowCommonPopup("오류", "ID는 3자 이상 입력해주세요.", true, true, false, null, null);
            return;
        }
        if (pwInputField.text.Length < 6)
        {
            IntroUIManager.instance.ShowCommonPopup("오류", "비밀번호는 6자 이상 입력해주세요.", true, true, false, null, null);
            return;
        }

        isConnecting = true;
        PlayFabManager.instance.IntroUserLoginProcess(idInputField.text, pwInputField.text, () =>
        {
            PlayFabManager.instance.GetUserData(() =>
            {
                isConnecting = false;
                SaveDataManager.instance.SetIDPW(idInputField.text, pwInputField.text, autoLogin.isOn);
                IntroController.instance.MoveNextIntroProcess();
                ShowPopup(false);
            });

        }, (error) =>
        {
            isConnecting = false;
            IntroUIManager.instance.ShowCommonPopup("오류", $"PlayFab 로그인에 실패하였습니다.\n{error}", false, true, false, null, null);
        });

    }

    public void OnClickShowRegisterPopup()
    {
        IntroUIManager.instance.ShowRegisterPopup();
    }
}

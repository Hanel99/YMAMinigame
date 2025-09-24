using UnityEngine.UI;

public class LoginPopup : PopupBase
{
    public static LoginPopup instance { get; private set; }

    public InputField idInputField;
    public InputField pwInputField;
    public Toggle autoLogin;
    public Text processText;
    public Button loginButton;
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
            processText.text = "";
            loginButton.enabled = true;
            isConnecting = false;

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
            processText.text = "ID는 3자 이상 입력해주세요.";
            return;
        }
        if (pwInputField.text.Length < 6)
        {
            processText.text = "비밀번호는 6자 이상 입력해주세요.";
            return;
        }

        isConnecting = true;
        loginButton.enabled = false;
        processText.text = "로그인 진행 중...";
        PlayFabManager.instance.IntroUserLoginProcess(idInputField.text, pwInputField.text, () =>
        {
            PlayFabManager.instance.GetUserData(() =>
            {
                isConnecting = false;
                SaveDataManager.instance.SetIDPW(idInputField.text, pwInputField.text, autoLogin.isOn);
                IntroUIManager.instance.UpdateIDText(idInputField.text);
                IntroController.instance.MoveNextIntroProcess();
                ShowPopup(false);
            });

        }, (error) =>
        {
            isConnecting = false;
            loginButton.enabled = true;
            processText.text = "";

            var text = PlayFabManager.instance.GetPlayFabErrorText(error.Error, error.GenerateErrorReport());
            IntroUIManager.instance.ShowCommonPopup("오류", $"PlayFab 로그인에 실패하였습니다.\n\n{text}", false, true, false, null, null);
        });

    }

    public void OnClickShowRegisterPopup()
    {
        IntroUIManager.instance.ShowRegisterPopup();
    }
}

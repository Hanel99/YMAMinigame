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


        // reguster process

        PlayFabManager.instance.IntroUserRegisterProcess(idInputField.text, pwInputField.text, () =>
        {
            CommonPopup popup = null;
            popup = IntroUIManager.instance.ShowCommonPopup("성공", "회원가입이 완료되었습니다.", false, true, false, null, () =>
            {
                popup?.ShowPopup(false);
                ShowPopup(false);
            });
        }, (error) =>
        {
            IntroUIManager.instance.ShowCommonPopup("오류", $"회원가입에 실패했습니다.\n{error}", false, true, false, null, null);
        });

    }
}

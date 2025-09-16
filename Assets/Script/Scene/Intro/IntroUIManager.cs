using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class IntroUIManager : MonoBehaviour
{
    public static IntroUIManager instance { get; private set; }

    public GameObject completeDim;
    public GameObject errorDim;

    public GameObject titleLogo;
    public GameObject titleText;

    public Text stateText;
    public Text versionText;
    public Text errorDimText;
    public SceneAnimation sceneDim;
    public Button removeLoginDataButton;

    public Transform popupRoot;
    public List<PopupBase> popupList = new();



    private void Awake()
    {
        instance = this;
    }


    public void ShowCompleteDim(bool show)
    {
        completeDim.SetActive(show);
    }

    public void ShowErrorDim(string errorText = "")
    {
        errorDimText.text = errorText;
        errorDim.SetActive(true);
    }


    public void UpdateVersionText(string version)
    {
        StringBuilder sb = new StringBuilder();

#if DEV
        sb.Append("DEV-");
#else
        sb.Append("Live-");
#endif
        sb.Append($"Ver.{version}");

        versionText.text = sb.ToString();
    }

    public void UpdateStateText(IntroState state)
    {
        switch (state)
        {
            case IntroState.Ready:
                stateText.text = "게임을 시작합니다.";
                break;

            case IntroState.InitManagers:
                stateText.text = "매니저를 초기화 합니다.";
                break;

            case IntroState.ServerUpdate:
                stateText.text = "서버 데이터를 업데이트 합니다.";
                break;

            case IntroState.LoadUserData:
                stateText.text = LocalizeManager.instance.GetString($"intro.process.{state}");
                break;

            case IntroState.PlayFabLogin:
                stateText.text = "PlayFab 로그인";
                break;

            case IntroState.Complete:
                stateText.text = "";
                break;

            default:
                stateText.text = "";
                break;
        }
    }



    public void OnClickEnterButton()
    {
        IntroController.instance.GoToGameSelectScene();
    }

    public void ShowSceneMoveAnimation(bool showOpen, Action callback = null)
    {
        sceneDim.ShowAnimation(showOpen, callback);
    }

    public void OnClickRemoveLoginDataButton()
    {
        ShowCommonPopup("로그인 정보 삭제", "로그인 정보를 초기화 하겠습니까?", false, true, true, null, () => { IntroController.instance.RemoveLoginData(); });
    }





    public CommonPopup ShowCommonPopup(string titleText, string descText, bool showClose, bool showOK, bool showNo, Action closeCallback = null, Action OKCallback = null)
    {
        var popup = _ShowPopup<CommonPopup>();
        popup.ShowPopup(titleText, descText, showClose, showOK, showNo, closeCallback, OKCallback);
        return popup;
    }

    public void ShowLoginPopup()
    {
        _ShowPopup<LoginPopup>().ShowPopup();
    }

    public void ShowRegisterPopup()
    {
        _ShowPopup<RegisterPopup>().ShowPopup();
    }


    //매개변수 없는 팝업의 경우
    public void ShowPopup<T>() where T : PopupBase
    {
        _ShowPopup<T>().ShowPopup(true);
    }

    private T _ShowPopup<T>() where T : PopupBase
    {
        var popupName = typeof(T).Name;

        var popup = GameResourceManager.instance.GetPopup<T>(popupRoot);
        if (popup != null)
        {
            popupList.RemoveAll(x => x == null);
            popupList.Add(popup);
        }
        return popup;
    }
}

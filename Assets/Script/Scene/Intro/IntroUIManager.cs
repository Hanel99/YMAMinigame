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

    public void UpdateStateText(IntroState state, string addText = "")
    {

        switch (state)
        {
            case IntroState.Ready:
                stateText.text = "게임을 시작합니다.";
                break;

            case IntroState.InitManagers:
                stateText.text = "매니저를 초기화 합니다.";
                break;

            case IntroState.CheckAppVersion:
                stateText.text = "앱 버전 체크";
                break;

            case IntroState.CheckMaintenance:
                stateText.text = "온라인 접속 확인";
                break;

            case IntroState.LoadUserData:
                stateText.text = LocalizeManager.instance.GetString($"intro.process.{state}");
                break;

            case IntroState.ServerUpdate:
                stateText.text = $"{LocalizeManager.instance.GetString($"intro.process.{state}")} {addText}";
                break;

            case IntroState.PlayFabLogin:
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





    public Transform popupRoot;
    public List<GameObject> popupList = new();


    public CommonPopup ShowCommonPopup(string titleText, string descText, bool showClose, bool showOK, bool showNo, Action closeCallback = null, Action OKCallback = null)
    {
        var popup = ShowPopup<CommonPopup>();
        popup.GetComponent<CommonPopup>().ShowPopup(titleText, descText, showClose, showOK, showNo, closeCallback, OKCallback);
        return popup.GetComponent<CommonPopup>();
    }

    public void ShowLoginPopup()
    {
        var popup = ShowPopup<LoginPopup>();
        popup.GetComponent<LoginPopup>().ShowPopup();
    }

    public void ShowRegisterPopup()
    {
        var popup = ShowPopup<RegisterPopup>();
        popup.GetComponent<RegisterPopup>().ShowPopup();
    }

    public GameObject ShowPopup<T>()
    {
        var popupName = typeof(T).Name;

        var popup = ResourceManager.instance.GetPopup(popupName, popupRoot);
        if (popup != null)
        {
            popupList.RemoveAll(x => x == null);
            popupList.Add(popup);
        }
        return popup;
    }

}

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


public class WingTtoGameUIManager : MonoBehaviour
{
    public static WingTtoGameUIManager instance { get; private set; }

    public Transform popupRoot;
    public List<PopupBase> popupList = new();

    [Header("Root")]
    public Transform rootDim;
    public SceneAnimation sceneDim;

    [Header("InGame")]
    public Text scoreText;



    private void Awake()
    {
        instance = this;
    }


    public void ResetInGameUI()
    {
        rootDim.gameObject.SetActive(true);
    }
    public void ShowRootDim(bool show)
    {
        rootDim.gameObject.SetActive(show);
    }



    public void ShowResult()
    {
        _ShowPopup<GameResultPopup>().ShowPopup(0, 0, null, SceneName.YMAWingTto);
    }

    public void ShowPausePopup()
    {
        _ShowPopup<PausePopup>().ShowPopup();
    }

    public void ShowLevelUpPopup()
    {
        _ShowPopup<LevelUpPopup>().ShowPopup();
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

    public void CloseTopPopup()
    {
        if (popupList.Count > 0)
        {
            popupList.RemoveAll(x => x == null);
            var popup = popupList[popupList.Count - 1];
            var popupBase = popup.GetComponent<PopupBase>();

            if (popupBase.isActBackKey)
                popupBase.OnClickClose();
        }
    }

    public bool IsPopupOpen()
    {
        popupList.RemoveAll(x => x == null);
        return popupList.Count > 0;
    }

    public void ShowSceneMoveAnimation(bool showOpen, Action callback = null)
    {
        sceneDim.ShowAnimation(showOpen, callback);
    }




    // InGame UI Logic
    public void ShowDim(bool show, string dimText = "")
    {


    }
    public void OnClickPause()
    {
        //TODO 인게임 pause 처리


        _ShowPopup<PausePopup>().ShowPopup();
    }

}

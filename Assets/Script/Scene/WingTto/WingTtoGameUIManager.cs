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
    public GameObject inGameDim;
    public Text dimText;
    public GameObject startButton;

    public Text distanceText;
    public Text getSetText;
    public Text currentSpeedText;
    public Text earnCoinText;
    public Text earnExpText;


    //private
    WingTtoGameManager gameManager => WingTtoGameManager.instance;




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
    public void ShowDim(bool show, string dimText = "", bool showStartButton = false)
    {
        inGameDim.SetActive(show);
        this.dimText.text = dimText;
        startButton.SetActive(showStartButton);
    }

    public void InitUI()
    {
        inGameDim.SetActive(false);
        dimText.text = "";
        distanceText.text = "";
        getSetText.text = "";

        currentSpeedText.text = "";
        earnCoinText.text = "";
        earnExpText.text = "";
    }


    public void UpdateGetSetText(string Text)
    {
        getSetText.text = Text;
    }

    public void UpdateSpeedText(float value)
    {
        currentSpeedText.text = $"Speed : {value:F0}";
    }

    public void UpdateCoinText(int value)
    {
        earnCoinText.text = $"Coin : {value}";
    }

    public void UpdateExpText(int value)
    {
        earnExpText.text = $"Exp : {value}";
    }



    public void OnClickPause()
    {
        //TODO 인게임 pause 처리

        _ShowPopup<PausePopup>().ShowPopup();
        gameManager.SetPause(true);
    }

    void Update()
    {
        distanceText.text = gameManager.GetFormattedDistance();
    }

}

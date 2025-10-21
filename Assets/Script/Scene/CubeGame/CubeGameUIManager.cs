using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


public class CubeGameUIManager : MonoBehaviour
{
    public static CubeGameUIManager instance { get; private set; }

    public Transform popupRoot;
    public List<PopupBase> popupList = new();

    [Header("Root")]
    public Transform rootDim;
    public SceneAnimation sceneDim;

    [Header("InGame")]
    public Text timerText;
    public Text scoreText;

    public List<CubeGameCountUI> cubeGameCountUIList = new();
    private Dictionary<CubeState, CubeGameCountUI> cubeGameCountUIDic = new();
    public GameObject dimObject;
    public Text dimText;



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



    public void ShowResult(int score, int earnCoinAmount, int exp)
    {
        _ShowPopup<CubeGameResultPopup>().ShowPopup(score, earnCoinAmount, exp);
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

    public void OnClickPauseButton()
    {
        CubeGameManager.instance.SetPause(true);
        ShowPausePopup();
    }

    public void UpdateLeftTimeText(float leftTime)
    {
        timerText.text = $"남은 시간 : {leftTime.ToString("F2")}";
    }

    public void UpdateScoreText(int score)
    {
        scoreText.text = $"점수 : {score}";
    }

    public void UpdateCubeCountText(Dictionary<CubeState, int> countDic)
    {
        foreach (CubeState state in Enum.GetValues(typeof(CubeState)))
        {
            if (state == CubeState.Wait || state == CubeState.Stop) continue;

            if (countDic.ContainsKey(state))
                UpdateCubeCountText(state, countDic[state]);
        }
    }

    public void UpdateCubeCountText(CubeState state, int count)
    {
        if (state == CubeState.Wait || state == CubeState.Stop)
            return;

        if (cubeGameCountUIDic.ContainsKey(state))
            cubeGameCountUIDic[state].UpdateCountText(count);
    }

    public void InitCountUI()
    {
        foreach (var ui in cubeGameCountUIList)
        {
            ui.InitUI(ui.state);
            cubeGameCountUIDic[ui.state] = ui;
        }
    }

    public void ShowDim(bool show, string text = "")
    {
        dimText.text = text;
        dimObject.SetActive(show);
    }

}

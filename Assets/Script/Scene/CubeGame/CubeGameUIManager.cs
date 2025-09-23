using System;
using System.Collections.Generic;
using System.Text;
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
    public Text devText; //dev용 터치 횟수 기록.



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



    public void ShowResult(int touchCount, int earnCoinAmount)
    {
        _ShowPopup<GameResultPopup>().ShowPopup(touchCount, earnCoinAmount, null, SceneName.YMACubeGame);
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
        scoreText.text = score.ToString();
    }

    public void UpdateCubeCountText(Dictionary<CubeState, int> countDic)
    {
        StringBuilder sb = new StringBuilder();
        foreach (CubeState state in Enum.GetValues(typeof(CubeState)))
        {
            int count = 0;
            if (countDic.ContainsKey(state))
            {
                count = countDic[state];
            }
            sb.Append($"{state} : {count} / ");
        }
        devText.text = sb.ToString();
    }

}

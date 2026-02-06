using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameResultPopup : PopupBase
{
    public static GameResultPopup instance { get; private set; }

    public Image resultDim;
    public Text tryCountText;
    public Text earnCoinText;
    public Text totalCoinText;
    public GameObject lobbyButton;
    public GameObject retryButton;

    //private
    private Coroutine cor;
    private Action callbackAction;
    private SceneName sceneName;


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


    public void StopResultCoroutine()
    {
        if (cor != null)
            StopCoroutine(cor);
    }

    public void ShowPopup(int touchCount, int earnCoinAmount, List<int> collectCardIdList, SceneName sceneName, Action callback = null)
    {
        HideResultObjects();
        ShowPopup();
        callbackAction = callback;
        this.sceneName = sceneName;
        cor = StartCoroutine(co_resultProcess(touchCount, earnCoinAmount, collectCardIdList));
    }

    private void HideResultObjects()
    {
        tryCountText.text = "";
        earnCoinText.text = "";
        totalCoinText.text = "";
        lobbyButton.SetActive(false);
        retryButton.SetActive(false);
    }


    private IEnumerator co_resultProcess(int touchCount, int earnCoinAmount, List<int> collectCardIdList)
    {
        resultDim.DOFade(0.7f, 1).SetEase(Ease.Linear).From(0f);
        yield return new WaitForSeconds(0.2f);

        tryCountText.text = $"시도 횟수 : {touchCount}";
        yield return new WaitForSeconds(0.3f);

        earnCoinText.text = $"획득 코인 : {earnCoinAmount:N0}";
        yield return new WaitForSeconds(0.3f);

        totalCoinText.text = $"총 보유 코인 : {SaveDataManager.instance.playerData.coin:N0}";
        yield return new WaitForSeconds(0.3f);

        if (collectCardIdList != null && collectCardIdList.Count > 0)
            callbackAction?.Invoke();

        lobbyButton.SetActive(true);
        retryButton.SetActive(true);
        cor = null;
        yield break;
    }



    public override void OnClickClose()
    {
        if (sceneName == SceneName.YMAMatch2CardGame)
        {
            if (CardGameUIManager.instance != null && !CardGameUIManager.instance.IsPopupQueueEmpty)
                return;
        }
        else if (sceneName == SceneName.YMAFindAIWordGame)
        {
            if (FindAIWordGameUIManager.instance != null && !FindAIWordGameUIManager.instance.IsPopupQueueEmpty)
                return;
        }

        SceneMoveManager.instance.MoveScene(SceneName.LobbyScene);
        ShowPopup(false);
    }

    public void OnClickRetry()
    {
        if (sceneName == SceneName.YMAMatch2CardGame)
        {
            if (CardGameUIManager.instance != null && !CardGameUIManager.instance.IsPopupQueueEmpty)
                return;
        }
        else if (sceneName == SceneName.YMAFindAIWordGame)
        {
            if (FindAIWordGameUIManager.instance != null && !FindAIWordGameUIManager.instance.IsPopupQueueEmpty)
                return;
        }

        SceneMoveManager.instance.MoveScene(sceneName);
    }
}


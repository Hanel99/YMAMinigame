using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System;

public class CubeGameResultPopup : PopupBase
{
    public static CubeGameResultPopup instance { get; private set; }

    public Image resultDim;
    public Text scoreText;
    public Text expText;
    public Text earnCoinText;
    public Text totalCoinText;
    public GameObject lobbyButton;
    public GameObject retryButton;

    //private
    private Coroutine cor;
    private Action showNewCardAction;
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

    public void ShowPopup(int score, int earnCoinAmount, int exp, SceneName sceneName, Action newCardAction = null)
    {
        HideResultObjects();
        ShowPopup();
        showNewCardAction = newCardAction;
        this.sceneName = sceneName;
        cor = StartCoroutine(co_resultProcess(score, earnCoinAmount, exp));
    }

    private void HideResultObjects()
    {
        scoreText.text = "";
        expText.text = "";
        earnCoinText.text = "";
        totalCoinText.text = "";
        lobbyButton.SetActive(false);
        retryButton.SetActive(false);
    }


    private IEnumerator co_resultProcess(int score, int earnCoinAmount, int exp)
    {
        resultDim.DOFade(0.7f, 1).SetEase(Ease.Linear).From(0f);
        yield return new WaitForSeconds(0.2f);

        scoreText.text = $"점수 : {score}";
        yield return new WaitForSeconds(0.3f);

        expText.text = $"획득 경험치 : {exp}";
        yield return new WaitForSeconds(0.3f);

        earnCoinText.text = $"획득 코인 : {earnCoinAmount}";
        yield return new WaitForSeconds(0.3f);

        totalCoinText.text = $"총 보유 코인 : {SaveDataManager.instance.playerData.coin}";
        yield return new WaitForSeconds(0.3f);

        lobbyButton.SetActive(true);
        retryButton.SetActive(true);
        cor = null;
        yield break;
    }



    public override void OnClickClose()
    {
        SceneMoveManager.instance.MoveScene(SceneName.LobbyScene);
        ShowPopup(false);
    }

    public void OnClickRetry()
    {
        SceneMoveManager.instance.MoveScene(sceneName);
    }
}


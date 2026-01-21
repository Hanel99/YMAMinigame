using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    public void ShowPopup(int score, int earnCoinAmount, int exp)
    {
        HideResultObjects();
        ShowPopup();
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


    // Consts
    private const float FADE_DURATION = 1f;
    private const float INITIAL_DELAY = 0.2f;
    private const float TEXT_DELAY = 0.3f;

    private IEnumerator co_resultProcess(int score, int earnCoinAmount, int exp)
    {
        resultDim.DOFade(0.7f, FADE_DURATION).SetEase(Ease.Linear).From(0f);
        yield return new WaitForSeconds(INITIAL_DELAY);

        scoreText.text = $"점수 : {score}";
        yield return new WaitForSeconds(TEXT_DELAY);

        expText.text = $"획득 경험치 : {exp}";
        yield return new WaitForSeconds(TEXT_DELAY);

        earnCoinText.text = $"획득 코인 : {earnCoinAmount}";
        yield return new WaitForSeconds(TEXT_DELAY);

        totalCoinText.text = $"총 보유 코인 : {SaveDataManager.instance.playerData.coin}";
        yield return new WaitForSeconds(TEXT_DELAY);

        lobbyButton.SetActive(true);
        retryButton.SetActive(true);
        cor = null;
    }



    public override void OnClickClose()
    {
        SceneMoveManager.instance.MoveScene(SceneName.LobbyScene);
        ShowPopup(false);
    }

    public void OnClickRetry()
    {
        SceneMoveManager.instance.MoveScene(SceneName.YMACubeGame);
    }
}


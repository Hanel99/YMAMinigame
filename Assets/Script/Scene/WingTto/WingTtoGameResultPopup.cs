using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class WingTtoGameResultPopup : PopupBase
{
    public static WingTtoGameResultPopup instance { get; private set; }

    public Image resultDim;
    public Text distanceText;
    public Text myBestScoreText;
    public GameObject newRecordObject;
    public Text expItemText;
    public Text coinItemText;
    public Text earnExpText;
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

    public void ShowPopup(float distance, int coin, int exp, int totalCoin, int totalExp)
    {
        HideResultObjects();
        ShowPopup();
        cor = StartCoroutine(co_resultProcess(distance, coin, exp, totalCoin, totalExp));
    }

    private void HideResultObjects()
    {
        distanceText.text = "";
        myBestScoreText.text = "";
        newRecordObject.gameObject.SetActive(false);
        expItemText.text = "";
        coinItemText.text = "";
        earnExpText.text = "";
        earnCoinText.text = "";
        totalCoinText.text = "";

        lobbyButton.SetActive(false);
        retryButton.SetActive(false);
    }


    private IEnumerator co_resultProcess(float distance, int coin, int exp, int totalCoin, int totalExp)
    {
        resultDim.DOFade(0.7f, 1).SetEase(Ease.Linear).From(0f);
        yield return new WaitForSeconds(0.2f);

        distanceText.text = $"도달 거리 : {distance:F0}";
        yield return new WaitForSeconds(0.3f);

        float bestScore = SaveDataManager.instance.playerData.wingTtoHighScore;
        myBestScoreText.text = $"최고 기록 : {bestScore:F0}";
        if (bestScore == distance)
            newRecordObject.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.3f);

        expItemText.text = $"획득 경험치 : {exp}";
        coinItemText.text = $"획득 코인 : {coin}";
        yield return new WaitForSeconds(0.3f);

        earnExpText.text = $"총 획득 경험치 : {totalExp}";
        earnCoinText.text = $"총 획득 코인 : {totalCoin}";
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
        SceneMoveManager.instance.MoveScene(SceneName.YMAWingTto);
    }
}

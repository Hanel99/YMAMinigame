using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Globalization;


public class FinalQuizInfoPopup : PopupBase
{
    public static FinalQuizInfoPopup instance { get; private set; }


    public GameObject completeGroup;
    public GameObject infoGroup;

    [Header("Complete Group")]
    public Text nameText;
    public Text dateText;

    [Header("Info Group")]
    public Image matchCardGameReferImage;
    public Image wordGameReferImage;
    public Image cubeGameReferImage;
    public Image wingTtoReferImage;
    public Button examButton;
    public Text infoText;

    [Header("Warning Group")]
    public GameObject warningMiniPopup;


    private FinalQuizPlayData finalQuizPlayData => SaveDataManager.instance.playerData.finalQuizPlayData;




    protected override void OnAwake()
    {
        instance = this;
    }


    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            SetFinalQuizData();
            _OpenUI();
        }
        else
        {
            _CloseWindow();
        }
    }


    public void SetFinalQuizData()
    {
        completeGroup.SetActive(finalQuizPlayData.playEndRoll);
        infoGroup.SetActive(!finalQuizPlayData.playEndRoll);

        if (finalQuizPlayData.playEndRoll)
        {
            nameText.text = $"{SaveDataManager.instance.playerData.name} 님";
            dateText.text = finalQuizPlayData.completeTime.ToString("yyyy년 M월 d일\ntt hh시 mm분", new CultureInfo("ko-KR"));
            return;
        }

        matchCardGameReferImage.gameObject.SetActive(finalQuizPlayData.matchCardGame);
        wordGameReferImage.gameObject.SetActive(finalQuizPlayData.findAIWordGame);
        cubeGameReferImage.gameObject.SetActive(finalQuizPlayData.cubeGame);
        wingTtoReferImage.gameObject.SetActive(finalQuizPlayData.wingTto);

        int count = 0;
        if (finalQuizPlayData.matchCardGame) count++;
        if (finalQuizPlayData.findAIWordGame) count++;
        if (finalQuizPlayData.cubeGame) count++;
        if (finalQuizPlayData.wingTto) count++;

        infoText.text = count < 4 ? $"동의서가 {4 - count}개 부족한 듯 하다." : "벽반 가입 시험에 응시할 수 있습니다.";

    }




    public void OnClickExamButton()
    {
        if (finalQuizPlayData.matchCardGame && finalQuizPlayData.findAIWordGame && finalQuizPlayData.cubeGame && finalQuizPlayData.wingTto)
            OnClickShowWarningMiniPopup(true);
        else
            examButton.transform.DOShakePosition(0.2f, 30, 100);
    }

    public void OnClickTryExamButton()
    {
        finalQuizPlayData.matchCardGame = false;
        finalQuizPlayData.findAIWordGame = false;
        finalQuizPlayData.cubeGame = false;
        finalQuizPlayData.wingTto = false;
        finalQuizPlayData.tryCount++;

        SaveDataManager.instance.SavePlayerData();
        EnterExam().Forget();
    }

    private async UniTask EnterExam()
    {
        isActBackKey = false;
        OnClickShowWarningMiniPopup(false);

        await UniTask.Delay(1000);

        SceneMoveManager.instance.MoveScene(SceneName.YMAFinalQuiz);
    }

    public void OnClickShowWarningMiniPopup(bool show)
    {
        warningMiniPopup.gameObject.SetActive(show);
    }
}

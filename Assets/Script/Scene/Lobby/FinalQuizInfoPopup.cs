using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


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

        matchCardGameReferImage.gameObject.SetActive(finalQuizPlayData.matchCardGame == FinalReferState.Completed);
        wordGameReferImage.gameObject.SetActive(finalQuizPlayData.findAIWordGame == FinalReferState.Completed);
        cubeGameReferImage.gameObject.SetActive(finalQuizPlayData.cubeGame == FinalReferState.Completed);
        wingTtoReferImage.gameObject.SetActive(finalQuizPlayData.wingTto == FinalReferState.Completed);

        int count = 0;
        if (finalQuizPlayData.matchCardGame == FinalReferState.Completed) count++;
        if (finalQuizPlayData.findAIWordGame == FinalReferState.Completed) count++;
        if (finalQuizPlayData.cubeGame == FinalReferState.Completed) count++;
        if (finalQuizPlayData.wingTto == FinalReferState.Completed) count++;

        infoText.text = count < 4 ? $"동의서가 {4 - count}개 부족한 듯 하다." : "벽반 가입 시험에 응시할 수 있습니다.";

    }




    public void OnClickExamButton()
    {
        //@@@ debug;
        SceneMoveManager.instance.MoveScene(SceneName.YMAFinalQuiz);
        return;


        if (finalQuizPlayData.matchCardGame == FinalReferState.Completed && finalQuizPlayData.findAIWordGame == FinalReferState.Completed
            && finalQuizPlayData.cubeGame == FinalReferState.Completed && finalQuizPlayData.wingTto == FinalReferState.Completed)
            OnClickShowWarningMiniPopup(true);
        else
        {
            examButton.transform.localPosition = new Vector3(0, 0, 0);
            examButton.transform.DOShakePosition(0.2f, 30, 100);
        }
    }

    public void OnClickTryExamButton()
    {
        SaveDataManager.instance.SetFinalQuizReferData(GameType.MatchCardGame, FinalReferState.Unlocked);
        SaveDataManager.instance.SetFinalQuizReferData(GameType.FindAIWordGame, FinalReferState.Unlocked);
        SaveDataManager.instance.SetFinalQuizReferData(GameType.CubeGame, FinalReferState.Unlocked);
        SaveDataManager.instance.SetFinalQuizReferData(GameType.WingTto, FinalReferState.Unlocked);
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

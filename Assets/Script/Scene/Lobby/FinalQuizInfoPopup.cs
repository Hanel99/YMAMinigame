using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Michsky.UI.Shift;
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
    public Image matchCardGameReferBackImage;
    public Image wordGameReferBackImage;
    public Image cubeGameReferBackImage;
    public Image wingTtoReferBackImage;

    public Image matchCardGameReferImage;
    public Image wordGameReferImage;
    public Image cubeGameReferImage;
    public Image wingTtoReferImage;

    public Button examButton;
    public Text infoText;

    [Header("Warning Group")]
    public CanvasGroup warningMiniPopup;
    public GameObject sceneMoveDim;


    private FinalQuizPlayData FinalQuizPlayData => SaveDataManager.instance.playerData.finalQuizPlayData;




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
        completeGroup.SetActive(FinalQuizPlayData.playEndRoll);
        infoGroup.SetActive(!FinalQuizPlayData.playEndRoll);
        sceneMoveDim.SetActive(false);

        if (FinalQuizPlayData.playEndRoll)
        {
            nameText.text = $"{SaveDataManager.instance.playerData.name} 님";
            dateText.text = FinalQuizPlayData.completeTime.ToString("yyyy년 M월 d일\ntt hh시 mm분", new CultureInfo("ko-KR"));
            return;
        }

        matchCardGameReferBackImage.gameObject.SetActive(FinalQuizPlayData.matchCardGame >= FinalReferState.Unlocked);
        wordGameReferBackImage.gameObject.SetActive(FinalQuizPlayData.findAIWordGame >= FinalReferState.Unlocked);
        cubeGameReferBackImage.gameObject.SetActive(FinalQuizPlayData.cubeGame >= FinalReferState.Unlocked);
        wingTtoReferBackImage.gameObject.SetActive(FinalQuizPlayData.wingTto >= FinalReferState.Unlocked);

        matchCardGameReferImage.gameObject.SetActive(FinalQuizPlayData.matchCardGame == FinalReferState.Completed);
        wordGameReferImage.gameObject.SetActive(FinalQuizPlayData.findAIWordGame == FinalReferState.Completed);
        cubeGameReferImage.gameObject.SetActive(FinalQuizPlayData.cubeGame == FinalReferState.Completed);
        wingTtoReferImage.gameObject.SetActive(FinalQuizPlayData.wingTto == FinalReferState.Completed);

        int count = 0;
        if (FinalQuizPlayData.matchCardGame == FinalReferState.Completed) count++;
        if (FinalQuizPlayData.findAIWordGame == FinalReferState.Completed) count++;
        if (FinalQuizPlayData.cubeGame == FinalReferState.Completed) count++;
        if (FinalQuizPlayData.wingTto == FinalReferState.Completed) count++;

        infoText.text = count < 4 ? $"동의서가 {4 - count}개 부족한 듯 하다." : "벽반 가입 시험에 응시할 수 있습니다.";

    }




    public void OnClickExamButton()
    {
        //@@@ debug;
        // SceneMoveManager.instance.MoveScene(SceneName.YMAFinalQuiz);
        // return;


        if (FinalQuizPlayData.matchCardGame == FinalReferState.Completed && FinalQuizPlayData.findAIWordGame == FinalReferState.Completed
            && FinalQuizPlayData.cubeGame == FinalReferState.Completed && FinalQuizPlayData.wingTto == FinalReferState.Completed)
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
        FinalQuizPlayData.tryCount++;

        SaveDataManager.instance.SavePlayerData();
        EnterExam().Forget();
    }

    private async UniTask EnterExam()
    {
        isActBackKey = false;
        OnClickShowWarningMiniPopup(false);
        sceneMoveDim.SetActive(true);

        matchCardGameReferImage.transform.DOScale(0.55f, 0.8f).From(0.4f).SetEase(Ease.OutQuart);
        wordGameReferImage.transform.DOScale(0.55f, 0.8f).From(0.4f).SetEase(Ease.OutQuart);
        cubeGameReferImage.transform.DOScale(0.55f, 0.8f).From(0.4f).SetEase(Ease.OutQuart);
        wingTtoReferImage.transform.DOScale(0.55f, 0.8f).From(0.4f).SetEase(Ease.OutQuart);

        matchCardGameReferImage.DOFade(0, 0.8f).SetEase(Ease.Linear);
        wordGameReferImage.DOFade(0, 0.8f).SetEase(Ease.Linear);
        cubeGameReferImage.DOFade(0, 0.8f).SetEase(Ease.Linear);
        wingTtoReferImage.DOFade(0, 0.8f).SetEase(Ease.Linear);

        await UniTask.Delay(1000);

        SceneMoveManager.instance.MoveScene(SceneName.YMAFinalQuiz);
    }

    public void OnClickShowWarningMiniPopup(bool show)
    {
        if (show)
        {
            warningMiniPopup.gameObject.SetActive(show);
            warningMiniPopup.alpha = 0;
            warningMiniPopup.DOFade(1, 0.8f).SetEase(Ease.OutQuart);
            warningMiniPopup.transform.Find("blurDim")?.GetComponent<BlurManager>()?.BlurInAnim();
        }
        else
        {
            warningMiniPopup.alpha = 1;
            warningMiniPopup.DOFade(0, 0.4f).SetEase(Ease.OutQuart).OnComplete(() => warningMiniPopup.gameObject.SetActive(show));
            warningMiniPopup.transform.Find("blurDim")?.GetComponent<BlurManager>()?.BlurOutAnim();
        }
    }
}

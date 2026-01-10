using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using System;
using System.Text;


public class FinalQuizManager : MonoBehaviour
{
    public static FinalQuizManager instance { get; private set; }

    public FinalQuizUIManager uiManager => FinalQuizUIManager.instance;

    private FinalQuizPlayData finalQuizPlayData => SaveDataManager.instance.playerData.finalQuizPlayData;
    private List<FinalQuizMetaData> finalQuizMetaDataList = new();
    private FinalQuizMetaData currentQuizData;


    private FinalGameState gameState = FinalGameState.Intro;
    private int currentQuizIndex = 0;
    private int maxWrongCount = 0;
    private int wrongCount = 0;




    public float QuizTotalLeftTimeMax => quizTotalLeftTimeMax;
    public float QuizLeftTimeMax => quizLeftTimeMax;

    private float quizTotalLeftTimeMax = 0;
    private float quizLeftTimeMax = 0;
    private float quizTotalLeftTime = 0;
    private float quizLeftTime = 0;
    private float phase2PlusTime = 0;









    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        finalQuizMetaDataList = GameResourceManager.instance.GetAllFinalQuizMetaData();
        currentQuizIndex = 0;
        currentQuizData = GetQuizData(currentQuizIndex);

        uiManager.ShowSceneMoveAnimation(true);

        gameState = FinalGameState.Intro;
        StartStateProcess();
    }


    void Update()
    {
        // #if UNITY_EDITOR && DEV
        //         if (Input.GetKeyDown(KeyCode.C))
        //         {
        //             HLLogger.Log($"force {gameState} state clear");

        //             SetNextState();
        //             StartStateProcess();
        //         }
        // #endif

        if (gameState == FinalGameState.Ready)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SetState(FinalGameState.Phase1);
                StartStateProcess();
            }
        }


        if (gameState == FinalGameState.Phase1 || gameState == FinalGameState.Phase2)
        {
            quizTotalLeftTime -= Time.deltaTime;
            if (gameState == FinalGameState.Phase2)
                quizLeftTime -= Time.deltaTime;

            uiManager.UpdateTimerText(quizTotalLeftTime, quizLeftTime);
            if (quizTotalLeftTime < 0 || quizLeftTime < 0)
            {
                if (quizTotalLeftTime < 0) quizTotalLeftTime = 0;
                if (quizLeftTime < 0) quizLeftTime = 0;

                uiManager.UpdateTimerText(quizTotalLeftTime, quizLeftTime);
                SetState(FinalGameState.GameOver);
                StartStateProcess();
            }
        }

    }



    public void SetState(FinalGameState state)
    {
        gameState = state;
    }
    public void SetNextState()
    {
        if (gameState == FinalGameState.Ending || gameState == FinalGameState.GameResult)
            return;

        gameState += 1;
    }

    public void StartStateProcess()
    {
        HLLogger.Log($"gameState Action - {gameState}");
        switch (gameState)
        {
            case FinalGameState.Intro:
                IntroProcess();
                break;
            case FinalGameState.Ready:
                ReadyProcess();
                break;
            case FinalGameState.Phase1:
                Phase1Process();
                break;
            case FinalGameState.Phase1Complete:
                Phase1FinishProcess();
                break;
            case FinalGameState.Phase2:
                Phase2Process();
                break;
            case FinalGameState.Phase2Complete:
                Phase2FinishProcess();
                break;
            case FinalGameState.Ending:
                EndingProcess();
                break;
            case FinalGameState.GameOver:
                GameOverProcess();
                break;
            case FinalGameState.GameResult:
                GameResultProcess();
                break;
        }
    }

    private void IntroProcess()
    {
        var tryCount = finalQuizPlayData.tryCount;

        currentQuizIndex = 0;
        maxWrongCount = tryCount;
        wrongCount = 0;

        quizTotalLeftTime = 15f + tryCount * 5f;
        quizLeftTime = 2f + tryCount;
        phase2PlusTime = 5f + tryCount * 5f;

        quizTotalLeftTimeMax = quizTotalLeftTime;
        quizLeftTimeMax = quizTotalLeftTime;


        uiManager.SetIntroUI();
        uiManager.SetIntroTextData(tryCount, quizTotalLeftTimeMax);
        uiManager.UpdateMaxTime();
        uiManager.IntroUIAnimation().Forget();
    }

    private void ReadyProcess()
    {

    }

    private void Phase1Process()
    {
        uiManager.SetPhase1UI();
        uiManager.UpdateTimerText(quizTotalLeftTime, quizLeftTime);
        uiManager.ShowQuizItem(GetQuizData());
        SoundManager.instance.PlayBGM(BGMType.FinalQuizPhase1);
    }

    private void Phase1FinishProcess()
    {
        HLLogger.Log("@@@ phase 1 complete");


        uiManager.FadeOutQuizItem(GetQuizData());
        uiManager.ShowPhase1CompleteAnimation();
        SoundManager.instance.StopBGM();
    }

    private void Phase2Process()
    {
        SoundManager.instance.PlayBGM(BGMType.FinalQuizPhase2);

    }

    private void Phase2FinishProcess()
    {
        SoundManager.instance.StopBGM();

    }

    private void EndingProcess()
    {

    }


    private void GameOverProcess()
    {

    }

    private void GameResultProcess()
    {

    }






    #region Data

    public FinalQuizMetaData GetQuizData(int index)
    {
        if (index < 0 || index >= finalQuizMetaDataList.Count)
            return null;

        return finalQuizMetaDataList[index];
    }

    public FinalQuizMetaData GetQuizData()
    {
        return GetQuizData(currentQuizIndex);
    }




    public void CheckIsAnswer(int answer)
    {
        if (gameState != FinalGameState.Phase1 && gameState != FinalGameState.Phase2)
        {
            HLLogger.Log("game over! dont touch");
            return;
        }

        if (currentQuizData.answer != 0 && currentQuizData.answer == answer)
        {
            //오답
            HLLogger.Log($"wrong answer - {wrongCount} / {maxWrongCount}");

            uiManager.XAnimation();
            wrongCount++;
            if (wrongCount >= maxWrongCount)
            {
                SetState(FinalGameState.GameOver);
                StartStateProcess();
                return;
            }
        }
        else
        {
            //정답
            uiManager.OAnimation();
        }

        SetNextQuiz();
    }

    public void SetNextQuiz()
    {
        currentQuizIndex++;

        HLLogger.Log($"next Quiz - {currentQuizIndex}");

        if (currentQuizIndex == 10)
        {
            SetState(FinalGameState.Phase1Complete);
            StartStateProcess();
            return;
        }
        else if (currentQuizIndex == 20)
        {
            SetState(FinalGameState.Phase2Complete);
            StartStateProcess();
            return;
        }

        currentQuizData = GetQuizData(currentQuizIndex);

        uiManager.ShowQuizItem(currentQuizData);
        uiManager.FadeOutQuizItem(currentQuizData);
    }

    #endregion







    public void MoveScene()
    {
        SceneMoveManager.instance.MoveScene(SceneName.LobbyScene, true);
    }


}


public enum FinalGameState
{
    Intro,
    Ready, // 시작 준비
    Phase1,
    Phase1Complete, //페이즈 1 성공
    Phase2,
    Phase2Complete, //페이즈 2 성공
    Ending,
    GameOver, // 게임 실패
    GameResult, // 게임 결과 노출
}
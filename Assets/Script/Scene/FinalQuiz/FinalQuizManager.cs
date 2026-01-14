using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;


public class FinalQuizManager : MonoBehaviour
{
    public static FinalQuizManager instance { get; private set; }

    public FinalQuizUIManager UiManager => FinalQuizUIManager.instance;

    private FinalQuizPlayData FinalQuizPlayData => SaveDataManager.instance.playerData.finalQuizPlayData;
    private List<FinalQuizMetaData> finalQuizMetaDataList = new();
    private FinalQuizMetaData currentQuizData;


    private FinalGameState gameState = FinalGameState.Intro;
    private int currentQuizIndex = 0;
    private int maxWrongCount = 0;
    private int wrongCount = 0;
    public int WrongCount => wrongCount;
    public float PlayTime => quizTotalLeftTimeMax - quizTotalLeftTime;




    public float QuizTotalLeftTimeMax => quizTotalLeftTimeMax;
    public float QuizLeftTimeMax => quizLeftTimeMax;
    public (float, float) GetPhase1ClearQuizTime()
    {
        return (quizTotalLeftTime, phase2PlusTime);
    }

    private float quizTotalLeftTimeMax = 0;
    private float quizLeftTimeMax = 0;
    private float quizTotalLeftTime = 0;
    private float quizLeftTime = 0;
    private float quizLeftTimeImageFillAmount = 0;
    private float phase2PlusTime = 0;
    private int lastCountdownTime = -1;










    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        finalQuizMetaDataList = GameResourceManager.instance.GetAllFinalQuizMetaData();
        currentQuizIndex = 0;
        currentQuizData = GetQuizData(currentQuizIndex);

        UiManager.ShowSceneMoveAnimation(true);

        gameState = FinalGameState.Intro;
        StartStateProcess();
    }


    void Update()
    {
#if UNITY_EDITOR && DEV
        // if (Input.GetKeyDown(KeyCode.C))
        // {
        //     HLLogger.Log($"force {gameState} state clear");

        //     SetNextState();
        //     StartStateProcess();
        // }

        if (Input.GetKeyDown(KeyCode.M))
        {
            quizTotalLeftTime += 10f;
            quizLeftTime += 10f;

            HLLogger.Log($"+10 / quizTotalLeftTime : {quizTotalLeftTime}, quizLeftTime : {quizLeftTime}");
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            quizLeftTime -= 1f;
            HLLogger.Log($"-1 / quizLeftTime : {quizLeftTime}");
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            quizTotalLeftTime -= 10f;
            HLLogger.Log($"-10 / quizTotalLeftTime : {quizTotalLeftTime}");
        }
#endif

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
            {
                quizLeftTime -= Time.deltaTime;
                quizLeftTimeImageFillAmount = quizLeftTime / quizLeftTimeMax;
            }



            UiManager.UpdateTimerText(quizTotalLeftTime, quizLeftTime, quizLeftTimeImageFillAmount);
            if (quizTotalLeftTime < 0 || quizLeftTime < 0)
            {
                if (quizTotalLeftTime < 0) quizTotalLeftTime = 0;
                if (quizLeftTime < 0)
                {
                    quizLeftTime = 0;
                    quizLeftTimeImageFillAmount = 0;
                }

                UiManager.UpdateTimerText(quizTotalLeftTime, quizLeftTime, quizLeftTimeImageFillAmount);
                SetState(FinalGameState.GameOver);
                StartStateProcess();
                return;
            }

            //10초 카운트
            if (quizTotalLeftTime <= 10.0f && quizTotalLeftTime > 0f)
            {
                int currentIntTime = Mathf.CeilToInt(quizTotalLeftTime);
                if (currentIntTime != lastCountdownTime)
                {
                    lastCountdownTime = currentIntTime;
                    UiManager.ShowCountdown(lastCountdownTime);
                }
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
        HLLogger.Log($"@@@ GameState Action - {gameState}", LogColor.cyan);
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
        var tryCount = FinalQuizPlayData.tryCount;

        currentQuizIndex = 0;
        maxWrongCount = tryCount;
        wrongCount = 0;

        quizTotalLeftTime = 15f + tryCount * 5f;
        quizLeftTime = 2f + tryCount;
        phase2PlusTime = 5f + tryCount * 5f;
        lastCountdownTime = -1;

        quizTotalLeftTimeMax = quizTotalLeftTime;
        quizLeftTimeMax = quizLeftTime;

        HLLogger.Log($"quizTotalLeftTimeMax : {quizTotalLeftTimeMax} / quizLeftTimeMax : {quizLeftTimeMax} / phase2PlusTime : {phase2PlusTime}");


        UiManager.SetIntroUI();
        UiManager.SetIntroTextData(tryCount, quizTotalLeftTimeMax);
        UiManager.UpdateMaxTime();
        UiManager.IntroUIAnimation().Forget();
    }

    private void ReadyProcess()
    {

    }

    private void Phase1Process()
    {
        UiManager.SetPhase1UI();
        UiManager.UpdateTimerText(quizTotalLeftTime, quizLeftTime, quizLeftTimeImageFillAmount);
        UiManager.ShowQuizItem(GetQuizData());
        SoundManager.instance.PlayBGM(BGMType.FinalQuizPhase1);
    }

    private void Phase1FinishProcess()
    {
        HLLogger.Log("@@@ phase 1 complete");

        UiManager.UpdateQuizProgress(10);
        UiManager.FadeOutQuizItem(GetQuizData());
        UiManager.ShowPhase1CompleteAnimation().Forget();
        SoundManager.instance.StopBGM();
    }

    private void Phase2Process()
    {
        quizTotalLeftTime += phase2PlusTime;

        UiManager.UpdateQuizProgress(0, true);
        UiManager.UpdateTimerText(quizTotalLeftTime, quizLeftTime, quizLeftTimeImageFillAmount);
        UiManager.ShowQuizItem(GetQuizData());
        SoundManager.instance.PlayBGM(BGMType.FinalQuizPhase2);
    }

    private void Phase2FinishProcess()
    {
        UiManager.FadeOutQuizItem(currentQuizData);
        UiManager.UpdateQuizProgress(10);
        SoundManager.instance.StopBGM();

    }

    private void EndingProcess()
    {

    }


    private void GameOverProcess()
    {
        SoundManager.instance.StopBGM();
        UiManager.ShowGameOverAnimation();
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

        quizLeftTime += 0.2f;
        if (currentQuizData.answer != 0 && currentQuizData.answer != answer)
        {
            //오답

            UiManager.XAnimation().Forget();
            wrongCount++;
            UiManager.UpdateWrongCount(wrongCount);
            HLLogger.Log($"wrong answer - {wrongCount} / {maxWrongCount}");
            if (wrongCount >= maxWrongCount)
            {
                SetState(FinalGameState.GameOver);
                StartStateProcess();
                return;
            }
            UiManager.UpdateQuizProgress(currentQuizIndex + 1);
        }
        else
        {
            //정답
            UiManager.OAnimation().Forget();
            UiManager.UpdateQuizProgress(currentQuizIndex + 1);
        }

        quizLeftTime = quizLeftTimeMax;
        DOVirtual.DelayedCall(0.2f, () => { SetNextQuiz(); });
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

        UiManager.FadeOutQuizItem(currentQuizData);
        UiManager.ShowQuizItem(currentQuizData);
    }


    public void AddPhase2Time()
    {
        quizTotalLeftTime += phase2PlusTime;
    }

    #endregion







    public void MoveLobbyScene()
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
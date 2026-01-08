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

    private List<FinalQuizMetaData> finalQuizMetaDataList = new();
    private int currentQuizIndex = 0;
    private FinalQuizMetaData currentQuizData;
    private int wrongCount = 0;

    private int maxWrongCount => SaveDataManager.instance.playerData.finalQuizPlayData.tryCount;



    private FinalGameState gameState = FinalGameState.Intro;

    private float timer = 0;









    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        finalQuizMetaDataList = GameResourceManager.instance.GetAllFinalQuizMetaData();
        currentQuizIndex = 0;
        currentQuizData = GetQuizData(currentQuizIndex);

        FinalQuizUIManager.instance.ShowSceneMoveAnimation(true);
        SoundManager.instance.PlayBGM(BGMType.CubeGame);

        gameState = FinalGameState.Intro;
        StartStateProcess();
    }


    void Update()
    {
#if UNITY_EDITOR && DEV
        if (Input.GetKeyDown(KeyCode.C))
        {
            HLLogger.Log($"force clear");

            SetNextState();
            StartStateProcess();
        }
#endif

        if (gameState == FinalGameState.Phase1)
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                timer = 0;
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
        gameState += 1;
    }

    public void StartStateProcess()
    {
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
        FinalQuizUIManager.instance.IntroUIAnimation().Forget();
    }

    private void ReadyProcess()
    {

    }

    private void Phase1Process()
    {

    }

    private void Phase1FinishProcess()
    {

    }

    private void Phase2Process()
    {

    }

    private void Phase2FinishProcess()
    {

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
        if (currentQuizData.answer == answer)
        {
            wrongCount++;
            // if (quizdata)

        }

        SetNextQuiz();
    }

    public void SetNextQuiz()
    {
        currentQuizIndex++;
        if (currentQuizIndex >= finalQuizMetaDataList.Count)
        {
            SetState(FinalGameState.GameOver);
            StartStateProcess();
            return;
        }

        currentQuizData = GetQuizData(currentQuizIndex);
    }

    #endregion


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
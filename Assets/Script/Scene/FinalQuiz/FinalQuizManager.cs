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






    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        finalQuizMetaDataList = GameResourceManager.instance.GetAllFinalQuizMetaData();
        currentQuizIndex = 0;
        currentQuizData = GetQuizData(currentQuizIndex);

        StartProcess().Forget();
    }

    private async UniTaskVoid StartProcess()
    {
        FinalQuizUIManager.instance.ShowSceneMoveAnimation(true);
        SoundManager.instance.PlayBGM(BGMType.CubeGame);

        FinalQuizUIManager.instance.StartUIAnimation().Forget();
    }


    void Update()
    {
#if UNITY_EDITOR && DEV
        if (Input.GetKeyDown(KeyCode.C))
        {
            HLLogger.Log($"force clear");

            FinishProcess(0).Forget();
        }
#endif
    }


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
            FinishProcess().Forget();
            return;
        }

        currentQuizData = GetQuizData(currentQuizIndex);
    }






    private async UniTask FinishProcess(int delayTime = 3000)
    {
        // 3초 딜레이
        await UniTask.Delay(delayTime);

        //1회만에 바로 맞춘경우 15000. 1회 틀릴때마다 500씩 감소. 최소 12000
        // int earnCoinAmount = 0;
        // if (delayTime < 3000) //강제 치트를 쓴 경우
        //     earnCoinAmount = 30000;
        // else
        //     earnCoinAmount = (hints.Count - currentHintIndex) * 1000 + 10000;

        // QuestManager.instance.AddFindAIWordData(1, currentHintIndex < 3 ? 1 : 0);
        // FindAIWordGameUIManager.instance.ShowResult(currentHintIndex + 1, earnCoinAmount);

        // if (SaveDataManager.instance.AddExp(1))
        // {
        //     await UniTask.Delay(1500);
        //     FindAIWordGameUIManager.instance.ShowLevelUpPopup();
        // }
    }
}

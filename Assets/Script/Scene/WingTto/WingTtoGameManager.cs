using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using System;
using System.Text;


public class WingTtoGameManager : MonoBehaviour
{
    public static WingTtoGameManager instance { get; private set; }



    // private
    private int score;
    private int randomSeed;
    private System.Random randomGenerator;


    private InGameState _inGameState;
    public InGameState inGameState => _inGameState;
    private Queue<Action> UpdateActionQueue = new();





    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        score = 0;
        _inGameState = InGameState.Ready;

        randomSeed = UnityEngine.Random.Range(0, 100000);
        randomGenerator = new System.Random(randomSeed);
        HLLogger.Log($"Random Seed: {randomSeed}");

        StartProcess().Forget();
    }

    private async UniTaskVoid StartProcess()
    {
        WingTtoGameUIManager.instance.ShowSceneMoveAnimation(true);
        SoundManager.instance.PlayBGM(BGMType.CubeGame);
        await UniTask.Delay(2000);
        WingTtoGameUIManager.instance.ShowDim(false);


        _inGameState = InGameState.Play;
    }

    void Update()
    {
        if (inGameState == InGameState.Play && Input.GetKeyDown(KeyCode.Escape))
        {
            if (WingTtoGameUIManager.instance.IsPopupOpen())
            {
                WingTtoGameUIManager.instance.CloseTopPopup();
                SetPause(false);
            }
            else
            {
                WingTtoGameUIManager.instance.ShowPausePopup();
                SetPause(true);
            }
        }


        if (_inGameState == InGameState.Play)
        {

        }

        if (UpdateActionQueue.Count > 0)
        {
            UpdateActionQueue.Dequeue().Invoke();
        }



#if UNITY_EDITOR && DEV
        // c를 사용할 수 있어서 이 게임만 ctrl과 조합.
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
        {
            HLLogger.Log($"force clear");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (_inGameState == InGameState.Play)
                _inGameState = InGameState.Pause;
            else if (_inGameState == InGameState.Pause)
                _inGameState = InGameState.Play;
        }
#endif
    }


    private async UniTaskVoid TimeOverProcess()
    {
        HLLogger.Log("Time Over");
        WingTtoGameUIManager.instance.ShowDim(true, "게임 종료!");
        await UniTask.Delay(2000);

        FinishProcess().Forget();
    }


    private async UniTaskVoid FinishProcess()
    {
        //TODO 계산식 변경 필요
        int exp = (int)Math.Pow(score / 2500f, 0.7f);
        exp = Math.Max(1, exp);

        int earnCoinAmount = (int)(30000 * (1 - Math.Exp(-score / 40000f)));
        earnCoinAmount = Math.Max(1000, earnCoinAmount);

        //@@@ 임시
        exp = 0;
        earnCoinAmount = 0;

        HLLogger.Log($"Score : {score} / coin : {earnCoinAmount} / exp : {exp}");

        SaveDataManager.instance.AddCoin(earnCoinAmount, false);

        //@@@ 임시
        WingTtoGameUIManager.instance.ShowResult();

        if (SaveDataManager.instance.AddExp(exp))
        {
            await UniTask.Delay(1500);
            WingTtoGameUIManager.instance.ShowLevelUpPopup();
        }
    }





    public void SetPause(bool isPause)
    {
        _inGameState = isPause ? InGameState.Pause : InGameState.Play;
    }



    public float GetRandomFloat(float min, float max)
    {
        return min + (float)randomGenerator.NextDouble() * (max - min);
    }
}

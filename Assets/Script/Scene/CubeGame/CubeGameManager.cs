using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using System;
using System.Text;


public class CubeGameManager : MonoBehaviour
{
    public static CubeGameManager instance { get; private set; }
    public List<CubeGameCube> cubeList = new();



    // private
    private int score;
    private float leftTime;
    private float maxTime;
    private int randomSeed;
    private System.Random randomGenerator;


    private InGameState _inGameState;
    public InGameState inGameState => _inGameState;

    private Dictionary<CubeState, int> countDic = new();

    private Queue<Action> UpdateActionQueue = new();





    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        score = 0;
        leftTime = 30f;
        maxTime = 30f;
        _inGameState = InGameState.Ready;
        countDic.Clear();

        randomSeed = UnityEngine.Random.Range(0, 100000);
        if (StaticGameData.useServerSeed)
            randomSeed = StaticGameData.serverGameSeed;

        randomGenerator = new System.Random(randomSeed);
        HLLogger.Log($"Random Seed: {randomSeed}");

        foreach (CubeState state in Enum.GetValues(typeof(CubeState)))
        {
            countDic.Add(state, 0);
        }
        cubeList[0].SetKeyCode(KeyCode.Q);
        cubeList[1].SetKeyCode(KeyCode.W);
        cubeList[2].SetKeyCode(KeyCode.E);

        cubeList[3].SetKeyCode(KeyCode.A);
        cubeList[4].SetKeyCode(KeyCode.S);
        cubeList[5].SetKeyCode(KeyCode.D);

        cubeList[6].SetKeyCode(KeyCode.Z);
        cubeList[7].SetKeyCode(KeyCode.X);
        cubeList[8].SetKeyCode(KeyCode.C);

        StartProcess().Forget();
    }

    private async UniTaskVoid StartProcess()
    {
        CubeGameUIManager.instance.ShowSceneMoveAnimation(true);
        SoundManager.instance.PlayBGM(BGMType.CubeGame);

        CubeGameUIManager.instance.UpdateLeftTimeText(leftTime);
        CubeGameUIManager.instance.UpdateScoreText(score);
        CubeGameUIManager.instance.UpdateCubeCountText(countDic);
        CubeGameUIManager.instance.InitCountUI();
        CubeGameUIManager.instance.ShowDim(true, "준비!");
        await UniTask.Delay(2000);
        CubeGameUIManager.instance.ShowDim(false);


        _inGameState = InGameState.Play;
        foreach (var cube in cubeList)
        {
            (float wait, float tooFast, float fast, float perfect, float slow, float tooSlow) = MakeStateTime();
            cube.StartCube(wait, tooFast, fast, perfect, slow, tooSlow);
        }
    }

    void Update()
    {
        if (inGameState == InGameState.Play && Input.GetKeyDown(KeyCode.Escape))
        {
            if (CubeGameUIManager.instance.IsPopupOpen())
            {
                CubeGameUIManager.instance.CloseTopPopup();
                SetPause(false);
            }
            else
            {
                CubeGameUIManager.instance.ShowPausePopup();
                SetPause(true);
            }
        }


        if (_inGameState == InGameState.Play)
        {
            leftTime -= Time.deltaTime;
            CubeGameUIManager.instance.UpdateLeftTimeText(leftTime);

            if (leftTime <= 0f)
            {
                leftTime = 0f;
                CubeGameUIManager.instance.UpdateLeftTimeText(leftTime);
                _inGameState = InGameState.Finish;
                TimeOverProcess().Forget();
            }
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
            leftTime = 0f;
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
        CubeGameUIManager.instance.ShowDim(true, "게임 종료!");
        foreach (var cube in cubeList) cube.StopCube();
        await UniTask.Delay(2000);

        FinishProcess().Forget();
    }


    private async UniTaskVoid FinishProcess()
    {
        int exp = (int)Math.Pow(score / 2500f, 0.7f);
        exp = Math.Max(1, exp);

        int earnCoinAmount = (int)(30000 * (1 - Math.Exp(-score / 40000f)));
        earnCoinAmount = Math.Max(1000, earnCoinAmount);

        HLLogger.Log($"Score : {score} / coin : {earnCoinAmount} / exp : {exp}");

        SaveDataManager.instance.AddCoin(earnCoinAmount, false);
        CubeGameUIManager.instance.ShowResult(score, earnCoinAmount, exp);

        if (SaveDataManager.instance.AddExp(exp))
        {
            await UniTask.Delay(1500);
            CubeGameUIManager.instance.ShowLevelUpPopup();
        }
    }





    public void SetPause(bool isPause)
    {
        _inGameState = isPause ? InGameState.Pause : InGameState.Play;
    }


    public void CubeClickProcess(CubeGameCube cube, CubeState state)
    {
        if (inGameState != InGameState.Play) return;

        // 점수, 시간 처리.
        int addScore = state switch
        {
            CubeState.TooFast => -100,
            CubeState.Fast => 100,
            CubeState.Perfect => 300,
            CubeState.Slow => 100,
            CubeState.TooSlow => -100,

            _ => 0,
        };

        float addTime = state switch
        {
            CubeState.TooFast => -0.5f,
            CubeState.Fast => 0f,
            CubeState.Perfect => 0.5f,
            CubeState.Slow => 0f,
            CubeState.TooSlow => -0.5f,

            _ => 0f,
        };

        score += addScore;
        score = Math.Max(0, score);
        leftTime += addTime;
        leftTime = Math.Min(maxTime, leftTime);
        countDic[state]++;

        CubeGameUIManager.instance.UpdateScoreText(score);
        CubeGameUIManager.instance.UpdateCubeCountText(state, countDic[state]);

        if (_inGameState == InGameState.Play)
        {
            (float wait, float tooFast, float fast, float perfect, float slow, float tooSlow) = MakeStateTime();
            cube.StartCube(wait, tooFast, fast, perfect, slow, tooSlow);
        }
    }

    private (float wait, float tooFast, float fast, float perfect, float slow, float tooSlow) MakeStateTime()
    {
        float wait = GetRandomFloat(0.5f, 3.5f);

        float too = GetRandomFloat(0.3f, 0.6f);
        float middle = GetRandomFloat(0.4f, 0.6f);
        float perfect = GetRandomFloat(0.5f, 0.6f);

        return (wait, too, middle, perfect, middle, too);
    }

    public float GetRandomFloat(float min, float max)
    {
        return min + (float)randomGenerator.NextDouble() * (max - min);
    }
}

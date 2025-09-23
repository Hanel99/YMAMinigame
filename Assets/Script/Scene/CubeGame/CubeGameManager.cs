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
        leftTime = 0f;
        _inGameState = InGameState.Ready;
        countDic.Clear();

        foreach (CubeState state in Enum.GetValues(typeof(CubeState)))
        {
            countDic.Add(state, 0);
        }
        cubeList[0].SetKeyCode(KeyCode.Q);
        // cubeList[1].SetKeyCode(KeyCode.W);
        // cubeList[2].SetKeyCode(KeyCode.E);

        // cubeList[3].SetKeyCode(KeyCode.A);
        // cubeList[4].SetKeyCode(KeyCode.S);
        // cubeList[5].SetKeyCode(KeyCode.D);

        // cubeList[6].SetKeyCode(KeyCode.Z);
        // cubeList[7].SetKeyCode(KeyCode.X);
        // cubeList[8].SetKeyCode(KeyCode.C);

        StartProcess().Forget();
    }

    private async UniTaskVoid StartProcess()
    {
        CubeGameUIManager.instance.ShowSceneMoveAnimation(true);
        SoundManager.instance.PlayBGM(BGMType.CubeGame);

        leftTime = 60f;
        CubeGameUIManager.instance.UpdateLeftTimeText(leftTime);
        CubeGameUIManager.instance.UpdateScoreText(score);
        CubeGameUIManager.instance.UpdateCubeCountText(countDic);
        await UniTask.Delay(3000);

        _inGameState = InGameState.Play;
        foreach (var cube in cubeList)
        {
            (float idle, float good, float great, float perfect, float bad, float miss) = MakeStateTime();
            cube.StartCube(idle, good, great, perfect, bad, miss);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
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
                TimeOverProcess();
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


    private void TimeOverProcess()
    {
        foreach (var cube in cubeList) cube.StopCube();
        // UniTask.DelayFrame(2);
        // foreach (var cube in cubeList) cube.StopCube();
        // 안정을 위해 한번 더...

        UniTask.WaitForSeconds(3000);

        FinishProcess();
    }


    private void FinishProcess()
    {
        int earnCoinAmount = score * 10;
        int exp = Math.Max(1, score / 100);
        HLLogger.Log($"coin : {earnCoinAmount}, exp : {exp}");

        SaveDataManager.instance.AddCoin(earnCoinAmount, false);
        CubeGameUIManager.instance.ShowResult(1, earnCoinAmount);
        //TODO 전용 result 팝업 만들것

        if (SaveDataManager.instance.AddExp(exp))
        {
            DOVirtual.DelayedCall(1.5f, () => CardGameUIManager.instance.ShowLevelUpPopup());
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
            CubeState.Perfect => 3,
            CubeState.Great => 2,
            CubeState.Good => 1,
            CubeState.Bad => 0,
            CubeState.Miss => -1,

            _ => 0,
        };

        score += addScore;
        score = Math.Max(0, score);
        countDic[state]++;

        if (state == CubeState.Perfect)
            leftTime += 1f;

        CubeGameUIManager.instance.UpdateScoreText(score);
        CubeGameUIManager.instance.UpdateCubeCountText(countDic);

        if (_inGameState == InGameState.Play)
        {
            HLLogger.Log("@@@ re start cube");

            (float idle, float good, float great, float perfect, float bad, float miss) = MakeStateTime();
            cube.StartCube(idle, good, great, perfect, bad, miss);
        }
    }

    private (float idle, float good, float great, float perfect, float bad, float miss) MakeStateTime()
    {
        float idle = 1f;
        float good = 1f;
        float great = 1f;
        float perfect = 1f;
        float bad = 1f;
        float miss = 1f;

        return (idle, good, great, perfect, bad, miss);
    }
}

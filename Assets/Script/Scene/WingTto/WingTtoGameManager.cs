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


    public WingTtoPlayer player;


    //private
    private WingTtoObjectPool pool => WingTtoObjectPool.instance;
    private List<WingTtoObject> spawnObjectList = new();




    //object 움직임 속도
    private float _normalSpeed = 6f;
    private float _specialSpeed = 9f;
    public float normalSpeed => _normalSpeed;
    public float specialSpeed => _specialSpeed;


    // 날아간 거리 계산
    private float distanceMultiplier = 3.33f;
    private float currentDistance = 0f;
    private float startTime;


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
        // SoundManager.instance.PlayBGM(BGMType.WingTto);

        string dimText = "이 게임은 가로로 플레이 하는 게임입니다.";
#if UNITY_ANDROID
        dimText += "\n\n화면을 가로로 잡고 플레이 해 주세요";
#endif
        WingTtoGameUIManager.instance.ShowDim(false, dimText);

        await UniTask.Delay(1000);
        _inGameState = InGameState.Play;
        startTime = Time.time;
        player.StartProcess();
        HLLogger.Log("game start");
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


        if (Input.GetKeyDown(KeyCode.Space))
        {
            //@@@ test
            WingTtoObject obj = pool.GetObject(WingTtoObjectType.Stone);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            //@@@ test
            WingTtoObject obj = pool.GetObject(WingTtoObjectType.Gimbab);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            //@@@ test
            WingTtoObject obj = pool.GetObject(WingTtoObjectType.Wall);
        }


        if (_inGameState == InGameState.Play)
        {
            UpdateDistance(Time.deltaTime);
            RandomSpawnObject(Time.deltaTime);
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



    #region Calc Distance

    private void UpdateDistance(float deltaTime)
    {
        currentDistance += deltaTime * (Input.GetMouseButton(1) ? specialSpeed : normalSpeed) * distanceMultiplier;
    }

    public string GetFormattedDistance()
    {
        return $"{currentDistance:0f}m";
    }

    // 정수 거리 반환
    public int GetDistance()
    {
        return Mathf.FloorToInt(currentDistance);
    }

    #endregion




    #region Spawn Process

    float temp = 10f;
    private void RandomSpawnObject(float deltaTime)
    {
        temp -= deltaTime;
        if (temp <= 0)
        {
            temp = 10f;
            WingTtoObject obj = pool.GetObject(WingTtoObjectType.Wall);
        }
    }

    public void AddSpawnObject(WingTtoObject obj)
    {
        spawnObjectList.Add(obj);
    }

    public void RemoveSpawnObject(WingTtoObject obj)
    {
        spawnObjectList.Remove(obj);
    }




    #endregion





    public void SetPause(bool isPause)
    {
        _inGameState = isPause ? InGameState.Pause : InGameState.Play;

        foreach (WingTtoObject obj in spawnObjectList)
        {
            obj?.SetPause(isPause);
        }
    }



    public float GetRandomFloat(float min, float max)
    {
        return min + (float)randomGenerator.NextDouble() * (max - min);
    }
}

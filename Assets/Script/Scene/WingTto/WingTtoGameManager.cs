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
    private bool isClickRight = false;
    private float speedUpMultiplier = 1.5f;
    private float _normalSpeed = 6f;
    public float normalSpeed => _normalSpeed;


    // 날아간 거리 계산
    private float distanceMultiplier = 0.9f;
    private float currentDistance = 0f;


    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        score = 0;
        _inGameState = InGameState.Ready;

        randomSeed = UnityEngine.Random.Range(0, 100000);
        randomSeed = 0;
        //@@@ 임시 설정

        randomGenerator = new System.Random(randomSeed);
        HLLogger.Log($"Random Seed: {randomSeed}");

        StartProcess();
    }

    private void StartProcess()
    {
        WingTtoGameUIManager.instance.ShowSceneMoveAnimation(true);
        SoundManager.instance.PlayBGM(BGMType.WingTto);

        string dimText = "이 게임은 가로로 플레이 하는 게임입니다.";
#if UNITY_ANDROID
        dimText += "\n\n화면을 가로로 잡고 플레이 해 주세요";
#endif
        WingTtoGameUIManager.instance.ShowDim(false, dimText);
    }

    public void OnClickStartButton()
    {
        ClickStartProcess().Forget();
    }

    public async UniTaskVoid ClickStartProcess()
    {
        HLLogger.Log("getset...");
        _inGameState = InGameState.GetSet;
        player.StartProcess();

        WingTtoGameUIManager.instance.SetGetSetText("3");
        await UniTask.Delay(600);
        WingTtoGameUIManager.instance.SetGetSetText("2");
        await UniTask.Delay(600);
        WingTtoGameUIManager.instance.SetGetSetText("1");
        await UniTask.Delay(600);
        WingTtoGameUIManager.instance.SetGetSetText("GO!!");
        await UniTask.Delay(600);
        WingTtoGameUIManager.instance.SetGetSetText("");

        _inGameState = InGameState.Play;
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


        if (_inGameState == InGameState.Play && _inGameState == InGameState.GetSet)
        {
            // PC - 마우스 입력
            if (Input.GetMouseButton(1))
            {
                isClickRight = true;
            }

            // 모바일 - 터치 입력 (마우스 입력 덮어쓰기)
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began ||
                    touch.phase == TouchPhase.Stationary ||
                    touch.phase == TouchPhase.Moved)
                {
                    // 화면 절반 기준으로 좌우 구분
                    float screenHalfWidth = Screen.width / 2f;

                    if (touch.position.x > screenHalfWidth)
                    {
                        // 오른쪽 화면 
                        isClickRight = true;
                    }
                }
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
            SetGameOver();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (_inGameState == InGameState.Play)
                SetPause(true);
            else if (_inGameState == InGameState.Pause)
                SetPause(false);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            player.CollisionOnOff(true);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            player.CollisionOnOff(false);
        }
#endif
    }

    void FixedUpdate()
    {
        if (_inGameState == InGameState.Play)
        {
            UpdateDistance(Time.deltaTime);
            RandomSpawnObject(Time.deltaTime);
        }
    }



    public void SetGameOver()
    {
        SetPause(true);
        GameOverProcess().Forget();
    }

    private async UniTaskVoid GameOverProcess()
    {
        HLLogger.Log("Game Over");
        WingTtoGameUIManager.instance.ShowDim(true, "게임 종료!");
        await UniTask.Delay(2000);

        FinishProcess().Forget();
    }


    private async UniTaskVoid FinishProcess()
    {
        //TODO 계산식 변경 필요
        // int exp = (int)Math.Pow(score / 2500f, 0.7f);
        // exp = Math.Max(1, exp);

        // int earnCoinAmount = (int)(30000 * (1 - Math.Exp(-score / 40000f)));
        // earnCoinAmount = Math.Max(1000, earnCoinAmount);

        //@@@ 임시
        int exp = 0;
        int earnCoinAmount = 0;

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

    float tempSpeed = 0f;
    int currentPhase = 0;
    private float[] phaseBorder = { 100f, 250f, 500f, 750f, 1000f, 1500f, 2000f };
    private float[] phaseSpeed = { 7f, 8f, 9f, 10f, 11f, 12f, 13f };

    private void UpdateDistance(float deltaTime)
    {
        tempSpeed = normalSpeed * (isClickRight ? speedUpMultiplier : 1f);
        currentDistance += deltaTime * tempSpeed * distanceMultiplier;

        if (currentPhase < phaseBorder.Length && currentDistance >= phaseBorder[currentPhase])
        {
            AdvancePhase();
        }
    }

    public string GetFormattedDistance()
    {
        return $"{currentDistance:F1}m";
    }

    // 정수 거리 반환
    public int GetDistance()
    {
        return Mathf.FloorToInt(currentDistance);
    }


    void AdvancePhase()
    {
        currentPhase++;
        _normalSpeed = phaseSpeed[currentPhase - 1];
        foreach (WingTtoObject obj in spawnObjectList)
        {
            obj?.UpdateSpeed();
        }

        Debug.Log($"Phase {currentPhase} 진입!  -> normalSpeed : {_normalSpeed}");
    }

    #endregion




    #region Spawn Process

    float calcWallTime = 10f;
    float calcGimbabTime = 7f;
    float calcStoneTime = 4f;
    float spawnWallTime = 0.15f;
    float spawnGimbabTime = 29f;
    float spawnStoneTime = 1.4f;

    float topWallPositionY = 9.5f;
    float wallDistance = 9.5f;
    float maxTopPosition = 9.5f;
    float maxBottomPosition = 5f;

    private void RandomSpawnObject(float deltaTime)
    {
        calcWallTime -= deltaTime * (isClickRight ? speedUpMultiplier : 1f);
        calcGimbabTime -= deltaTime * (isClickRight ? speedUpMultiplier : 1f);
        calcStoneTime -= deltaTime * (isClickRight ? speedUpMultiplier : 1f);


        if (calcWallTime <= 0)
        {
            calcWallTime = spawnWallTime;

            wallDistance -= 0.0025f;
            wallDistance = Mathf.Max(wallDistance, 6.2f);
            topWallPositionY += GetRandomFloat(-0.5f, 0.5f);
            // topWallPositionY = Mathf.Clamp(topWallPositionY, maxBottomPosition, maxTopPosition);
            topWallPositionY = Mathf.Clamp(topWallPositionY, Mathf.Max(maxBottomPosition, -10f + wallDistance * 2), maxTopPosition);


            pool.GetObject(WingTtoObjectType.Wall, topWallPositionY, topWallPositionY);
            pool.GetObject(WingTtoObjectType.Wall, topWallPositionY - wallDistance * 2, topWallPositionY - wallDistance * 2);
        }
        if (calcGimbabTime <= 0)
        {
            if (wallDistance < 6.7f) // 포션 지급 중지(난이도 상승)
                return;
            calcGimbabTime = spawnGimbabTime;
            float safeTop = topWallPositionY - 6f;
            float safeBottom = topWallPositionY - wallDistance * 2 + 6f;

            pool.GetObject(WingTtoObjectType.Gimbab, safeBottom, safeTop);
        }
        if (calcStoneTime <= 0)
        {
            if (wallDistance < 7.1f) //안전지역 없음
                return;

            calcStoneTime = spawnStoneTime;
            float safeTop = topWallPositionY - 6f;
            float safeBottom = topWallPositionY - wallDistance * 2 + 6f;

            pool.GetObject(WingTtoObjectType.Stone, safeBottom, safeTop);
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
        player.SetPause(isPause);
    }



    public float GetRandomFloat(float min, float max)
    {
        return min + (float)randomGenerator.NextDouble() * (max - min);
    }
}

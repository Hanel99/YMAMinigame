using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;


public class WingTtoGameManager : MonoBehaviour
{
    public static WingTtoGameManager instance { get; private set; }



    // private
    private int randomSeed;
    private System.Random randomGenerator;


    private InGameState _inGameState;
    public InGameState inGameState => _inGameState;
    private Queue<Action> UpdateActionQueue = new();


    public WingTtoPlayer player;
    public List<WingTtoGameBGMove> bgList;

    private bool isFlyPressed = false;
    private bool isSpeedPressed = false;

    public bool IsFlyPressed => isFlyPressed;
    public bool IsSpeedPressed => isSpeedPressed;

    /// <summary>플레이어가 현재 벽 충돌(Crash) 상태인지 여부 — 플라이어 이동 방향 결정에 사용</summary>
    public bool IsPlayerCrashing => player != null && player.IsCrashing;


    //private
    private WingTtoObjectPool pool => WingTtoObjectPool.instance;
    private List<WingTtoObject> spawnObjectList = new();
    private int tutorialStep = 0;


    [Header("플라이어 설정")]
    public GameObject flyerPrefab;
    public Transform flyerParent;

    private const int MAX_FLYER_COUNT = 5;
    private const float FLYER_SPAWN_INTERVAL = 8f;
    private const float FLYER_SPAWN_X = 14f;
    private const float FLYER_SPAWN_Y_MIN = -2f;
    private const float FLYER_SPAWN_Y_MAX = 3f;

    private readonly List<WingTtoFlyer> activeFlyerList = new();
    private readonly Queue<WingTtoFlyer> flyerPool = new();
    private float calcFlyerTime = FLYER_SPAWN_INTERVAL;



    //object 움직임 속도

    private float speedUpMultiplier = 1.5f;
    private float _originalSpeed = 6f;
    private float _normalSpeed = 6f;
    public float normalSpeed => _normalSpeed;


    // 날아간 거리 계산
    private float distanceMultiplier = 1.6f;
    private float currentDistance = 0f;
    public float CurrentDistance => currentDistance;



    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _inGameState = InGameState.Ready;
        randomSeed = UnityEngine.Random.Range(0, 100000);
        if (StaticGameData.useServerSeed)
            randomSeed = StaticGameData.serverGameSeed;

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

        WingTtoGameUIManager.instance.InitUI();
        WingTtoGameUIManager.instance.ShowDim(true, dimText, true);
    }

    public void OnClickStartButton()
    {
        if (tutorialStep < 1)
        {
            tutorialStep++;
            WingTtoGameUIManager.instance.UpdateTutorialText(tutorialStep);
        }
        else
            ClickStartProcess().Forget();
    }

    public async UniTaskVoid ClickStartProcess()
    {
        _inGameState = InGameState.GetSet;
        player.StartProcess();

        WingTtoGameUIManager.instance.ShowDim(false);
        WingTtoGameUIManager.instance.UpdateGetSetText("3");
        SoundManager.instance.PlaySFX(SFXType.Ready);
        await UniTask.Delay(600);
        WingTtoGameUIManager.instance.UpdateGetSetText("2");
        SoundManager.instance.PlaySFX(SFXType.Ready);
        await UniTask.Delay(600);
        WingTtoGameUIManager.instance.UpdateGetSetText("1");
        SoundManager.instance.PlaySFX(SFXType.Ready);
        await UniTask.Delay(600);
        WingTtoGameUIManager.instance.UpdateGetSetText("GO!!");
        SoundManager.instance.PlaySFX(SFXType.Go);
        await UniTask.Delay(600);
        WingTtoGameUIManager.instance.UpdateGetSetText("");

        _inGameState = InGameState.Play;
        WingTtoGameUIManager.instance.UpdateExpText(0);
        WingTtoGameUIManager.instance.UpdateCoinText(0);
        WingTtoGameUIManager.instance.UpdateSpeedText(_normalSpeed);
        WingTtoGameUIManager.instance.FadeGuideUI();
    }

    void Update()
    {
        CheckClickTouch();

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



        if (Input.GetKeyDown(KeyCode.Q))
        {
            AddSpeed(1);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            AddSpeed(-1);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            player.UpdateHPUI(100);
        }
#endif
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && _inGameState == InGameState.Play)
        {
            if (!WingTtoGameUIManager.instance.IsPopupOpen())
            {
                WingTtoGameUIManager.instance.ShowPausePopup();
                SetPause(true);
            }
        }
    }

    void CheckClickTouch()
    {
        if (_inGameState == InGameState.Play || _inGameState == InGameState.GetSet)
        {
            // PC - 마우스 입력
#if UNITY_EDITOR || UNITY_STANDALONE

            // // 왼쪽 마우스 버튼 누름
            // if (Input.GetMouseButtonDown(0))
            // {
            // }

            // // 왼쪽 마우스 버튼 뗌
            // if (Input.GetMouseButtonUp(0))
            // {
            // }

            // 오른쪽 마우스 버튼 누름
            if (Input.GetMouseButtonDown(1))
            {
                player.TiltForward();
            }

            // 오른쪽 마우스 버튼 뗌
            if (Input.GetMouseButtonUp(1))
            {
                player.ReturnToNormal();
            }


            isFlyPressed = Input.GetMouseButton(0);
            isSpeedPressed = Input.GetMouseButton(1);

#else

            // 모바일 - 터치 입력
            bool leftTouching = false;
            bool rightTouching = false;
            float screenHalfWidth = Screen.width / 2f;

            // 모든 터치 확인 (멀티터치)
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                // 활성 터치만 확인
                if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
                {
                    if (touch.position.x > screenHalfWidth)
                    {
                        rightTouching = true;
                    }
                    else
                    {
                        leftTouching = true;
                    }
                }
            }

            // 이전 프레임과 비교해서 상태 변화 감지
            // 오른쪽 터치 상태 변화
            // if (rightTouching && !isFlyPressed)
            // {
            //     // 오른쪽이 새로 눌림                
            // }
            // else if (!rightTouching && isFlyPressed)
            // {
            //     // 오른쪽 터치가 전부 없어짐                
            // }

            // 왼쪽 터치 상태 변화
            if (leftTouching && !isSpeedPressed)
            {
                // 왼쪽이 새로 눌림
                player.TiltForward();
            }
            else if (!leftTouching && isSpeedPressed)
            {
                // 왼쪽 터치가 전부 없어짐
                player.ReturnToNormal();
            }

            // 실시간 상태 업데이트
            isFlyPressed = rightTouching;
            isSpeedPressed = leftTouching;

#endif
        }
    }

    void FixedUpdate()
    {
        if (_inGameState == InGameState.Play)
        {
            UpdateDistance(Time.deltaTime);
            RandomSpawnObject(Time.deltaTime);
            UpdateFlyerSpawn(Time.deltaTime);
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


    // Calcs
    private const float SCORE_COIN_BASE = 1000f;
    private const float SCORE_COIN_FACTOR = 97000f;
    private const float SCORE_COIN_DIVISOR = 2900f;
    private const float SCORE_COIN_POW = 1.5f;

    private async UniTaskVoid FinishProcess()
    {
        int exp = 1 + player.EarnExp;
        int earnCoinAmount = player.EarnCoin + (int)Math.Max(SCORE_COIN_BASE, currentDistance + SCORE_COIN_FACTOR * Math.Pow(currentDistance / SCORE_COIN_DIVISOR, SCORE_COIN_POW));

        QuestManager.instance.AddWingTtoData(1, player.collectCountDic[WingTtoObjectType.Gimbab], player.collectCountDic[WingTtoObjectType.SpeedUp]
                                            , player.collectCountDic[WingTtoObjectType.Coin], player.collectCountDic[WingTtoObjectType.Exp], player.crashCount, currentDistance);

        QuestManager.instance.AddSpecialMission(QuestDetailType.S_ReachScoreWithoutGimbab, player.Q_FirstGimbabDistance >= 0 ? player.Q_FirstGimbabDistance : currentDistance);
        QuestManager.instance.AddSpecialMission(QuestDetailType.S_ReachScoreWithCrash, player.Q_FirstCrashDistance);


        HLLogger.Log($"distance : {currentDistance} / coin : {earnCoinAmount} / exp : {exp}");

        if (SaveDataManager.instance.playerData.finalQuizPlayData.playEndRoll)
        {
            earnCoinAmount = Math.Min(StaticGameData.MAX_COIN_VALUE, earnCoinAmount * 50);
            exp *= 10;
        }

        SaveDataManager.instance.AddCoin(earnCoinAmount, false);
        SaveDataManager.instance.SetWingTtoHighScore(currentDistance);

        WingTtoGameUIManager.instance.ShowResult(currentDistance, player.EarnCoin, player.EarnExp, earnCoinAmount, exp);

        // final refer
        if (FinalReferManager.instance.IsFinalReferUnlock(GameType.WingTto))
        {
            SaveDataManager.instance.SetFinalQuizReferData(GameType.WingTto, FinalReferState.Unlocked);
            WingTtoGameUIManager.instance.AddPopupToQueue(() =>
                WingTtoGameUIManager.instance.ShowReferPopup(GameType.WingTto, WingTtoGameUIManager.instance.ShowNextPopup));
        }
        else if (FinalReferManager.instance.IsFinalReferStateUnlocked(GameType.WingTto))
        {
            if (CheckReferMission())
            {
                SaveDataManager.instance.SetFinalQuizReferData(GameType.WingTto, FinalReferState.Completed);
                WingTtoGameUIManager.instance.AddPopupToQueue(() =>
                    WingTtoGameUIManager.instance.ShowReferPopup(GameType.WingTto, WingTtoGameUIManager.instance.ShowNextPopup));
            }
        }

        if (SaveDataManager.instance.AddExp(exp))
        {
            WingTtoGameUIManager.instance.AddPopupToQueue(() =>
                WingTtoGameUIManager.instance.ShowLevelUpPopup(WingTtoGameUIManager.instance.ShowNextPopup));
        }


        // 1.5초 뒤 팝업 큐 실행 시작
        await UniTask.Delay(1500);
        WingTtoGameUIManager.instance.ShowNextPopup();
    }



    #region Calc Distance

    float tempSpeed = 0f;

    private void UpdateDistance(float deltaTime)
    {
        tempSpeed = normalSpeed * (isSpeedPressed ? speedUpMultiplier : 1f);
        currentDistance += deltaTime * tempSpeed * distanceMultiplier;
    }

    public void AddSpeed(int value)
    {
        _normalSpeed += value;
        foreach (WingTtoObject obj in spawnObjectList)
        {
            obj?.UpdateSpeed();
        }
        foreach (var bg in bgList)
        {
            bg?.UpdateSpeed(_normalSpeed / _originalSpeed);
        }
        // 플라이어는 normalSpeed를 직접 참조하므로 별도 업데이트 불필요

        WingTtoGameUIManager.instance.UpdateSpeedText(_normalSpeed);
        SoundManager.instance.PlaySFX(SFXType.WingTtoSpeedUp);
        Debug.Log($"Speed UP! -> normalSpeed : {_normalSpeed}");
    }


    #endregion




    #region Spawn Process

    // Initial delays
    private const float INITIAL_TIME_WALL = 20f;
    private const float INITIAL_TIME_GIMBAB = 15f;
    private const float INITIAL_TIME_STONE = 1f;
    private const float INITIAL_TIME_SPEED_UP = 3f;
    private const float INITIAL_TIME_COIN = 4f;
    private const float INITIAL_TIME_EXP = 5f;

    // Spawn intervals
    private const float SPAWN_TIME_WALL = 0.15f;
    private const float SPAWN_TIME_GIMBAB = 29f;
    private const float SPAWN_TIME_STONE = 1.4f;
    private const float SPAWN_TIME_SPEED_UP = 21f;
    private const float SPAWN_TIME_COIN = 12f;
    private const float SPAWN_TIME_EXP = 27f;

    // Positions & Distances
    private const float WALL_GAP_Y = 6f; // Gap between wall and safe zone edge
    private const float WALL_DISTANCE_DECREASE_RATE = 0.0025f;
    private const float WALL_DISTANCE_MIN = 6.2f;
    private const float WALL_DISTANCE_STOP_GIMBAB = 6.5f;
    private const float WALL_DISTANCE_NO_SAFE = 7.1f;

    private const float POS_Y_MAX_TOP = 9.5f;
    private const float POS_Y_MAX_BOTTOM = 5f;
    private const float POS_Y_INITIAL_TOP = 9.5f;
    private const float POS_Y_INITIAL_DISTANCE = 9.5f;
    private const float POS_Y_LIMIT_BOTTOM_FACTOR = -10f;


    float calcWallTime = INITIAL_TIME_WALL;
    float calcGimbabTime = INITIAL_TIME_GIMBAB;
    float calcStoneTime = INITIAL_TIME_STONE;
    float calcSpeedUpTime = INITIAL_TIME_SPEED_UP;
    float calcCoinTime = INITIAL_TIME_COIN;
    float calcExpTime = INITIAL_TIME_EXP;

    float spawnWallTime = SPAWN_TIME_WALL;
    float spawnGimbabTime = SPAWN_TIME_GIMBAB;
    float spawnStoneTime = SPAWN_TIME_STONE;
    float spawnSpeedUpTime = SPAWN_TIME_SPEED_UP;
    float spawnCoinTime = SPAWN_TIME_COIN;
    float spawnExpTime = SPAWN_TIME_EXP;

    float topWallPositionY = POS_Y_INITIAL_TOP;
    float wallDistance = POS_Y_INITIAL_DISTANCE;
    float maxTopPosition = POS_Y_MAX_TOP;
    float maxBottomPosition = POS_Y_MAX_BOTTOM;

    private void RandomSpawnObject(float deltaTime)
    {
        float speedMultiplier = isSpeedPressed ? speedUpMultiplier : 1f;

        calcWallTime -= deltaTime * speedMultiplier;
        calcGimbabTime -= deltaTime * speedMultiplier;
        calcStoneTime -= deltaTime * speedMultiplier;
        calcSpeedUpTime -= deltaTime * speedMultiplier;
        calcCoinTime -= deltaTime * speedMultiplier;
        calcExpTime -= deltaTime * speedMultiplier;


        if (calcWallTime <= 0)
        {
            calcWallTime = spawnWallTime;

            wallDistance -= WALL_DISTANCE_DECREASE_RATE;
            wallDistance = Mathf.Max(wallDistance, WALL_DISTANCE_MIN);
            topWallPositionY += GetRandomFloat(-0.5f, 0.5f);
            topWallPositionY = Mathf.Clamp(topWallPositionY, Mathf.Max(maxBottomPosition, POS_Y_LIMIT_BOTTOM_FACTOR + wallDistance * 2), maxTopPosition);


            pool.GetObject(WingTtoObjectType.Wall, topWallPositionY, topWallPositionY);
            pool.GetObject(WingTtoObjectType.Wall, topWallPositionY - wallDistance * 2, topWallPositionY - wallDistance * 2);
        }

        if (calcGimbabTime <= 0)
        {
            calcGimbabTime = spawnGimbabTime;
            (float safeTop, float safeBottom) = GetSafeZone();

            if (wallDistance >= WALL_DISTANCE_STOP_GIMBAB) // 포션 지급 중지(난이도 상승)
                pool.GetObject(WingTtoObjectType.Gimbab, safeBottom, safeTop);
        }

        if (calcStoneTime <= 0)
        {
            calcStoneTime = spawnStoneTime;
            (float safeTop, float safeBottom) = GetSafeZone();

            if (wallDistance >= WALL_DISTANCE_NO_SAFE) //안전지역 없음
                pool.GetObject(WingTtoObjectType.Stone, safeBottom, safeTop);
        }

        if (calcSpeedUpTime <= 0)
        {
            calcSpeedUpTime = spawnSpeedUpTime;
            (float safeTop, float safeBottom) = GetSafeZone();

            if (GetRandomBool(80, 20)) // 80%확률로 생성
                pool.GetObject(WingTtoObjectType.SpeedUp, safeBottom, safeTop);
        }

        if (calcCoinTime <= 0)
        {
            calcCoinTime = spawnCoinTime;
            (float safeTop, float safeBottom) = GetSafeZone();

            if (GetRandomBool(70, 30)) // 70%확률로 생성
                pool.GetObject(WingTtoObjectType.Coin, safeBottom, safeTop);
        }

        if (calcExpTime <= 0)
        {
            calcExpTime = spawnExpTime;
            (float safeTop, float safeBottom) = GetSafeZone();

            if (GetRandomBool(50, 50)) // 50%확률로 생성
                pool.GetObject(WingTtoObjectType.Exp, safeBottom, safeTop);
        }
    }

    private (float top, float bottom) GetSafeZone()
    {
        return (topWallPositionY - WALL_GAP_Y, topWallPositionY - wallDistance * 2 + WALL_GAP_Y);
    }

    public void AddSpawnObject(WingTtoObject obj)
    {
        spawnObjectList.Add(obj);
    }

    public void RemoveSpawnObject(WingTtoObject obj)
    {
        spawnObjectList.Remove(obj);
    }


    // 스폰 확률: 0~2000m=100%, 2000~4000m=선형 감소, 4000m 이상=0%
    private void UpdateFlyerSpawn(float deltaTime)
    {
        if (flyerPrefab == null) return;

        calcFlyerTime -= deltaTime;
        if (calcFlyerTime > 0f) return;

        calcFlyerTime = FLYER_SPAWN_INTERVAL;

        if (activeFlyerList.Count >= MAX_FLYER_COUNT) return;

        int spawnChance;
        if (currentDistance < 2000f)
        {
            spawnChance = 100;
        }
        else
        {
            int distanceStep = Mathf.FloorToInt(currentDistance / 100f);
            float distanceClamped = Mathf.Clamp(distanceStep * 100f, 2000f, 4000f);
            spawnChance = Mathf.RoundToInt(Mathf.Lerp(100, 0, (distanceClamped - 2000f) / 2000f));
        }

        if (!GetRandomBool(spawnChance, 100 - spawnChance)) return;

        SpawnFlyer();
    }

    private void SpawnFlyer()
    {
        WingTtoFlyer flyer;

        if (flyerPool.Count > 0)
        {
            flyer = flyerPool.Dequeue();
        }
        else
        {
            Transform parent = flyerParent != null ? flyerParent : transform;
            GameObject go = Instantiate(flyerPrefab, parent);
            flyer = go.GetComponent<WingTtoFlyer>();
            if (flyer == null)
                flyer = go.AddComponent<WingTtoFlyer>();
        }

        float spawnY = GetRandomFloat(FLYER_SPAWN_Y_MIN, FLYER_SPAWN_Y_MAX);
        flyer.Activate(new Vector3(FLYER_SPAWN_X, spawnY, 0f));

        flyer.SetMasterIcon(PickRandomMasterForFlyer());

        activeFlyerList.Add(flyer);
    }

    // 플레이어 자신 + Other + 현재 활성 플라이어 마스터 제외 후 랜덤 선택
    private CardMaster PickRandomMasterForFlyer()
    {
        CardMaster playerMaster = SaveDataManager.instance.playerData.master;
        int totalCount = (int)CardMaster.Count;

        var usedMasters = new System.Collections.Generic.HashSet<CardMaster>();
        usedMasters.Add(CardMaster.Other);
        usedMasters.Add(playerMaster);

        foreach (WingTtoFlyer activeFlyer in activeFlyerList)
        {
            if (activeFlyer != null && activeFlyer.AssignedMaster != CardMaster.Count)
                usedMasters.Add(activeFlyer.AssignedMaster);
        }

        var candidates = new System.Collections.Generic.List<CardMaster>();
        for (int i = 0; i < totalCount; i++)
        {
            CardMaster m = (CardMaster)i;
            if (!usedMasters.Contains(m))
                candidates.Add(m);
        }

        if (candidates.Count == 0)
            return CardMaster.Other;

        return candidates[randomGenerator.Next(0, candidates.Count)];
    }

    public void DespawnFlyer(WingTtoFlyer flyer)
    {
        if (flyer == null) return;
        flyer.Despawn();
        activeFlyerList.Remove(flyer);
        flyerPool.Enqueue(flyer);
    }




    #endregion



    public async UniTaskVoid PausePopupCloseAction()
    {
        WingTtoGameUIManager.instance.UpdateGetSetText("3");
        SoundManager.instance.PlaySFX(SFXType.Ready);
        await UniTask.Delay(600);
        WingTtoGameUIManager.instance.UpdateGetSetText("2");
        SoundManager.instance.PlaySFX(SFXType.Ready);
        await UniTask.Delay(600);
        WingTtoGameUIManager.instance.UpdateGetSetText("1");
        SoundManager.instance.PlaySFX(SFXType.Ready);
        await UniTask.Delay(600);
        WingTtoGameUIManager.instance.UpdateGetSetText("GO!!");
        SoundManager.instance.PlaySFX(SFXType.Go);
        await UniTask.Delay(600);
        WingTtoGameUIManager.instance.UpdateGetSetText("");

        SetPause(false);
    }

    public void SetPause(bool isPause)
    {
        _inGameState = isPause ? InGameState.Pause : InGameState.Play;

        foreach (WingTtoObject obj in spawnObjectList)
        {
            obj?.SetPause(isPause);
        }
        foreach (var bg in bgList)
        {
            bg?.SetPause(isPause);
        }

        // 플라이어는 크래시 여부와 무관하게 일시정지만 전달 (크래시 모드는 SetFlyerCrashMode로 별도 제어)
        foreach (WingTtoFlyer flyer in activeFlyerList)
        {
            flyer?.SetPause(isPause);
        }

        player.SetPause(isPause);
    }

    // CrashProcess 시작/종료 시 호출 — 플라이어에게 크래시 모드를 직접 전달
    public void SetFlyerCrashMode(bool crash)
    {
        foreach (WingTtoFlyer flyer in activeFlyerList)
        {
            flyer?.SetCrashMode(crash);
        }
    }



    public float GetRandomFloat(float min, float max)
    {
        return min + (float)randomGenerator.NextDouble() * (max - min);
    }

    public bool GetRandomBool(int success, int fail)
    {
        int total = success + fail;
        bool result = randomGenerator.Next(0, total) < success;

        return result;
    }



    private bool CheckReferMission()
    {
        // 1의 자리 또는 10의 자리의 숫자가 3인 경우
        int score = (int)currentDistance;
        return (score % 10 == 3) || ((score / 10) % 10 == 3);
    }
}

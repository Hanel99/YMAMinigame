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


    //private
    private WingTtoObjectPool pool => WingTtoObjectPool.instance;
    private List<WingTtoObject> spawnObjectList = new();
    private int tutorialStep = 0;




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
        int exp = 1 + player.EarnExp;
        int earnCoinAmount = player.EarnCoin + (int)Math.Max(1000f, currentDistance + 97000f * Math.Pow(currentDistance / 2900f, 1.5f));

        QuestManager.instance.AddWingTtoData(1, player.collectCountDic[WingTtoObjectType.Gimbab], player.collectCountDic[WingTtoObjectType.SpeedUp]
                                            , player.collectCountDic[WingTtoObjectType.Coin], player.collectCountDic[WingTtoObjectType.Exp]);

        QuestManager.instance.AddSpecialMission(QuestDetailType.S_ReachScoreWithoutGimbab, player.Q_FirstGimbabDistance >= 0 ? player.Q_FirstGimbabDistance : currentDistance);
        QuestManager.instance.AddSpecialMission(QuestDetailType.S_ReachScoreWithCrash, player.Q_FirstCrashDistance);


        HLLogger.Log($"distance : {currentDistance} / coin : {earnCoinAmount} / exp : {exp}");

        SaveDataManager.instance.AddCoin(earnCoinAmount, false);
        SaveDataManager.instance.SetWingTtoHighScore(currentDistance);

        WingTtoGameUIManager.instance.ShowResult(currentDistance, player.EarnCoin, player.EarnExp, earnCoinAmount, exp);

        if (SaveDataManager.instance.AddExp(exp))
        {
            await UniTask.Delay(1500);
            WingTtoGameUIManager.instance.ShowLevelUpPopup();
        }


        // final refer
        if (FinalReferManager.instance.IsFinalReferUnlock(GameType.WingTto))
        {
            SaveDataManager.instance.SetFinalQuizReferData(GameType.WingTto, FinalReferState.Unlocked);
            DOVirtual.DelayedCall(1.2f, () => WingTtoGameUIManager.instance.ShowReferPopup(GameType.WingTto));
        }
        else if (FinalReferManager.instance.IsFinalReferStateUnlocked(GameType.WingTto))
        {
            if (CheckReferMission())
            {
                SaveDataManager.instance.SetFinalQuizReferData(GameType.WingTto, FinalReferState.Completed);
                DOVirtual.DelayedCall(1.2f, () => WingTtoGameUIManager.instance.ShowReferPopup(GameType.WingTto));
            }
        }
    }



    #region Calc Distance

    float tempSpeed = 0f;

    private void UpdateDistance(float deltaTime)
    {
        tempSpeed = normalSpeed * (isSpeedPressed ? speedUpMultiplier : 1f);
        currentDistance += deltaTime * tempSpeed * distanceMultiplier;
    }

    public string GetFormattedDistance()
    {
        return $"{currentDistance:F0}m";
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

        WingTtoGameUIManager.instance.UpdateSpeedText(_normalSpeed);
        SoundManager.instance.PlaySFX(SFXType.WingTtoSpeedUp);
        Debug.Log($"Speed UP! -> normalSpeed : {_normalSpeed}");
    }


    #endregion




    #region Spawn Process

    float calcWallTime = 20f;
    float calcGimbabTime = 15f;
    float calcStoneTime = 1f;
    float calcSpeedUpTime = 3f;
    float calcCoinTime = 4f;
    float calcExpTime = 5f;

    float spawnWallTime = 0.15f;
    float spawnGimbabTime = 29f;
    float spawnStoneTime = 1.4f;
    float spawnSpeedUpTime = 21f;
    float spawnCoinTime = 12f;
    float spawnExpTime = 27f;

    float topWallPositionY = 9.5f;
    float wallDistance = 9.5f;
    float maxTopPosition = 9.5f;
    float maxBottomPosition = 5f;

    private void RandomSpawnObject(float deltaTime)
    {
        calcWallTime -= deltaTime * (isSpeedPressed ? speedUpMultiplier : 1f);
        calcGimbabTime -= deltaTime * (isSpeedPressed ? speedUpMultiplier : 1f);
        calcStoneTime -= deltaTime * (isSpeedPressed ? speedUpMultiplier : 1f);
        calcSpeedUpTime -= deltaTime * (isSpeedPressed ? speedUpMultiplier : 1f);
        calcCoinTime -= deltaTime * (isSpeedPressed ? speedUpMultiplier : 1f);
        calcExpTime -= deltaTime * (isSpeedPressed ? speedUpMultiplier : 1f);


        if (calcWallTime <= 0)
        {
            calcWallTime = spawnWallTime;

            wallDistance -= 0.0025f;
            wallDistance = Mathf.Max(wallDistance, 6.2f);
            topWallPositionY += GetRandomFloat(-0.5f, 0.5f);
            topWallPositionY = Mathf.Clamp(topWallPositionY, Mathf.Max(maxBottomPosition, -10f + wallDistance * 2), maxTopPosition);


            pool.GetObject(WingTtoObjectType.Wall, topWallPositionY, topWallPositionY);
            pool.GetObject(WingTtoObjectType.Wall, topWallPositionY - wallDistance * 2, topWallPositionY - wallDistance * 2);
        }
        if (calcGimbabTime <= 0)
        {
            calcGimbabTime = spawnGimbabTime;
            float safeTop = topWallPositionY - 6f;
            float safeBottom = topWallPositionY - wallDistance * 2 + 6f;

            if (wallDistance >= 6.5f) // 포션 지급 중지(난이도 상승)
                pool.GetObject(WingTtoObjectType.Gimbab, safeBottom, safeTop);
        }
        if (calcStoneTime <= 0)
        {
            calcStoneTime = spawnStoneTime;
            float safeTop = topWallPositionY - 6f;
            float safeBottom = topWallPositionY - wallDistance * 2 + 6f;

            if (wallDistance >= 7.1f) //안전지역 없음
                pool.GetObject(WingTtoObjectType.Stone, safeBottom, safeTop);
        }

        if (calcSpeedUpTime <= 0)
        {
            calcSpeedUpTime = spawnSpeedUpTime;
            float safeTop = topWallPositionY - 6f;
            float safeBottom = topWallPositionY - wallDistance * 2 + 6f;

            if (GetRandomBool(80, 20)) // 80%확률로 생성
                pool.GetObject(WingTtoObjectType.SpeedUp, safeBottom, safeTop);
        }

        if (calcCoinTime <= 0)
        {
            calcCoinTime = spawnCoinTime;
            float safeTop = topWallPositionY - 6f;
            float safeBottom = topWallPositionY - wallDistance * 2 + 6f;

            if (GetRandomBool(70, 30)) // 70%확률로 생성
                pool.GetObject(WingTtoObjectType.Coin, safeBottom, safeTop);
        }

        if (calcExpTime <= 0)
        {
            calcExpTime = spawnExpTime;
            float safeTop = topWallPositionY - 6f;
            float safeBottom = topWallPositionY - wallDistance * 2 + 6f;

            if (GetRandomBool(50, 50)) // 50%확률로 생성
                pool.GetObject(WingTtoObjectType.Exp, safeBottom, safeTop);
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

        player.SetPause(isPause);
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
        // 1의 자릿수의 숫자가 3인 상태로 게임을 끝낼 것

        return currentDistance.ToString("F0").EndsWith("3");
    }
}

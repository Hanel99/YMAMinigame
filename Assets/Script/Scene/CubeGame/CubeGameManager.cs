using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;


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

    private bool Q_WithoutTooFastOrTooSlow = true;
    private int Q_WithoutTooFastOrTooSlowScore = 0;
    private bool Q_OnlyPerfect = true;
    private int Q_OnlyPerfectScore = 0;
    private int combo = 0;




    private void Awake()
    {
        instance = this;
    }

    // Consts
    private const float GAME_LIMIT_TIME = 30f;
    private const float MAX_LIMIT_TIME = 30f;
    private const int START_DELAY = 2000;
    private const int FINISH_DELAY = 2000;
    private const int LEVEL_UP_DELAY = 1500;
    private const float REFER_POPUP_DELAY = 1.2f;

    // Score & Time Values
    private const int SCORE_PERFECT = 300;
    private const int SCORE_FAST_SLOW = 100;
    private const int SCORE_TOO_FAST_SLOW = -100;

    private const float TIME_PERFECT = 0.5f;
    private const float TIME_FAST_SLOW = 0f;
    private const float TIME_TOO_FAST_SLOW = -0.5f;

    private void Start()
    {
        score = 0;
        combo = 0;
        leftTime = GAME_LIMIT_TIME;
        maxTime = MAX_LIMIT_TIME;
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
        cubeList[0].SetKeyCodes(KeyCode.Q, KeyCode.Keypad7);
        cubeList[1].SetKeyCodes(KeyCode.W, KeyCode.Keypad8);
        cubeList[2].SetKeyCodes(KeyCode.E, KeyCode.Keypad9);

        cubeList[3].SetKeyCodes(KeyCode.A, KeyCode.Keypad4);
        cubeList[4].SetKeyCodes(KeyCode.S, KeyCode.Keypad5);
        cubeList[5].SetKeyCodes(KeyCode.D, KeyCode.Keypad6);

        cubeList[6].SetKeyCodes(KeyCode.Z, KeyCode.Keypad1);
        cubeList[7].SetKeyCodes(KeyCode.X, KeyCode.Keypad2);
        cubeList[8].SetKeyCodes(KeyCode.C, KeyCode.Keypad3);

        StartProcess().Forget();
    }

    private async UniTaskVoid StartProcess()
    {
        CubeGameUIManager.instance.ShowSceneMoveAnimation(true);
        SoundManager.instance.PlayBGM(BGMType.CubeGame);

        CubeGameUIManager.instance.UpdateLeftTimeText(leftTime);
        CubeGameUIManager.instance.UpdateLeft10TimeBar(leftTime / 10f);
        CubeGameUIManager.instance.UpdateScoreText(score);
        CubeGameUIManager.instance.UpdateComboText(combo);
        CubeGameUIManager.instance.UpdateCubeCountText(countDic);
        CubeGameUIManager.instance.InitCountUI();
        CubeGameUIManager.instance.ShowDim(true, "준비!");
        await UniTask.Delay(START_DELAY);
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
            CubeGameUIManager.instance.UpdateLeft10TimeBar(leftTime / 10f);

            if (leftTime <= 0f)
            {
                leftTime = 0f;
                CubeGameUIManager.instance.UpdateLeftTimeText(leftTime);
                CubeGameUIManager.instance.UpdateLeft10TimeBar(leftTime / 10f);
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

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && _inGameState == InGameState.Play)
        {
            if (!CubeGameUIManager.instance.IsPopupOpen())
            {
                CubeGameUIManager.instance.ShowPausePopup();
                SetPause(true);
            }
        }
    }


    private async UniTaskVoid TimeOverProcess()
    {
        CubeGameUIManager.instance.ShowDim(true, "게임 종료!");
        foreach (var cube in cubeList) cube.StopCube();
        await UniTask.Delay(FINISH_DELAY);

        FinishProcess().Forget();
    }


    private async UniTaskVoid FinishProcess()
    {
        int exp = (int)Math.Max(1f, score / 10000f + 1.5f * Math.Log(score / 500f + 1f, 5.5f));
        int earnCoinAmount = (int)Math.Max(1000f, score * 3f / 4f + 26000f * Math.Log(score / 60000f + 1f, 1.5f));

        HLLogger.Log($"Score : {score} / coin : {earnCoinAmount} / exp : {exp}");

        QuestManager.instance.AddCubeData(1, countDic[CubeState.TooFast], countDic[CubeState.Fast], countDic[CubeState.Perfect],
                                            countDic[CubeState.Slow], countDic[CubeState.TooSlow], countDic.Values.Sum(), score);
        QuestManager.instance.AddSpecialMission(QuestDetailType.S_ReachScoreWithoutTooFastOrTooSlow, Q_WithoutTooFastOrTooSlowScore);
        QuestManager.instance.AddSpecialMission(QuestDetailType.S_ReachScoreWithOnlyPerfect, Q_OnlyPerfectScore);

        if (SaveDataManager.instance.playerData.finalQuizPlayData.playEndRoll)
        {
            earnCoinAmount = Math.Min(StaticGameData.MAX_COIN_VALUE, earnCoinAmount * 50);
            exp *= 10;
        }

        SaveDataManager.instance.AddCoin(earnCoinAmount, false);
        SaveDataManager.instance.SetCubeGameHighScore(score);
        CubeGameUIManager.instance.ShowResult(score, earnCoinAmount, exp);

        // final refer
        if (FinalReferManager.instance.IsFinalReferUnlock(GameType.CubeGame))
        {
            SaveDataManager.instance.SetFinalQuizReferData(GameType.CubeGame, FinalReferState.Unlocked);
            CubeGameUIManager.instance.AddPopupToQueue(() =>
                CubeGameUIManager.instance.ShowReferPopup(GameType.CubeGame, CubeGameUIManager.instance.ShowNextPopup));
        }
        else if (FinalReferManager.instance.IsFinalReferStateUnlocked(GameType.CubeGame))
        {
            if (CheckReferMission())
            {
                SaveDataManager.instance.SetFinalQuizReferData(GameType.CubeGame, FinalReferState.Completed);
                CubeGameUIManager.instance.AddPopupToQueue(() =>
                    CubeGameUIManager.instance.ShowReferPopup(GameType.CubeGame, CubeGameUIManager.instance.ShowNextPopup));
            }
        }

        if (SaveDataManager.instance.AddExp(exp))
        {
            CubeGameUIManager.instance.AddPopupToQueue(() =>
                CubeGameUIManager.instance.ShowLevelUpPopup(CubeGameUIManager.instance.ShowNextPopup));
        }


        // 1.5초 뒤 팝업 큐 실행 시작
        await UniTask.Delay(1500);
        CubeGameUIManager.instance.ShowNextPopup();
    }





    public void SetPause(bool isPause)
    {
        _inGameState = isPause ? InGameState.Pause : InGameState.Play;
    }

    public void MissProcess()
    {
        if (inGameState != InGameState.Play) return;

        SoundManager.instance.PlaySFX(SFXType.CubeMiss);
        combo = 0;
        CubeGameUIManager.instance.UpdateComboText(combo);
    }


    public void CubeClickProcess(CubeGameCube cube, CubeState state)
    {
        if (inGameState != InGameState.Play) return;

        // 콤보 처리
        bool isComboIncrement = state == CubeState.Perfect || state == CubeState.Fast || state == CubeState.Slow;
        if (isComboIncrement)
        {
            combo++;
        }
        else
        {
            combo = 0;
        }

        // 점수, 시간 처리.
        int baseScore = CalculateScore(state);
        int addScore = baseScore > 0 ? baseScore + combo : baseScore; // 점수가 양수일 때만 콤보 보너스 적용
        float addTime = CalculateTime(state);

        score += addScore;
        score = Math.Max(0, score);
        leftTime += addTime;
        leftTime = Math.Min(maxTime, leftTime);
        countDic[state]++;

        //퀘스트 특수 미션 처리
        Q_OnlyPerfect = Q_OnlyPerfect && state == CubeState.Perfect;
        if (Q_OnlyPerfect)
        {
            Q_OnlyPerfectScore = score;
        }

        Q_WithoutTooFastOrTooSlow = Q_WithoutTooFastOrTooSlow && state != CubeState.TooFast && state != CubeState.TooSlow;
        if (Q_WithoutTooFastOrTooSlow)
        {
            Q_WithoutTooFastOrTooSlowScore = score;
        }

        CubeGameUIManager.instance.UpdateScoreText(score);
        CubeGameUIManager.instance.UpdateComboText(combo);
        CubeGameUIManager.instance.UpdateCubeCountText(state, countDic[state]);

        if (_inGameState == InGameState.Play)
        {
            (float wait, float tooFast, float fast, float perfect, float slow, float tooSlow) = MakeStateTime();
            cube.StartCube(wait, tooFast, fast, perfect, slow, tooSlow);
        }
    }

    private int CalculateScore(CubeState state)
    {
        return state switch
        {
            CubeState.TooFast => SCORE_TOO_FAST_SLOW,
            CubeState.Fast => SCORE_FAST_SLOW,
            CubeState.Perfect => SCORE_PERFECT,
            CubeState.Slow => SCORE_FAST_SLOW,
            CubeState.TooSlow => SCORE_TOO_FAST_SLOW,
            _ => 0,
        };
    }

    private float CalculateTime(CubeState state)
    {
        return state switch
        {
            CubeState.TooFast => TIME_TOO_FAST_SLOW,
            CubeState.Fast => TIME_FAST_SLOW,
            CubeState.Perfect => TIME_PERFECT,
            CubeState.Slow => TIME_FAST_SLOW,
            CubeState.TooSlow => TIME_TOO_FAST_SLOW,
            _ => 0f,
        };
    }

    private (float wait, float tooFast, float fast, float perfect, float slow, float tooSlow) MakeStateTime()
    {
        // 0개일 때: 1 ~ 1.5
        // 200개일 때:
        // too 0.2 ~ 0.5
        // middle 0.15 ~ 0.3
        // perfect 0.1 ~ 0.2    

        int perfectCount = 0;
        if (countDic.ContainsKey(CubeState.Perfect))
        {
            perfectCount = countDic[CubeState.Perfect];
        }

        // 0 ~ 200 사이의 값을 0 ~ 1로 정규화
        float t = Mathf.Clamp01(perfectCount / 200f);

        float wait = GetRandomFloat(0.5f, 3.5f);

        // Lerp(start, end, t)
        float too = GetRandomFloat(Mathf.Lerp(1f, 0.2f, t), Mathf.Lerp(1.5f, 0.5f, t));
        float middle = GetRandomFloat(Mathf.Lerp(1f, 0.15f, t), Mathf.Lerp(1.5f, 0.3f, t));
        float perfect = GetRandomFloat(Mathf.Lerp(1f, 0.1f, t), Mathf.Lerp(1.5f, 0.2f, t));

        return (wait, too, middle, perfect, middle, too);
    }

    public float GetRandomFloat(float min, float max)
    {
        return min + (float)randomGenerator.NextDouble() * (max - min);
    }





    private bool CheckReferMission()
    {
        // 각각의 판정 타이밍마다 3번씩 터치하고 게임을 끝낼 것

        return countDic[CubeState.TooFast] == 3 && countDic[CubeState.Fast] == 3 && countDic[CubeState.Perfect] == 3 && countDic[CubeState.Slow] == 3 && countDic[CubeState.TooSlow] == 3;
    }
}

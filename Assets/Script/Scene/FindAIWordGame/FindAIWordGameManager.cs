using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class FindAIWordGameManager : MonoBehaviour
{
    public static FindAIWordGameManager instance { get; private set; }

    private int wordIndex;
    private string currentKeyword;
    private List<string> hints;
    private int currentHintIndex = 0;

    private CancellationTokenSource apiCts;
    private StringBuilder sb = new StringBuilder();

    // final refer mission
    private bool isReferMissionSuccess = false;
    private bool hasInputThree = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        FindAIWordGameInGameView.instance.InitUI();
        sb.Clear();
        StartGameProcess().Forget();
    }



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (FindAIWordGameUIManager.instance.IsPopupOpen())
                FindAIWordGameUIManager.instance.CloseTopPopup();
            else
                FindAIWordGameUIManager.instance.ShowPausePopup();
        }
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            FindAIWordGameInGameView.instance.OnClickSubmit();
        }

#if UNITY_EDITOR && DEV
        if (Input.GetKeyDown(KeyCode.C))
        {
            HLLogger.Log($"force clear");

            FinishProcess(0).Forget();
        }
#endif
    }


    // Consts
    private const int LOADING_DELAY = 500;
    private const int FAIL_DELAY = 1500;
    private const int WRONG_DELAY = 1500;
    private const int FINISH_DELAY = 3000;
    private const int LEVEL_UP_DELAY = 1500;
    private const float REFER_POPUP_DELAY = 1.2f;

    private const int REWARD_MAX = 30000; // 치트 사용 시
    private const int REWARD_BASE = 10000;
    private const int REWARD_PER_HINT = 1000;

    private const string MSG_LOADING_PREFIX = "AI에게 힌트 받아오는 중";
    private const string MSG_FAIL_LOAD = "힌트를 받아오지 못했습니다.\n다른 게임을 이용해주세요.";
    private const string MSG_CANCEL = "힌트 요청이 취소되었습니다.";
    private const string MSG_FAIL_GAME = "실패!\n정답은 ";
    private const string MSG_SUCCESS = "정답!\n축하합니다!";
    private const string MSG_WRONG = "오답!";


    private async UniTask StartGameProcess()
    {
        // 기존 작업 취소
        apiCts?.Cancel();
        apiCts = new CancellationTokenSource();

        isReferMissionSuccess = false;
        hasInputThree = false;

        FindAIWordGameUIManager.instance.ShowSceneMoveAnimation(true);
        SoundManager.instance.PlayBGM(BGMType.WordGame);

        var textUpdateCts = new CancellationTokenSource();
        UpdateLoadingText(textUpdateCts.Token).Forget();

        try
        {
            if (SaveDataManager.instance.playerData.geminiHints2.Count > 0)
            {
                var gameHint = SaveDataManager.instance.playerData.geminiHints2[0];

                hints = gameHint.value;
                currentKeyword = gameHint.key;
                wordIndex = gameHint.index;

                SaveDataManager.instance.playerData.geminiHints2.Remove(gameHint);
            }
            else
            {
                // Gemini로부터 힌트 받아오기
                int index = -1;
                (index, currentKeyword) = LocalizeManager.instance.GetRandomAIWordString();
                wordIndex = index;
                hints = await GeminiApiManager.instance.GetHintsForKeyword(currentKeyword).AttachExternalCancellation(apiCts.Token);
            }

            textUpdateCts.Cancel();

            if (hints == null || hints.Count == 0)
            {
                HLLogger.LogError("힌트를 불러오지 못했습니다.");
                FindAIWordGameInGameView.instance.UpdateHintText(MSG_FAIL_LOAD);
                await UniTask.Delay(FAIL_DELAY);
                SceneMoveManager.instance.MoveScene(SceneName.LobbyScene);
                return;
            }

            currentHintIndex = 0;
            ShowNextHint().Forget();

            // 다음 데이터 미리 로딩
            GeminiApiManager.instance.SaveHintsPreload().Forget();
        }

        catch (OperationCanceledException)
        {
            HLLogger.Log("힌트 요청이 취소되었습니다.");
            FindAIWordGameInGameView.instance.UpdateHintText(MSG_CANCEL);
        }
        finally
        {
            apiCts.Dispose();
            apiCts = null;
        }
    }

    public void CancelHintRequest()
    {
        apiCts?.Cancel();
    }

    private async UniTask UpdateLoadingText(CancellationToken token)
    {
        try
        {
            string[] dots = { ".", "..", "..." };
            StringBuilder sb = new StringBuilder(MSG_LOADING_PREFIX);
            int index = 0;

            while (!token.IsCancellationRequested)
            {
                sb.Length = 0;
                sb.Append(MSG_LOADING_PREFIX);
                sb.Append(dots[index]);
                FindAIWordGameInGameView.instance.UpdateHintText(sb.ToString());
                index = (index + 1) % dots.Length;
                await UniTask.Delay(LOADING_DELAY, cancellationToken: token); // 0.5초마다 갱신
            }
        }

        catch (OperationCanceledException)
        {
            // 정상 취소 처리
        }
        finally
        {

        }
    }

    private async UniTask ShowNextHint(int delayTime = 0)
    {
        await UniTask.Delay(delayTime);

        if (currentHintIndex < hints.Count)
        {
            FindAIWordGameInGameView.instance.EnableAnswerButton(true);
            FindAIWordGameInGameView.instance.UpdateTryCountText(currentHintIndex + 1);

            sb.AppendLine(hints[currentHintIndex]);
            FindAIWordGameInGameView.instance.UpdateHintText(sb.ToString());
        }
        else
        {
            FindAIWordGameInGameView.instance.UpdateHintText(MSG_FAIL_GAME + currentKeyword);
            FinishProcess().Forget();
        }
    }

    public void OnPlayerAnswer(string answer)
    {
        // 빈칸인 경우 넘김
        if (answer == string.Empty) return;

        bool isCorrect = answer.Trim() == currentKeyword;
        CheckReferMission(currentHintIndex + 1, answer, isCorrect);

        FindAIWordGameInGameView.instance.EnableAnswerButton(false);

        if (isCorrect)
        {
            FindAIWordGameInGameView.instance.UpdateHintText(MSG_SUCCESS);
            FinishProcess().Forget();
        }
        else
        {
            FindAIWordGameInGameView.instance.UpdateHintText(MSG_WRONG);
            FindAIWordGameInGameView.instance.ResetAnswerField();

            currentHintIndex++;
            ShowNextHint(WRONG_DELAY).Forget();
        }
    }


    private async UniTask FinishProcess(int delayTime = FINISH_DELAY)
    {
        apiCts?.Cancel();
        apiCts?.Dispose();
        apiCts = null;

        // 3초 딜레이
        await UniTask.Delay(delayTime);

        //1회만에 바로 맞춘경우 15000. 1회 틀릴때마다 500씩 감소. 최소 12000
        int earnCoinAmount = CalculateReward(delayTime);

        QuestManager.instance.AddFindAIWordData(1, currentHintIndex < 3 ? 1 : 0);

        SaveDataManager.instance.AddOwnWord(wordIndex);
        SaveDataManager.instance.AddCoin(earnCoinAmount);

        FindAIWordGameUIManager.instance.ShowResult(currentHintIndex + 1, earnCoinAmount);

        if (SaveDataManager.instance.AddExp(1))
        {
            await UniTask.Delay(LEVEL_UP_DELAY);
            FindAIWordGameUIManager.instance.ShowLevelUpPopup();
        }


        // final refer
        if (FinalReferManager.instance.IsFinalReferUnlock(GameType.FindAIWordGame))
        {
            SaveDataManager.instance.SetFinalQuizReferData(GameType.FindAIWordGame, FinalReferState.Unlocked);
            DOVirtual.DelayedCall(REFER_POPUP_DELAY, () => FindAIWordGameUIManager.instance.ShowReferPopup(GameType.FindAIWordGame));
        }
        else if (FinalReferManager.instance.IsFinalReferStateUnlocked(GameType.FindAIWordGame))
        {
            if (isReferMissionSuccess)
            {
                SaveDataManager.instance.SetFinalQuizReferData(GameType.FindAIWordGame, FinalReferState.Completed);
                DOVirtual.DelayedCall(REFER_POPUP_DELAY, () => FindAIWordGameUIManager.instance.ShowReferPopup(GameType.FindAIWordGame));
            }
        }
    }

    private int CalculateReward(int delayTime)
    {
        if (delayTime < FINISH_DELAY) //강제 치트를 쓴 경우
            return REWARD_MAX;

        return (hints.Count - currentHintIndex) * REWARD_PER_HINT + REWARD_BASE;
    }



    private void CheckReferMission(int tryCount, string answer, bool isCorrect)
    {
        // 1 2번째 트라이에 3을 입력 하여 실패한 뒤
        // 3번째 트라이에 성공할 것.

        if ((tryCount == 1 || tryCount == 2) && answer == "3")
        {
            hasInputThree = true;
        }
        else if (tryCount == 3 && isCorrect && hasInputThree)
        {
            isReferMissionSuccess = true;
        }
    }
}

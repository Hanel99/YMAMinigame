using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using System;
using System.Text;

public class FindAIWordGameManager : MonoBehaviour
{
    public static FindAIWordGameManager instance { get; private set; }

    private string currentKeyword;
    private List<string> hints;
    private int currentHintIndex = 0;

    private CancellationTokenSource apiCts;
    private StringBuilder sb = new StringBuilder();

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


    private async UniTask StartGameProcess()
    {
        // 기존 작업 취소
        apiCts?.Cancel();
        apiCts = new CancellationTokenSource();

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

                SaveDataManager.instance.playerData.geminiHints2.Remove(gameHint);
            }
            else
            {
                // Gemini로부터 힌트 받아오기
                currentKeyword = LocalizeManager.instance.GetRandomAIWordString();
                hints = await GeminiApiManager.instance.GetHintsForKeyword(currentKeyword).AttachExternalCancellation(apiCts.Token);
            }

            textUpdateCts.Cancel();

            if (hints == null || hints.Count == 0)
            {
                HLLogger.LogError("힌트를 불러오지 못했습니다.");
                FindAIWordGameInGameView.instance.UpdateHintText("힌트를 받아오지 못했습니다.\n다른 게임을 이용해주세요.");
                await UniTask.Delay(1500);
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
            FindAIWordGameInGameView.instance.UpdateHintText("힌트 요청이 취소되었습니다.");
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
            string baseText = "AI에게 힌트 받아오는 중";
            string[] dots = { ".", "..", "..." };
            StringBuilder sb = new StringBuilder(baseText);
            int index = 0;

            while (!token.IsCancellationRequested)
            {
                sb.Length = 0;
                sb.Append(baseText);
                sb.Append(dots[index]);
                FindAIWordGameInGameView.instance.UpdateHintText(sb.ToString());
                index = (index + 1) % dots.Length;
                await UniTask.Delay(500, cancellationToken: token); // 0.5초마다 갱신
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
            FindAIWordGameInGameView.instance.UpdateHintText("실패!\n정답은 " + currentKeyword);
            FinishProcess().Forget();
        }
    }

    public void OnPlayerAnswer(string answer)
    {
        // 빈칸인 경우 넘김
        if (answer == string.Empty) return;

        FindAIWordGameInGameView.instance.EnableAnswerButton(false);

        if (answer.Trim() == currentKeyword)
        {
            FindAIWordGameInGameView.instance.UpdateHintText("정답!\n축하합니다!");
            FinishProcess().Forget();
        }
        else
        {
            FindAIWordGameInGameView.instance.UpdateHintText("오답!");
            FindAIWordGameInGameView.instance.ResetAnswerField();

            currentHintIndex++;
            ShowNextHint(1500).Forget();
        }
    }


    private async UniTask FinishProcess(int delayTime = 3000)
    {
        apiCts?.Cancel();
        apiCts?.Dispose();
        apiCts = null;

        // 3초 딜레이
        await UniTask.Delay(delayTime);

        //1회만에 바로 맞춘경우 15000. 1회 틀릴때마다 500씩 감소. 최소 12000
        int earnCoinAmount = 0;
        if (delayTime < 3000) //강제 치트를 쓴 경우
            earnCoinAmount = 30000;
        else
            earnCoinAmount = (hints.Count - currentHintIndex) * 1000 + 10000;

        SaveDataManager.instance.AddCoin(earnCoinAmount);
        FindAIWordGameUIManager.instance.ShowResult(currentHintIndex + 1, earnCoinAmount);

        if (SaveDataManager.instance.AddExp(1))
        {
            await UniTask.Delay(1500);
            DOVirtual.DelayedCall(1.5f, () => CardGameUIManager.instance.ShowLevelUpPopup());
        }
    }
}

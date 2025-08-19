using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

public class FindAIWordGameManager : MonoBehaviour
{
    public static FindAIWordGameManager instance { get; private set; }

    private List<string> keywordTable = new List<string>
    {
        "사과", "축구", "비행기", "피카츄", "피아노"
    };

    private string currentKeyword;
    private List<string> hints;
    private int currentHintIndex = 0;

    private void Awake()
    {
        instance = this;
    }

    private async void Start()
    {
        FindAIWordGameInGameView.instance.InitUI();
        await StartGameProcess();
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

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.C))
        {
            HLLogger.Log($"force clear");
            FinishProcess(0);
        }
#endif
    }


    private async UniTask StartGameProcess()
    {
        FindAIWordGameUIManager.instance.ShowSceneMoveAnimation(true);


        // 랜덤 키워드 선택
        currentKeyword = keywordTable[Random.Range(0, keywordTable.Count)];
        HLLogger.Log("정답 키워드 선택됨 (숨김): " + currentKeyword);

        FindAIWordGameInGameView.instance.UpdateHintText("AI에게 힌트 받아오는 중...");

        // Gemini로부터 힌트 받아오기
        hints = await GeminiApiManager.instance.GetHintsForKeyword(currentKeyword);

        if (hints == null || hints.Count == 0)
        {
            HLLogger.LogError("힌트를 불러오지 못했습니다.");
            FindAIWordGameInGameView.instance.UpdateHintText("힌트를 받아오지 못했습니다.\n다른 게임을 이용해주세요.");
            FindAIWordGameUIManager.instance.ShowPausePopup();
            return;
        }
        HLLogger.Log($"받은 힌트들:\n{string.Join("\n", hints)}");

        currentHintIndex = 0;
        ShowNextHint();
    }

    private async void ShowNextHint(int delayTime = 0)
    {
        await UniTask.Delay(delayTime);

        if (currentHintIndex < hints.Count)
        {
            HLLogger.Log($"힌트 {currentHintIndex + 1}: {hints[currentHintIndex]}");
            FindAIWordGameInGameView.instance.UpdateTryCountText(currentHintIndex + 1);
            FindAIWordGameInGameView.instance.UpdateHintText(hints[currentHintIndex]);
        }
        else
        {
            HLLogger.Log("모든 힌트를 다 사용");
            FindAIWordGameInGameView.instance.UpdateHintText("실패!\n정답은 " + currentKeyword);
            FinishProcess();
        }
    }

    public void OnPlayerAnswer(string answer)
    {
        if (answer.Trim() == currentKeyword)
        {
            HLLogger.Log($"{answer.Trim()} - 정답!");
            FindAIWordGameInGameView.instance.UpdateHintText("정답!\n축하합니다!");
            FinishProcess();
        }
        else
        {
            HLLogger.Log($"{answer.Trim()} - 오답");
            FindAIWordGameInGameView.instance.UpdateHintText("오답!");
            FindAIWordGameInGameView.instance.ResetAnswerField();

            currentHintIndex++;
            ShowNextHint(1500);
        }
    }


    private async void FinishProcess(int delayTime = 3000)
    {
        // 3초 딜레이
        await UniTask.Delay(delayTime);

        int earnCoinAmount = 0;
        if (delayTime < 3000) //강제 치트를 쓴 경우
            earnCoinAmount = 3200;
        else
            earnCoinAmount = (hints.Count - currentHintIndex) * 200 + 1200;

        SaveDataManager.instance.AddCoin(earnCoinAmount);
        FindAIWordGameUIManager.instance.ShowResult(currentHintIndex + 1, earnCoinAmount);

        if (SaveDataManager.instance.AddExp(1))
        {
            await UniTask.Delay(1500);
            DOVirtual.DelayedCall(1.5f, () => CardGameUIManager.instance.ShowLevelUpPopup());
        }
    }
}

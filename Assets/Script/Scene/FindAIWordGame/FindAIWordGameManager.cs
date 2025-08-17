using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class FindAIWordGameManager : MonoBehaviour
{
    private List<string> keywordTable = new List<string>
    {
        "사과", "축구", "비행기", "피카츄", "피아노"
    };

    private string currentKeyword;
    private List<string> hints;
    private int currentHintIndex = 0;

    private async void Start()
    {
        await StartNewGame();
    }

    private async UniTask StartNewGame()
    {
        // 랜덤 키워드 선택
        currentKeyword = keywordTable[Random.Range(0, keywordTable.Count)];
        HLLogger.Log("정답 키워드 선택됨 (숨김): " + currentKeyword);

        // Gemini로부터 힌트 받아오기
        hints = await GeminiApiManager.instance.GetHintsForKeyword(currentKeyword);

        if (hints == null || hints.Count == 0)
        {
            HLLogger.LogError("힌트를 불러오지 못했습니다.");
            return;
        }

        currentHintIndex = 0;
        ShowNextHint();
    }

    private void ShowNextHint()
    {
        if (currentHintIndex < hints.Count)
        {
            HLLogger.Log($"힌트 {currentHintIndex + 1}: {hints[currentHintIndex]}");
        }
        else
        {
            HLLogger.Log("모든 힌트를 다 사용했습니다. 게임 오버!");
        }
    }

    public void OnPlayerAnswer(string answer)
    {
        if (answer.Trim() == currentKeyword)
        {
            HLLogger.Log("정답! 게임 클리어 🎉");
        }
        else
        {
            currentHintIndex++;
            if (currentHintIndex >= hints.Count)
            {
                HLLogger.Log("실패! 정답은 " + currentKeyword);
            }
            else
            {
                ShowNextHint();
            }
        }
    }
}

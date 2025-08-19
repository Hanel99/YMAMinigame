using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FindAIWordGameInGameView : MonoBehaviour
{
    public static FindAIWordGameInGameView instance { get; private set; }
    public Text tryCountText;
    public Text hintText;
    public InputField answer;

    private void Awake()
    {
        instance = this;
    }

    public void InitUI()
    {
        tryCountText.text = "";
        hintText.text = "";
        answer.text = "";
    }


    public void UpdateTryCountText(int count)
    {
        tryCountText.text = $"시도 횟수 : {count}";
    }

    public void UpdateHintText(string hint)
    {
        hintText.text = hint;
    }

    public void ResetAnswerField()
    {
        answer.text = "";
    }


    public void OnClickPause()
    {
        FindAIWordGameUIManager.instance.ShowPausePopup();
    }

    public void OnClickSubmit()
    {
        FindAIWordGameManager.instance.OnPlayerAnswer(answer.text);
    }
}

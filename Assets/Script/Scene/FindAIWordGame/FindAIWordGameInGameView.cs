using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FindAIWordGameInGameView : MonoBehaviour
{
    public static FindAIWordGameInGameView instance { get; private set; }
    public Text tryCountText;
    public InputField answer;

    private void Awake()
    {
        instance = this;
    }


    public void UpdateTryCountText(int count)
    {
        tryCountText.text = $"시도 횟수 : {count}";
    }


    public void OnClickPause()
    {
        CardGameUIManager.instance.ShowPausePopup();
    }

    public void OnClickSubmit()
    {
        FindAIWordGameManager.instance.OnPlayerAnswer(answer.text);
    }
}

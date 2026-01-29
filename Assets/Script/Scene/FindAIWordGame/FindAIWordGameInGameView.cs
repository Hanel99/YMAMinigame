using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class FindAIWordGameInGameView : MonoBehaviour
{
    public static FindAIWordGameInGameView instance { get; private set; }
    public Text tryCountText;
    public Text hintText;
    public InputField answerField;
    public Button answerButton;

    private void Awake()
    {
        instance = this;
    }

    public void InitUI()
    {
        tryCountText.text = "";
        hintText.text = "";
        answerField.text = "";
        answerButton.interactable = false;
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
        answerField.text = "";
    }

    public void EnableAnswerButton(bool enable)
    {
        answerButton.interactable = enable;
    }

    public void SetInputFieldFocus()
    {
        EventSystem.current.SetSelectedGameObject(answerField.gameObject);
        answerField.ActivateInputField();
    }

    public void OnClickPause()
    {
        FindAIWordGameUIManager.instance.ShowPausePopup();
    }

    public void OnClickSubmit()
    {
        if (answerButton.interactable == false)
            return;

        FindAIWordGameManager.instance.OnPlayerAnswer(answerField.text);
    }
}

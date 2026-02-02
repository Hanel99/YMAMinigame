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
    public Button nextHintButton;
    public Button giveUpButton;

    private const string MSG_NEXT_HINT = "다음 힌트 확인";
    private const string MSG_GIVE_UP = "포기하셨습니다.";

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
        nextHintButton.interactable = false;
        giveUpButton.interactable = false;
    }


    public void UpdateTryCountText(int count)
    {
        tryCountText.text = $"시도 횟수 : {count}";
    }

    public void UpdateHintText(string hint)
    {
        hintText.text = hint;
    }

    public void GlitchHintText(string text)
    {
        SoundManager.instance.PlaySFX("SFX_glitch");
        hintText.GetComponent<GlitchText>().ChangeTextWithGlitch(text);
    }

    public void ResetAnswerField()
    {
        answerField.text = "";
    }

    public void EnableButtons(bool enable)
    {
        answerButton.interactable = enable;
        nextHintButton.interactable = enable;
        giveUpButton.interactable = enable;
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
        if (CheckButtonInteractable() == false)
            return;

        FindAIWordGameManager.instance.OnPlayerAnswer(answerField.text);
    }

    public void OnClickNextHint()
    {
        if (CheckButtonInteractable() == false)
            return;

        FindAIWordGameManager.instance.OnPlayerAnswer(MSG_NEXT_HINT);
    }

    public void OnClickGiveUp()
    {
        if (CheckButtonInteractable() == false)
            return;

        FindAIWordGameManager.instance.OnPlayerAnswer(MSG_GIVE_UP);
    }

    private bool CheckButtonInteractable()
    {
        return answerButton.interactable;
    }
}

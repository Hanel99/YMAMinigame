using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
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

    [Header("Animations")]
    [SerializeField] private Color correctColor = Color.green;
    [SerializeField] private Color wrongColor = Color.red;
    [SerializeField] private Color defaultColor = Color.white;

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

    public async UniTask UpdateHintWithTyping(string baseText, string newHint)
    {
        hintText.DOKill();
        int targetLength = newHint.Length;

        // 신규 힌트 타이핑 연출
        await DOTween.To(() => 0, x =>
        {
            hintText.text = baseText + newHint.Substring(0, x);
        }, targetLength, 0.3f).SetEase(Ease.Linear).AsyncWaitForCompletion();

        // 최종 텍스트 설정 (개행 포함)
        hintText.text = baseText + newHint + "\n";
    }

    public void UpdateHintText(string hint, bool isTyping = false)
    {
        hintText.DOKill();
        // 1. 타이핑 연출이 아니거나 텍스트가 비어있으면 즉시 변경
        if (!isTyping || string.IsNullOrEmpty(hint))
        {
            hintText.text = hint;
            return;
        }

        string currentText = hintText.text;

        // 2. 기존 텍스트가 유지되면서 뒤에 추가되는 상황인지 확인 (Append 조건)
        if (!string.IsNullOrEmpty(currentText) && hint.StartsWith(currentText))
        {
            int startCharCount = currentText.Length;
            DOTween.To(() => startCharCount, x =>
            {
                hintText.text = hint.Substring(0, x);
            }, hint.Length, 0.3f).SetEase(Ease.Linear);
        }
        else
        {
            // 3. 텍스트가 완전히 바뀌는 상황
            hintText.text = "";
            hintText.DOText(hint, 0.3f).SetEase(Ease.Linear);
        }
    }

    public async UniTask ShowResultEffect(bool isCorrect)
    {
        if (isCorrect)
        {
            SoundManager.instance.PlaySFX(SFXType.Victory);
            hintText.DOColor(correctColor, 0.3f);
            // 아주 살짝 커졌다 돌아오는 은은한 연출로 변경
            hintText.transform.DOScale(1.05f, 0.2f).SetLoops(2, LoopType.Yoyo);
        }
        else
        {
            SoundManager.instance.PlaySFX(SFXType.X);
            hintText.DOColor(wrongColor, 0.1f).OnComplete(() => hintText.DOColor(defaultColor, 0.5f));
            // 흔들림 강도를 절반 이하로 대폭 축소 (15f -> 5f)
            hintText.transform.DOShakePosition(0.3f, 5f, 15, 90f);
        }

        await UniTask.Delay(300);
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

        AnimateButtonClick(answerButton);
        FindAIWordGameManager.instance.OnPlayerAnswer(answerField.text);
    }

    public void OnClickNextHint()
    {
        if (CheckButtonInteractable() == false)
            return;

        AnimateButtonClick(nextHintButton);
        FindAIWordGameManager.instance.OnPlayerAnswer(MSG_NEXT_HINT);
    }

    public void OnClickGiveUp()
    {
        if (CheckButtonInteractable() == false)
            return;

        AnimateButtonClick(giveUpButton);
        FindAIWordGameManager.instance.OnPlayerAnswer(MSG_GIVE_UP);
    }

    private void AnimateButtonClick(Button button)
    {
        button.transform.DOScale(0.95f, 0.05f).OnComplete(() => button.transform.DOScale(1f, 0.05f));
    }

    private bool CheckButtonInteractable()
    {
        return answerButton.interactable;
    }
}

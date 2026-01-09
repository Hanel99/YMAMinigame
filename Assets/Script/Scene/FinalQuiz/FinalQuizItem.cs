using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FinalQuizItem : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    public Text quizText;
    public Text option1Text;
    public Text option2Text;
    public Text option3Text;

    public Image bgImage;
    public Image option1Image;
    public Image option2Image;
    public Image option3Image;




    void Awake()
    {
        // var rectTransform = GetComponent<RectTransform>();

        // #if UNITY_ANDROID
        //         rectTransform.sizeDelta = new Vector2(700, 100);
        // #else
        //         rectTransform.sizeDelta = new Vector2(1100, 100);
        // #endif
    }


    public void SetQuizData(FinalQuizMetaData quizData)
    {
        quizText.text = $"Q{quizData.index}\n{quizData.quiz}";
        option1Text.text = quizData.option1;
        option2Text.text = quizData.option2;
        option3Text.text = quizData.option3;
    }

    public void SetQuizPhase2Color()
    {
        bgImage.color = new Color32(100, 100, 100, 1);
        option1Image.color = new Color32(180, 180, 180, 1);
        option2Image.color = new Color32(180, 180, 180, 1);
        option3Image.color = new Color32(180, 180, 180, 1);
    }

    public void FlipQuizItem(bool moveUp)
    {
        float localMoveFrom = moveUp ? 0f : -550f;
        float localMoveTo = moveUp ? 550f : 0f;
        float fadeFrom = moveUp ? 1 : 0;
        float fadeTo = moveUp ? 0 : 1;

        gameObject.SetActive(true);

        canvasGroup.transform.DOLocalMoveY(localMoveTo, 0.3f).From(localMoveFrom).SetEase(Ease.InCubic);
        canvasGroup.DOFade(fadeTo, 0.3f).From(fadeFrom).SetEase(Ease.InCubic);
    }

    public void OnClickOption(int optionNumber)
    {
        FinalQuizManager.instance.CheckIsAnswer(optionNumber);
    }
}

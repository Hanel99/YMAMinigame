using UnityEngine;
using UnityEngine.UI;

public class FinalQuizItem : MonoBehaviour
{
    public Text quizText;
    public Text option1Text;
    public Text option2Text;
    public Text option3Text;

    private int answer = 0;


    public void SetQuizData(FinalQuizMetaData quizData)
    {
        quizText.text = quizData.quiz;
        option1Text.text = quizData.option1;
        option2Text.text = quizData.option2;
        option3Text.text = quizData.option3;


    }


    public void OnClickOption(int optionNumber)
    {
        FinalQuizManager.instance.CheckIsAnswer(optionNumber);
    }


}

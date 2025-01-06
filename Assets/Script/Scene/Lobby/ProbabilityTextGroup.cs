using System;
using UnityEngine.UI;
using UnityEngine;

public class ProbabilityTextGroup : MonoBehaviour
{
    public Text rangeText;
    public Text amountText;

    public void SetText(CardGrade grade)
    {
        var value = StaticGameData.RandomValue[5 - (int)grade];
        var maxValue = StaticGameData.RandomValue[(int)CardGrade.Count - 1];

        if (grade != CardGrade.Black)
        {
            var upperValue = StaticGameData.RandomValue[5 - (int)grade - 1];
            value -= upperValue;
        }
        float percent = (float)value / (float)maxValue;

        rangeText.text = LocalizeManager.instance.GetString($"grade.desc.{grade.ToString()}");
        amountText.text = $"{percent * 100}%";
    }
}

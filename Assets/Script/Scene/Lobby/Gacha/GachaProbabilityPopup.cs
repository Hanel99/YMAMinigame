using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GachaProbabilityPopup : PopupBase
{
    public static GachaProbabilityPopup instance { get; private set; }
    public List<ProbabilityTextGroup> textList = new List<ProbabilityTextGroup>();


    protected override void OnAwake()
    {
        instance = this;
    }


    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            SetData();
            _OpenUI();
        }
        else
        {
            _CloseWindow();
        }
    }


    private void SetData()
    {
        for (int i = 0; i < (int)CardGrade.Count; ++i)
            textList[i].SetText((CardGrade)((int)CardGrade.Count - i - 1));
    }
}

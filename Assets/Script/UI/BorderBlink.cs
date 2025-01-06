using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class BorderBlink : MonoBehaviour
{
    private Image border;
    private float borderAniFrom = 0.5f;
    private float borderAniDuration = 0.5f;
    private Ease borderAniEase = Ease.InSine;

    private void Start()
    {
        if (border == null)
        {
            var borderObject = transform.Find("bgBorder");
            border = borderObject.GetComponent<Image>();
        }

        StartAnimation();
    }

    public void StartAnimation()
    {
        border.DOKill();
        border.DOFade(1, borderAniDuration).From(borderAniFrom).SetEase(borderAniEase).SetLoops(-1, LoopType.Yoyo);
    }

}

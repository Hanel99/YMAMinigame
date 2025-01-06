using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SceneAnimation : MonoBehaviour
{
    public RectTransform upImage;
    public RectTransform downImage;
    public GameObject cover;

    float from;
    float to;
    float duration = 0.5f;
    Ease ease = Ease.OutQuad;
    float height = Screen.height * 2;
    bool isAniActing = false;

    public void ShowAnimation(bool showOpen, Action callback)
    {
        if (isAniActing) return;

        isAniActing = true;
        cover.SetActive(true);
        this.gameObject.SetActive(true);

        height = GameObject.Find("Canvas").GetComponent<RectTransform>().rect.height;
        from = showOpen ? 0f : height;
        to = showOpen ? height : 0f;

        upImage.DOLocalMoveY(to, duration).From(from).SetEase(ease);
        downImage.DOLocalMoveY(-to, duration).From(-from).SetEase(ease).OnComplete(() =>
        {
            isAniActing = false;
            cover.SetActive(false);

            if (showOpen)
                this.gameObject.SetActive(false);
            else
                callback?.Invoke();
        });



    }





}

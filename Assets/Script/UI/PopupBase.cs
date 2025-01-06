using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;
using UnityEngine.UI;


public class PopupBase : MonoBehaviour
{
    public bool IsActive() => gameObject.activeSelf;

    private Action closeCallBack = null;
    protected RectTransform bgTransform;
    private Image dim;
    private Image border;
    public bool isActBackKey = true;

    private RectTransform _rectTransform = null;
    public RectTransform rectTransform
    {
        get
        {
            if (_rectTransform == null)
                _rectTransform = transform as RectTransform;
            return _rectTransform;
        }
    }


    // == Awake ==============================================================================================================================================================

    #region Awake

    private void Awake()
    {
        OnAwake();
        SetBaseUIObjects();
        SetUIObjects();
        SetBaseData();
    }

    protected virtual void OnAwake()
    { }


    private void SetBaseUIObjects()
    {
        // gameObject.SetActive(true);
        //@@@ 이걸 BG가 하고있음. 변경필요
        var bgObject = transform.Find("bg");
        if (bgObject != null)
            bgTransform = bgObject.GetComponent<RectTransform>();

        var dimObject = transform.Find("dim");
        if (dimObject != null)
            dim = dimObject.GetComponent<Image>();

        var borderObject = transform.Find("bg/bgBorder");
        if (borderObject != null)
            border = borderObject.GetComponent<Image>();
    }

    protected virtual void SetUIObjects()
    { }

    protected virtual void SetBaseData()
    { }

    #endregion Awake




    // == Start ==============================================================================================================================================================

    #region Start

    private void Start()
    {
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        OnStart();
    }

    protected virtual void OnStart()
    { }

    #endregion Start

    // == OnDestroy ==============================================================================================================================================================

    #region OnDestroy

    private void OnDestroy()
    {
        _OnDestroy();
    }

    protected virtual void _OnDestroy()
    { }

    #endregion OnDestroy

    // == Open / Close ==============================================================================================================================================================

    #region Open / Close

    public virtual void ShowPopup(bool enable)
    {
    }

    protected virtual void _OpenUI()
    {
        if (gameObject.activeInHierarchy == false)
            gameObject.SetActive(true);

        OnAnimation(true);
    }

    protected virtual void _CloseWindow()
    {
        if (gameObject.activeInHierarchy == false)
            return;

        OnAnimation(false);
    }

    public virtual void OnClickClose()
    {
        if (isOpenCloseAnimationActing) return;

        ShowPopup(false);
    }

    public void SetTitle(string text)
    {
        var titleObject = transform.Find("bg/Title");
        if (titleObject != null)
            titleObject.GetComponent<Text>().text = text;
    }



    #endregion Open / Close

    // == DOTweenAnimation & Callback ==============================================================================================================================================================

    #region DOTweenAnimation & Callback

    private float dimAniDuration = 0.3f;
    private float scaleOpenAniDuration = 0.15f;
    private float scaleCloseAniDuration = 0.1f;
    private float borderAniDuration = 2f;
    private Ease dimAniEase = Ease.OutCubic;
    private Ease scaleAniEase = Ease.OutCubic;
    private Ease borderAniEase = Ease.InQuad;
    protected bool isOpenCloseAnimationActing = false;


    private void OnAnimation(bool showOpenAnim, bool skipAnimation = false)
    {
        if (isOpenCloseAnimationActing) return;

        if (showOpenAnim)
        {
            isOpenCloseAnimationActing = true;
            if (skipAnimation)
            {
                dimAniDuration = 0f;
                scaleOpenAniDuration = 0f;
                scaleCloseAniDuration = 0f;
            }

            //팝업 열림 애니메이션
            dim.DOFade(0.7f, dimAniDuration).SetEase(dimAniEase).From(0);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(bgTransform.DOScale(new Vector3(1f, 0.01f, 1f), scaleOpenAniDuration).SetEase(scaleAniEase).From(0.01f));
            sequence.Append(bgTransform.DOScale(new Vector3(1f, 1f, 1f), scaleOpenAniDuration).SetEase(scaleAniEase));
            sequence.OnComplete(() =>
            {
                isOpenCloseAnimationActing = false;
            });

            //보더 애니메이션
            border.DOFade(1, borderAniDuration).From(0.2f).SetEase(borderAniEase).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            isOpenCloseAnimationActing = true;
            if (skipAnimation)
            {
                dimAniDuration = 0f;
                scaleOpenAniDuration = 0f;
                scaleCloseAniDuration = 0f;
            }

            //팝업 닫힘 애니메이션
            dim.DOFade(0f, dimAniDuration).SetEase(dimAniEase).From(0.7f);
            Sequence sequence = DOTween.Sequence();
            sequence.Append(bgTransform.DOScale(new Vector3(1f, 0.01f, 1f), scaleCloseAniDuration).SetEase(scaleAniEase).From(1f));
            sequence.Append(bgTransform.DOScale(new Vector3(0.01f, 0.01f, 1f), scaleCloseAniDuration).SetEase(scaleAniEase));
            sequence.OnComplete(() =>
            {
                DoCloseAnimCallback();
                isOpenCloseAnimationActing = false;
            });
        }
    }



    public void SetCloseCallBack(Action callback)
    {
        closeCallBack = callback;
    }
    private void DoCloseAnimCallback()
    {
        closeCallBack?.Invoke();
        this.gameObject.Recycle();
    }



    #endregion DOTweenAnimation & Callback
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using ChocDino.UIFX;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class IntroUIManager : MonoBehaviour
{
    public static IntroUIManager instance { get; private set; }

    public GameObject completeDim;
    public GameObject errorDim;

    public GameObject titleLogo;
    public GameObject titleText;

    public Text stateText;
    public Text versionText;
    public Text errorDimText;
    public SceneAnimation sceneDim;
    public Button removeLoginDataButton;
    public Text IDText;

    public Transform popupRoot;
    public List<PopupBase> popupList = new();



    private void Awake()
    {
        instance = this;

        // 초기 상태에서 비활성화
        if (titleLogo != null) titleLogo.SetActive(false);
        if (titleText != null) titleText.SetActive(false);
    }


    public void ShowCompleteDim(bool show)
    {
        completeDim.SetActive(show);
    }

    public void ShowErrorDim(string errorText = "")
    {
        errorDimText.text = errorText;
        errorDim.SetActive(true);
    }

    public void InitIntroText()
    {
        versionText.text = "";
        IDText.text = "";

        if (titleLogo != null) titleLogo.SetActive(false);
        if (titleText != null) titleText.SetActive(false);
    }


    public void UpdateVersionText(string version)
    {
        StringBuilder sb = new StringBuilder();

#if DEV
        sb.Append("DEV");
#else
        sb.Append("Live");
#endif
        sb.Append($"-A_{Application.version}");
        sb.Append($"-S_{version}");

        versionText.text = sb.ToString();
    }

    public void UpdateStateText(IntroState state)
    {
        switch (state)
        {
            case IntroState.Ready:
                stateText.text = "게임을 시작합니다.";
                break;

            case IntroState.InitManagers:
                stateText.text = "매니저를 초기화 합니다.";
                break;

            case IntroState.ServerUpdate:
                stateText.text = "서버 데이터를 업데이트 합니다.";
                break;

            case IntroState.LoadUserData:
                stateText.text = LocalizeManager.instance.GetString($"intro.process.{state}");
                break;

            case IntroState.PlayFabLogin:
                stateText.text = "PlayFab 로그인";
                break;

            case IntroState.Complete:
                stateText.text = "";
                break;

            default:
                stateText.text = "";
                break;
        }
    }

    public void UpdateIDText(string id)
    {
        IDText.text = $"ID : {id}";
    }

    /// <summary>
    /// 타이틀 로고와 텍스트 애니메이션 재생
    /// </summary>
    public void PlayTitleAnimation()
    {
        // 로고 연출
        if (titleLogo != null)
        {
            var logoGlow = titleLogo.GetComponent<GlowFilter>();
            var logoCG = titleLogo.GetComponent<CanvasGroup>();
            if (logoCG == null) logoCG = titleLogo.AddComponent<CanvasGroup>();

            if (logoGlow != null)
            {
                // 1초 뒤에 로고 연출 시작
                DOVirtual.DelayedCall(1f, () =>
                {
                    titleLogo.SetActive(true);
                    logoGlow.Strength = 0;
                    logoCG.alpha = 0;

                    Sequence logoSeq = DOTween.Sequence();
                    // 1초간 페이드 인 추가
                    logoSeq.Append(logoCG.DOFade(1f, 1f).From(0));
                    // 0에서 1로 상승 (유저 수정값 2f 유지)
                    logoSeq.Append(DOTween.To(() => logoGlow.Strength, x => logoGlow.Strength = x, 1f, 2f).SetEase(Ease.InQuad));
                    // 1에서 0.15로 감소 (유저 수정값 1f 유지)
                    logoSeq.Append(DOTween.To(() => logoGlow.Strength, x => logoGlow.Strength = x, 0.15f, 1f).SetEase(Ease.InOutSine));
                });
            }
        }

        // 텍스트 연출 (로고 연출 시작 후 2초 뒤, 즉 총 3초 뒤)
        if (titleText != null)
        {
            var textGlow = titleText.GetComponent<GlowFilter>();
            var textCG = titleText.GetComponent<CanvasGroup>();
            if (textCG == null) textCG = titleText.AddComponent<CanvasGroup>();

            if (textGlow != null)
            {
                // 3초 뒤에 텍스트 연출 시작
                DOVirtual.DelayedCall(3f, () =>
                {
                    titleText.SetActive(true);
                    textGlow.Strength = 0;
                    textCG.alpha = 0;

                    Sequence textSeq = DOTween.Sequence();
                    // 1초간 페이드 인 추가
                    textSeq.Append(textCG.DOFade(1f, 1f).From(0));
                    // 0에서 1로 상승 (유저 수정값 1f 유지)
                    textSeq.Append(DOTween.To(() => textGlow.Strength, x => textGlow.Strength = x, 1f, 1f).SetEase(Ease.InQuad));
                    // 0.3까지 감소 (유저 수정값 2f 유지)
                    textSeq.Append(DOTween.To(() => textGlow.Strength, x => textGlow.Strength = x, 0.3f, 2f).SetEase(Ease.OutQuad));
                    // 0.3 ~ 0.45 반복 이동 (유저 수정값 2f 유지)
                    textSeq.Append(DOTween.To(() => textGlow.Strength, x => textGlow.Strength = x, 0.45f, 2f)
                        .SetEase(Ease.InOutSine)
                        .SetLoops(-1, LoopType.Yoyo));
                });
            }
        }
    }



    public void OnClickEnterButton()
    {
        IntroController.instance.GoToGameSelectScene();
    }

    public void ShowSceneMoveAnimation(bool showOpen, Action callback = null)
    {
        sceneDim.ShowAnimation(showOpen, callback);
    }

    public void OnClickRemoveLoginDataButton()
    {
        ShowCommonPopup("로그인 정보 삭제", "로그인 정보를 초기화 하겠습니까?", false, true, true, null, () => { IntroController.instance.RemoveLoginData(); }, false);
    }





    public CommonPopup ShowCommonPopup(string titleText, string descText, bool showClose, bool showOK, bool showNo, Action closeCallback = null, Action OKCallback = null, bool isActBackKey = true)
    {
        var popup = _ShowPopup<CommonPopup>();
        popup.ShowPopup(titleText, descText, showClose, showOK, showNo, closeCallback, OKCallback);
        popup.isActBackKey = isActBackKey;
        return popup;
    }

    public void ShowLoginPopup()
    {
        _ShowPopup<LoginPopup>().ShowPopup();
    }

    public void ShowRegisterPopup()
    {
        _ShowPopup<RegisterPopup>().ShowPopup();
    }


    //매개변수 없는 팝업의 경우
    public void ShowPopup<T>() where T : PopupBase
    {
        _ShowPopup<T>().ShowPopup(true);
    }

    private T _ShowPopup<T>() where T : PopupBase
    {
        var popupName = typeof(T).Name;

        var popup = GameResourceManager.instance.GetPopup<T>(popupRoot);
        if (popup != null)
        {
            popupList.RemoveAll(x => x == null);
            popupList.Add(popup);
        }
        return popup;
    }

    public void CloseAllPopup()
    {
        foreach (var popup in popupList)
        {
            popup.ShowPopup(false);
        }
    }
}

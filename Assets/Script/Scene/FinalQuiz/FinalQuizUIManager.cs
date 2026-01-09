using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FinalQuizUIManager : MonoBehaviour
{
    public static FinalQuizUIManager instance { get; private set; }

    public Transform popupRoot;
    public List<PopupBase> popupList = new();

    [Header("Root")]
    public Transform rootDim;
    public SceneAnimation sceneDim;



    [Header("Intro")]
    public Text examInfoText;
    public Text lifeInfoText;
    public Text touchToStartText;



    [Header("InGame")]
    public GameObject examStartText;
    public FinalQuizItem[] quizItems;
    public Text totalLeftTimeText;
    public Text quizLeftTimeText;
    public Text wrongCountText;
    public Image OImage;
    public Image XImage;


    [Header("Panel")]

    public FinalQuizGameResultPanel gameResultPanel;
    public FinalQuizEndingPanel endingCreditPanel;


    [Header("ETC")]

    public GameObject dimObject;
    public Text dimText;



    //private
    private float quizTotalLeftTimeMax = 0;
    private float quizLeftTimeMax = 0;



    private void Awake()
    {
        instance = this;
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

    public void CloseTopPopup()
    {
        if (popupList.Count > 0)
        {
            popupList.RemoveAll(x => x == null);
            var popup = popupList[popupList.Count - 1];
            var popupBase = popup.GetComponent<PopupBase>();

            if (popupBase.isActBackKey)
                popupBase.OnClickClose();
        }
    }

    public bool IsPopupOpen()
    {
        popupList.RemoveAll(x => x == null);
        return popupList.Count > 0;
    }

    public void ShowSceneMoveAnimation(bool showOpen, Action callback = null)
    {
        sceneDim.ShowAnimation(showOpen, callback);
    }




    // InGame UI Logic    

    //Intro

    public void SetIntroTextData(int examTryCount, float totalTime)
    {
        examInfoText.text = $"제 <size=90>{examTryCount}</size>회\n연모아 벽반 가입 인증시험";
        lifeInfoText.text = $"제한 시간 : {totalTime:00}초\n\n오답 허용 범위 : {examTryCount}회";
    }

    public void UpdateMaxTime()
    {
        quizTotalLeftTimeMax = FinalQuizManager.instance.QuizTotalLeftTimeMax;
        quizLeftTimeMax = FinalQuizManager.instance.QuizLeftTimeMax;
    }

    public async UniTask IntroUIAnimation()
    {
        await UniTask.Delay(2000);

        examInfoText.gameObject.SetActive(true);
        await UniTask.Delay(1000);

        lifeInfoText.gameObject.SetActive(true);
        await UniTask.Delay(1000);

        touchToStartText.gameObject.SetActive(true);
        touchToStartText.DOFade(0.4f, 0.4f).SetEase(Ease.InCubic).From(1).SetLoops(-1, LoopType.Yoyo);

        FinalQuizManager.instance.SetNextState();
        FinalQuizManager.instance.StartStateProcess();
    }


    //phase 1 & 2 quizItem

    public void ShowQuizItem(FinalQuizMetaData quizData)
    {
        if (quizData == null) return;

        int showItemIndex = quizData.index % 2;
        quizItems[showItemIndex].SetQuizData(quizData);
        quizItems[showItemIndex].FlipQuizItem(false);
    }

    public void FadeOutQuizItem(FinalQuizMetaData quizData)
    {
        if (quizData == null || quizData.index <= 1) return;

        int showItemIndex = (quizData.index + 1) % 2;
        quizItems[showItemIndex].FlipQuizItem(true);
    }





    #region state UI Setting

    // intro

    public void SetIntroUI()
    {
        totalLeftTimeText.gameObject.SetActive(false);
        quizLeftTimeText.gameObject.SetActive(false);

        examInfoText.gameObject.SetActive(false);
        lifeInfoText.gameObject.SetActive(false);
        touchToStartText.gameObject.SetActive(false);
        examStartText.gameObject.SetActive(false);

        quizItems[0].gameObject.SetActive(false);
        quizItems[1].gameObject.SetActive(false);

        totalLeftTimeText.gameObject.SetActive(false);
        quizLeftTimeText.gameObject.SetActive(false);
        wrongCountText.gameObject.SetActive(false);
        OImage.gameObject.SetActive(false);
        XImage.gameObject.SetActive(false);

        gameResultPanel.gameObject.SetActive(false);
        endingCreditPanel.gameObject.SetActive(false);

    }




    // Phase 1
    public void SetPhase1UI()
    {
        touchToStartText.DOKill();
        touchToStartText.gameObject.SetActive(false);
        examInfoText.gameObject.SetActive(false);
        lifeInfoText.gameObject.SetActive(false);

        totalLeftTimeText.gameObject.SetActive(true);
        quizLeftTimeText.gameObject.SetActive(true);

        examStartText.gameObject.SetActive(true);
        examStartText.GetComponent<Text>().DOFade(0f, 0.4f).From(1f).SetEase(Ease.InCubic);
        examStartText.transform.DOScale(10f, 0.4f).From(1f).SetEase(Ease.InCubic).OnComplete(() => { examStartText.gameObject.SetActive(false); });
    }





    #endregion




    public void UpdateTimerText(float quizLeftTotalTime, float quizLeftTime)
    {
        totalLeftTimeText.text = $"남은 시간 : {quizLeftTotalTime:F1}초";
        quizLeftTimeText.text = $"남은 시간 : {quizLeftTime:F1}초";
    }










    public void OnClickExitButton()
    {
#if UNITY_EDITOR && DEV
        FinalQuizManager.instance.MoveScene();

#endif
    }

    public void ShowDim(bool show, string text = "")
    {
        dimText.text = text;
        dimObject.SetActive(show);
    }
}

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
    public Image bg;
    public GameObject examStartText;
    public FinalQuizItem[] quizItems;
    public Text totalLeftTimeText;
    public Text quizLeftTimeText;
    public Text wrongCountText;
    public Image OImage;
    public Image XImage;
    public Slider quizProgressSlider;
    public Text countdownText;


    [Header("Phase1Clear")]
    public GameObject phaseClearPanel;
    public Image phaseClearDim;
    public Image warningIcon;
    public Image warningTextImage;


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



    public void ShowCountdown(int time)
    {
        if (countdownText == null) return;

        countdownText.text = time.ToString();
        countdownText.gameObject.SetActive(true);

        countdownText.transform.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(-5, 5));
        countdownText.DOFade(0f, 0.8f).From(1f).SetEase(Ease.OutQuad);
        countdownText.transform.DOScale(3f, 0.8f).From(1f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            countdownText.gameObject.SetActive(false);
        });
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

    public async UniTask OAnimation()
    {
        OImage.gameObject.SetActive(true);
        await UniTask.Delay(200);
        OImage.gameObject.SetActive(false);
        await UniTask.Delay(200);
        OImage.gameObject.SetActive(true);
        await UniTask.Delay(600);
        OImage.gameObject.SetActive(false);
    }


    public async UniTask XAnimation()
    {
        XImage.gameObject.SetActive(true);
        XImage.color = Color.white;

        Sequence seq = DOTween.Sequence();
        seq.Append(XImage.rectTransform.DOShakeAnchorPos(1f, new Vector3(30f, 0f, 0f), 20, 90, false, true));
        seq.Join(XImage.DOFade(0f, 0.5f).SetDelay(0.5f).From(1f));
        await seq.Play().AsyncWaitForCompletion();

        XImage.gameObject.SetActive(false);
    }






    #region State Animation

    public async UniTask ShowPhase1CompleteAnimation()
    {
        //페이즈 1 클리어
        warningIcon.gameObject.SetActive(false);
        warningTextImage.gameObject.SetActive(false);
        phaseClearPanel.gameObject.SetActive(true);

        // 완료된 것 처럼 흰 배경 밝아짐
        phaseClearDim.gameObject.SetActive(true);
        phaseClearDim.DOFade(0.6f, 2f).From(0f).SetEase(Ease.Linear);
        await UniTask.Delay(2000);

        // 갑자기 멈추면서 느낌표 마크
        warningIcon.gameObject.SetActive(true);
        await UniTask.Delay(1000);

        // 오류 이미지 켜짐
        warningTextImage.gameObject.SetActive(true);
        await UniTask.Delay(3000);

        // 지직 거린 뒤 배경 까만색 전환
        // 타이머 연출 추가 (10초 늘어남, 문제당 카운트 추가)
        // 브금 다시 켜지면서 다음 퀴즈 등장

        phaseClearDim.DOFade(0f, 1f).From(0.6f).SetEase(Ease.Linear);
        bg.DOColor(new Color32(40, 40, 40, 255), 1f).SetEase(Ease.Linear);
        quizItems[0].SetQuizPhase2Color();
        quizItems[1].SetQuizPhase2Color();
        await UniTask.Delay(1000);

        phaseClearDim.gameObject.SetActive(false);
        warningIcon.gameObject.SetActive(false);
        warningTextImage.gameObject.SetActive(false);

        FinalQuizManager.instance.SetNextState();
        FinalQuizManager.instance.StartStateProcess();
    }

    public void ShowPhase2CompleteAnimation()
    {
        //페이즈 2 클리어

        // 진짜 완료.
        // 검은 배경이 하얗게 밝아짐
        // 이 시점에서 세이브 저장
        // 이번엔 타이머도 같이 사라짐

    }


    public void ShowGameOverAnimation()
    {

    }

    public void ShowCompleteAnimation()
    {

    }




    #endregion





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
        quizProgressSlider.gameObject.SetActive(false);
        countdownText.gameObject.SetActive(false);

        gameResultPanel.gameObject.SetActive(false);
        endingCreditPanel.gameObject.SetActive(false);
        phaseClearPanel.gameObject.SetActive(false);
    }




    // Phase 1
    public void SetPhase1UI()
    {
        touchToStartText.DOKill();
        touchToStartText.gameObject.SetActive(false);
        examInfoText.gameObject.SetActive(false);
        lifeInfoText.gameObject.SetActive(false);

        totalLeftTimeText.gameObject.SetActive(true);
        wrongCountText.gameObject.SetActive(true);

        examStartText.gameObject.SetActive(true);
        examStartText.GetComponent<Text>().DOFade(0f, 0.4f).From(1f).SetEase(Ease.InCubic);
        examStartText.transform.DOScale(10f, 0.4f).From(1f).SetEase(Ease.InCubic).OnComplete(() => { examStartText.gameObject.SetActive(false); });

        quizProgressSlider.value = 0;
        quizProgressSlider.gameObject.SetActive(true);
    }

    // Phase 2
    public void SetPhase2UI()
    {
        quizLeftTimeText.gameObject.SetActive(true);
    }





    #endregion




    public void UpdateTimerText(float quizLeftTotalTime, float quizLeftTime)
    {
        totalLeftTimeText.text = $"잔여 시간 : {quizLeftTotalTime:F1}초";
        quizLeftTimeText.text = $"제한 시간 : {quizLeftTime:F1}초";
    }


    public void TimerAddPhase2TimeAnimation(float quizLeftTotalTime, float plusTime)
    {
        //TODO 시간 추가 연출 만들어야 함

        // totalLeftTimeText.text = $"잔여 시간 : {quizLeftTotalTime:F1}초";
        // quizLeftTimeText.text = $"제한 시간 : {quizLeftTime:F1}초";
    }

    public void UpdateQuizProgress(int index, bool immediate = false)
    {
        int temp = index % 10;
        float value = (float)temp / 10;

        if (immediate)
            quizProgressSlider.value = value;
        else
        {
            quizProgressSlider.DOKill();
            quizProgressSlider.DOValue(value, 0.2f).SetEase(Ease.InCubic);
        }
    }

    public void UpdateWrongCount(int wrongCount)
    {
        wrongCountText.text = $"오답\n{wrongCount}";
    }










    public void OnClickExitButton()
    {
#if UNITY_EDITOR && DEV
        FinalQuizManager.instance.MoveLobbyScene();

#endif
    }

    public void ShowDim(bool show, string text = "")
    {
        dimText.text = text;
        dimObject.SetActive(show);
    }
}

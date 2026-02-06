using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;



public class CardGameUIManager : MonoBehaviour
{
    public static CardGameUIManager instance { get; private set; }

    public Transform popupRoot;
    public List<PopupBase> popupList = new();

    [Header("Root")]
    public CardGameInGameView inGamePopup;
    public CardGameCheckCardPopup checkCardPopup;

    public Transform rootDim;
    public SceneAnimation sceneDim;





    private void Awake()
    {
        instance = this;
    }


    public void ResetInGameUI()
    {
        inGamePopup.gameObject.SetActive(true);
        checkCardPopup.gameObject.SetActive(false);
        rootDim.gameObject.SetActive(true);
    }
    public void ShowRootDim(bool show)
    {
        rootDim.gameObject.SetActive(show);
    }






    // Popup Queue
    private Queue<Action> popupQueue = new Queue<Action>();

    public void AddPopupToQueue(Action popupAction)
    {
        popupQueue.Enqueue(popupAction);
    }

    public void ShowNextPopup()
    {
        if (popupQueue.Count > 0)
        {
            var action = popupQueue.Dequeue();
            action?.Invoke();
        }
    }

    public bool IsPopupQueueEmpty => popupQueue.Count == 0;


    public void ShowResult(int touchCount, int earnCoinAmount, List<int> collectCardIdList)
    {
        // 결과 팝업 닫힘 콜백 제거 (큐 시스템 사용 예정)
        _ShowPopup<GameResultPopup>().ShowPopup(touchCount, earnCoinAmount, collectCardIdList, SceneName.YMAMatch2CardGame, null);
    }

    public void ShowCardCheckPopup(List<Card> selectCardList, Action callback = null)
    {
        checkCardPopup.gameObject.SetActive(true);
        checkCardPopup.StartCheckProcess(selectCardList, callback);
    }

    public void ShowNewCardPopup(List<int> newCardIdList, Action onClose = null)
    {
        var popup = _ShowPopup<GachaResultPopup>();
        popup.SetTitle("New Card");
        popup.ShowPopup(newCardIdList, null);

        if (onClose != null)
            popup.SetCloseCallBack(onClose);
    }

    public void ShowNewCardDetailPopup(CardMetaData data)
    {
        _ShowPopup<CollectionDetailPopup>().ShowPopup(data);
    }

    public void ShowPausePopup()
    {
        _ShowPopup<PausePopup>().ShowPopup();
    }



    public void ShowReferPopup(GameType gameType, Action onClose = null)
    {
        var popup = _ShowPopup<FinalQuizReferPopup>();
        popup.ShowPopup(gameType);

        if (onClose != null)
            popup.SetCloseCallBack(onClose);
    }





    //매개변수 없는 팝업의 경우
    public void ShowPopup<T>(Action onClose = null) where T : PopupBase
    {
        var popup = _ShowPopup<T>();
        popup.ShowPopup(true);

        if (onClose != null)
            popup.SetCloseCallBack(onClose);
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
}

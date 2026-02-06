using System;
using System.Collections.Generic;
using UnityEngine;


public class FindAIWordGameUIManager : MonoBehaviour
{
    public static FindAIWordGameUIManager instance { get; private set; }

    public Transform popupRoot;
    public List<PopupBase> popupList = new();

    [Header("Root")]
    public Transform rootDim;
    public SceneAnimation sceneDim;





    private void Awake()
    {
        instance = this;
    }


    public void ResetInGameUI()
    {
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


    public void ShowResult(int touchCount, int earnCoinAmount)
    {
        // 결과 팝업 닫힘 콜백 제거 (큐 시스템 사용 예정)
        _ShowPopup<GameResultPopup>().ShowPopup(touchCount, earnCoinAmount, null, SceneName.YMAFindAIWordGame, null);
    }

    public void ShowPausePopup()
    {
        _ShowPopup<PausePopup>().ShowPopup();
    }

    public void ShowLevelUpPopup(Action onClose = null)
    {
        var popup = _ShowPopup<LevelUpPopup>();
        popup.ShowPopup(true);
        if (onClose != null)
            popup.SetCloseCallBack(onClose);
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

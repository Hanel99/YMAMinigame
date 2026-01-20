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






    public void ShowResult(int touchCount, int earnCoinAmount, List<int> collectCardIdList)
    {
        _ShowPopup<GameResultPopup>().ShowPopup(touchCount, earnCoinAmount, collectCardIdList, SceneName.YMAMatch2CardGame, () => ShowNewCardPopup(collectCardIdList));
    }

    public void ShowCardCheckPopup(List<Card> selectCardList, Action callback = null)
    {
        checkCardPopup.gameObject.SetActive(true);
        checkCardPopup.StartCheckProcess(selectCardList, callback);
    }

    public void ShowNewCardPopup(List<int> newCardIdList)
    {
        var popup = _ShowPopup<GachaResultPopup>();
        popup.SetTitle("New Card");
        popup.ShowPopup(newCardIdList, null);
    }

    public void ShowNewCardDetailPopup(CardMetaData data)
    {
        _ShowPopup<CollectionDetailPopup>().ShowPopup(data);
    }

    public void ShowPausePopup()
    {
        _ShowPopup<PausePopup>().ShowPopup();
    }



    public void ShowReferPopup(GameType gameType)
    {
        _ShowPopup<FinalQuizReferPopup>().ShowPopup(gameType);
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
}

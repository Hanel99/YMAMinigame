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
    public List<GameObject> popupList = new();


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
        var popup = ShowPopup<CardGameResultPopup>();
        popup.GetComponent<CardGameResultPopup>().ShowPopup(touchCount, earnCoinAmount, collectCardIdList);
    }

    public void ShowCardCheckPopup(List<Card> selectCardList, Action callback = null)
    {
        checkCardPopup.gameObject.SetActive(true);
        checkCardPopup.StartCheckProcess(selectCardList, callback);
    }

    public void ShowNewCardPopup(List<int> newCardIdList)
    {
        var popup = ShowPopup<GachaResultPopup>();
        var resultPopup = popup.GetComponent<GachaResultPopup>();
        resultPopup.SetTitle("New Card");
        resultPopup.ShowPopup(newCardIdList, null);
    }

    public void ShowNewCardDetailPopup(CardMetaData data)
    {
        var popup = ShowPopup<CollectionDetailPopup>();
        popup.GetComponent<CollectionDetailPopup>().ShowPopup(data);
    }

    public void ShowPausePopup()
    {
        var popup = ShowPopup<PausePopup>();
        popup.GetComponent<PausePopup>().ShowPopup();
    }

    public void ShowLevelUpPopup()
    {
        var popup = ShowPopup<LevelUpPopup>();
        popup.GetComponent<LevelUpPopup>().ShowPopup();
    }


    public GameObject ShowPopup<T>()
    {
        var popupName = typeof(T).Name;

        var popup = ResourceManager.instance.GetPopup(popupName, popupRoot);
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

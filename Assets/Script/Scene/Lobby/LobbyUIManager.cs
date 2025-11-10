using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager instance { get; private set; }

    public Transform popupRoot;
    public List<PopupBase> popupList = new();
    public SceneAnimation sceneDim;


    private void Awake()
    {
        instance = this;
    }


    public CommonPopup ShowCommonPopup(string titleText, string descText, bool showClose, bool showOK, bool showNo, Action closeCallback = null, Action OKCallback = null)
    {
        var popup = _ShowPopup<CommonPopup>();
        popup.ShowPopup(titleText, descText, showClose, showOK, showNo, closeCallback, OKCallback);
        return popup;
    }
    public void ShowGameSelectPopup(GameType type)
    {
        _ShowPopup<GameSelectPopup>().ShowPopup(type);
    }

    public void ShowCollectionDetailPopup(CardMetaData data)
    {
        _ShowPopup<CollectionDetailPopup>().ShowPopup(data);
    }

    public void ShowGachaResultPopup(List<int> cardIdList, List<int> newCardIdList)
    {
        _ShowPopup<GachaResultPopup>().ShowPopup(cardIdList, newCardIdList);
    }
    public void ShowHowToPlayPopup(string descKey)
    {
        _ShowPopup<HowToPlayPopup>().ShowPopup(descKey);
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

            if (popup.isActBackKey)
                popup.OnClickClose();
        }
    }

    public bool IsPopupOpen()
    {
        popupList.RemoveAll(x => x == null);
        return popupList.Count > 0;
    }

    public void ShowClosePopup()
    {
        ShowCommonPopup("게임 종료", "게임을 종료하시겠습니까?", true, true, true, null,
        () =>
        {
            PlayFabManager.instance.SavePlayerData(true, () => Application.Quit());
        });
    }

    public void ShowSceneMoveAnimation(bool showOpen, Action callback = null)
    {
        sceneDim.ShowAnimation(showOpen, callback);
    }
}

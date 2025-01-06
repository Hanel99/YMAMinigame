using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    public static LobbyUIManager instance { get; private set; }

    public Transform popupRoot;
    public List<GameObject> popupList = new();
    public SceneAnimation sceneDim;


    private void Awake()
    {
        instance = this;
    }


    public CommonPopup ShowCommonPopup(string titleText, string descText, bool showClose, bool showOK, bool showNo, Action closeCallback = null, Action OKCallback = null)
    {
        var popup = ShowPopup<CommonPopup>();
        popup.GetComponent<CommonPopup>().ShowPopup(titleText, descText, showClose, showOK, showNo, closeCallback, OKCallback);
        return popup.GetComponent<CommonPopup>();
    }
    public void ShowGameSelectPopup(GameType type)
    {
        var popup = ShowPopup<GameSelectPopup>();
        popup.GetComponent<GameSelectPopup>().ShowPopup(type);
    }

    public void ShowSettingPopup()
    {
        var popup = ShowPopup<SettingPopup>();
        popup.GetComponent<SettingPopup>().ShowPopup();
    }

    public void ShowUserProfilePopup()
    {
        var popup = ShowPopup<UserProfilePopup>();
        popup.GetComponent<UserProfilePopup>().ShowPopup();
    }

    public void ShowCollectionPopup()
    {
        var popup = ShowPopup<CollectionPopup>();
        popup.GetComponent<CollectionPopup>().ShowPopup();
    }

    public void ShowCollectionDetailPopup(CardMetaData data)
    {
        var popup = ShowPopup<CollectionDetailPopup>();
        popup.GetComponent<CollectionDetailPopup>().ShowPopup(data);
    }

    public void ShowPlayerDataSettingPopup()
    {
        var popup = ShowPopup<PlayerDataSettingPopup>();
        popup.GetComponent<PlayerDataSettingPopup>().ShowPopup();
    }

    public void ShowGachaPopup()
    {
        var popup = ShowPopup<GachaPopup>();
        popup.GetComponent<GachaPopup>().ShowPopup();
    }

    public void ShowGachaResultPopup(List<int> cardIdList, List<int> newCardIdList)
    {
        var popup = ShowPopup<GachaResultPopup>();
        popup.GetComponent<GachaResultPopup>().ShowPopup(cardIdList, newCardIdList);
    }

    public void ShowGachaProbabilityPopup()
    {
        var popup = ShowPopup<GachaProbabilityPopup>();
        popup.GetComponent<GachaProbabilityPopup>().ShowPopup();
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

    public void ShowClosePopup()
    {
        ShowCommonPopup("게임 종료", "게임을 종료하시겠습니까?", true, true, true, null,
        () =>
        {
            Application.Quit();
        });
    }

    public void ShowSceneMoveAnimation(bool showOpen, Action callback = null)
    {
        sceneDim.ShowAnimation(showOpen, callback);
    }
}

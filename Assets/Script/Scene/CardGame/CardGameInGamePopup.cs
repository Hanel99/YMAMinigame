using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGameInGameView : MonoBehaviour
{
    public static CardGameInGameView instance { get; private set; }


    public Transform cardRoot;
    public Text tryCountText;
    public List<Card> cardList = new();
    public bool isAllOpen => cardList.FindAll(x => x.isShow == false).Count == 0;


    private void Awake()
    {
        instance = this;
    }


    public void InitCard()
    {
        for (int i = 0; i < cardList.Count; ++i)
        {
            cardList[i].Init(i);
        }
        ResetCardUI();
    }

    public void ResetCardUI()
    {
        foreach (var item in cardList)
        {
            item.ShowCardImage(false);
            item.RemoveCardData();
        }
    }

    public void CloseCardUI(int index)
    {
        if (index < 0 || index >= cardList.Count) return;
        cardList[index].ShowCardImage(false);
    }

    public void SetCardUI(List<int> cardIdList)
    {
        for (int i = 0; i < cardList.Count; ++i)
        {
            cardList[i].SetImage(cardIdList[i]);
        }
    }

    public void UpdateTryCountText(int count)
    {
        tryCountText.text = $"시도 횟수 : {count}";
    }


    public void OnClickPause()
    {
        CardGameUIManager.instance.ShowPausePopup();
    }


}

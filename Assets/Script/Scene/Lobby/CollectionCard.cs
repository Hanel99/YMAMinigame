using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionCard : MonoBehaviour
{
    public Image mask;
    public Image frontImage;
    public Image border;
    public Image backImage;

    private CardMetaData cardMetaData;
    private int _cardNumber;
    private bool _isShow = false;


    public int cardId => cardMetaData.Id;
    public int cardNumber => _cardNumber;
    public bool isShow => _isShow;



    public void Init(int number)
    {
        _isShow = false;
        _cardNumber = number;
    }

    public void SetImage(int id, bool showCard = true)
    {
        SetCardData(id);
        SetCardImage();
        ShowCardImage(showCard);
    }


    private void SetCardData(int id)
    {
        cardMetaData = ResourceManager.instance.GetCardMetaData(id);
    }

    private void SetCardImage()
    {
        if (cardMetaData == null)
        {
            HLLogger.Log("@@@ card data is null");
            return;
        }

        frontImage.sprite = ResourceManager.instance.GetCardImage(cardMetaData.ImageNumber);
        border.color = StaticGameData.GetGradeBorderColor(cardMetaData.Grade);
    }

    public void ShowCardImage(bool showFront)
    {
        mask.gameObject.SetActive(showFront);
        frontImage.gameObject.SetActive(showFront);
        border.transform.parent.gameObject.SetActive(showFront);
        backImage.gameObject.SetActive(!showFront);
        _isShow = showFront;
    }


    public void OnClickCard()
    {
        if (_isShow == false) return;

        HLLogger.Log($"@@@ click {cardMetaData.Id} card");
        LobbyUIManager.instance.ShowCollectionDetailPopup(cardMetaData);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public Image mask;
    public Image frontImage;
    public Image border;
    public Image backImage;

    //@@@ temp
    public Text cardIdText;

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

    public void SetImage(int id, bool showCard = false)
    {
        SetCardData(id);
        SetCardImage();
        ShowCardImage(showCard);
    }


    private void SetCardData(int id)
    {
        cardMetaData = ResourceManager.instance.GetCardMetaData(id);
        cardIdText.text = StaticGameData.showDevTestText ? cardMetaData.Id.ToString() : "";
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

    public void RemoveCardData()
    {
        cardMetaData = null;
    }

    public void ShowCardImage(bool showFront)
    {
        mask.gameObject.SetActive(showFront);
        frontImage.gameObject.SetActive(showFront);
        border.gameObject.SetActive(showFront);
        backImage.gameObject.SetActive(!showFront);
        _isShow = showFront;
    }


    public void OnClickCard()
    {
        if (_isShow) return;

        HLLogger.Log($"@@@ click card : {this.gameObject.name} / {cardMetaData.Id}");
        ShowCardImage(true);

        CardGameManager.instance.CardClickProcess(this);
    }


}

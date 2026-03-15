using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private Image mask;
    [SerializeField] private Image frontImage;
    [SerializeField] private Image border;
    [SerializeField] private Image backImage;

    //@@@ temp
    [SerializeField] private Text cardIdText;

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
        cardMetaData = GameResourceManager.instance.GetCardMetaData(id);
#if DEV
        cardIdText.text = cardMetaData.Id.ToString();
#else
        cardIdText.text = "";
#endif
    }

    private void SetCardImage()
    {
        if (cardMetaData == null)
        {
            HLLogger.Log("@@@ card data is null");
            return;
        }

        frontImage.sprite = GameResourceManager.instance.GetCardImage(cardMetaData.ImageNumber);
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
        if (_isShow || DOTween.IsTweening(transform)) return;

        Flip(true);
        CardGameManager.instance.CardClickProcess(this);
    }

    public void Flip(bool showFront, bool playSound = true, Action onComplete = null)
    {
        if (_isShow == showFront)
        {
            onComplete?.Invoke();
            return;
        }

        float duration = 0.2f;
        transform.DOPunchPosition(new Vector3(-15f, 0, 0), duration * 2, 0, 0);
        transform.DORotate(new Vector3(0, 90, 0), duration).SetEase(Ease.InQuad).OnComplete(() =>
        {
            ShowCardImage(showFront);
            transform.DORotate(new Vector3(0, 0, 0), duration).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                onComplete?.Invoke();
            });
        });

        if (playSound)
            SoundManager.instance.PlaySFX(SFXType.CardFlip);
    }


}

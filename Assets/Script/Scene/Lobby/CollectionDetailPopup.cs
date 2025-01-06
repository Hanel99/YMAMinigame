using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionDetailPopup : PopupBase
{
    public static CollectionDetailPopup instance { get; private set; }

    public Image cardImage;
    public Text nameText;
    public Text numberText;
    public Text gradeText;
    public Text masterText;
    public Text descText;

    private CardMetaData cardData;


    protected override void OnAwake()
    {
        instance = this;
    }

    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            _OpenUI();
        }
        else
        {
            _CloseWindow();
        }
    }


    public void ShowPopup(CardMetaData data)
    {
        cardData = data;
        UpdateCardData();
        ShowPopup();
    }


    public void UpdateCardData()
    {
        cardImage.sprite = ResourceManager.instance.GetCardImage(cardData.ImageNumber);

        nameText.text = LocalizeManager.instance.GetString($"card.name.{cardData.Id}");
        numberText.text = $"No.{cardData.Id.ToString("D3")}";
        gradeText.text = LocalizeManager.instance.GetString($"grade.name.{cardData.Grade}");
        masterText.text = LocalizeManager.instance.GetString($"master.name.{cardData.Master}");
        descText.text = LocalizeManager.instance.GetString($"card.desc.{cardData.Id}");
    }
}

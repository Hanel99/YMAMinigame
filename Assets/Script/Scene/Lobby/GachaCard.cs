using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GachaCard : MonoBehaviour
{
    public Image frontImage;
    public Image border;
    public GameObject newIcon;
    public Text newText => newIcon.GetComponent<Text>();
    public Outline newOutline => newIcon.GetComponent<Outline>();


    private CardMetaData cardMetaData;


    public void SetImage(int id, bool showNew = false)
    {
        SetCardData(id);
        SetCardImage(showNew);
    }


    private void SetCardData(int id)
    {
        cardMetaData = ResourceManager.instance.GetCardMetaData(id);
    }

    private void SetCardImage(bool showNew)
    {
        if (cardMetaData == null)
        {
            HLLogger.Log("@@@ card data is null");
            return;
        }

        frontImage.sprite = ResourceManager.instance.GetCardImage(cardMetaData.ImageNumber);
        border.color = StaticGameData.GetGradeBorderColor(cardMetaData.Grade);

        newIcon.SetActive(showNew);
        if (showNew)
        {
            newText.color = StaticGameData.GetGradeBorderColor(cardMetaData.Grade);
            newOutline.effectColor = StaticGameData.GetGradeReverseBorderColor(cardMetaData.Grade);
        }
    }

    public void OnClickCard()
    {
        HLLogger.Log($"@@@ click {cardMetaData.Id} card");
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.Equals(SceneName.LobbyScene.ToString()))
            LobbyUIManager.instance.ShowCollectionDetailPopup(cardMetaData);
        if (sceneName.Equals(SceneName.YMAMatch2CardGame.ToString()))
            CardGameUIManager.instance.ShowNewCardDetailPopup(cardMetaData);

    }
}
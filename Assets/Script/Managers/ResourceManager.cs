using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{

    //! GameResourceManager로 리소스 데이터 옮기기!  
    //! 데이터 다 옮기고 삭제 예정

    public static ResourceManager instance { get; private set; }

    public ResourceScriptableData resourceScriptableData;



    [Header("- ExcelData")]
    public CardData cardData;
    public StringData stringData;
    public LevelData levelData;



    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }


    #region GetSprite


    public Sprite GetCardImage(int imageNumber)
    {
        Sprite sprite = resourceScriptableData.cardImages.Find(x => x.name == imageNumber.ToString("D4"));

        return sprite;
    }

    public Sprite GetGameImage(GameType type)
    {
        string str = ((int)type).ToString("D2");
        Sprite sprite = resourceScriptableData.gameImages.Find(x => x.name.Contains(str));

        return sprite;
    }

    public Sprite GetMasterIcon(CardMaster master)
    {
        string str = master.ToString();
        Sprite sprite = resourceScriptableData.masterImages.Find(x => x.name.Contains(str));
        if (sprite == null)
            sprite = resourceScriptableData.masterImages.Find(x => x.name.Contains("Other"));

        return sprite;
    }




    #endregion



    #region GetMetaData

    public CardMetaData GetCardMetaData(int id)
    {
        return cardData.Data.Find(x => x.Id == id);
    }

    public List<CardMetaData> GetCardMetaData(CardMaster master)
    {
        return cardData.Data.FindAll(x => x.Master == master);
    }
    public List<CardMetaData> GetCardMetaData(CardGrade grade)
    {
        return cardData.Data.FindAll(x => x.Grade == grade);
    }

    public List<int> GetCardIds(CardMaster master)
    {
        var list = GetCardMetaData(master);
        List<int> ids = new();
        foreach (var card in list)
            ids.Add(card.Id);

        return ids;
    }
    public List<int> GetCardIds(CardGrade grade)
    {
        var list = GetCardMetaData(grade);
        List<int> ids = new();
        foreach (var card in list)
            ids.Add(card.Id);

        return ids;
    }

    public List<int> GetAllCardIds()
    {
        var list = cardData.Data;
        List<int> ids = new();
        foreach (var card in list)
        {
            ids.Add(card.Id);
        }
        return ids;
    }

    public int GetTotalCardCount()
    {
        var list = cardData.Data;
        return list.Count;
    }


    public int GetLevelRequireExp(int level)
    {
        return levelData.Data.Find(x => x.level == level).exp;
    }

    public int GetLevelUnlockValue(int level)
    {
        return levelData.Data.Find(x => x.level == level).unlock;
    }


    #endregion



    #region GetPopup

    public GameObject GetPopup(string popupName, Transform parent)
    {
        var prefab = resourceScriptableData.popups.Find(x => x.name.Equals(popupName));
        if (prefab == null)
        {
            HLLogger.LogWarning($"@@@ {popupName} is not in popupList");
            return null;
        }

        var go = prefab.Spawn(parent);
        go.transform.localScale = Vector3.one;

        return go;
    }


    #endregion






}

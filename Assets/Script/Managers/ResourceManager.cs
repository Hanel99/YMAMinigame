using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager instance { get; private set; }

    public ResourceScriptableData resourceScriptableData;

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
        return resourceScriptableData.cardData.Data.Find(x => x.Id == id);
    }

    public List<CardMetaData> GetCardMetaData(CardMaster master)
    {
        return resourceScriptableData.cardData.Data.FindAll(x => x.Master == master);
    }
    public List<CardMetaData> GetCardMetaData(CardGrade grade)
    {
        return resourceScriptableData.cardData.Data.FindAll(x => x.Grade == grade);
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
        var list = resourceScriptableData.cardData.Data;
        List<int> ids = new();
        foreach (var card in list)
        {
            ids.Add(card.Id);
        }
        return ids;
    }

    public int GetTotalCardCount()
    {
        var list = resourceScriptableData.cardData.Data;
        return list.Count;
    }


    public int GetLevelRequireExp(int level)
    {
        return resourceScriptableData.levelData.Data.Find(x => x.level == level).exp;
    }

    public int GetLevelUnlockValue(int level)
    {
        return resourceScriptableData.levelData.Data.Find(x => x.level == level).unlock;
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

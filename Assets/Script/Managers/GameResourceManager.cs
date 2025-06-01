using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;


public class GameResourceManager : MonoBehaviour
{
    // 리소스매니저, 리소스 데이터 옮기기 완료 후 리네이밍 예정
    public static GameResourceManager instance { get; private set; }

    private bool _isLoaded = false;



    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public CardData cardData;
    public StringData stringData;
    public LevelData levelData;

    public List<Sprite> cardImages = new();
    public List<Sprite> gameImages = new();
    public List<Sprite> masterImages = new();

    public GameObject cardPrefab;
    public List<GameObject> popups = new();








    public async Task LoadAsync()
    {
        if (_isLoaded) return;

        // ScriptableObject
        var dataList = await Addressables.LoadAssetsAsync<ScriptableObject>(StaticGameData.AddressLabels.data, null).Task;
        foreach (var data in dataList)
        {
            switch (data)
            {
                case CardData cd: cardData = cd; break;
                case StringData sd: stringData = sd; break;
                case LevelData ld: levelData = ld; break;
            }
        }

        // Images
        // cardImages = new List<Sprite>(await Addressables.LoadAssetsAsync<Sprite>(StaticGameData.AddressLabels.cardImage, null).Task);
        // gameImages = new List<Sprite>(await Addressables.LoadAssetsAsync<Sprite>(StaticGameData.AddressLabels.gameIcon, null).Task);
        // masterImages = new List<Sprite>(await Addressables.LoadAssetsAsync<Sprite>(StaticGameData.AddressLabels.masterImage, null).Task);

        // Prefabs
        // var popups = await Addressables.LoadAssetsAsync<GameObject>(StaticGameData.AddressLabels.popup, null).Task;
        // this.popups = new List<GameObject>(popups);

        // var gamePrefabList = await Addressables.LoadAssetsAsync<GameObject>(StaticGameData.AddressLabels.gamePrefab, null).Task;
        // cardPrefab = gamePrefabList.Count > 0 ? gamePrefabList[0] : null;

        _isLoaded = true;
    }

















    #region GetSprite


    public Sprite GetCardImage(int imageNumber)
    {
        Sprite sprite = cardImages.Find(x => x.name == imageNumber.ToString("D4"));

        return sprite;
    }

    public Sprite GetGameImage(GameType type)
    {
        string str = ((int)type).ToString("D2");
        Sprite sprite = gameImages.Find(x => x.name.Contains(str));

        return sprite;
    }

    public Sprite GetMasterIcon(CardMaster master)
    {
        string str = master.ToString();
        Sprite sprite = masterImages.Find(x => x.name.Contains(str));
        if (sprite == null)
            sprite = masterImages.Find(x => x.name.Contains("Other"));

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
        var prefab = popups.Find(x => x.name.Equals(popupName));
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

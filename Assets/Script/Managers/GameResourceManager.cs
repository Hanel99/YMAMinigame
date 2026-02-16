using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

/// <summary>
/// AddressableResourceManager에서 가져온 리소스를 관리. 데이터가 필요하면 여기서 가져와 쓰면 됨.
/// </summary>
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
    public TowerWeaponLevelData towerWeaponLevelData;
    public TowerUserLevelData towerUserLevelData;
    public TowerBossLevelData towerBossLevelData;
    public QuestData questData;
    public FinalQuizData finalQuizData;
    public TowerWeaponJewelData towerWeaponJewelData;


    public List<Sprite> cardImages = new();
    public List<Sprite> gameImages = new();
    public List<Sprite> masterImages = new();
    public List<Sprite> towerBossImages = new();
    public List<Sprite> questGradeImages = new();

    public List<GameObject> popups = new();

    public List<AudioClip> BGMList = new();
    public List<AudioClip> SFXList = new();








    //@@@ TODO 리모트로 수정 후 인트로에서 이걸로 초반에 전부 다운로드 하는 기능 추가 필요.
    public async UniTask LoadAsync()
    {
        if (_isLoaded) return;

        // ScriptableObject
        var scriptableDatas = await AddressableResourceManager.instance.LoadAssetsByLabelAsync<ScriptableObject>(StaticGameData.AddressLabels.SOData);

        foreach (var sData in scriptableDatas)
        {
            switch (sData)
            {
                case CardData data:
                    cardData = data;
                    break;
                case LevelData data:
                    levelData = data;
                    break;
                case TowerWeaponLevelData data:
                    towerWeaponLevelData = data;
                    break;
                case TowerUserLevelData data:
                    towerUserLevelData = data;
                    break;
                case TowerBossLevelData data:
                    towerBossLevelData = data;
                    break;
                case StringData data:
                    stringData = data;
                    break;
                case QuestData data:
                    questData = data;
                    break;
                case FinalQuizData data:
                    finalQuizData = data;
                    break;
                case TowerWeaponJewelData data:
                    towerWeaponJewelData = data;
                    break;
                default:
                    Debug.LogWarning($"Unknown config type: {sData.name}");
                    break;
            }
        }

        // Images - AddressableResourceManager를 통해 로드
        cardImages = await AddressableResourceManager.instance.LoadAssetsByLabelAsync<Sprite>(StaticGameData.AddressLabels.CardImage);
        gameImages = await AddressableResourceManager.instance.LoadAssetsByLabelAsync<Sprite>(StaticGameData.AddressLabels.GameImage);
        masterImages = await AddressableResourceManager.instance.LoadAssetsByLabelAsync<Sprite>(StaticGameData.AddressLabels.MasterIcon);
        towerBossImages = await AddressableResourceManager.instance.LoadAssetsByLabelAsync<Sprite>(StaticGameData.AddressLabels.TowerBossImage);
        questGradeImages = await AddressableResourceManager.instance.LoadAssetsByLabelAsync<Sprite>(StaticGameData.AddressLabels.QuestGradeImage);

        // Prefabs
        popups = await AddressableResourceManager.instance.LoadAssetsByLabelAsync<GameObject>(StaticGameData.AddressLabels.PopupGroup);
        BGMList = await AddressableResourceManager.instance.LoadAssetsByLabelAsync<AudioClip>(StaticGameData.AddressLabels.BGMGroup);
        SFXList = await AddressableResourceManager.instance.LoadAssetsByLabelAsync<AudioClip>(StaticGameData.AddressLabels.SFXGroup);


        MakePrivData();
        _isLoaded = true;
    }

    private void MakePrivData()
    {
        MakeContentUnlockLevelDic();
        MakeCardDataDic();
        MakeCardImageDic();
    }


    #region Data Caching

    private Dictionary<int, CardMetaData> _cardMetaDataDic = new();
    private Dictionary<int, Sprite> _cardImageDic = new();

    private void MakeCardDataDic()
    {
        _cardMetaDataDic.Clear();
        if (cardData == null) return;

        foreach (var data in cardData.Data)
        {
            if (!_cardMetaDataDic.ContainsKey(data.Id))
                _cardMetaDataDic.Add(data.Id, data);
        }
    }

    private void MakeCardImageDic()
    {
        _cardImageDic.Clear();
        foreach (var sprite in cardImages)
        {
            if (int.TryParse(sprite.name, out int id))
            {
                if (!_cardImageDic.ContainsKey(id))
                    _cardImageDic.Add(id, sprite);
            }
        }
    }

    #endregion


    #region GetSprite


    public Sprite GetCardImage(int imageNumber)
    {
        if (_cardImageDic.TryGetValue(imageNumber, out var sprite))
            return sprite;

        // Fallback or Reload if needed? Currently return null if not found.
        return null;
    }

    public Sprite GetGameImage(GameType type)
    {
        string str = ((int)type).ToString("D2");
        Sprite sprite = gameImages.Find(x => x.name.Contains(str));

        if (sprite == null && gameImages.Count > 0)
            sprite = gameImages[gameImages.Count - 1];

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

    public Sprite GetTowerBossImage(int bossNumber, bool isWin = false, bool isLose = false)
    {
        if (isWin)
            return towerBossImages.Find(x => x.name.Contains("Win"));
        if (isLose)
            return towerBossImages.Find(x => x.name.Contains("Lose"));

        var sprite = towerBossImages.Find(x => x.name.Contains(bossNumber.ToString("D2")));
        return sprite ?? towerBossImages.Find(x => x.name.Contains("00"));
    }

    public Sprite GetQuestGradeIcon(QuestGrade grade)
    {
        string str = grade.ToString();
        return questGradeImages.Find(x => x.name.Contains(str));
    }

    public Sprite GetQuestGradeIcon(QuestType type, bool isHidden = false)
    {
        if (isHidden)
            return questGradeImages.Find(x => x.name.Contains("Hidden"));

        return type == QuestType.Daily
            ? questGradeImages.Find(x => x.name.Contains("Daily"))
            : questGradeImages.Find(x => x.name.Contains("Normal"));
    }


    #endregion



    #region GetMetaData

    // CardData
    public CardMetaData GetCardMetaData(int id)
    {
        _cardMetaDataDic.TryGetValue(id, out var data);
        return data;
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
        return list.ConvertAll(x => x.Id);
    }
    public List<int> GetCardIds(CardGrade grade)
    {
        var list = GetCardMetaData(grade);
        return list.ConvertAll(x => x.Id);
    }

    public List<int> GetAllCardIds()
    {
        return cardData.Data.ConvertAll(x => x.Id);
    }

    public int GetTotalCardCount()
    {
        var list = cardData.Data;
        return list.Count;
    }



    // levelData
    public int GetLevelRequireExp(int level)
    {
        return levelData.Data.Find(x => x.level == level).exp;
    }

    public int GetLevelUnlockValue(int level)
    {
        return levelData.Data.Find(x => x.level == level).unlock;
    }

    private Dictionary<int, int> contentUnlockLevelDic = new();
    private void MakeContentUnlockLevelDic()
    {
        contentUnlockLevelDic.Clear();
        for (int i = 0; i < levelData.Data.Count; ++i)
        {
            var unlock = levelData.Data[i].unlock;
            if (contentUnlockLevelDic.ContainsKey(unlock) == false)
            {
                contentUnlockLevelDic.Add(unlock, levelData.Data[i].level);
            }
        }
    }

    public int GetContentUnlockLevel(UnlockContent unlockContent)
    {
        return contentUnlockLevelDic[(int)unlockContent];
    }

    public int GetContentUnlockLevel(int unlockContent)
    {
        return contentUnlockLevelDic[unlockContent];
    }


    // TowerData
    public TowerWeaponLevelMetaData GetTowerWeaponLevelMetaData(int level)
    {
        return towerWeaponLevelData.Data.Find(x => x.level == level);
    }

    public TowerUserLevelMetaData GetTowerUserLevelMetaData(int level)
    {
        return towerUserLevelData.Data.Find(x => x.level == level);
    }

    public int GetTowerUserLevelRequireCoin(TowerUserStatType type, int level)
    {
        var levelData = towerUserLevelData.Data.Find(x => x.level == level);
        int value = (type) switch
        {
            TowerUserStatType.Atk => levelData.atkRequireCoin,
            TowerUserStatType.Def => levelData.defRequireCoin,
            TowerUserStatType.HP => levelData.hpRequireCoin,
            TowerUserStatType.CriRate => levelData.criRateRequireCoin,
            TowerUserStatType.CriDmg => levelData.criDmgRequireCoin,
            _ => -1,
        };

        return value;
    }

    public T GetTowerUserLevelStatValue<T>(TowerUserStatType type, int level)
    {
        var metaData = GetTowerUserLevelMetaData(level);
        if (metaData == null)
            return default;

        object value = type switch
        {
            TowerUserStatType.Atk => (object)metaData.atk,
            TowerUserStatType.Def => (object)metaData.def,
            TowerUserStatType.HP => (object)metaData.hp,
            TowerUserStatType.CriRate => (object)metaData.criRate,
            TowerUserStatType.CriDmg => (object)metaData.criDmg,
            _ => (object)0,
        };

        return (T)System.Convert.ChangeType(value, typeof(T));
    }

    public TowerBossLevelMetaData GetTowerBossLevelMetaData(int floor)
    {
        return towerBossLevelData.Data.Find(x => x.level == floor);
    }



    // tower jewel
    public List<TowerWeaponJewelMetaData> GetTowerJewelGradeList(TowerJewelGrade grade)
    {
        return towerWeaponJewelData.Data.FindAll(x => x.grade == grade);
    }


    // questData

    public QuestMetaData GetQuestMetaData(int index)
    {
        return questData.Data.Find(x => x.id == index);
    }

    public QuestMetaData GetQuestMetaData(QuestDetailType detailType, QuestDetailType2 detailType2, int subId)
    {
        return questData.Data.Find(x => x.detailType == detailType && x.detailType2 == detailType2 && x.subId == subId);
    }

    public List<QuestMetaData> GetAllQuestMetaData()
    {
        return questData.Data;
    }

    public List<QuestMetaData> GetQuestMetaData(QuestGame questGame)
    {
        return questData.Data.FindAll(x => x.questGame == questGame);
    }


    // finalQuizData

    public FinalQuizMetaData GetFinalQuizMetaData(int index)
    {
        return finalQuizData.Data.Find(x => x.index == index);
    }

    public List<FinalQuizMetaData> GetAllFinalQuizMetaData()
    {
        return finalQuizData.Data;
    }

    #endregion

    #region GetSound

    public AudioClip GetBGM(BGMType type)
    {
        return GetBGM($"BGM_{type}");
    }

    public AudioClip GetBGM(string bgmName)
    {
        return BGMList.Find(x => x.name.Equals(bgmName));
    }

    public AudioClip GetSFX(SFXType type)
    {
        return GetSFX($"SFX_{type}");
    }

    public AudioClip GetSFX(string sfxName)
    {
        return SFXList.Find(x => x.name.Equals(sfxName));
    }

    #endregion



    #region GetPopup

    public T GetPopup<T>(Transform parent) where T : PopupBase
    {
        string popupName = typeof(T).Name;

        var prefab = popups.Find(x => x.name.Equals(popupName));
        if (prefab == null)
        {
            HLLogger.LogWarning($"@@@ {popupName} is not in popupList");
            return null;

            //TODO @@@ 없으면 어드레서블에서 로드 시도를 하고 싶었는데 비동기인지라 어싱크로 처리해야 해서 일단 주석처리
            // var loadPopup = await AddressableResourceManager.instance.LoadPopupAsync<T>(popupName);
            // if (loadPopup == null)
            // {
            //     HLLogger.LogError($"@@@ Failed to load popup: {popupName}");
            //     return null;
            // }
            // popups.Add(loadPopup);
        }

        // GameObject에서 요구하는 팝업 컴포넌트를 가져옴
        var popup = prefab.GetComponent<T>();
        if (popup == null)
        {
            HLLogger.LogError($"@@@ {popupName} does not have component of type {typeof(T).Name}");
            return null;
        }

        var go = prefab.Spawn(parent);
        go.transform.localScale = Vector3.one;

        return go.GetComponent<T>();
    }


    #endregion
}

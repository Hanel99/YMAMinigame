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
        var scriptableDatas = await AddressableResourceManager.instance.LoadAllScriptableDataAsync(StaticGameData.AddressLabels.SOData);

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
                default:
                    Debug.LogWarning($"Unknown config type: {sData.name}");
                    break;
            }
        }

        // Images - AddressableResourceManager를 통해 로드
        cardImages = await AddressableResourceManager.instance.LoadSpritesByLabelAsync(StaticGameData.AddressLabels.CardImage);
        gameImages = await AddressableResourceManager.instance.LoadSpritesByLabelAsync(StaticGameData.AddressLabels.GameImage);
        masterImages = await AddressableResourceManager.instance.LoadSpritesByLabelAsync(StaticGameData.AddressLabels.MasterIcon);
        towerBossImages = await AddressableResourceManager.instance.LoadSpritesByLabelAsync(StaticGameData.AddressLabels.TowerBossImage);
        questGradeImages = await AddressableResourceManager.instance.LoadSpritesByLabelAsync(StaticGameData.AddressLabels.QuestGradeImage);

        // Prefabs
        popups = await AddressableResourceManager.instance.LoadPrefabsByLabelAsync(StaticGameData.AddressLabels.PopupGroup);
        BGMList = await AddressableResourceManager.instance.LoadAudioClipsByLabelAsync(StaticGameData.AddressLabels.BGMGroup);
        SFXList = await AddressableResourceManager.instance.LoadAudioClipsByLabelAsync(StaticGameData.AddressLabels.SFXGroup);

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

        if (sprite == null)
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
        Sprite sprite = null;
        if (isWin)
            sprite = towerBossImages.Find(x => x.name.Contains("Win"));
        else if (isLose)
            sprite = towerBossImages.Find(x => x.name.Contains("Lose"));
        else
        {
            sprite = towerBossImages.Find(x => x.name.Contains(bossNumber.ToString("D2")));
        }
        if (sprite == null)
            sprite = towerBossImages.Find(x => x.name.Contains("00"));

        return sprite;
    }

    public Sprite GetQuestGradeIcon(QuestGrade grade)
    {
        string str = grade.ToString();
        Sprite sprite = questGradeImages.Find(x => x.name.Contains(str));
        if (sprite == null)
            sprite = null;

        return sprite;
    }

    public Sprite GetQuestGradeIcon(QuestType type, bool isHidden = false)
    {
        if (isHidden)
            return questGradeImages.Find(x => x.name.Contains("Hidden"));
        else if (type == QuestType.Daily)
            return questGradeImages.Find(x => x.name.Contains("Daily"));
        else
            return questGradeImages.Find(x => x.name.Contains("Normal"));
    }


    #endregion



    #region GetMetaData

    // CardData
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



    // levelData
    public int GetLevelRequireExp(int level)
    {
        return levelData.Data.Find(x => x.level == level).exp;
    }

    public int GetLevelUnlockValue(int level)
    {
        return levelData.Data.Find(x => x.level == level).unlock;
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
            TowerUserStatType.atk => levelData.atkRequireCoin,
            TowerUserStatType.def => levelData.defRequireCoin,
            TowerUserStatType.hp => levelData.hpRequireCoin,
            TowerUserStatType.criRate => levelData.criRateRequireCoin,
            TowerUserStatType.criDmg => levelData.criDmgRequireCoin,
            _ => -1,
        };

        return value;
    }

    public T GetTowerUserLevelStatValue<T>(TowerUserStatType type, int level)
    {
        return (T)typeof(TowerUserLevelMetaData).GetField(type.ToString()).GetValue(GetTowerUserLevelMetaData(level));
    }

    public TowerBossLevelMetaData GetTowerBossLevelMetaData(int floor)
    {
        return towerBossLevelData.Data.Find(x => x.level == floor);
    }



    // questData

    public QuestMetaData GetQuestMetaData(int index)
    {
        return questData.Data.Find(x => x.id == index);
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

    #endregion

    #region GetSound

    public AudioClip GetBGM(BGMType type)
    {
        string bgmName = $"BGM_{type}";
        return BGMList.Find(x => x.name.Equals(bgmName));
    }

    public AudioClip GetSFX(SFXType type)
    {
        string sfxName = $"SFX_{type}";
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

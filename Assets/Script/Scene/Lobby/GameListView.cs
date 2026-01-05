using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameListView : MonoBehaviour
{
    public static GameListView instance { get; private set; }

    public GameObject settingBtn;
    public GameObject userProfileBtn;
    public GameObject gameQuitBtn;

    public LobbyButton gachaBtn;
    public LobbyButton towerBtn;
    public LobbyButton rankingBtn;
    public LobbyButton collectionBtn;
    public LobbyButton questBtn;
    public LobbyButton finalQuizBtn;

    public List<GameSelectIcon> gameList = new();

    //userProfile
    public Image userIcon;
    public Text userName;
    public Text userLevel;
    public Text userExp;
    public Slider expSlider;



    private void Awake()
    {
        instance = this;
    }




    private void Start()
    {
        StartGameViewSettingProcess();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (LobbyUIManager.instance.IsPopupOpen())
                LobbyUIManager.instance.CloseTopPopup();
            else
                LobbyUIManager.instance.ShowClosePopup();
        }

#if UNITY_EDITOR && DEV
        // if (Input.GetKeyDown(KeyCode.L))
        // {
        //     if (SaveDataManager.instance.AddExp(100, true))
        //     {
        //         LobbyUIManager.instance.ShowPopup<LevelUpPopup>();
        //     }
        //     UpdateUserProfileProcess();
        // }



        // if (Input.GetKeyDown(KeyCode.A))
        //     SoundManager.instance.PlaySFX(SFXType.BtnOK);
        // if (Input.GetKeyDown(KeyCode.S))
        //     SoundManager.instance.PlayLoopSFX(SFXType.Warning);
        // if (Input.GetKeyDown(KeyCode.D))
        //     SoundManager.instance.StopLoopSFX();

        // if (Input.GetKeyDown(KeyCode.Z))
        //     SoundManager.instance.PlayBGM(BGMType.Lobby, 0f);
        // if (Input.GetKeyDown(KeyCode.X))
        //     SoundManager.instance.PlayBGM(BGMType.Lobby, 1f);
        // if (Input.GetKeyDown(KeyCode.C))
        //     SoundManager.instance.FadeOutBGM();

        // if (Input.GetKeyDown(KeyCode.U))
        //     QuestManager.instance.AddQuestProgress(QuestDetailType.ReachTotalTouchCount, QuestDetailType2.None, 10);
        // if (Input.GetKeyDown(KeyCode.I))
        // {
        //     var s1 = QuestManager.instance.GetQuestValue(QuestDetailType.ReachTotalTouchCount, QuestDetailType2.None);
        //     HLLogger.Log($"@@@ Quest Value : {s1} ");
        // }


        if (Input.GetKeyDown(KeyCode.G))
            HLLogger.Log(SaveDataManager.instance.JsonPlayerData);
#endif
    }

    private void StartGameViewSettingProcess()
    {
        Application.targetFrameRate = StaticGameData.targetFrameRate;
        if (SaveDataManager.instance.playerData.geminiHints2 != null && SaveDataManager.instance.playerData.geminiHints2.Count < 5)
        {
            int loopCount = 5 - SaveDataManager.instance.playerData.geminiHints2.Count;
            for (int i = 0; i < loopCount; ++i)
                GeminiApiManager.instance.SaveHintsPreload().Forget();
        }

        for (int i = 0; i < gameList.Count; ++i)
        {
            if (i >= (int)GameType.Count)
                gameList[i].gameObject.SetActive(false);
            else
                gameList[i].SetGameData((GameType)i);
        }

        settingBtn.GetComponent<Button>().onClick.AddListener(OnClickSettingButton);
        userProfileBtn.GetComponent<Button>().onClick.AddListener(OnClickUserProfileButton);
        gameQuitBtn.GetComponent<Button>().onClick.AddListener(OnClickGameQuit);

        gachaBtn.SetButtonData(LobbyIconType.Gacha, OnClickGachaButton);
        towerBtn.SetButtonData(LobbyIconType.Tower, OnClickTowerButton);
        rankingBtn.SetButtonData(LobbyIconType.Ranking, OnClickRankingButton);
        collectionBtn.SetButtonData(LobbyIconType.Collection, OnClickCollectionButton);
        questBtn.SetButtonData(LobbyIconType.Quest, OnClickQuestButton);
        finalQuizBtn.SetButtonData(LobbyIconType.FinalQuiz, OnClickFinalQuizButton);

        UpdateUserProfileProcess();
        IntroDataProcess();
        LobbyUIManager.instance.ShowSceneMoveAnimation(true);
        SoundManager.instance.PlayBGM(BGMType.Lobby);
    }



    public void UpdateUserProfileProcess()
    {
        var playerData = SaveDataManager.instance.playerData;

        userIcon.sprite = GameResourceManager.instance.GetMasterIcon(playerData.master);
        userName.text = playerData.name;
        userLevel.text = playerData.level.ToString();
        if (playerData.maxExp < 0)
        {
            userExp.text = $"MAX";
            expSlider.value = 1;
        }
        else
        {
            userExp.text = $"{playerData.exp}/{playerData.maxExp}";
            expSlider.value = (float)playerData.exp / (float)playerData.maxExp;
        }

    }



    private void IntroDataProcess()
    {
        if (SaveDataManager.instance.playerData.isNewUser)
        {
            SaveDataManager.instance.playerData.isNewUser = false;
            CommonPopup popup = null;
            popup = LobbyUIManager.instance.ShowCommonPopup("환영합니다!", "연모아 게임에 오신걸 환영합니다!\n게임에서 사용하실 이름과 연모아에서 사용중인 이름을 선택해주세요.", false, true, false, null, () =>
            {
                LobbyUIManager.instance.ShowPopup<PlayerDataSettingPopup>();
                popup.ShowPopup(false);
            });
            popup.isActBackKey = false;
        }
        else if (StaticGameData.introData.isFirstLogin)
        {
            StaticGameData.introData.isFirstLogin = false;
            SaveDataManager.instance.AddCoin(StaticGameData.introData.firstLoginCoinAmount);
            LobbyUIManager.instance.ShowCommonPopup("데일리 보너스", $"오늘 첫 로그인 기념으로\n{StaticGameData.introData.firstLoginCoinAmount} 골드를 드립니다.", true, true, false);
        }
    }




    public void OnClickSettingButton()
    {
        LobbyUIManager.instance.ShowPopup<SettingPopup>();
    }

    public void OnClickUserProfileButton()
    {
        LobbyUIManager.instance.ShowPopup<UserProfilePopup>();
    }

    public void OnClickGameQuit()
    {
        LobbyUIManager.instance.ShowClosePopup();
    }



    public void OnClickGachaButton()
    {
        LobbyUIManager.instance.ShowPopup<GachaPopup>();
    }

    public void OnClickTowerButton()
    {
        LobbyUIManager.instance.ShowPopup<TowerGamePopup>();
    }

    public void OnClickRankingButton()
    {
        LobbyUIManager.instance.ShowPopup<RankingPopup>();
    }
    public void OnClickCollectionButton()
    {
        LobbyUIManager.instance.ShowPopup<CollectionPopup>();
    }

    public void OnClickQuestButton()
    {
        LobbyUIManager.instance.ShowPopup<QuestPopup>();
    }

    public void OnClickFinalQuizButton()
    {
        // LobbyUIManager.instance.ShowPopup<QuestPopup>();
    }






}

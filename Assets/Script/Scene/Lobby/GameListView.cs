using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameListView : MonoBehaviour
{
    public static GameListView instance { get; private set; }

    public GameObject settingBtn;
    public GameObject userProfileBtn;
    public GameObject gachaBtn;
    public GameObject collectionBtn;
    public GameObject gameQuitBtn;

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

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.L))
            LobbyUIManager.instance.ShowLevelUpPopup();

        // if (Input.GetKeyDown(KeyCode.U))
        //     UpdateUserProfileProcess();

        // if (Input.GetKeyDown(KeyCode.G))
        //     LobbyUIManager.instance.ShowGachaProbabilityPopup();
#endif
    }

    private void StartGameViewSettingProcess()
    {
        Application.targetFrameRate = 60;
        SaveDataManager.instance.RemoveNotUseCardList();
        // CheckServerMaintenance();
        //@@@ 필요한가?

        for (int i = 0; i < gameList.Count; ++i)
        {
            if (i == (int)GameType.MatchCardGame)
                gameList[0].SetGameData(GameType.MatchCardGame);
            else
                gameList[i].gameObject.SetActive(false);
        }

        settingBtn.GetComponent<Button>().onClick.AddListener(OnClickSettingButton);
        userProfileBtn.GetComponent<Button>().onClick.AddListener(OnClickUserProfileButton);
        gachaBtn.GetComponent<Button>().onClick.AddListener(OnClickGachaButton);
        collectionBtn.GetComponent<Button>().onClick.AddListener(OnClickCollectionButton);
        gameQuitBtn.GetComponent<Button>().onClick.AddListener(OnClickGameQuit);

        UpdateUserProfileProcess();
        IntroDataProcess();
        LobbyUIManager.instance.ShowSceneMoveAnimation(true);
    }


    private void CheckServerMaintenance()
    {
        ServerManager.instance.CheckServerMaintenance((sheetData) =>
        {
            CommonPopup popup = null;
            switch (sheetData)
            {
                case "0":
                    //이상 없음. 접속 가능
                    break;

                case "1":
                    //DevTest만 입장 가능
#if !DevTest
                    popup = LobbyUIManager.instance.ShowCommonPopup("공지", $"서버 점검 중입니다.\nCode.{sheetData}", false, true, false, null, () =>
                   {
                       Application.Quit();
                   });
                    popup.isActBackKey = false;
#endif
                    break;

                case "2":
                    //Editor만 입장 가능
#if !UNITY_EDITOR
                    popup = LobbyUIManager.instance.ShowCommonPopup("공지", $"서버 점검 중입니다.\nCode.{sheetData}", false, true, false, null, () =>
                    {
                        Application.Quit();
                    });
                    popup.isActBackKey = false;
#endif
                    break;

                default:
                    popup = LobbyUIManager.instance.ShowCommonPopup("공지", $"서버 점검 중입니다.\nCode.{sheetData}", false, true, false, null, () =>
                    {
                        Application.Quit();
                    });
                    popup.isActBackKey = false;
                    break;
            }
        });
    }


    public void UpdateUserProfileProcess()
    {
        var playerData = SaveDataManager.instance.playerData;

        userIcon.sprite = ResourceManager.instance.GetMasterIcon(playerData.master);
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
        if (StaticGameData.introData.isNewUser)
        {
            StaticGameData.introData.isNewUser = false;
            CommonPopup popup = null;
            popup = LobbyUIManager.instance.ShowCommonPopup("환영합니다!", "연모아 게임에 오신걸 환영합니다!\n게임에서 사용하실 이름과 연모아에서 사용중인 이름을 선택해주세요.", false, true, false, null, () =>
            {
                LobbyUIManager.instance.ShowPlayerDataSettingPopup();
                popup.ShowPopup(false);
            });
            popup.isActBackKey = false;
        }
        else if (StaticGameData.introData.isFirstLogin)
        {
            StaticGameData.introData.isFirstLogin = false;
            LobbyUIManager.instance.ShowCommonPopup("데일리 보너스", $"오늘 첫 로그인 기념으로\n{StaticGameData.introData.firstLoginCoinAmount} 골드를 드립니다.", true, true, false);
        }
    }




    public void OnClickSettingButton()
    {
        LobbyUIManager.instance.ShowSettingPopup();
    }

    public void OnClickUserProfileButton()
    {
        LobbyUIManager.instance.ShowUserProfilePopup();
    }

    public void OnClickGachaButton()
    {
        LobbyUIManager.instance.ShowGachaPopup();
    }

    public void OnClickCollectionButton()
    {
        LobbyUIManager.instance.ShowCollectionPopup();
    }

    public void OnClickGameQuit()
    {
        LobbyUIManager.instance.ShowClosePopup();
    }




}

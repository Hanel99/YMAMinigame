using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Text;

public class IntroController : MonoBehaviour
{
    public class IntroData
    {
        public bool isFirstLogin = false;
        public int firstLoginCoinAmount = 10000;
    }


    public static IntroController instance { get; private set; }

    public IntroState state = IntroState.Ready;


    private void Awake()
    {
        instance = this;
    }


    void Start()
    {
        StartIntroProcess();
    }






    private void StartIntroProcess()
    {
        IntroUIManager.instance.UpdateStateText(state);
        HLLogger.Log($"Intro State : {state}");
        switch (state)
        {
            case IntroState.Ready:
                ReadyProcess();
                break;

            case IntroState.InitManagers:
                InitManagersProcess();
                break;

            case IntroState.ResourceLoad:
                ResourceLoadProcess();
                break;

            case IntroState.ServerUpdate:
#if UNITY_EDITOR && DEV
                state++;
                StartIntroProcess();
                // StartCoroutine(nameof(ServerUpdateProcess));
#else
                StartCoroutine(nameof(ServerUpdateProcess));
#endif
                break;

            case IntroState.LoadUserData:
                LoadUserDataProcess();
                break;


            case IntroState.PlayFabLogin:
                PlayFabLoginProcess();
                break;

            case IntroState.Complete:
                StartCoroutine(nameof(CompleteProcess));
                break;

            default:
                state = IntroState.Error;
                IntroUIManager.instance.ShowErrorDim();
                break;
        }
    }


    private void ReadyProcess()
    {
        IntroUIManager.instance.ShowCompleteDim(false);
        // #if UNITY_EDITOR
        //         StaticGameData.showDevTestText = true;
        // #else
        //         StaticGameData.showDevTestText = false;
        // #endif
        IntroUIManager.instance.ShowSceneMoveAnimation(true);
        IntroUIManager.instance.InitIntroText();

        state++;
        StartIntroProcess();
    }


    private void InitManagersProcess()
    {
        SaveDataManager.instance.Init();
        SoundManager.instance.InitializeSoundManager();

        Application.targetFrameRate = StaticGameData.targetFrameRate;

#if UNITY_STANDALONE_WIN
        //원래는 데이터 로드 프로세스에 해야하지만 사이즈 설정 기능상 초반에 먼저 로딩하도록 배치
        SaveDataManager.instance.LoadOtherPlayerData();
        var screenSizeData = SaveDataManager.instance.otherPlayerData;
        FullScreenMode screenMode = screenSizeData.isFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
        RefreshRate refreshRate = new RefreshRate() { numerator = (uint)StaticGameData.targetFrameRate, denominator = 1 };

        Screen.SetResolution(screenSizeData.resolutionWidth, screenSizeData.resolutionHeight, screenMode, refreshRate);
        HLLogger.Log($"@@@ Resolution : {screenSizeData.resolutionWidth}/{screenSizeData.resolutionHeight} ({refreshRate.numerator} Hz) / FS? {screenSizeData.isFullScreen}");
#endif

        state++;
        StartIntroProcess();
    }


    private void ResourceLoadProcess()
    {
        UniTask.Void(async () => await WaitGameResourceLoadAsync());
    }

    private async UniTask WaitGameResourceLoadAsync()
    {
        HLLogger.Log($"Game resources load start");
        await GameResourceManager.instance.LoadAsync();

        HLLogger.Log($"Game resources load complete");
        state++;
        StartIntroProcess();
    }


    //시트 데이터를 한번에 받아와 통합 처리.
    //서버 점검, 최소 앱 버전도 여기서 확인.
    private IEnumerator ServerUpdateProcess()
    {
        bool apiComplete = true;
        bool isServerMaintenance = false;
        bool isNeedAppUpdate = false;

        yield return null;

        apiComplete = false;
        ServerManager.instance.SendSheetAPI((sheetData) =>
        {
            var totalSheetData = ServerManager.instance.SplitSheetData(sheetData);

            for (int i = 0; i < totalSheetData.Count; i++)
            {
                var data = totalSheetData[i];
                if (data.Count() == 0) continue;
                if (i >= (int)SheetRangeType.Count)
                {
                    HLLogger.LogWarning($"not use data : {i} / {data.Count()}");
                    continue;
                }

                var rangeType = (SheetRangeType)i;

                switch (rangeType)
                {
                    case SheetRangeType.ServerVersion:
                        SaveDataManager.instance.playerData.serverDataVersion = data[0];

                        IntroUIManager.instance.UpdateVersionText(data[0]);
                        break;

                    case SheetRangeType.EventDateTimeRange:
                        StaticGameData.UpdateEventDateTimeFromServer(data);
                        break;

                    case SheetRangeType.ServerMaintenance:
                        switch (data[0])
                        {
                            case "0":
                                //이상 없음. 접속 가능
                                break;

                            case "1":
                                //DevTest만 입장 가능
#if !DEV
                                IntroUIManager.instance.ShowErrorDim($"서버 점검 중입니다. Code.{data[0]}");
                                isServerMaintenance = true;
#endif
                                break;

                            case "2":
                                //Editor만 입장 가능
#if !UNITY_EDITOR
                                IntroUIManager.instance.ShowErrorDim($"서버 점검 중입니다. Code.{data[0]}");
                                isServerMaintenance = true;
#endif
                                break;

                            case "3":
                                //점검 테스트. 전부 접속 불가
                                IntroUIManager.instance.ShowErrorDim($"서버 점검 중입니다. Code.{data[0]}");
                                isServerMaintenance = true;
                                break;

                            default:
                                IntroUIManager.instance.ShowErrorDim($"서버 점검 중입니다. Code.{data[0]}");
                                isServerMaintenance = true;
                                break;

                        }
                        break;

                    case SheetRangeType.AppMinVersion:
                        System.Version appVersion = new System.Version(Application.version);
                        System.Version serverVersion = new System.Version(data[0]);

                        if (appVersion < serverVersion)
                        {
                            IntroUIManager.instance.ShowErrorDim($"앱 업데이트가 있습니다.\n최신 버전 앱으로 업데이트 해 주세요.\n{appVersion}/{serverVersion}");
                            isNeedAppUpdate = true;
                        }
                        break;

                    case SheetRangeType.RedeemCodes:
                        StaticGameData.UpdateRedeemCodeFromServer(data);
                        break;

                    case SheetRangeType.GameSeed:
                        StaticGameData.UpdateGameSeedFromServer(data[0]);
                        break;

                    default:
                        HLLogger.LogWarning($"Unknown SheetRangeType : {rangeType}");
                        break;
                }
            }
            apiComplete = true;

        });
        yield return new WaitUntil(() => apiComplete);

        if (isServerMaintenance) HLLogger.LogWarning($"Server maintenance");
        if (isNeedAppUpdate) HLLogger.LogWarning($"Need app update");

        if (isServerMaintenance == false && isNeedAppUpdate == false)
        {
            state++;
            StartIntroProcess();
        }
    }



    private void LoadUserDataProcess()
    {
        SaveDataManager.instance.LoadPlayerData();
        SaveDataManager.instance.LoadOtherPlayerData();

        StaticGameData.introData.isFirstLogin = SaveDataManager.instance.IsTodayFirstLogin();

        // sound data update
        SoundManager.instance.UpdateVolumes();
        SoundManager.instance.PlayBGM(BGMType.Intro);

        state++;
        StartIntroProcess();
    }

    private void PlayFabLoginProcess()
    {
        if (SaveDataManager.instance.playerData.autoLogin)
        {
            var loginIDPWData = SaveDataManager.instance.GetIDPW();
            PlayFabManager.instance.IntroUserLoginProcess(loginIDPWData.Item1, loginIDPWData.Item2, () =>
            {
                PlayFabManager.instance.GetUserData(() =>
                {
                    IntroUIManager.instance.UpdateIDText(loginIDPWData.Item1);
                    MoveNextIntroProcess();
                });
            }, (error) =>
            {
                IntroUIManager.instance.ShowCommonPopup("오류", $"PlayFab 로그인에 실패하였습니다.\n{error}", false, true, false, null, () => { IntroUIManager.instance.ShowLoginPopup(); });
            });
        }
        else
        {
            // 로그인 창 열림
            IntroUIManager.instance.ShowLoginPopup();
        }
    }


    public void MoveNextIntroProcess()
    {
        state++;
        StartIntroProcess();
    }


    private void CompleteProcess()
    {
        IntroUIManager.instance.ShowCompleteDim(true);
    }







    public void RemoveLoginData()
    {
        SaveDataManager.instance.RemoveLoginData();
        IntroUIManager.instance.ShowCommonPopup("삭제 완료", "로그인 정보가 삭제되었습니다.\n게임을 재실행 해 주십시오.", false, true, false, () => { Application.Quit(); }, () => { Application.Quit(); });
    }


    public void GoToGameSelectScene()
    {
        if (state == IntroState.Error)
        {
            Application.Quit();
            return;
        }

        if (state != IntroState.Complete)
            return;

        SceneMoveManager.instance.MoveScene(SceneName.LobbyScene);
    }
}

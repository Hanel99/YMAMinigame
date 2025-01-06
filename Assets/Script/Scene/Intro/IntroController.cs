using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    public class IntroData
    {
        public bool isNewUser = false;
        public bool isFirstLogin = false;
        public int firstLoginCoinAmount = 0;
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
        switch (state)
        {
            case IntroState.Ready:
                ReadyProcess();
                break;

            case IntroState.InitManagers:
                InitManagersProcess();
                break;

            case IntroState.CheckAppVersion:
#if UNITY_EDITOR && DevTest
                state++;
                StartIntroProcess();
#else
                CheckAppVersionProcess();
#endif
                break;

            case IntroState.CheckMaintenance:
#if UNITY_EDITOR && DevTest
                state++;
                StartIntroProcess();
#else
                CheckMaintenanceProcess();
#endif
                break;

            case IntroState.LoadUserData:
                LoadUserDataProcess();
                break;

            case IntroState.ServerUpdate:
#if UNITY_EDITOR && DevTest
                state++;
                StartIntroProcess();
                // StartCoroutine(nameof(ServerUpdateProcess));
#else
                StartCoroutine(nameof(ServerUpdateProcess));
#endif
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
        Application.targetFrameRate = 60;

        IntroUIManager.instance.ShowCompleteDim(false);
#if UNITY_EDITOR
        StaticGameData.showDevTestText = true;
#else
        StaticGameData.showDevTestText = false;
#endif
        IntroUIManager.instance.ShowSceneMoveAnimation(true);


        state++;
        StartIntroProcess();
    }

    private void InitManagersProcess()
    {
        SaveDataManager.instance.Init();

        state++;
        StartIntroProcess();
    }

    private void CheckAppVersionProcess()
    {
        IntroUIManager.instance.UpdateStateText(IntroState.CheckAppVersion);
        ServerManager.instance.SendSheetAPI(SheetRangeType.AppMinVersion, (sheetData) =>
        {
            System.Version appVersion = new System.Version(Application.version);
            System.Version serverVersion = new System.Version(sheetData);

            if (appVersion >= serverVersion)
            {
                //이상 없음. 접속 가능
                state++;
                StartIntroProcess();
            }
            else
            {
                IntroUIManager.instance.ShowErrorDim($"앱 업데이트가 있습니다.\n최신 버전 앱으로 업데이트 해 주세요.\n{appVersion}/{serverVersion}");
            }
        });
    }


    private void CheckMaintenanceProcess()
    {
        IntroUIManager.instance.UpdateStateText(IntroState.CheckMaintenance);
        ServerManager.instance.SendSheetAPI(SheetRangeType.ServerMaintenance, (sheetData) =>
        {
            switch (sheetData)
            {
                case "0":
                    //이상 없음. 접속 가능
                    state++;
                    StartIntroProcess();
                    break;

                case "1":
                    //DevTest만 입장 가능
#if DevTest
                    state++;
                    StartIntroProcess();
#else
                    IntroUIManager.instance.ShowErrorDim($"서버 점검 중입니다. Code.{sheetData}");
#endif
                    break;

                case "2":
                    //Editor만 입장 가능
#if UNITY_EDITOR
                    state++;
                    StartIntroProcess();
#else
                    IntroUIManager.instance.ShowErrorDim($"서버 점검 중입니다. Code.{sheetData}");
#endif
                    break;

                default:
                    IntroUIManager.instance.ShowErrorDim($"서버 점검 중입니다. Code.{sheetData}");
                    break;

            }
        });
    }

    private void LoadUserDataProcess()
    {
        SaveDataManager.instance.LoadPlayerData();
        StaticGameData.introData.isFirstLogin = SaveDataManager.instance.IsTodayFirstLogin();

        state++;
        StartIntroProcess();
    }

    private IEnumerator ServerUpdateProcess()
    {
        bool apiComplete = true;
        yield return null;


        //@ 1. 데이터 버전 체크
        apiComplete = false;
        IntroUIManager.instance.UpdateStateText(IntroState.ServerUpdate, "1");
        ServerManager.instance.SendSheetAPI(SheetRangeType.DataVersion, (sheetData) =>
        {
            if (SaveDataManager.instance.playerData.serverDataVersion.Equals(sheetData) == false)
                SaveDataManager.instance.playerData.serverDataVersion = sheetData;

            IntroUIManager.instance.UpdateVersionText(sheetData);
            apiComplete = true;
        });
        yield return new WaitUntil(() => apiComplete);

        //@ 2. 이벤트 타임 검증
        apiComplete = false;
        IntroUIManager.instance.UpdateStateText(IntroState.ServerUpdate, "2");
        ServerManager.instance.SendSheetAPI(SheetRangeType.EventDateTimeRange, (sheetData) =>
        {
            StaticGameData.UpdateEventDateTimeFromServer(sheetData);
            apiComplete = true;
        });
        yield return new WaitUntil(() => apiComplete);

        //@ 3. 카드 등급 랜덤 가중치 업데이트
        apiComplete = false;
        IntroUIManager.instance.UpdateStateText(IntroState.ServerUpdate, "3");
        ServerManager.instance.SendSheetAPI(SheetRangeType.RandomValue, (sheetData) =>
        {
            StaticGameData.UpdateRandomValueFromServer(sheetData);
            apiComplete = true;
        });
        yield return new WaitUntil(() => apiComplete);


        //@ 4. 오늘 첫 로그인인 경우 서버에서 지정한 로그인 보너스 코인 획득
        if (StaticGameData.introData.isFirstLogin)
        {
            apiComplete = false;

            IntroUIManager.instance.UpdateStateText(IntroState.ServerUpdate, "4");
            ServerManager.instance.SendSheetAPI(SheetRangeType.TodayFirstLoginReward, (sheetData) =>
            {
                if (int.TryParse(sheetData, out int amount))
                {
                    SaveDataManager.instance.AddCoin(amount);
                    StaticGameData.introData.firstLoginCoinAmount = amount;
                }

                apiComplete = true;
            });
            yield return new WaitUntil(() => apiComplete);
        }


        //@ 5. 가챠 가격 갱신
        apiComplete = false;
        IntroUIManager.instance.UpdateStateText(IntroState.ServerUpdate, "5");
        ServerManager.instance.SendSheetAPI(SheetRangeType.GachaPrice, (sheetData) =>
        {
            StaticGameData.UpdateGachaPriceFromServer(sheetData);
            apiComplete = true;
        });
        yield return new WaitUntil(() => apiComplete);


        //@ 6. 리딤코드 최신화
        apiComplete = false;
        IntroUIManager.instance.UpdateStateText(IntroState.ServerUpdate, "6");
        ServerManager.instance.SendSheetAPI(SheetRangeType.RedeemCodes, (sheetData) =>
        {
            StaticGameData.UpdateRedeemCodeFromServer(sheetData);
            apiComplete = true;
        });
        yield return new WaitUntil(() => apiComplete);


        //@ 00. 템플릿
        // apiComplete = false;
        // IntroUIManager.instance.UpdateStateText(IntroState.ServerUpdate, "00"); <-늘어날수록 숫자를 늘릴것.
        // ServerManager.instance.SendSheetAPI(SheetRangeType.RandomValue, (sheetData) =>  <- 받아올 시트 종류를 지정
        // {
        //     StaticGameData.UpdateRandomValueFromServer(sheetData); <- 받아온 뒤 콜백으로 데이터 사용
        //     apiComplete = true;
        // });
        // yield return new WaitUntil(() => apiComplete);



        state++;
        StartIntroProcess();
    }

    private void CompleteProcess()
    {
        IntroUIManager.instance.ShowCompleteDim(true);
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

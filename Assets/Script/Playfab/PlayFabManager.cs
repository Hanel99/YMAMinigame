using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;


public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager instance { get; private set; }
    public bool isConnecting { get; private set; } = false;
    private bool isLoginSuccess = false;

    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }


    public void Start()
    {
        isLoginSuccess = false;

        SetPlayFabTitleID();
    }

    public void SetPlayFabTitleID()
    {
#if DEV
        PlayFabSettings.staticSettings.TitleId = ConfigManager.instance.Config.playFabTitleId;
#elif LIVE
        PlayFabSettings.staticSettings.TitleId = ConfigManager.instance.Config.playFabLiveTitleId;
#endif
    }


    public void IntroUserRegisterProcess(string id, string pw, Action onSuccess, Action<PlayFabError> onFailure)
    {
        var request = new RegisterPlayFabUserRequest
        {
            // Email = idInputField.text,
            Username = id,
            Password = pw,
            RequireBothUsernameAndEmail = false,
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, result =>
        {
            Debug.Log("Register Success!");
            isLoginSuccess = true;
            onSuccess?.Invoke();
        }, error =>
        {
            Debug.LogWarning("Register Failed");
            Debug.LogError("Here's some debug information:");
            Debug.LogError(error.GenerateErrorReport());
            onFailure?.Invoke(error);
        });
    }

    public void IntroUserLoginProcess(string id, string pw, Action onSuccess, Action<PlayFabError> onFailure)
    {
        var request = new LoginWithPlayFabRequest
        {
            Username = id,
            Password = pw,
        };

        PlayFabClientAPI.LoginWithPlayFab(request, result =>
        {
            Debug.Log("Login Success!");
            isLoginSuccess = true;
            onSuccess?.Invoke();
        }, error =>
        {
            if (error.Error == PlayFabErrorCode.AccountNotFound)
            {
                Debug.Log("계정이 없음");
            }

            Debug.LogWarning("Login Failed");
            Debug.LogError("Here's some debug information:");
            Debug.LogError(error.GenerateErrorReport());
            onFailure?.Invoke(error);
        });
    }



    public void SetDisplayName()
    {
        var request = new UpdateUserTitleDisplayNameRequest { DisplayName = SaveDataManager.instance.playerData.name };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, (UpdateUserTitleDisplayNameResult) =>
        {
        },
        (error) =>
        {
            // 특별한 에러 처리
            if (error.Error == PlayFabErrorCode.NameNotAvailable)
            {
                Debug.LogError("이미 사용 중인 닉네임입니다: " + name);
            }
            else if (error.Error == PlayFabErrorCode.InvalidParams)
            {
                Debug.LogError("유효하지 않은 닉네임 형식입니다.");
            }
            else if (error.Error == PlayFabErrorCode.ProfileDoesNotExist)
            {
                Debug.LogError("프로필이 존재하지 않습니다. 먼저 로그인이 필요합니다.");
            }
            else
            {

            }
        });
    }





    public void GetUserData(Action callback)
    {
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request, (result) =>
        {
            if (result.Data.ContainsKey("useServerData"))
            {
                var tempData = result.Data["useServerData"].Value.ToLower();
                var tempText = tempData.Equals("true");
                HLLogger.Log($"use serverData? : {tempText}");
                StaticGameData.useServerData = tempText;
            }

            if (result.Data.ContainsKey("PlayerData"))
            {
                var jsonData = result.Data["PlayerData"].Value;
                SaveDataManager.instance.SaveJsonPlayerData(jsonData);
            }
            else
            {
                HLLogger.Log("No PlayerData found");
                SaveDataManager.instance.RemovePlayerData();
                SaveDataManager.instance.LoadPlayerData();
            }
            callback?.Invoke();

        }, (error) =>
        {
            callback?.Invoke();
        });
    }



    #region SaveData


    private Coroutine savePlayerDataCoroutine;
    private int saveCallCount = 0;

    // SavePlayerData를 호출하면 3초간 대기 후 저장, 중복 호출 시 기존 대기 취소
    public void SavePlayerData(bool forceSave = false, Action callback = null)
    {
        saveCallCount++;

        if (savePlayerDataCoroutine != null)
        {
            StopCoroutine(savePlayerDataCoroutine);
            savePlayerDataCoroutine = null;
        }

        if (saveCallCount >= 10 || forceSave)
        {
            // 10회 이상 호출 또는 강제 시작 시 즉시 저장
            saveCallCount = 0;
            SavePlayerDataProcess(
                () => { callback?.Invoke(); savePlayerDataCoroutine = null; },
                (err) => { savePlayerDataCoroutine = null; }
            );
        }
        else
        {
            savePlayerDataCoroutine = StartCoroutine(Co_SavePlayerData(
                () => { callback?.Invoke(); savePlayerDataCoroutine = null; },
                (err) => { savePlayerDataCoroutine = null; }
            ));
        }
    }

    IEnumerator Co_SavePlayerData(Action success, Action<string> failure)
    {
        yield return new WaitForSeconds(2f); // 2초 대기

        saveCallCount = 0; // 저장 시 카운트 초기화
        SavePlayerDataProcess(success, failure);
    }




    private void SavePlayerDataProcess(Action success, Action<string> failure)
    {
        success += () => HLLogger.Log("PlayFab SavePlayerData Success", LogColor.silver);
        failure += (str) => HLLogger.Log($"PlayFab SavePlayerData Failed", LogColor.silver);

        if (isLoginSuccess == false)
        {
            HLLogger.Log("PlayFab Not Login State - SavePlayerData Skip", LogColor.silver);
            success?.Invoke();
            return;
        }

        HLLogger.Log("@@@ PlayFab Save Request Start", LogColor.silver);

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "PlayerData", SaveDataManager.instance.JsonPlayerData },
                { "savedTime", SaveDataManager.instance.playerData.savedTime.ToString() },
                { "serverDataVersion", SaveDataManager.instance.playerData.serverDataVersion },
                { "useServerData", SaveDataManager.instance.playerData.useServerData.ToString() },
                { "endingTime", SaveDataManager.instance.playerData.finalQuizPlayData.completeTime.ToString("yyyy-MM-dd HH:mm:ss") },
                }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result => success?.Invoke(),
            error => failure?.Invoke(error.GenerateErrorReport()));

        UpdateLeaderBoard();
    }

    public string GetPlayFabErrorText(PlayFabErrorCode errorCode, string message)
    {
        switch (errorCode)
        {
            // PlayFabErrorCode enum 참조 : https://learn.microsoft.com/ko-kr/gaming/playfab/api-references/global-api-method-error-codes
            // register
            case PlayFabErrorCode.InvalidEmailAddress:
                return "유효하지 않은 이메일 주소입니다.";

            case PlayFabErrorCode.InvalidUsername:
                return "유효하지 않은 사용자명입니다.";

            case PlayFabErrorCode.InvalidPassword:
                return "잘못된 비밀번호입니다.";

            case PlayFabErrorCode.EmailAddressNotAvailable:
                return "이미 사용 중인 이메일 주소입니다.";

            case PlayFabErrorCode.UsernameNotAvailable:
                return "이미 사용 중인 사용자명입니다.";

            case PlayFabErrorCode.AccountBanned:
                return "차단된 계정입니다.";

            case PlayFabErrorCode.ServiceUnavailable:
                return "서비스를 일시적으로 사용할 수 없습니다. 잠시 후 다시 시도해주세요.";


            // login
            case PlayFabErrorCode.AccountNotFound:
                return "존재하지 않는 계정입니다.";

            case PlayFabErrorCode.InvalidUsernameOrPassword:
                return "아이디 또는 비밀번호가 올바르지 않습니다.";

            case PlayFabErrorCode.AccountDeleted:
                return "삭제된 계정입니다.";

            case PlayFabErrorCode.ExpiredAuthToken:
                return "인증 토큰이 만료되었습니다. 다시 로그인해주세요.";

            case PlayFabErrorCode.InvalidAuthToken:
                return "유효하지 않은 인증 토큰입니다.";

            case PlayFabErrorCode.AuthTokenDoesNotExist:
                return "인증 토큰을 찾을 수 없습니다.";

            case PlayFabErrorCode.NotAuthenticated:
                return "인증되지 않은 사용자입니다.";

            case PlayFabErrorCode.InternalServerError:
                return "서버 내부 오류가 발생했습니다. 잠시 후 다시 시도해주세요.";

            case PlayFabErrorCode.InvalidRequest:
                return "잘못된 요청입니다.";

            case PlayFabErrorCode.ConnectionError:
                return "네트워크 연결을 확인해주세요.";

            case PlayFabErrorCode.InvalidParams:
                return "입력 정보가 올바르지 않습니다.";

            case PlayFabErrorCode.PlayerSecretNotConfigured:
                return "플레이어 시크릿이 설정되지 않았습니다.";

            case PlayFabErrorCode.APIRequestsDisabledForTitle:
                return "현재 API 요청이 비활성화되어 있습니다.";

            case PlayFabErrorCode.InvalidTitleId:
                return "유효하지 않은 타이틀 ID입니다.";

            default:
                return $"통신 중 오류가 발생했습니다: {message}";
        }
    }


    #endregion



    #region Leaderboard

    public void UpdateLeaderBoard()
    {
        var request = new UpdatePlayerStatisticsRequest { Statistics = new List<StatisticUpdate>() };

        // level, exp
        var levelStat = new StatisticUpdate();
#if DEV
        levelStat.StatisticName = "LevelDev";
#elif LIVE
        levelStat.StatisticName = "LevelLive";
#else
        levelStat.StatisticName = "Level";
#endif
        levelStat.Value = SaveDataManager.instance.playerData.level * 10000 + SaveDataManager.instance.playerData.exp;
        request.Statistics.Add(levelStat);


        // tower
        var towerStat = new StatisticUpdate();
#if DEV
        towerStat.StatisticName = "TowerDev";
#elif LIVE
        towerStat.StatisticName = "TowerLive";
#else
        towerStat.StatisticName = "Tower";
#endif
        towerStat.Value = SaveDataManager.instance.playerData.towerFloor;
        request.Statistics.Add(towerStat);


        // cubeGame
        var cubeGameStat = new StatisticUpdate();
#if DEV
        cubeGameStat.StatisticName = "CubeGameDev";
#elif LIVE
        cubeGameStat.StatisticName = "CubeGameLive";
#else
        cubeGameStat.StatisticName = "CubeGame";
#endif
        cubeGameStat.Value = SaveDataManager.instance.playerData.cubeGameHighScore;
        request.Statistics.Add(cubeGameStat);


        // wingtto
        var wingttoStat = new StatisticUpdate();
#if DEV
        wingttoStat.StatisticName = "WingTtoDev";
#elif LIVE
        wingttoStat.StatisticName = "WingTtoLive";
#else
        wingttoStat.StatisticName = "WingTto";
#endif
        wingttoStat.Value = (int)(SaveDataManager.instance.playerData.wingTtoHighScore * 100);
        request.Statistics.Add(wingttoStat);

        UpdateLeaderBoard(request);
    }


    public void UpdateEndingLeaderBoard()
    {
        var request = new UpdatePlayerStatisticsRequest { Statistics = new List<StatisticUpdate>() };

        var ending = new StatisticUpdate();
#if DEV
        ending.StatisticName = "EndingDev";
#elif LIVE
        ending.StatisticName = "EndingLive";
#else
        ending.StatisticName = "Ending";
#endif
        int time = int.Parse(DateTime.Now.ToString("MMddHHmm")) * -1; //음수로 해서 역순으로 출력되도록(빠른 순으로 위에 정렬)
        ending.Value = time;
        request.Statistics.Add(ending);

        UpdateLeaderBoard(request);
    }

    public void UpdateEndingProgressLeaderBoard()
    {
        var request = new UpdatePlayerStatisticsRequest { Statistics = new List<StatisticUpdate>() };

        var endingProgress = new StatisticUpdate();
#if DEV
        endingProgress.StatisticName = "EndingProgressDev";
#elif LIVE
        endingProgress.StatisticName = "EndingProgressLive";
#else
        endingProgress.StatisticName = "EndingProgress";
#endif
        endingProgress.Value = SaveDataManager.instance.playerData.finalQuizPlayData.enterQuizIndex;
        request.Statistics.Add(endingProgress);

        UpdateLeaderBoard(request);
    }

    public void UpdateLeaderBoard(UpdatePlayerStatisticsRequest request)
    {
        PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderBoardUpdateSuccess, OnLeaderBoardUpdateError);
    }

    private void OnLeaderBoardUpdateSuccess(UpdatePlayerStatisticsResult result)
    {
        Debug.Log("Player statistics updated successfully!");
    }

    private void OnLeaderBoardUpdateError(PlayFabError error)
    {
        Debug.LogError("Stats update failed: " + error.GenerateErrorReport());
    }



    public async UniTask<List<PlayerLeaderboardEntry>> GetLeaderboard(string statisticName)
    {
        var tcs = new TaskCompletionSource<List<PlayerLeaderboardEntry>>();

        var topRequest = new GetLeaderboardRequest
        {
            StatisticName = statisticName,
            StartPosition = 0,
            MaxResultsCount = 10
        };

        PlayFabClientAPI.GetLeaderboard(topRequest,
            result => tcs.SetResult(result.Leaderboard),
            error => tcs.SetException(new System.Exception(error.GenerateErrorReport()))
        );

        return await tcs.Task;
    }


    public async UniTask<PlayerLeaderboardEntry> GetPlayerRanking(string statisticName)
    {
        var tcs = new TaskCompletionSource<PlayerLeaderboardEntry>();

        var playerRequest = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = statisticName,
            MaxResultsCount = 1
        };

        PlayFabClientAPI.GetLeaderboardAroundPlayer(playerRequest,
            result =>
            {
                if (result.Leaderboard.Count > 0)
                    tcs.SetResult(result.Leaderboard[0]);
                else
                    tcs.SetException(new System.Exception("플레이어 데이터를 찾을 수 없습니다."));
            },
            error => tcs.SetException(new System.Exception(error.GenerateErrorReport()))
        );

        return await tcs.Task;
    }


    #endregion




}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using PlayFab.ProfilesModels;


public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager instance { get; private set; }
    public bool isConnecting { get; private set; } = false;

    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }


    public void Start()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
            PlayFabSettings.staticSettings.TitleId = "11EF8D";
    }




    public void IntroUserRegisterProcess(string id, string pw, Action onSuccess, Action<string> onFailure)
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
            onSuccess?.Invoke();
        }, error =>
        {
            Debug.LogWarning("Register Failed");
            Debug.LogError("Here's some debug information:");
            Debug.LogError(error.GenerateErrorReport());
            onFailure?.Invoke(error.GenerateErrorReport());
        });
    }

    public void IntroUserLoginProcess(string id, string pw, Action onSuccess, Action<string> onFailure)
    {
        var request = new LoginWithPlayFabRequest
        {
            Username = id,
            Password = pw,
        };

        PlayFabClientAPI.LoginWithPlayFab(request, result =>
        {
            Debug.Log("Login Success!");
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
            onFailure?.Invoke(error.GenerateErrorReport());
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
    public void SavePlayerData(bool forceSave = false)
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
                () => { savePlayerDataCoroutine = null; },
                (err) => { savePlayerDataCoroutine = null; }
            );
        }
        else
        {
            savePlayerDataCoroutine = StartCoroutine(Co_SavePlayerData(
                () => { savePlayerDataCoroutine = null; },
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

        HLLogger.Log("@@@ PlayFab Save Request Start", LogColor.silver);

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "PlayerData", SaveDataManager.instance.JsonPlayerData },

                { "savedTime", SaveDataManager.instance.playerData.savedTime.ToString() },

                // 한번에 10개밖에 저장을 못함... 위 플레이어데이터에 싹다 들어가니까 눈에 보여야되는거만 넣자

                // { "mid", SaveDataManager.instance.playerData.mid.ToString() },
                // { "name", SaveDataManager.instance.playerData.name },
                // { "languageType", ((int)SaveDataManager.instance.playerData.languageType).ToString() },
                // { "master", ((int)SaveDataManager.instance.playerData.master).ToString() },
                // { "playFabLoginID", SaveDataManager.instance.playerData.playFabLoginID },
                // { "playFabLoginPW", SaveDataManager.instance.playerData.playFabLoginPW },

                { "serverDataVersion", SaveDataManager.instance.playerData.serverDataVersion },
                { "level", SaveDataManager.instance.playerData.level.ToString() },
                { "exp", SaveDataManager.instance.playerData.exp.ToString() },
                { "coin", SaveDataManager.instance.playerData.coin.ToString() },
                { "mileage", SaveDataManager.instance.playerData.mileage.ToString() },
                { "ownCardList", string.Join(",", SaveDataManager.instance.playerData.ownCardList) },
                { "usingRedeemCode", string.Join(",", SaveDataManager.instance.playerData.usingRedeemCode) },

                }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result => success?.Invoke(),
            error => failure?.Invoke(error.GenerateErrorReport()));

        UpdatePlayerLevelLeaderBoard();
    }


    #endregion



    #region Leaderboard

    // 레벨과 경험치를 Statistics에 업데이트
    public void UpdatePlayerLevelLeaderBoard()
    {
        var request = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
        {
            new StatisticUpdate
            {
                StatisticName = "Level",
                Value = SaveDataManager.instance.playerData.level
            },
            new StatisticUpdate
            {
                StatisticName = "Exp",
                Value = SaveDataManager.instance.playerData.exp
            }
        }
        };

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


    #endregion




}

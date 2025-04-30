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


    public void GetAccountInfo()
    {
        var request = new GetAccountInfoRequest() { Username = SaveDataManager.instance.playerData.id };
        PlayFabClientAPI.GetAccountInfo(request, (GetAccountInfoResult) =>
        {
            var s1 = GetAccountInfoResult.AccountInfo;



        }, (PlayFabError) =>
        {
            Debug.LogWarning("GetUserData Failed");
            Debug.LogError(PlayFabError.GenerateErrorReport());
        });
    }


    public void SetDisplayName(string name)
    {
        var request = new UpdateUserTitleDisplayNameRequest { DisplayName = name };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, (UpdateUserTitleDisplayNameResult) => { }, (PlayFabError) => { });
    }





    public void GetUserData()
    {
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request, (result) =>
        {
            foreach (var data in result.Data)
            {
                HLLogger.Log($"{data.Key} : {data.Value.Value}");
            }
        }, (error) =>
        {

        });
    }


    public void SavePlayerData()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> { { "PlayerData", SaveDataManager.instance.JsonPlayerData }, }
        };

        PlayFabClientAPI.UpdateUserData(request,
            result => Debug.Log("Save Complete"),
            error => Debug.LogError($"Failed: {error.GenerateErrorReport()}"));
    }



}

using UnityEngine;
using UnityEditor;
using System.Text;


#if UNITY_EDITOR
public class EasySaveEditor : Editor
{
    [MenuItem("SaveDataEditor/RemovePlayerData")]
    public static void RemovePlayerData()
    {
        ES3.DeleteKey(StaticGameData.SAVE_PLAYER_DATA_KEY);
        ES3.DeleteKey(StaticGameData.SAVE_PLAYER_OTHER_DATA_KEY);
        HLLogger.Log("Remove Complete.");
    }


    [MenuItem("SaveDataEditor/RemovePlayerIDPW")]
    public static void RemovePlayerIDPWData()
    {
        if (ES3.KeyExists(StaticGameData.SAVE_PLAYER_DATA_KEY) == false)
            HLLogger.Log("save date is null.");

        PlayerData playerData = new PlayerData();
        ES3.LoadInto(StaticGameData.SAVE_PLAYER_DATA_KEY, playerData);

        SaveDataManager.instance.SetIDPW("", "", false);
        HLLogger.Log("Remove ID PW Complete");
    }

    [MenuItem("SaveDataEditor/SetPlayerLevel1")]
    public static void SetPlayerLevel1()
    {
        if (ES3.KeyExists(StaticGameData.SAVE_PLAYER_DATA_KEY) == false)
            HLLogger.Log("save date is null.");

        PlayerData playerData = new PlayerData();
        ES3.LoadInto(StaticGameData.SAVE_PLAYER_DATA_KEY, playerData);

        playerData.level = 1;
        ES3.Save(StaticGameData.SAVE_PLAYER_DATA_KEY, playerData);
        HLLogger.Log("Set Player Level to 1 Complete");
    }

    [MenuItem("SaveDataEditor/ShowPlayerData")]
    public static void ShowPlayerData()
    {
        if (ES3.KeyExists(StaticGameData.SAVE_PLAYER_DATA_KEY) == false)
            HLLogger.Log("save date is null.");

        PlayerData playerData = new PlayerData();
        ES3.LoadInto(StaticGameData.SAVE_PLAYER_DATA_KEY, playerData);
        HLLogger.Log($"data\n{JsonUtility.ToJson(playerData)}");
    }





    [MenuItem("SaveDataEditor/(Do PlayScene) AddAllCardData")]
    public static void AddAllCardData()
    {
        if (ES3.KeyExists(StaticGameData.SAVE_PLAYER_DATA_KEY) == false)
            HLLogger.Log("save date is null.");

        PlayerData playerData = new PlayerData();
        ES3.LoadInto(StaticGameData.SAVE_PLAYER_DATA_KEY, playerData);

        SaveDataManager.instance.AddOwnCardList(GameResourceManager.instance.GetAllCardIds());
        HLLogger.Log("All All Card Complete");
    }

    [MenuItem("SaveDataEditor/(Do PlayScene) RemoveAllCardData")]
    public static void RemoveAllCardData()
    {
        if (ES3.KeyExists(StaticGameData.SAVE_PLAYER_DATA_KEY) == false)
            HLLogger.Log("save date is null.");

        PlayerData playerData = new PlayerData();
        ES3.LoadInto(StaticGameData.SAVE_PLAYER_DATA_KEY, playerData);

        SaveDataManager.instance.RemoveOwnCardList();
        HLLogger.Log("Remove All Card Complete");
    }


    [MenuItem("SaveDataEditor/(Do PlayScene) Add 1000000 Coin")]
    public static void Add1000000Coin()
    {
        if (ES3.KeyExists(StaticGameData.SAVE_PLAYER_DATA_KEY) == false)
            HLLogger.Log("save date is null.");

        PlayerData playerData = new PlayerData();
        ES3.LoadInto(StaticGameData.SAVE_PLAYER_DATA_KEY, playerData);

        SaveDataManager.instance.AddCoin(1000000);
        HLLogger.Log("Add 100000 Coin Complete");
    }

    [MenuItem("SaveDataEditor/(Do PlayScene) Set 4400 Coin ")]
    public static void Set4400Coin()
    {
        if (ES3.KeyExists(StaticGameData.SAVE_PLAYER_DATA_KEY) == false)
            HLLogger.Log("save date is null.");

        PlayerData playerData = new PlayerData();
        ES3.LoadInto(StaticGameData.SAVE_PLAYER_DATA_KEY, playerData);

        SaveDataManager.instance.SetCoin(4400);
        HLLogger.Log("Set 4400 Coin Complete");
    }

    [MenuItem("SaveDataEditor/(Do PlayScene) Set 20000 Mileage")]
    public static void Set20000Mileage()
    {
        if (ES3.KeyExists(StaticGameData.SAVE_PLAYER_DATA_KEY) == false)
            HLLogger.Log("save date is null.");

        PlayerData playerData = new PlayerData();
        ES3.LoadInto(StaticGameData.SAVE_PLAYER_DATA_KEY, playerData);

        SaveDataManager.instance.SetMilage(20000);
        HLLogger.Log("Set 20000 Mileage Complete");
    }


    [MenuItem("SaveDataEditor/(Do PlayScene) Add 700 exp")]
    public static void Add700Exp()
    {
        if (ES3.KeyExists(StaticGameData.SAVE_PLAYER_DATA_KEY) == false)
            HLLogger.Log("save date is null.");

        PlayerData playerData = new PlayerData();
        ES3.LoadInto(StaticGameData.SAVE_PLAYER_DATA_KEY, playerData);

        // var exp = playerData.maxExp;
        var exp = 700;
        SaveDataManager.instance.AddExp(exp);
        HLLogger.Log($"Add {exp} Exp complete");
    }
}
#endif
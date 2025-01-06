using UnityEngine;
using UnityEditor;
using System.Text;


public class EasySaveEditor : Editor
{
    [MenuItem("SaveDataEditor/RemovePlayerData")]
    public static void RemovePlayerData()
    {
        ES3.DeleteKey(StaticGameData.SAVE_PLAYER_DATA_KEY);
        HLLogger.Log("Remove Complete.");
    }

    [MenuItem("SaveDataEditor/ShowPlayerData")]
    public static void ShowPlayerData()
    {
        if (ES3.KeyExists(StaticGameData.SAVE_PLAYER_DATA_KEY) == false)
            HLLogger.Log("save date is null.");

        PlayerData playerData = new PlayerData();
        ES3.LoadInto(StaticGameData.SAVE_PLAYER_DATA_KEY, playerData);

        HLLogger.Log($"savedTime : {playerData.savedTime}");
        HLLogger.Log($"mid : {playerData.mid}");
        HLLogger.Log($"name : {playerData.name}");
        HLLogger.Log($"languageType : {playerData.languageType}");
        HLLogger.Log($"master : {playerData.master}");
        HLLogger.Log($"serverDataVersion : {playerData.serverDataVersion}");
        HLLogger.Log($"coin : {playerData.coin}");
        HLLogger.Log($"mileage : {playerData.mileage}");
        HLLogger.Log($"level : {playerData.level}");
        HLLogger.Log($"exp : {playerData.exp}");

        StringBuilder sb = new StringBuilder();
        foreach (var item in playerData.ownCardList)
        {
            sb.Append($"{item},");
        }
        HLLogger.Log($"ownCardList : {sb}");
    }





    [MenuItem("SaveDataEditor/(Do PlayScene) AddAllCardData")]
    public static void AddAllCardData()
    {
        if (ES3.KeyExists(StaticGameData.SAVE_PLAYER_DATA_KEY) == false)
            HLLogger.Log("save date is null.");

        PlayerData playerData = new PlayerData();
        ES3.LoadInto(StaticGameData.SAVE_PLAYER_DATA_KEY, playerData);

        SaveDataManager.instance.AddOwnCardList(ResourceManager.instance.GetAllCardIds());
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


    [MenuItem("SaveDataEditor/(Do PlayScene) Add 100000 Coin")]
    public static void Add100000Coin()
    {
        if (ES3.KeyExists(StaticGameData.SAVE_PLAYER_DATA_KEY) == false)
            HLLogger.Log("save date is null.");

        PlayerData playerData = new PlayerData();
        ES3.LoadInto(StaticGameData.SAVE_PLAYER_DATA_KEY, playerData);

        SaveDataManager.instance.AddCoin(100000);
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

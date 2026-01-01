using System.Collections;
using System.Collections.Generic;
using System;


[Serializable]
public class PlayerData
{
    public DateTime savedTime = DateTime.Now;
    public int mid = 0;
    public string name = "";
    public LanguageType languageType = LanguageType.ko;
    public CardMaster master = CardMaster.Other;

    public string playFabLoginID = "";
    public string playFabLoginPW = "";
    public bool autoLogin = false;
    public bool isNewUser = true;


    public string serverDataVersion = "0";
    public string recentAppVersion = "0";

    public int level = 1;
    public int exp = 0;
    public int coin = 0;
    public int mileage = 0;
    public List<int> ownCardList = new();
    public List<int> ownWordList = new();
    public List<string> usingRedeemCode = new();

    public string defineState = "";
    public bool playEndRoll = false;


    public int maxExp => GameResourceManager.instance.GetLevelRequireExp(level);


    //TowerData
    public int towerFloor = 1;
    public TowerGameUserStatLevelData towerGameUserStatLevelData;
    public TowerGameUserWeaponData towerGameUserWeaponData;

    //GameHighScore
    public int cubeGameHighScore;
    public float wingTtoHighScore;

    // gemini Data
    public List<GeminiHint> geminiHints2;

    // Quest Data
    public QuestUserPlayData questUserPlayData = new QuestUserPlayData();


    //server switch
    public bool useServerData;


    public PlayerData()
    {
        savedTime = DateTime.Now;
        mid = 0;
        name = $"Player{mid.ToString("D4")}";
        languageType = LanguageType.ko;
        master = CardMaster.Other;
        playFabLoginID = "";
        playFabLoginPW = "";
        autoLogin = false;
        isNewUser = true;

        serverDataVersion = "0";
        recentAppVersion = "0";
        level = 1;
        exp = 0;
        coin = 0;
        mileage = 0;
        ownCardList.Clear();
        ownWordList.Clear();
        usingRedeemCode.Clear();

        defineState = "";
        playEndRoll = false;

        towerFloor = 1;
        towerGameUserStatLevelData = new TowerGameUserStatLevelData();
        towerGameUserWeaponData = new TowerGameUserWeaponData();
        geminiHints2 = new List<GeminiHint>();
        questUserPlayData = new QuestUserPlayData();
        cubeGameHighScore = 0;
        wingTtoHighScore = 0f;

        // server switch
        useServerData = false;
    }
}


[System.Serializable]
public class GeminiHint
{
    public int index;
    public string key;
    public List<string> value;

    public GeminiHint(int index, string key, List<string> value)
    {
        this.index = index;
        this.key = key;
        this.value = value;
    }
}
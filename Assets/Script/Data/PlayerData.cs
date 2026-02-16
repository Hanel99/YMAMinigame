using System;
using System.Collections;
using System.Collections.Generic;


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
    public int unlockContent = 0;
    public List<int> ownCardList = new();
    public List<int> ownWordList = new();
    public List<string> usingRedeemCode = new();
    public string defineState = "";

    //Final Quiz Data
    public FinalQuizPlayData finalQuizPlayData;


    //TowerData
    public int towerFloor = 1;
    public TowerGameUserStatLevelData towerGameUserStatLevelData;
    public TowerGameUserWeaponData towerGameUserWeaponData;
    public List<TowerJewelUserData> towerJewelUserDataList;

    //GameHighScore
    public int cubeGameHighScore;
    public float wingTtoHighScore;

    // gemini Data
    public List<GeminiHint> geminiHints2;

    // Quest Data
    public QuestUserPlayData questUserPlayData = new QuestUserPlayData();



    //server switch
    public bool useServerData;


    // Method
    public int maxExp => GameResourceManager.instance.GetLevelRequireExp(level);


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
        unlockContent = 0;
        ownCardList.Clear();
        ownWordList.Clear();
        usingRedeemCode.Clear();

        defineState = "";

        finalQuizPlayData = new FinalQuizPlayData();

        towerFloor = 1;
        towerGameUserStatLevelData = new TowerGameUserStatLevelData();
        towerGameUserWeaponData = new TowerGameUserWeaponData();
        towerJewelUserDataList = new List<TowerJewelUserData>();

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

[System.Serializable]
public class FinalQuizPlayData
{
    public FinalReferState matchCardGame = FinalReferState.Locked;
    public FinalReferState findAIWordGame = FinalReferState.Locked;
    public FinalReferState cubeGame = FinalReferState.Locked;
    public FinalReferState wingTto = FinalReferState.Locked;

    public int enterQuizIndex = 0;
    public int tryCount = 0;
    public int XCount = 0;
    public int OCount = 0;
    public bool playEndRoll = false;
    public DateTime completeTime = new DateTime(2025, 1, 1, 0, 0, 0, 0);
}

[Serializable]
public class TowerJewelUserData
{
    public TowerJewelType type = TowerJewelType.None;
    public TowerJewelGrade grade = TowerJewelGrade.Normal;
    public float value = 0f;
}

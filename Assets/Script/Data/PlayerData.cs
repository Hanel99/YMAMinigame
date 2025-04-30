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

    public string id = "";
    public string pw = "";
    public bool autoLogin = false;


    public string serverDataVersion = "0";

    public int level = 1;
    public int exp = 0;
    public int coin = 0;
    public int mileage = 0;
    public List<int> ownCardList = new();
    public List<string> usingRedeemCode = new();

    public int maxExp => ResourceManager.instance.GetLevelRequireExp(level);


    public PlayerData()
    {
        savedTime = DateTime.Now;
        mid = 0;
        name = $"Player{mid.ToString("D4")}";
        languageType = LanguageType.ko;
        master = CardMaster.Other;
        id = "";
        pw = "";
        autoLogin = false;

        serverDataVersion = "0";
        level = 1;
        exp = 0;
        coin = 0;
        mileage = 0;
        ownCardList.Clear();
        usingRedeemCode.Clear();
    }
}

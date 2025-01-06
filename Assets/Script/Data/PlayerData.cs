using System.Collections;
using System.Collections.Generic;
using System;


[Serializable]
public class PlayerData
{
    public DateTime savedTime;
    public int mid;
    public string name;
    public LanguageType languageType;
    public CardMaster master;

    public string serverDataVersion;

    public int level;
    public int exp;
    public int coin;
    public int mileage;
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

        serverDataVersion = "0";
        level = 1;
        exp = 0;
        coin = 0;
        mileage = 0;
        ownCardList.Clear();
        usingRedeemCode.Clear();
    }
}

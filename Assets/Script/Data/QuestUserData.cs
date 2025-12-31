using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class QuestUserPlayData
{
    public List<int> completedQuestIds = new List<int>();

    Q_MatchCardGame matchCardGame;
    Q_FindAIWordGame findAIWordGame;
    Q_CubeGame cubeGame;
    Q_WingTto wingTto;
    Q_TowerGame towerGame;
    Q_Gacha gacha;
    Q_Common common;




    public QuestUserPlayData()
    {
        completedQuestIds = new List<int>();

        matchCardGame = new();
        findAIWordGame = new();
        cubeGame = new();
        wingTto = new();
        towerGame = new();
        gacha = new();
        common = new();
    }




}



[Serializable]
public class Q_GameBase
{
    public int playCount;
}

[Serializable]
public class Q_MatchCardGame : Q_GameBase
{
    public int underQuestClearCount;
}

[Serializable]
public class Q_FindAIWordGame : Q_GameBase
{
    public int underQuestClearCount;
}


[Serializable]
public class Q_CubeGame : Q_GameBase
{
    public int reachMaxScore;
    public int touchCount_TooFast;
    public int touchCount_Fast;
    public int touchCount_Perfect;
    public int touchCount_Slow;
    public int touchCount_TooSlow;
    public int totalTouchCount;

    public int reachScoreWithoutTooFastOrTooSlow;
    public int reachScoreWithOnlyPerfect;

}

[Serializable]
public class Q_WingTto : Q_GameBase
{
    public int reachMaxScore;
    public int collectCount_Gimbab;
    public int collectCount_SpeedUp;
    public int collectCount_Coin;
    public int collectCount_Exp;


    public int reachScoreWithoutGimbab;
    public int reachScoreWithCrash;
}


[Serializable]
public class Q_TowerGame : Q_GameBase
{
    public int winWithAvoid;
    public int weaponEnchantCount_Up;
    public int weaponEnchantCount_Stay;
    public int weaponEnchantCount_Down;
    public int weaponTotalEnchantCount;
}


[Serializable]
public class Q_Gacha : Q_GameBase
{
    public int playGacha;
    public int playMileageGacha;
}


[Serializable]
public class Q_Common
{
    public int collectCoin;
    public int useCoin;
    public int equipEtcIcon;
}
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


    // matchCardGame
    public void AddMatchCardData(int playCount, int underQuestClearCount)
    {
        matchCardGame.playCount += playCount;
        matchCardGame.underQuestClearCount += underQuestClearCount;
    }


    //findAIWordGame
    public void AddFindAIWordData(int playCount, int underQuestClearCount)
    {
        findAIWordGame.playCount += playCount;
        findAIWordGame.underQuestClearCount += underQuestClearCount;
    }


    //cubeGame
    public void AddCubeData(int playCount, int touchCount_TooFast, int touchCount_Fast, int touchCount_Perfect, int touchCount_Slow, int touchCount_TooSlow, int totalTouchCount)
    {
        cubeGame.playCount += playCount;
        cubeGame.touchCount_TooFast += touchCount_TooFast;
        cubeGame.touchCount_Fast += touchCount_Fast;
        cubeGame.touchCount_Perfect += touchCount_Perfect;
        cubeGame.touchCount_Slow += touchCount_Slow;
        cubeGame.touchCount_TooSlow += touchCount_TooSlow;
        cubeGame.totalTouchCount += totalTouchCount;
    }


    //wingTto
    public void AddWingTtoData(int playCount, int reachMaxScore, int collectCount_Gimbab, int collectCount_SpeedUp, int collectCount_Coin, int collectCount_Exp)
    {
        wingTto.playCount += playCount;
        wingTto.reachMaxScore += reachMaxScore;
        wingTto.collectCount_Gimbab += collectCount_Gimbab;
        wingTto.collectCount_SpeedUp += collectCount_SpeedUp;
        wingTto.collectCount_Coin += collectCount_Coin;
        wingTto.collectCount_Exp += collectCount_Exp;
    }


    //towerGame
    public void AddTowerCombatData(int playCount, int winWithAvoid)
    {
        towerGame.playCount += playCount;
        towerGame.winWithAvoid += winWithAvoid;
    }

    public void AddTowerEnchantData(TowerGameResultType resultType)
    {
        switch (resultType)
        {
            case TowerGameResultType.up:
                AddTowerEnchantData(1, 1, 0, 0);
                break;
            case TowerGameResultType.stay:
                AddTowerEnchantData(1, 0, 1, 0);
                break;
            case TowerGameResultType.down:
                AddTowerEnchantData(1, 0, 0, 1);
                break;
            default:
                break;
        }
    }

    private void AddTowerEnchantData(int weaponTotalEnchantCount, int weaponEnchantCount_Up, int weaponEnchantCount_Stay, int weaponEnchantCount_Down)
    {
        towerGame.weaponTotalEnchantCount += weaponTotalEnchantCount;
        towerGame.weaponEnchantCount_Up += weaponEnchantCount_Up;
        towerGame.weaponEnchantCount_Stay += weaponEnchantCount_Stay;
        towerGame.weaponEnchantCount_Down += weaponEnchantCount_Down;
    }



    // gacha
    public void AddGachaData(bool isMileageGacha, int playCount)
    {
        AddGachaData(playCount, isMileageGacha ? 0 : playCount, isMileageGacha ? playCount : 0);
    }

    private void AddGachaData(int playCount, int playGacha, int playMileageGacha)
    {
        gacha.playCount += playCount;
        gacha.playGacha += playGacha;
        gacha.playMileageGacha += playMileageGacha;
    }




    // common
    public void AddCommonData_CollectCoin(int value)
    {
        common.collectCoin += value;
    }

    public void AddCommonData_UseCoin(int value)
    {
        common.useCoin += value;
    }

    public void AddCommonData_EquipEtcIcon()
    {
        common.equipEtcIcon += 1;
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
}
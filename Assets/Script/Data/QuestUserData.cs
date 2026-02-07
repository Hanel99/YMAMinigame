using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


[Serializable]
public class QuestUserPlayData
{
    public List<int> completedQuestIds = new List<int>();
    public Dictionary<int, string> completedQuestData = new Dictionary<int, string>();

    [SerializeField]
    private Q_MatchCardGame _matchCardGame;
    public Q_MatchCardGame matchCardGame => _matchCardGame;

    [SerializeField]
    private Q_FindAIWordGame _findAIWordGame;
    public Q_FindAIWordGame findAIWordGame => _findAIWordGame;

    [SerializeField]
    private Q_CubeGame _cubeGame;
    public Q_CubeGame cubeGame => _cubeGame;

    [SerializeField]
    private Q_WingTto _wingTto;
    public Q_WingTto wingTto => _wingTto;

    [SerializeField]
    private Q_TowerGame _towerGame;
    public Q_TowerGame towerGame => _towerGame;

    [SerializeField]
    private Q_Gacha _gacha;
    public Q_Gacha gacha => _gacha;

    [SerializeField]
    private Q_Common _common;
    public Q_Common common => _common;




    public QuestUserPlayData()
    {
        completedQuestIds = new List<int>();
        completedQuestData = new Dictionary<int, string>();

        _matchCardGame = new();
        _findAIWordGame = new();
        _cubeGame = new();
        _wingTto = new();
        _towerGame = new();
        _gacha = new();
        _common = new();
    }


    // matchCardGame
    public void AddMatchCardData(int playCount, int underQuestClearCount)
    {
        matchCardGame.playCount += playCount;
        matchCardGame.underQuestClearCount += underQuestClearCount;

        // HLLogger.Log($"Q match card data - playcount {matchCardGame.playCount} (+ {playCount}), underQuestClearCount {matchCardGame.underQuestClearCount} (+ {underQuestClearCount})");
    }


    //findAIWordGame
    public void AddFindAIWordData(int playCount, int underQuestClearCount)
    {
        findAIWordGame.playCount += playCount;
        findAIWordGame.underQuestClearCount += underQuestClearCount;

        // HLLogger.Log($"Q ai word data - playcount {findAIWordGame.playCount} (+ {playCount}), underQuestClearCount {findAIWordGame.underQuestClearCount} (+ {underQuestClearCount})");
    }


    //cubeGame
    public void AddCubeData(int playCount, int touchCount_TooFast, int touchCount_Fast, int touchCount_Perfect, int touchCount_Slow, int touchCount_TooSlow, int totalTouchCount, int totalScore)
    {
        cubeGame.playCount += playCount;
        cubeGame.touchCount_TooFast += touchCount_TooFast;
        cubeGame.touchCount_Fast += touchCount_Fast;
        cubeGame.touchCount_Perfect += touchCount_Perfect;
        cubeGame.touchCount_Slow += touchCount_Slow;
        cubeGame.touchCount_TooSlow += touchCount_TooSlow;
        cubeGame.totalTouchCount += totalTouchCount;
        cubeGame.totalScore += totalScore;

        // HLLogger.Log($"Q cube data - playcount {cubeGame.playCount} (+ {playCount})");
        // HLLogger.Log($"Q cube data - touchcount {cubeGame.touchCount_TooFast} (+ {touchCount_TooFast})");
        // HLLogger.Log($"Q cube data - touchcount {cubeGame.touchCount_Fast} (+ {touchCount_Fast})");
        // HLLogger.Log($"Q cube data - touchcount {cubeGame.touchCount_Perfect} (+ {touchCount_Perfect})");
        // HLLogger.Log($"Q cube data - touchcount {cubeGame.touchCount_Slow} (+ {touchCount_Slow})");
        // HLLogger.Log($"Q cube data - touchcount {cubeGame.touchCount_TooSlow} (+ {touchCount_TooSlow})");
        // HLLogger.Log($"Q cube data - totalCount {cubeGame.totalTouchCount} (+ {totalTouchCount})");
        // HLLogger.Log($"Q cube data - totalScore {cubeGame.totalScore} (+ {totalScore})");
    }

    //wingTto
    public void AddWingTtoData(int playCount, int collectCount_Gimbab, int collectCount_SpeedUp, int collectCount_Coin, int collectCount_Exp, int crashCount, float flyDistance)
    {
        wingTto.playCount += playCount;
        wingTto.collectCount_Gimbab += collectCount_Gimbab;
        wingTto.collectCount_SpeedUp += collectCount_SpeedUp;
        wingTto.collectCount_Coin += collectCount_Coin;
        wingTto.collectCount_Exp += collectCount_Exp;
        wingTto.crashCount += crashCount;
        wingTto.totalFlyDistance += flyDistance;

        // HLLogger.Log($"Q wing tto data - playcount {wingTto.playCount} (+ {playCount})");
        // HLLogger.Log($"Q wing tto data - collect Gimbab {wingTto.collectCount_Gimbab} (+ {collectCount_Gimbab})");
        // HLLogger.Log($"Q wing tto data - collect SpeedUp {wingTto.collectCount_SpeedUp} (+ {collectCount_SpeedUp})");
        // HLLogger.Log($"Q wing tto data - collect Coin {wingTto.collectCount_Coin} (+ {collectCount_Coin})");
        // HLLogger.Log($"Q wing tto data - collect Exp {wingTto.collectCount_Exp} (+ {collectCount_Exp})");
        // HLLogger.Log($"Q wing tto data - crashCount {wingTto.crashCount} (+ {crashCount})");
        // HLLogger.Log($"Q wing tto data - totalFlyDistance {wingTto.totalFlyDistance} (+ {flyDistance})");
    }


    //towerGame
    public void AddTowerCombatData(int playCount)
    {
        towerGame.playCount += playCount;
        // HLLogger.Log($"Q tower data - playcount {towerGame.playCount} (+ {playCount})");
    }

    public void AddTowerEnchantData(TowerGameResultType resultType)
    {
        switch (resultType)
        {
            case TowerGameResultType.Up:
                AddTowerEnchantData(1, 1, 0, 0);
                break;
            case TowerGameResultType.Stay:
                AddTowerEnchantData(1, 0, 1, 0);
                break;
            case TowerGameResultType.Down:
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

        // HLLogger.Log($"Q tower enchant data - total {towerGame.weaponTotalEnchantCount} (+ {weaponTotalEnchantCount})");
        // if (weaponEnchantCount_Up > 0) HLLogger.Log($"Q tower enchant data - up {towerGame.weaponEnchantCount_Up} (+ {weaponEnchantCount_Up})");
        // if (weaponEnchantCount_Stay > 0) HLLogger.Log($"Q tower enchant data - stay {towerGame.weaponEnchantCount_Stay} (+ {weaponEnchantCount_Stay})");
        // if (weaponEnchantCount_Down > 0) HLLogger.Log($"Q tower enchant data - down {towerGame.weaponEnchantCount_Down} (+ {weaponEnchantCount_Down})");
    }



    // gacha
    public void AddGachaData(bool isMileageGacha, int playCount)
    {
        AddGachaData(playCount, isMileageGacha ? 0 : playCount, isMileageGacha ? playCount : 0);
    }

    private void AddGachaData(int playCount, int playGacha, int playMileageGacha)
    {
        gacha.playCount += playCount;
        gacha.playGachaCount += playGacha;
        gacha.playMileageGachaCount += playMileageGacha;

        // HLLogger.Log($"Q gacha data - playcount {gacha.playCount} (+ {playCount}), playGacha {gacha.playGachaCount} (+ {playGacha}), playMileageGacha {gacha.playMileageGachaCount} (+ {playMileageGacha})");
    }




    // common
    public void AddCommonData_CollectCoin(int value)
    {
        common.collectCoin += value;
        // HLLogger.Log($"Q common data - collectCoin {common.collectCoin} (+ {value})");
    }

    public void AddCommonData_UseCoin(int value)
    {
        common.useCoin += value;
        // HLLogger.Log($"Q common data - useCoin {common.useCoin} (+ {value})");
    }


    //special

    public void AddSpecialMission(QuestDetailType type, float value)
    {
        switch (type)
        {
            // cubeGame
            case QuestDetailType.S_ReachScoreWithoutTooFastOrTooSlow:
                {
                    var oldVal = cubeGame.reachScoreWithoutTooFastOrTooSlow;
                    cubeGame.reachScoreWithoutTooFastOrTooSlow = math.max(cubeGame.reachScoreWithoutTooFastOrTooSlow, value.ToInt());
                    // HLLogger.Log($"Q special mission - S_ReachScoreWithoutTooFastOrTooSlow {oldVal} -> {cubeGame.reachScoreWithoutTooFastOrTooSlow}");
                }
                break;
            case QuestDetailType.S_ReachScoreWithOnlyPerfect:
                {
                    var oldVal = cubeGame.reachScoreWithOnlyPerfect;
                    cubeGame.reachScoreWithOnlyPerfect = math.max(cubeGame.reachScoreWithOnlyPerfect, value.ToInt());
                    // HLLogger.Log($"Q special mission - S_ReachScoreWithOnlyPerfect {oldVal} -> {cubeGame.reachScoreWithOnlyPerfect}");
                }
                break;

            //wingTto
            case QuestDetailType.S_ReachScoreWithoutGimbab:
                {
                    var oldVal = wingTto.reachScoreWithoutGimbab;
                    wingTto.reachScoreWithoutGimbab = Mathf.Max(wingTto.reachScoreWithoutGimbab, value);
                    // HLLogger.Log($"Q special mission - S_ReachScoreWithoutGimbab {oldVal} -> {wingTto.reachScoreWithoutGimbab}");
                }
                break;
            case QuestDetailType.S_ReachScoreWithCrash:
                {
                    var oldVal = wingTto.reachScoreWithCrash;
                    wingTto.reachScoreWithCrash = Mathf.Max(wingTto.reachScoreWithCrash, value);
                    // HLLogger.Log($"Q special mission - S_ReachScoreWithCrash {oldVal} -> {wingTto.reachScoreWithCrash}");
                }
                break;

            // tower
            case QuestDetailType.S_WinWithAvoid:
                {
                    var oldVal = towerGame.winWithAvoid;
                    towerGame.winWithAvoid += 1;
                    // HLLogger.Log($"Q special mission - S_WinWithAvoid {oldVal} -> {towerGame.winWithAvoid}");
                }
                break;

            //common
            case QuestDetailType.S_EquipEtcIcon:
                {
                    var oldVal = common.equipEtcIcon;
                    common.equipEtcIcon += 1;
                    // HLLogger.Log($"Q special mission - S_EquipEtcIcon {oldVal} -> {common.equipEtcIcon}");
                }
                break;
            default:
                break;
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
        public int touchCount_TooFast;
        public int touchCount_Fast;
        public int touchCount_Perfect;
        public int touchCount_Slow;
        public int touchCount_TooSlow;
        public int totalTouchCount;
        public float totalScore;

        public int reachScoreWithoutTooFastOrTooSlow;
        public int reachScoreWithOnlyPerfect;

    }

    [Serializable]
    public class Q_WingTto : Q_GameBase
    {
        public int collectCount_Gimbab;
        public int collectCount_SpeedUp;
        public int collectCount_Coin;
        public int collectCount_Exp;

        public int crashCount;
        public float totalFlyDistance;

        public float reachScoreWithoutGimbab;
        public float reachScoreWithCrash;
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
        public int playGachaCount;
        public int playMileageGachaCount;
    }


    [Serializable]
    public class Q_Common
    {
        public long collectCoin;
        public long useCoin;
        public int equipEtcIcon;
    }
}
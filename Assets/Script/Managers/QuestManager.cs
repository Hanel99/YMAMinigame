using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance { get; private set; }
    public PlayerData playerData => SaveDataManager.instance.playerData;
    public QuestUserPlayData questUserPlayData => SaveDataManager.instance.playerData.questUserPlayData;

    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }



    #region Quest Progress 

    // matchCardGame
    public void AddMatchCardData(int playCount, int underQuestClearCount)
    {
        questUserPlayData.AddMatchCardData(playCount, underQuestClearCount);
    }


    //findAIWordGame
    public void AddFindAIWordData(int playCount, int underQuestClearCount)
    {
        questUserPlayData.AddFindAIWordData(playCount, underQuestClearCount);
    }


    //cubeGame
    public void AddCubeData(int playCount, int touchCount_TooFast, int touchCount_Fast, int touchCount_Perfect, int touchCount_Slow, int touchCount_TooSlow, int totalTouchCount)
    {
        questUserPlayData.AddCubeData(playCount, touchCount_TooFast, touchCount_Fast, touchCount_Perfect, touchCount_Slow, touchCount_TooSlow, totalTouchCount);
    }


    //wingTto
    public void AddWingTtoData(int playCount, int collectCount_Gimbab, int collectCount_SpeedUp, int collectCount_Coin, int collectCount_Exp)
    {
        questUserPlayData.AddWingTtoData(playCount, collectCount_Gimbab, collectCount_SpeedUp, collectCount_Coin, collectCount_Exp);
    }


    //towerGame
    public void AddTowerCombatData(int playCount)
    {
        questUserPlayData.AddTowerCombatData(playCount);
    }

    public void AddTowerEnchantData(TowerGameResultType resultType)
    {
        questUserPlayData.AddTowerEnchantData(resultType);
    }


    // gacha
    public void AddGachaData(bool isMileageGacha, int playCount)
    {
        questUserPlayData.AddGachaData(isMileageGacha, playCount);
    }


    // common
    public void AddCommonData_CollectCoin(int value)
    {
        questUserPlayData.AddCommonData_CollectCoin(value);
    }

    public void AddCommonData_UseCoin(int value)
    {
        questUserPlayData.AddCommonData_UseCoin(value);
    }


    //special
    public void AddSpecialMission(QuestDetailType type, float value)
    {
        questUserPlayData.AddSpecialMission(type, value);
    }






    public int GetQuestValue(QuestDetailType detailType, QuestDetailType2 questDetailType2)
    {
        switch (detailType)
        {
            // match card
            case QuestDetailType.PlayMatchCardGame:
                return questUserPlayData.matchCardGame.playCount;
            case QuestDetailType.FinishCardGameUnderCount:
                return questUserPlayData.matchCardGame.underQuestClearCount;

            // find AI word
            case QuestDetailType.PlayFindAIWordGame:
                return questUserPlayData.findAIWordGame.playCount;
            case QuestDetailType.FinishWordGameUnderCount:
                return questUserPlayData.findAIWordGame.underQuestClearCount;

            // cube
            case QuestDetailType.PlayCubeGame:
                return questUserPlayData.cubeGame.playCount;
            case QuestDetailType.ReachCubeScore:
                return playerData.cubeGameHighScore;
            case QuestDetailType.ReachTotalTouchCount:
                return questUserPlayData.cubeGame.totalTouchCount;
            case QuestDetailType.ReachCubeStateTouchCount:
                switch (questDetailType2)
                {
                    case QuestDetailType2.CubeState_TooFast:
                        return questUserPlayData.cubeGame.touchCount_TooFast;
                    case QuestDetailType2.CubeState_Fast:
                        return questUserPlayData.cubeGame.touchCount_Fast;
                    case QuestDetailType2.CubeState_Perfect:
                        return questUserPlayData.cubeGame.touchCount_Perfect;
                    case QuestDetailType2.CubeState_Slow:
                        return questUserPlayData.cubeGame.touchCount_Slow;
                    case QuestDetailType2.CubeState_TooSlow:
                        return questUserPlayData.cubeGame.touchCount_TooSlow;
                    default:
                        return 0;
                }
            case QuestDetailType.S_ReachScoreWithoutTooFastOrTooSlow:
                return questUserPlayData.cubeGame.reachScoreWithoutTooFastOrTooSlow;
            case QuestDetailType.S_ReachScoreWithOnlyPerfect:
                return questUserPlayData.cubeGame.reachScoreWithOnlyPerfect;

            // wingtto
            case QuestDetailType.PlayWingTto:
                return questUserPlayData.wingTto.playCount;
            case QuestDetailType.ReachWingTtoDistance:
                return playerData.wingTtoHighScore.ToInt();
            case QuestDetailType.CollectWingTtoItem:
                switch (questDetailType2)
                {
                    case QuestDetailType2.WingTtoItem_Gimbab:
                        return questUserPlayData.wingTto.collectCount_Gimbab;
                    case QuestDetailType2.WingTtoItem_SpeedUp:
                        return questUserPlayData.wingTto.collectCount_SpeedUp;
                    case QuestDetailType2.WingTtoItem_Coin:
                        return questUserPlayData.wingTto.collectCount_Coin;
                    case QuestDetailType2.WingTtoItem_Exp:
                        return questUserPlayData.wingTto.collectCount_Exp;
                    default:
                        return 0;
                }
            case QuestDetailType.S_ReachScoreWithoutGimbab:
                return questUserPlayData.wingTto.reachScoreWithoutGimbab.ToInt();
            case QuestDetailType.S_ReachScoreWithCrash:
                return questUserPlayData.wingTto.reachScoreWithCrash.ToInt();

            // tower
            case QuestDetailType.ReachTowerFloor:
                return playerData.towerFloor;
            case QuestDetailType.ReachStatLevel:
                switch (questDetailType2)
                {
                    case QuestDetailType2.TowerStat_Atk:
                        return playerData.towerGameUserStatLevelData.atkLevel;
                    case QuestDetailType2.TowerStat_Def:
                        return playerData.towerGameUserStatLevelData.defLevel;
                    case QuestDetailType2.TowerStat_Hp:
                        return playerData.towerGameUserStatLevelData.hpLevel;
                    case QuestDetailType2.TowerStat_CriRate:
                        return playerData.towerGameUserStatLevelData.criRateLevel;
                    case QuestDetailType2.TowerStat_CriDmg:
                        return playerData.towerGameUserStatLevelData.criDmgLevel;
                    default:
                        return 0;
                }
            case QuestDetailType.ReachWeaponLevel:
                return playerData.towerGameUserWeaponData.weaponLevel;
            case QuestDetailType.S_WinWithAvoid:
                return questUserPlayData.towerGame.winWithAvoid;

            // collection
            case QuestDetailType.CollectCard:
                return playerData.ownCardList.Count;
            case QuestDetailType.CollectWord:
                return playerData.ownWordList.Count;

            // gacha
            case QuestDetailType.PlayGacha:
                return questUserPlayData.gacha.playGachaCount;
            case QuestDetailType.PlayMileageGacha:
                return questUserPlayData.gacha.playMileageGachaCount;

            // common
            case QuestDetailType.CollectCoin:
                return questUserPlayData.common.collectCoin;
            case QuestDetailType.UseCoin:
                return questUserPlayData.common.useCoin;
            case QuestDetailType.ReachPlayerLevel:
                return playerData.level;
            case QuestDetailType.S_EquipEtcIcon:
                return questUserPlayData.common.equipEtcIcon;
            case QuestDetailType.E_PlayEndRoll:
                return playerData.playEndRoll ? 1 : 0;

            default:
                return 0;
        }
    }


    #endregion


    #region Quest Completion 

    public void CompleteQuest(int questId, System.Action onComplete = null)
    {
        // 퀘스트 완료 처리.
        if (!questUserPlayData.completedQuestIds.Contains(questId))
        {
            HLLogger.Log($"@@@ Quest completed: {questId}");
            questUserPlayData.completedQuestIds.Add(questId);
        }

        onComplete?.Invoke();
    }

    public bool IsQuestCompleted(int questId)
    {
        return questUserPlayData.completedQuestIds.Contains(questId);
    }

    #endregion
}

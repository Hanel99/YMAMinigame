using System;
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
    public void AddCubeData(int playCount, int touchCount_TooFast, int touchCount_Fast, int touchCount_Perfect, int touchCount_Slow, int touchCount_TooSlow, int totalTouchCount, int totalScore)
    {
        questUserPlayData.AddCubeData(playCount, touchCount_TooFast, touchCount_Fast, touchCount_Perfect, touchCount_Slow, touchCount_TooSlow, totalTouchCount, totalScore);
    }


    //wingTto
    public void AddWingTtoData(int playCount, int collectCount_Gimbab, int collectCount_SpeedUp, int collectCount_Coin, int collectCount_Exp, int crashCount, float flyDistance)
    {
        questUserPlayData.AddWingTtoData(playCount, collectCount_Gimbab, collectCount_SpeedUp, collectCount_Coin, collectCount_Exp, crashCount, flyDistance);
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
        return detailType switch
        {
            // match card
            QuestDetailType.PlayMatchCardGame => questUserPlayData.matchCardGame.playCount,
            QuestDetailType.FinishCardGameUnderCount => questUserPlayData.matchCardGame.underQuestClearCount,

            // find AI word
            QuestDetailType.PlayFindAIWordGame => questUserPlayData.findAIWordGame.playCount,
            QuestDetailType.FinishWordGameUnderCount => questUserPlayData.findAIWordGame.underQuestClearCount,

            // cube
            QuestDetailType.PlayCubeGame => questUserPlayData.cubeGame.playCount,
            QuestDetailType.ReachCubeScore => playerData.cubeGameHighScore,
            QuestDetailType.ReachTotalTouchCount => questUserPlayData.cubeGame.totalTouchCount,
            QuestDetailType.ReachCubeStateTouchCount => GetCubeTouchCount(questDetailType2),
            QuestDetailType.S_ReachScoreWithoutTooFastOrTooSlow => questUserPlayData.cubeGame.reachScoreWithoutTooFastOrTooSlow,
            QuestDetailType.S_ReachScoreWithOnlyPerfect => questUserPlayData.cubeGame.reachScoreWithOnlyPerfect,

            // wingtto
            QuestDetailType.PlayWingTto => questUserPlayData.wingTto.playCount,
            QuestDetailType.ReachWingTtoDistance => playerData.wingTtoHighScore.ToInt(),
            QuestDetailType.CollectWingTtoItem => GetWingTtoItemCount(questDetailType2),
            QuestDetailType.S_ReachScoreWithoutGimbab => questUserPlayData.wingTto.reachScoreWithoutGimbab.ToInt(),
            QuestDetailType.S_ReachScoreWithCrash => questUserPlayData.wingTto.reachScoreWithCrash.ToInt(),

            // tower
            QuestDetailType.ReachTowerFloor => playerData.towerFloor,
            QuestDetailType.ReachStatLevel => GetTowerStatLevel(questDetailType2),
            QuestDetailType.ReachWeaponLevel => playerData.towerGameUserWeaponData.weaponLevel,
            QuestDetailType.S_WinWithAvoid => questUserPlayData.towerGame.winWithAvoid,

            // collection
            QuestDetailType.CollectCard => playerData.ownCardList.Count,
            QuestDetailType.CollectWord => playerData.ownWordList.Count,

            // gacha
            QuestDetailType.PlayGacha => questUserPlayData.gacha.playGachaCount,
            QuestDetailType.PlayMileageGacha => questUserPlayData.gacha.playMileageGachaCount,

            // common
            QuestDetailType.CollectCoin => (int)Math.Clamp(questUserPlayData.common.collectCoin, int.MinValue, int.MaxValue),
            QuestDetailType.UseCoin => (int)Math.Clamp(questUserPlayData.common.useCoin, int.MinValue, int.MaxValue),
            QuestDetailType.ReachPlayerLevel => playerData.level,
            QuestDetailType.S_EquipEtcIcon => questUserPlayData.common.equipEtcIcon,
            QuestDetailType.E_PlayEndRoll => playerData.finalQuizPlayData.playEndRoll ? 1 : 0,

            _ => 0
        };
    }

    private int GetCubeTouchCount(QuestDetailType2 touchType)
    {
        return touchType switch
        {
            QuestDetailType2.CubeState_TooFast => questUserPlayData.cubeGame.touchCount_TooFast,
            QuestDetailType2.CubeState_Fast => questUserPlayData.cubeGame.touchCount_Fast,
            QuestDetailType2.CubeState_Perfect => questUserPlayData.cubeGame.touchCount_Perfect,
            QuestDetailType2.CubeState_Slow => questUserPlayData.cubeGame.touchCount_Slow,
            QuestDetailType2.CubeState_TooSlow => questUserPlayData.cubeGame.touchCount_TooSlow,
            _ => 0
        };
    }

    private int GetWingTtoItemCount(QuestDetailType2 itemType)
    {
        return itemType switch
        {
            QuestDetailType2.WingTtoItem_Gimbab => questUserPlayData.wingTto.collectCount_Gimbab,
            QuestDetailType2.WingTtoItem_SpeedUp => questUserPlayData.wingTto.collectCount_SpeedUp,
            QuestDetailType2.WingTtoItem_Coin => questUserPlayData.wingTto.collectCount_Coin,
            QuestDetailType2.WingTtoItem_Exp => questUserPlayData.wingTto.collectCount_Exp,
            _ => 0
        };
    }

    private int GetTowerStatLevel(QuestDetailType2 statType)
    {
        return statType switch
        {
            QuestDetailType2.TowerStat_Atk => playerData.towerGameUserStatLevelData.atkLevel,
            QuestDetailType2.TowerStat_Def => playerData.towerGameUserStatLevelData.defLevel,
            QuestDetailType2.TowerStat_Hp => playerData.towerGameUserStatLevelData.hpLevel,
            QuestDetailType2.TowerStat_CriRate => playerData.towerGameUserStatLevelData.criRateLevel,
            QuestDetailType2.TowerStat_CriDmg => playerData.towerGameUserStatLevelData.criDmgLevel,
            _ => 0
        };
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
            questUserPlayData.completedQuestData.Add(questId, DateTime.Now.ToString());
        }

        onComplete?.Invoke();
    }

    public bool IsQuestCompleted(int questId)
    {
        return questUserPlayData.completedQuestIds.Contains(questId);
    }

    #endregion
}

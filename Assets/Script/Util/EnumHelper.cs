using System;
using System.Reflection;





public enum SceneName
{
    IntroScene,
    LobbyScene,
    YMAMatch2CardGame,
    YMAFindAIWordGame,
    YMACubeGame,
    YMAWingTto,
    YMAFinalQuiz,
}



public enum GameType
{
    MatchCardGame = 0,
    FindAIWordGame = 1,
    CubeGame = 2,
    WingTto = 3,




    Count,

}

public enum LanguageType
{
    ko,
    // jp,
    // en,

    Count,
}





#region Intro

public enum IntroState
{
    Ready,
    InitManagers,
    ServerUpdate,
    ResourceLoad,
    LoadUserData,
    PlayFabLogin,
    Complete,

    Error,
}

#endregion



#region ServerData

public enum SheetRangeType
{
    ServerVersion,
    EventDateTimeRange,
    ServerMaintenance,
    AppMinVersion,
    RedeemCodes,
    GameSeed,

    Count,
}




#endregion



#region CardData



public enum CardGrade
{
    Normal,
    Rare,
    SuperRare,
    Silver,
    Gold,
    Black,

    Count,

}

public enum CardMaster
{
    Other,
    Hanel,
    Gathree,
    // Gaejang,
    Yoshi,
    Narae,
    Neemo,
    Nibel,
    Yjyj,
    Dami,
    Ruby,
    Manta,
    Walwaldog,
    Moon,
    Banana,
    Vorrom,
    Jjamong,
    Bbada,
    Samsa,
    // Seno,
    Sola,
    // Siroking,
    Akusi,
    // Yobara,
    Luo,
    Yulmu,
    // Name,
    Judy,
    Zoey,
    Kong,
    Kiwi,
    // Tamtam,
    H,
    Hyunbin,
    // Lune,
    // Hayu,


    Count,
}



#endregion




#region Lobby & GameData


public enum UnlockContent
{
    MatchCardGame = 0,
    FindAIWordGame = 1,
    CubeGame = 2,
    WingTto = 3,
    TowerGame = 4,
    EveryThing = 5,
}

public enum LobbyIconType
{
    Gacha,
    Tower,
    Ranking,
    Collection,
    Quest,
    FinalQuiz,
}




#endregion


#region CommonGame

public enum InGameState
{
    Ready,
    GetSet,
    Play,
    Pause,
    Finish,
}

#endregion




#region TowerGame

public enum TowerUserStatType
{
    atk,
    def,
    hp,
    criRate,
    criDmg
}

public enum TowerGameResultType
{
    stat,
    up,
    stay,
    down,
}


#endregion



#region CubeGame

public enum CubeState
{
    TooFast,
    Fast,
    Perfect,
    Slow,
    TooSlow,

    Wait,
    Stop,
}


#endregion


#region WingTto

public enum WingTtoObjectType
{
    Border,
    Gimbab,
    Stone,
    SpeedUp,
    Coin,
    Exp,
    Wall,

}

public enum WingTtoPlayerState
{
    Ready,
    Pause,
    Fly,
    Invincible,
    Crash,
    Die,

}

#endregion



#region Quest

public enum QuestType
{
    Daily,
    Achievement,
}


public enum QuestGame
{
    MatchCardGame,
    FindAIWordGame,
    CubeGame,
    WingTto,
    TowerGame,
    Gacha,
    Collection,
    Common,
}


public enum QuestDetailType
{
    // 1차 세부 퀘스트

    // match card
    PlayMatchCardGame,
    FinishCardGameUnderCount,

    // find AI word
    PlayFindAIWordGame,
    FinishWordGameUnderCount,

    // cube
    PlayCubeGame,
    ReachCubeScore,
    ReachTotalTouchCount,
    ReachCubeStateTouchCount,
    S_ReachScoreWithoutTooFastOrTooSlow,
    S_ReachScoreWithOnlyPerfect,


    // wingtto
    PlayWingTto,
    ReachWingTtoDistance,
    CollectWingTtoItem,
    S_ReachScoreWithoutGimbab,
    S_ReachScoreWithCrash,

    // tower
    ReachTowerFloor,
    ReachStatLevel,
    ReachWeaponLevel,
    S_WinWithAvoid,

    // collection
    CollectCard,
    CollectWord,

    // gacha
    PlayGacha,
    PlayMileageGacha,


    // common
    CollectCoin,
    UseCoin,
    ReachPlayerLevel,
    S_EquipEtcIcon,
    E_PlayEndRoll,
}

public enum QuestDetailType2
{
    // 추가 디테일이 필요한 경우
    None,

    // cube
    CubeState_TooFast,
    CubeState_Fast,
    CubeState_Perfect,
    CubeState_Slow,
    CubeState_TooSlow,

    // wingtto
    WingTtoItem_Gimbab,
    WingTtoItem_SpeedUp,
    WingTtoItem_Coin,
    WingTtoItem_Exp,

    // tower
    TowerStat_Atk,
    TowerStat_Def,
    TowerStat_Hp,
    TowerStat_CriRate,
    TowerStat_CriDmg,
}

public enum QuestGrade
{
    Basic,
    Difficult,
    Challenge,
    Insane,
    Nightmare,
    Unexpected,
}




public enum QuestState
{
    InProgress,
    ReadyToComplete,
    Complete,
}


public enum FinalReferState
{
    Locked,
    Unlocked,
    Completed
}




#endregion




#region Sound

public enum BGMType
{
    None,
    Intro,
    Lobby,
    CardGame,
    WordGame,
    CubeGame,
    WingTto,
    FinalQuizPhase1,
    FinalQuizPhase2,
    Ending,

    // Loading,
    // Result,
    // Other,

    Count,
}

public enum SFXType
{
    None,
    BtnOK,
    BtnNO,
    Warning,
    Coin,
    Explosion,

    Victory,

    //Wingtto
    Crash,
    WingTtoCoin,
    WingTtoExp,
    WingTtoSpeedUp,
    WingTtoGimbab,
    Ready,
    Go,

    //tower
    TowerAvoid,
    TowerAvoid2,
    TowerHit,
    TowerHit2,
    TowerStart,
    TowerDie,
    WeaponFail,
    WeaponSuccess,
    WeaponStay,


    // final quiz
    FinalDing1,
    FinalDing2,
    FinalClearFake,
    FinalClear,
    FinalFlip,
    FinalGameOver1,
    FinalGameOver2,
    FinalGameOver3,
    FinalCountDown,
    FinalWarning1,
    FinalWarning2,
    O,
    X,
    DiscordEnter,
    DiscordLeave,
    DiscordNoti,

    Ending,
    Count,
}

#endregion



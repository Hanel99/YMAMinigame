using System;
using System.Reflection;





public enum SceneName
{
    IntroScene,
    LobbyScene,
    YMAMatch2CardGame,
    YMAFindAIWordGame,
    YMACubeGame,
}



public enum GameType
{
    MatchCardGame = 0,
    FindAIWordGame = 1,
    CubeGame = 2,

    //WingTto, -> 윙또




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





#region CommonGame

public enum InGameState
{
    Ready,
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
    Perfect,
    Great,
    Good,
    Bad,
    Miss,
    Idle,
    Stop,
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

    // Loading,
    // Result,
    // Other,
    // Ending,

    Count,
}

public enum SFXType
{
    None,
    BtnOK,
    BtnNO,
    Warning,

    // Congratulations,
    // GameOver,
    // GameStart,
    // GameResult,


    Count,
}

#endregion



using System;
using System.Reflection;





public enum SceneName
{
    IntroScene,
    LobbyScene,
    YMAMatch2CardGame,
}



public enum GameType
{
    MatchCardGame = 0,

    //WingTto, -> 윙또
    //CubeGame, -> 버튼 빨리 누르기




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
    CheckAppVersion,
    CheckMaintenance,
    LoadUserData,
    ServerUpdate,
    Complete,

    Error,
}

#endregion



#region ServerData

public enum SheetRangeType
{
    DataVersion,
    EventDateTimeRange,
    RandomValue,
    TodayFirstLoginReward,
    GachaPrice,
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
    Seno,
    Sola,
    Siroking,
    Akusi,
    // Yobara,
    Luo,
    Yulmu,
    Name,
    Judy,
    Zoey,
    Kong,
    Kiwi,
    // Tamtam,
    H,
    Hyunbin,
    Lune,
    Hayu,


    Count,
}



#endregion





#region MatchCardGame

public enum InGameState
{
    Ready,
    Play,
    Pause,
    Finish,
}

public enum PlayerState
{
    Ready,

    WaitChooseCard,
    FlippingCard,

    CheckResult,
    GameOver,

    //... 기타 등등
}




#endregion



#region Sound

#endregion



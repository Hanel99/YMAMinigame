using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.Utilities;
using UnityEngine;


public static class StaticGameData
{
    public static int[] RandomValue = new int[]
    {
        10, // Black
        90, // Gold
        260, // Silver
        500, // SuperRare
        1000, // Rare
        10000, // Normal
    };
    public static int TotalRandomValue => RandomValue[RandomValue.Count() - 1];


    public static int[] GachaPrice = new int[]
    {
        550,  //1회
        5000,  //10회
        300, //1 미획득확정
    };

    public static Color GetGradeBorderColor(CardGrade grade)
    {
        Color color = grade switch
        {
            CardGrade.Normal => Color.white,
            CardGrade.Rare => Color.green,
            CardGrade.SuperRare => Color.blue,
            CardGrade.Silver => Color.gray,
            CardGrade.Gold => Color.yellow,
            CardGrade.Black => Color.black,
            _ => Color.white,

        };
        return color;
    }

    public static Color GetGradeReverseBorderColor(CardGrade grade)
    {
        Color color = grade switch
        {
            CardGrade.Normal => Color.black,
            _ => Color.white,
        };
        return color;
    }

    public static string[] RedeemCodes = new string[]
    {
        "GETCOIN1",
        "GETMILE1",
    };


    // 어드레서블 레이블 이름 관리
    public static class AddressLabels
    {
        public const string SOData = "SOData";
        public const string CardImage = "CardImage";
        public const string GameImage = "GameImage";
        public const string MasterIcon = "MasterIcon";
        public const string PopupGroup = "PopupGroup";
        public const string TowerBossImage = "TowerBoss";
        public const string BGMGroup = "BGM";
        public const string SFXGroup = "SFX";
    }



    public static readonly int MAX_COIN_VALUE = 99999999;
    public static readonly int MAX_MILEAGE_VALUE = 999999;
    public static readonly string SAVE_PLAYER_DATA_KEY = "YMASaveDataAlpha1";
    public static readonly string SAVE_PLAYER_OTHER_DATA_KEY = "YMALocalSaveData";
    public static readonly string SAVE_VERSION_DATA_KEY = "YMAGameVersion";
    private static readonly string DATETIME_FORMAT = "yyyy-MM-dd-HH-mm-ss";

    public static DateTime eventStartTime = DateTime.Now.AddSeconds(-20);
    public static DateTime eventEndTime = DateTime.Now.AddSeconds(-10);
    public static bool IsEventDuration()
    {
        var now = DateTime.Now;
        return now > eventStartTime && now < eventEndTime;
    }
    // public static bool showDevTestText = false;
    public static int targetFrameRate = 60;
    public static int serverGameSeed = 0;



    #region ServerData

    public static IntroController.IntroData introData = new IntroController.IntroData();

    public static void UpdateEventDateTimeFromServer(List<string> sheetData)
    {
        if (sheetData.IsNullOrEmpty()) return;

        eventStartTime = DateTime.ParseExact(sheetData[0], DATETIME_FORMAT, null);
        eventEndTime = DateTime.ParseExact(sheetData[1], DATETIME_FORMAT, null);
    }

    public static void UpdateRedeemCodeFromServer(List<string> sheetData)
    {
        if (sheetData.IsNullOrEmpty()) return;

        var array = sheetData.ToArray();
        array = array.Where(x => x != "").ToArray();

        RedeemCodes = array;
    }

    public static void UpdateGameSeedFromServer(string seed)
    {
        if (string.IsNullOrEmpty(seed)) return;
        if (!int.TryParse(seed, out int result))
        {
            HLLogger.LogWarning($"not int parse : {seed}");
            return;
        }

        serverGameSeed = result;
        HLLogger.Log($"@@@ Update Game Seed : {serverGameSeed}");
    }



    #endregion













    /// <summary>
    /// count만큼의 카드 id를 가져오는 메소드.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public static List<int> GetAllRandomCardIdList(int count = 1)
    {
        var list = GameResourceManager.instance.GetAllCardIds();
        list.Shuffle();

        var selectCardIdList = list.GetRange(0, count);

        StringBuilder sb = new StringBuilder();
        foreach (var item in selectCardIdList)
        {
            sb.Append($"{item},");
        }
        HLLogger.Log($"@@@ Select card List : {sb}");

        return selectCardIdList;
    }


    /// <summary>
    /// 해당 grade에서 count만큼의 카드 id를 가져오는 메소드.
    /// </summary>
    /// <param name="grade"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static List<int> GetRandomCardIdList(CardGrade grade, int count = 1)
    {
        var list = GameResourceManager.instance.GetCardIds(grade);
        list.Shuffle();

        var selectCardIdList = list.GetRange(0, count);

        StringBuilder sb = new StringBuilder();
        foreach (var item in selectCardIdList)
        {
            sb.Append($"{item},");
        }
        HLLogger.Log($"@@@ Select [{grade}] grade card List : {sb}");

        return selectCardIdList;
    }

    public static int GetRandomCardId(CardGrade grade)
    {
        var list = GameResourceManager.instance.GetCardIds(grade);
        list.Shuffle();

        HLLogger.Log($"@@@ Select [{grade}] grade card : {list[0]}");
        return list[0];
    }


    /// <summary>
    /// 각 시도에 지급되어야 할 카드의 등급을 결정하는 메소드
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public static List<CardGrade> GetRandomCardGradeList(int count = 1)
    {
        List<CardGrade> list = new();

        for (int i = 0; i < count; ++i)
        {
            var randomValue = UnityEngine.Random.Range(0, TotalRandomValue);
            CardGrade grade = CardGrade.Normal;

            if (randomValue < RandomValue[0])
                grade = CardGrade.Black;
            else if (randomValue < RandomValue[1])
                grade = CardGrade.Gold;
            else if (randomValue < RandomValue[2])
                grade = CardGrade.Silver;
            else if (randomValue < RandomValue[3])
                grade = CardGrade.SuperRare;
            else if (randomValue < RandomValue[4])
                grade = CardGrade.Rare;

            list.Add(grade);
        }

        HLLogger.Log($"Normal - {list.Count(x => x == CardGrade.Normal)}");
        HLLogger.Log($"Rare - {list.Count(x => x == CardGrade.Rare)}");
        HLLogger.Log($"SuperRare - {list.Count(x => x == CardGrade.SuperRare)}");
        HLLogger.Log($"Silver - {list.Count(x => x == CardGrade.Silver)}");
        HLLogger.Log($"Gold - {list.Count(x => x == CardGrade.Gold)}");
        HLLogger.Log($"Black - {list.Count(x => x == CardGrade.Black)}");
        return list;
    }

    /// <summary>
    /// 각 시도에 지급되어야 할 카드의 등급을 결정하는 메소드
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public static Dictionary<CardGrade, int> GetRandomCardGradeDic(int count = 1)
    {
        Dictionary<CardGrade, int> dic = new();

        for (int i = 0; i < count; ++i)
        {
            var randomValue = UnityEngine.Random.Range(0, TotalRandomValue);
            CardGrade grade = CardGrade.Normal;

            if (randomValue < RandomValue[0])
                grade = CardGrade.Black;
            else if (randomValue < RandomValue[1])
                grade = CardGrade.Gold;
            else if (randomValue < RandomValue[2])
                grade = CardGrade.Silver;
            else if (randomValue < RandomValue[3])
                grade = CardGrade.SuperRare;
            else if (randomValue < RandomValue[4])
                grade = CardGrade.Rare;

            if (dic.ContainsKey(grade))
                dic[grade]++;
            else
                dic.Add(grade, 1);
        }

        SortedDictionary<CardGrade, int> sortDic = new SortedDictionary<CardGrade, int>(dic);
        foreach (var item in sortDic)
        {
            HLLogger.Log($"{item.Key} - {item.Value}");
        }

        return dic;
    }
}
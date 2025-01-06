using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sirenix.Utilities;
using UnityEngine;


public static class StaticGameData
{
    public static int[] RandomValue = new int[]
    {
        100, // Black
        270, // Gold
        570, // Silver
        1370, // SuperRare
        2920, // Rare
        10000, // Normal
    };
    public static int TotalRandomValue => RandomValue[RandomValue.Count() - 1];

    public static int[] GachaPrice = new int[]
    {
        150,  //1회
        1400,  //10회
        150, //1 미획득확정
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
        "getmile", // 200마일리지
        "getgold", // 10000골드
        "getallcard", //모든 카드 해금
        "devtestopen", //데브테스트 기능 오픈
    };



    public static readonly int MAX_COIN_VALUE = 9999999;
    public static readonly int MAX_MILEAGE_VALUE = 999999;
    public static readonly string SAVE_PLAYER_DATA_KEY = "YMASaveDataAlpha1";
    public static readonly string SAVE_VERSION_DATA_KEY = "YMAGameVersion";
    private static readonly string DATETIME_FORMAT = "yyyy-MM-dd-HH-mm-ss";

    public static DateTime eventStartTime = DateTime.Now.AddSeconds(-20);
    public static DateTime eventEndTime = DateTime.Now.AddSeconds(-10);
    public static bool IsEventDuration()
    {
        var now = DateTime.Now;
        return now > eventStartTime && now < eventEndTime;
    }
    public static bool showDevTestText = false;



    #region ServerData

    public static IntroController.IntroData introData = new IntroController.IntroData();



    public static void UpdateRandomValueFromServer(string sheetData)
    {
        var list = SplitSheetData(sheetData);
        if (list.IsNullOrEmpty()) return;

        List<int> intList = ConvertStringListToIntList(list);
        if (intList.IsNullOrEmpty()) return;

        RandomValue = intList.ToArray();
    }

    public static void UpdateEventDateTimeFromServer(string sheetData)
    {
        var list = SplitSheetData(sheetData);
        if (list.IsNullOrEmpty()) return;

        eventStartTime = DateTime.ParseExact(list[0], DATETIME_FORMAT, null);
        eventEndTime = DateTime.ParseExact(list[1], DATETIME_FORMAT, null);
    }



    public static void UpdateGachaPriceFromServer(string sheetData)
    {
        var list = SplitSheetData(sheetData);
        if (list.IsNullOrEmpty()) return;

        List<int> intList = ConvertStringListToIntList(list);
        if (intList.IsNullOrEmpty()) return;

        GachaPrice = intList.ToArray();
    }

    public static void UpdateRedeemCodeFromServer(string sheetData)
    {
        var list = SplitSheetData(sheetData);
        if (list.IsNullOrEmpty()) return;

        RedeemCodes = list.ToArray();
    }


    #endregion













    /// <summary>
    /// count만큼의 카드 id를 가져오는 메소드.
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public static List<int> GetAllRandomCardIdList(int count = 1)
    {
        var list = ResourceManager.instance.GetAllCardIds();
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
        var list = ResourceManager.instance.GetCardIds(grade);
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
        var list = ResourceManager.instance.GetCardIds(grade);
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








    //서버 데이터 분리하는 코드. 각 시트의 범위를 받아와서 각기 가공해서 사용.
    //int로 가공하는건 자주 쓸거 같아 아래 공용코드 추가
    public static List<string> SplitSheetData(string input)
    {
        // string[] splitArray = input.Split('\t');
        // string[] splitArray = input.Split('\r\n');
        string[] splitArray = Regex.Split(input, "\r\n|\r|\n|\t");
        return splitArray.ToList();
    }

    public static List<int> ConvertStringListToIntList(List<string> stringList)
    {
        List<int> intList = new List<int>();
        foreach (string str in stringList)
        {
            if (int.TryParse(str, out int number))
                intList.Add(number);
        }
        return intList;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sirenix.Utilities;
using UnityEngine;


public static class StaticGameData
{
    public static int[] RandomValue = new int[]
    {
        30, // Black
        130, // Gold
        480, // Silver
        1380, // SuperRare
        2880, // Rare
        10000, // Normal
    };
    public static int TotalRandomValue => RandomValue[RandomValue.Count() - 1];


    public static int[] GachaPrice = new int[]
    {
        450,  //1회
        4000,  //10회
        200, //1 미획득확정
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
        public const string QuestGradeImage = "QuestGrade";
        public const string BGMGroup = "BGM";
        public const string SFXGroup = "SFX";
    }



    public static readonly int MAX_COIN_VALUE = 999999999; //9억 9천만
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
    public static readonly int targetFrameRate = 60;
    public static bool useServerSeed = false;
    public static int serverGameSeed = 0;
    public static string serverVersion = "0";
    public static bool useServerData = false;



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


    private static readonly Regex VersionRegex = new Regex(@"^\d+\.\d+\.\d+$");

    public static bool IsUnderVersion(string checkVersion, string baseVersion)
    {
        if (string.IsNullOrEmpty(checkVersion) || string.IsNullOrEmpty(baseVersion))
        {
            HLLogger.LogWarning($"Version is null or empty - check: '{checkVersion}', base: '{baseVersion}'");
            return false;
        }

        if (!IsValidVersionFormat(checkVersion) || !IsValidVersionFormat(baseVersion))
        {
            HLLogger.LogWarning($"Invalid version format - check: '{checkVersion}', base: '{baseVersion}'");
            return false;
        }

        if (checkVersion == baseVersion)
            return false;

        Version check = new Version(checkVersion);
        Version baseVer = new Version(baseVersion);

        return check < baseVer;
    }

    private static bool IsValidVersionFormat(string version)
    {
        return VersionRegex.IsMatch(version);

    }



    #endregion


    #region Tower

    public static readonly int unlockWeaponFloor = 100;
    public static readonly int[] unlockJewelFloor = new int[4] { 200, 300, 400, 500 };
    public static readonly float[] rankUpRate = new float[5] { 3f, 1f, 0.8f, 0.5f, 0.3f };

    #endregion














}
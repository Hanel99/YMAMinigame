using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

public class RankingPopup : PopupBase
{
    enum RankingType
    {
        Level,
        Tower,
        CubeGame,
        WingTto
    }

    public static RankingPopup instance { get; private set; }

    public GameObject rankingGroup;
    public GameObject myRankingGroup;
    public GameObject loadingText;

    public List<RankUserData> rankUserDataList;
    public RankUserData playerRankUserData;

    // Title Text
    public Text levelText;
    public Text expText;
    public Text singleText;



    private List<PlayerLeaderboardEntry> topPlayerDataList = new List<PlayerLeaderboardEntry>();
    private PlayerLeaderboardEntry myPlayerData;
    private RankingType currentRankingType = RankingType.Level;

    protected override void OnAwake()
    {
        instance = this;
    }


    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            OnClickLevelRankingTab();
            _OpenUI();
        }
        else
        {
            _CloseWindow();
        }
    }




    public void OnClickLevelRankingTab()
    {
        currentRankingType = RankingType.Level;
        string statisticName = string.Empty;
        levelText.text = "레벨";
        expText.text = "경험치";
        singleText.text = "";

#if DEV
        statisticName = "LevelDev";
#elif LIVE
        statisticName = "LevelLive";
#else
        statisticName = "Level";
#endif

        SetRankingUI(statisticName).Forget();
    }



    public void OnClickTowerRankingTab()
    {
        currentRankingType = RankingType.Tower;
        string statisticName = string.Empty;
        levelText.text = "";
        expText.text = "";
        singleText.text = "층";

#if DEV
        statisticName = "TowerDev";
#elif LIVE
        statisticName = "TowerLive";
#else
        statisticName = "Tower";
#endif

        SetRankingUI(statisticName).Forget();
    }


    public void OnClickTowerCubeGameTab()
    {
        currentRankingType = RankingType.CubeGame;
        string statisticName = string.Empty;
        levelText.text = "";
        expText.text = "";
        singleText.text = "점";

#if DEV
        statisticName = "CubeGameDev";
#elif LIVE
        statisticName = "CubeGameLive";
#else
        statisticName = "CubeGame";
#endif

        SetRankingUI(statisticName).Forget();
    }


    public void OnClickTowerWingTtoTab()
    {
        currentRankingType = RankingType.WingTto;
        string statisticName = string.Empty;
        levelText.text = "";
        expText.text = "";
        singleText.text = "미터";

#if DEV
        statisticName = "WingTtoDev";
#elif LIVE
        statisticName = "WingTtoLive";
#else
        statisticName = "WingTto";
#endif

        SetRankingUI(statisticName).Forget();
    }



    private async UniTask SetRankingUI(string statisticName)
    {
        rankingGroup.SetActive(false);
        playerRankUserData.gameObject.SetActive(false);
        loadingText.SetActive(true);

        if (statisticName == null)
        {
            Debug.LogError("statisticName is null");
            return;
        }

        try
        {
            (topPlayerDataList, myPlayerData) = await UniTask.WhenAll(PlayFabManager.instance.GetLeaderboard(statisticName), PlayFabManager.instance.GetPlayerRanking(statisticName));

            UpdateLevelTopUI();
            UpdateLevelMyUI();

            await UniTask.DelayFrame(1);

            rankingGroup.SetActive(true);
            playerRankUserData.gameObject.SetActive(true);
            loadingText.SetActive(false);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"리더보드 로드 실패: {e.Message}");
        }
    }





    #region Level Ranking


    void UpdateLevelTopUI()
    {
        for (int i = 0; i < rankUserDataList.Count(); i++)
        {
            if (i > topPlayerDataList.Count() - 1)
            {
                //데이터가 없음. 빈 데이터를 적용
                rankUserDataList[i].UpdateData(i + 1, "", -1, -1, -1);
                continue;
            }

            // 1~10등 데이터
            if (currentRankingType == RankingType.Level)
            {
                var level = topPlayerDataList[i].StatValue / 10000;
                var exp = topPlayerDataList[i].StatValue % 10000;
                rankUserDataList[i].UpdateData(topPlayerDataList[i].Position + 1, topPlayerDataList[i].DisplayName, level, exp, -1);
            }
            else if (currentRankingType == RankingType.Tower)
            {
                rankUserDataList[i].UpdateData(topPlayerDataList[i].Position + 1, topPlayerDataList[i].DisplayName, -1, -1, topPlayerDataList[i].StatValue);
            }
            else if (currentRankingType == RankingType.CubeGame)
            {
                rankUserDataList[i].UpdateData(topPlayerDataList[i].Position + 1, topPlayerDataList[i].DisplayName, -1, -1, topPlayerDataList[i].StatValue);
            }
            else if (currentRankingType == RankingType.WingTto)
            {
                float meter = 0.01f * topPlayerDataList[i].StatValue;
                rankUserDataList[i].UpdateData(topPlayerDataList[i].Position + 1, topPlayerDataList[i].DisplayName, -1, -1, meter, "m");
            }
        }
    }

    void UpdateLevelMyUI()
    {
        // 본인 데이터
        if (currentRankingType == RankingType.Level)
        {
            var level = myPlayerData.StatValue / 10000;
            var exp = myPlayerData.StatValue % 10000;
            playerRankUserData.UpdateData(myPlayerData.Position + 1, myPlayerData.DisplayName, level, exp, -1);
        }
        else if (currentRankingType == RankingType.Tower)
        {
            playerRankUserData.UpdateData(myPlayerData.Position + 1, myPlayerData.DisplayName, -1, -1, myPlayerData.StatValue);
        }
        else if (currentRankingType == RankingType.CubeGame)
        {
            playerRankUserData.UpdateData(myPlayerData.Position + 1, myPlayerData.DisplayName, -1, -1, myPlayerData.StatValue);
        }
        else if (currentRankingType == RankingType.WingTto)
        {
            float meter = 0.01f * myPlayerData.StatValue;
            playerRankUserData.UpdateData(myPlayerData.Position + 1, myPlayerData.DisplayName, -1, -1, meter, "m");
        }

    }

    #endregion






}

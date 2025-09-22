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
    public static RankingPopup instance { get; private set; }

    public GameObject rankingGroup;
    public GameObject myRankingGroup;
    public GameObject loadingText;

    public List<RankUserData> rankUserDataList;
    public RankUserData playerRankUserData;



    private List<PlayerLeaderboardEntry> topPlayerDataList = new List<PlayerLeaderboardEntry>();
    private PlayerLeaderboardEntry myPlayerData;

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
        string statisticName = string.Empty;
#if DEV
        statisticName = "LevelDev";
#elif LIVE
        statisticName = "LevelLive";
#else
        statisticName = "Level";
#endif

        SetRankingUI(statisticName);
    }



    public void OnClickTowerRankingTab()
    {
        string statisticName = string.Empty;
#if DEV
        statisticName = "TowerDev";
#elif LIVE
        statisticName = "TowerLive";
#else
        statisticName = "Tower";
#endif

        SetRankingUI(statisticName);
    }



    private async void SetRankingUI(string statisticName)
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

            if (statisticName.Contains("Level"))
            {
                UpdateLevelTopUI();
                UpdateLevelMyUI();
            }
            else if (statisticName.Contains("Tower"))
            {
                // UpdateTowerTopUI();
                // UpdateTowerMyUI();
            }


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
                rankUserDataList[i].UpdateData(i + 1, "", -1, -1);
                continue;
            }

            // 1~10등 데이터
            var level = topPlayerDataList[i].StatValue / 10000;
            var exp = topPlayerDataList[i].StatValue % 10000;
            rankUserDataList[i].UpdateData(topPlayerDataList[i].Position + 1, topPlayerDataList[i].DisplayName, level, exp);
        }
    }

    void UpdateLevelMyUI()
    {
        // 본인 데이터
        var level = myPlayerData.StatValue / 10000;
        var exp = myPlayerData.StatValue % 10000;
        playerRankUserData.UpdateData(myPlayerData.Position + 1, myPlayerData.DisplayName, level, exp);
    }

    #endregion

    #region Tower Ranking

    #endregion






}

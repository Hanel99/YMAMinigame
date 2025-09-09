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



    private List<PlayerLeaderboardEntry> topPlayers = new List<PlayerLeaderboardEntry>();
    private PlayerLeaderboardEntry playerRank;

    protected override void OnAwake()
    {
        instance = this;
    }


    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            SetData();
            _OpenUI();
        }
        else
        {
            _CloseWindow();
        }
    }


    private async void SetData()
    {
        try
        {
            rankingGroup.SetActive(false);
            playerRankUserData.gameObject.SetActive(false);
            loadingText.SetActive(true);

            (topPlayers, playerRank) = await UniTask.WhenAll(PlayFabManager.instance.GetLeaderboard(), PlayFabManager.instance.GetPlayerRanking());

            UpdateTopRankingsUI();
            UpdatePlayerRankUI();

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



    void UpdateTopRankingsUI()
    {
        for (int i = 0; i < rankUserDataList.Count(); i++)
        {
            if (i > topPlayers.Count() - 1)
            {
                // rankUserDataList[i].gameObject.SetActive(false);
                rankUserDataList[i].UpdateData(i + 1, "", -1, -1);
                continue;
            }

            // rankUserDataList[i].gameObject.SetActive(true);
            if (i == rankUserDataList.Count() - 1)
            {
                // 본인 데이터 
                var level = topPlayers[i].StatValue / 10000;
                var exp = topPlayers[i].StatValue % 10000;
                rankUserDataList[i].UpdateData(topPlayers[i].Position + 1, topPlayers[i].DisplayName, level, exp);
            }
            else
            {
                // 1~10등 데이터
                var level = topPlayers[i].StatValue / 10000;
                var exp = topPlayers[i].StatValue % 10000;
                rankUserDataList[i].UpdateData(topPlayers[i].Position + 1, topPlayers[i].DisplayName, level, exp);
            }
        }
    }

    void UpdatePlayerRankUI()
    {
        var level = playerRank.StatValue / 10000;
        var exp = playerRank.StatValue % 10000;
        playerRankUserData.UpdateData(playerRank.Position + 1, playerRank.DisplayName, level, exp);
    }
}

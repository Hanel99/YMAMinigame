using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DG.Tweening;
using UnityEngine;


public class CardGameManager : MonoBehaviour
{
    public static CardGameManager instance { get; private set; }

    private List<int> selectCardIdList = new();
    private List<Card> touchCardDataList = new();

    private int tryCount = 0;
    private int earnCoinAmount = 0;
    private List<int> collectCardIdList = new();


    // Consts
    private const int MAX_TRY_COUNT_PENALTY_THRESHOLD = 8;
    private const int MAX_TRY_COUNT_REFER_THRESHOLD = 14;
    private const int BASE_EARN_COIN = 20000;
    private const int MIN_EARN_COIN = 10000;
    private const int PENALTY_COIN_PER_TRY = 500;
    private const float GAME_END_DELAY = 1.5f;
    private const float RESULT_POPUP_DELAY = 1.2f;

    // final refer mission
    private bool isReferMissionSuccess = true;



    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // InGameTimer.instance.Init();
        StartCoroutine(nameof(StartGameProcess));
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CardGameUIManager.instance.IsPopupOpen())
                CardGameUIManager.instance.CloseTopPopup();
            else
                CardGameUIManager.instance.ShowPausePopup();
        }

#if UNITY_EDITOR && DEV    
        if (Input.GetKeyDown(KeyCode.C))
        {
            HLLogger.Log($"force clear");

            var cardList = new HashSet<int>(selectCardIdList);
            collectCardIdList = cardList.ToList();
            FinishProcess();
        }
#endif
    }



    private IEnumerator StartGameProcess()
    {
        ReadyProcess();
        yield return new WaitForSecondsRealtime(1);

        PlayProcess();
    }


    private void ReadyProcess()
    {
        CardGameUIManager.instance.ShowSceneMoveAnimation(true);

        //@@@ 카드 8종 선별, 리스트에 넣고 위치 셔플 후 각 카드에 데이터 셋팅.
        CardGameUIManager.instance.ResetInGameUI();
        selectCardIdList.Clear();
        touchCardDataList.Clear();
        collectCardIdList.Clear();
        tryCount = 0;

        var gradeList = StaticGameData.GetRandomCardGradeDic(8);
        foreach (var item in gradeList)
        {
            var idList = StaticGameData.GetRandomCardIdList(item.Key, item.Value);
            selectCardIdList.AddRange(idList);
            selectCardIdList.AddRange(idList);
            //카드 1쌍을 넣어야하니 두개 삽입
        }
        selectCardIdList.Shuffle();


        StringBuilder sb = new StringBuilder();
        foreach (var item in selectCardIdList)
        {
            sb.Append($"{item},");
        }
        HLLogger.Log("@@@ Select card List : " + sb);

        CardGameUIManager.instance.inGamePopup.InitCard();
        CardGameUIManager.instance.inGamePopup.SetCardUI(selectCardIdList);
    }

    private void PlayProcess()
    {
        HLLogger.Log("@@@ Game Start");
        CardGameUIManager.instance.ShowRootDim(false);
        SoundManager.instance.PlayBGM(BGMType.CardGame);
    }

    private bool IsGameFinish()
    {
        return CardGameUIManager.instance.inGamePopup.isAllOpen;
    }

    private void FinishProcess()
    {
        HLLogger.Log("@@@ Game Finish");

        CalcEarnCoinAmount();
        QuestManager.instance.AddMatchCardData(1, tryCount <= MAX_TRY_COUNT_REFER_THRESHOLD ? 1 : 0);
        int exp = 1;

        if (SaveDataManager.instance.playerData.finalQuizPlayData.playEndRoll)
        {
            earnCoinAmount = Math.Min(StaticGameData.MAX_COIN_VALUE, earnCoinAmount * 50);
            exp *= 10;
        }

        SaveDataManager.instance.AddCoin(earnCoinAmount);
        List<int> newCardIDList = SaveDataManager.instance.GetNotOwnCardList(collectCardIdList);
        SaveDataManager.instance.AddOwnCardList(collectCardIdList);
        CardGameUIManager.instance.ShowResult(tryCount, earnCoinAmount, newCardIDList);

        if (SaveDataManager.instance.AddExp(exp))
            DOVirtual.DelayedCall(GAME_END_DELAY, () => CardGameUIManager.instance.ShowPopup<LevelUpPopup>());


        // final refer
        if (FinalReferManager.instance.IsFinalReferUnlock(GameType.MatchCardGame))
        {
            SaveDataManager.instance.SetFinalQuizReferData(GameType.MatchCardGame, FinalReferState.Unlocked);
            DOVirtual.DelayedCall(RESULT_POPUP_DELAY, () => CardGameUIManager.instance.ShowReferPopup(GameType.MatchCardGame));
        }
        else if (FinalReferManager.instance.IsFinalReferStateUnlocked(GameType.MatchCardGame))
        {
            if (isReferMissionSuccess)
            {
                SaveDataManager.instance.SetFinalQuizReferData(GameType.MatchCardGame, FinalReferState.Completed);
                DOVirtual.DelayedCall(RESULT_POPUP_DELAY, () => CardGameUIManager.instance.ShowReferPopup(GameType.MatchCardGame));
            }
        }

    }
    private void CalcEarnCoinAmount()
    {
        earnCoinAmount = BASE_EARN_COIN;
        if (tryCount > MAX_TRY_COUNT_PENALTY_THRESHOLD)
            earnCoinAmount -= (tryCount - MAX_TRY_COUNT_PENALTY_THRESHOLD) * PENALTY_COIN_PER_TRY;
        if (earnCoinAmount <= MIN_EARN_COIN)
            earnCoinAmount = MIN_EARN_COIN;
    }




    public void CardClickProcess(Card cardData)
    {
        if (touchCardDataList.Contains(cardData))
            return;

        //카드 두개를 골랐음
        touchCardDataList.Add(cardData);
        if (touchCardDataList.Count == 2)
            CardMatchProcess();
    }

    private void CardMatchProcess()
    {
        tryCount++;
        bool isMatch = touchCardDataList[0].cardId == touchCardDataList[1].cardId;
        CardGameInGameView.instance.UpdateTryCountText(tryCount);
        if (isMatch)
            collectCardIdList.Add(touchCardDataList[0].cardId);
        CheckReferMission(tryCount, isMatch);

        CardGameUIManager.instance.ShowCardCheckPopup(touchCardDataList, () =>
        {
            if (touchCardDataList[0].cardId != touchCardDataList[1].cardId)
            {
                CardGameUIManager.instance.inGamePopup.CloseCardUI(touchCardDataList[0].cardNumber);
                CardGameUIManager.instance.inGamePopup.CloseCardUI(touchCardDataList[1].cardNumber);
            }

            touchCardDataList.Clear();
            if (IsGameFinish())
                FinishProcess();
        });
    }

    private void CheckReferMission(int tryCount, bool isMatch)
    {
        // 1, 2번째 트라이에는 실패해야 하고, 3번째 트라이에는 성공해야 함
        if (tryCount == 1 && isMatch) isReferMissionSuccess = false;
        else if (tryCount == 2 && isMatch) isReferMissionSuccess = false;
        else if (tryCount == 3 && !isMatch) isReferMissionSuccess = false;
    }
}

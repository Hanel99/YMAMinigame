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
        QuestManager.instance.AddMatchCardData(1, tryCount <= 14 ? 1 : 0);

        SaveDataManager.instance.AddCoin(earnCoinAmount);
        List<int> newCardIDList = SaveDataManager.instance.GetNotOwnCardList(collectCardIdList);
        SaveDataManager.instance.AddOwnCardList(collectCardIdList);
        CardGameUIManager.instance.ShowResult(tryCount, earnCoinAmount, newCardIDList);

        if (SaveDataManager.instance.AddExp(1))
            DOVirtual.DelayedCall(1.5f, () => CardGameUIManager.instance.ShowPopup<LevelUpPopup>());


        // final refer
        if (FinalReferManager.instance.IsFinalReferUnlock(GameType.MatchCardGame))
        {
            SaveDataManager.instance.SetFinalQuizReferData(GameType.MatchCardGame, FinalReferState.Unlocked);
            DOVirtual.DelayedCall(1.2f, () => CardGameUIManager.instance.ShowReferPopup(GameType.MatchCardGame));
        }
        else if (FinalReferManager.instance.IsFinalReferStateUnlocked(GameType.MatchCardGame))
        {
            if (isReferMissionSuccess)
            {
                SaveDataManager.instance.SetFinalQuizReferData(GameType.MatchCardGame, FinalReferState.Completed);
                DOVirtual.DelayedCall(1.2f, () => CardGameUIManager.instance.ShowReferPopup(GameType.MatchCardGame));
            }
        }

    }
    private void CalcEarnCoinAmount()
    {
        earnCoinAmount = 20000;
        if (tryCount > 8)
            earnCoinAmount -= (tryCount - 8) * 500;
        if (earnCoinAmount <= 10000)
            earnCoinAmount = 10000;
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
        if (tryCount == 1 && isMatch)
            isReferMissionSuccess = false;
        else if (tryCount == 2 && isMatch)
            isReferMissionSuccess = false;
        else if (tryCount == 3 && !isMatch)
            isReferMissionSuccess = false;
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class QuestPopup : PopupBase
{
    public static QuestPopup instance { get; private set; }

    [Header("UI Components")]
    [SerializeField] private Dropdown questFilterDropdown;
    [SerializeField] private QuestItem questItemPrefab;
    [SerializeField] private Transform questItemRoot;

    public List<QuestItem> questItemList = new List<QuestItem>();

    private CancellationTokenSource questSpawnCTS;

    protected override void OnAwake()
    {
        instance = this;
        SetupQuestFilter();
    }

    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            // 팝업을 열 때 필터를 기본값(첫 번째 항목)으로 설정하고 뷰를 업데이트합니다.
            questFilterDropdown.value = 0;
            OnQuestFilterChanged();
            _OpenUI();
        }
        else
        {
            questSpawnCTS?.Cancel(); // 팝업을 닫을 때 진행중인 업데이트를 취소합니다.
            _CloseWindow();
        }
    }

    /// <summary>
    /// 퀘스트 필터 드롭다운의 값이 변경될 때 호출됩니다.
    /// Unity 에디터의 Dropdown 컴포넌트에 있는 OnValueChanged 이벤트에 이 메서드를 연결해주세요.
    /// </summary>
    public void OnQuestFilterChanged()
    {
        questSpawnCTS?.Cancel();
        questSpawnCTS = new CancellationTokenSource();

        // 드롭다운의 현재 값(인덱스)을 QuestGame enum으로 변환합니다.
        // 이 방식이 동작하려면 드롭다운 옵션 순서와 enum 멤버 순서가 일치해야 합니다.
        QuestGame selectedGame = (QuestGame)questFilterDropdown.value;
        SetQuestItems(selectedGame, questSpawnCTS.Token).Forget();
    }

    /// <summary>
    /// QuestGame 열거형을 기반으로 드롭다운 필터 옵션을 설정합니다.
    /// </summary>
    private void SetupQuestFilter()
    {
        questFilterDropdown.ClearOptions();

        var options = new List<string>();
        // QuestGame 열거형의 모든 값을 가져와 옵션으로 추가합니다.
        foreach (QuestGame game in Enum.GetValues(typeof(QuestGame)))
        {
            options.Add(LocalizeManager.instance.GetString($"Quest.GameName.{game}"));
        }

        questFilterDropdown.AddOptions(options);
    }

    // Consts
    private const int BATCH_SIZE = 30;
    private const int DISABLE_BATCH_SIZE = 100;

    private async UniTask SetQuestItems(QuestGame questGame, CancellationToken token)
    {
        var questMetaDataList = GameResourceManager.instance.GetQuestMetaData(questGame);

        // 퀘스트 데이터 정렬
        questMetaDataList.Sort((a, b) =>
        {
            // 완료 여부 확인
            bool isCompleteA = QuestManager.instance.IsQuestCompleted(a.id);
            bool isCompleteB = QuestManager.instance.IsQuestCompleted(b.id);

            // 완료된 퀘스트가 뒤로 가도록 정렬 (false < true)
            if (isCompleteA != isCompleteB)
                return isCompleteA.CompareTo(isCompleteB);

            // 완료 상태가 같다면 ID 순으로 정렬 (오름차순)
            return a.id.CompareTo(b.id);
        });


        // 필터링된 리스트 생성
        var visibleList = new List<QuestMetaData>();
        foreach (var quest in questMetaDataList)
        {
            QuestMetaData preQuest = null;
            // 1. Explicit PreQuestId
            if (quest.preQuestId > 0)
            {
                var target = GameResourceManager.instance.GetQuestMetaData(quest.detailType, quest.detailType2, quest.preQuestId);
                // detailType과 detailType2가 모두 같은 경우에만 사전 퀘스트로 인정
                if (target != null && target.detailType == quest.detailType && target.detailType2 == quest.detailType2)
                {
                    preQuest = target;
                }
            }
            // 2. Implicit SubId (only if explicit is not set)
            else if (quest.subId > 1)
            {
                preQuest = questMetaDataList.Find(x => x.detailType == quest.detailType && x.detailType2 == quest.detailType2 && x.subId == quest.subId - 1);
            }

            if (preQuest != null)
            {
                bool isComplete = QuestManager.instance.IsQuestCompleted(preQuest.id);
                bool isReady = QuestManager.instance.IsQuestReady(preQuest);

                // 사전 퀘스트가 완료되었거나, 완료 대기 상태여야 현 퀘스트 노출
                if (!isComplete && !isReady)
                    continue;
            }

            visibleList.Add(quest);
        }

        int requiredCount = visibleList.Count;
        int currentCount = questItemList.Count;

        // questItem이 부족하면 추가 생성
        if (currentCount < requiredCount)
        {
            for (int i = currentCount; i < requiredCount; ++i)
            {
                if (token.IsCancellationRequested) return;
                var questItem = questItemPrefab.Spawn(questItemRoot);
                questItem.transform.localScale = Vector3.one;
                questItemList.Add(questItem);

                // 생성은 비용이 높으므로 짧은 주기로 yield
                if (i > 0 && i % BATCH_SIZE == 0)
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        // 1. 활성 리스트 처리 (데이터 설정 포함)
        for (int i = 0; i < requiredCount; ++i)
        {
            if (token.IsCancellationRequested) return;

            questItemList[i].SetData(visibleList[i]);
            questItemList[i].gameObject.SetActive(true);

            if (i > 0 && i % BATCH_SIZE == 0)
                await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        // 2. 비활성 리스트 처리 (단순 끄기)
        // 나머지는 비활성화. 단순 SetActive(false)는 비용이 낮으므로 한 번에 처리하거나 큰 배치를 사용

        for (int i = requiredCount; i < questItemList.Count; ++i)
        {
            if (token.IsCancellationRequested) return;

            questItemList[i].gameObject.SetActive(false);

            if ((i - requiredCount) > 0 && (i - requiredCount) % DISABLE_BATCH_SIZE == 0)
                await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        questItemPrefab.gameObject.SetActive(false);
        // 레이아웃 갱신을 위해 껐다 켜기
        questItemRoot.gameObject.SetActive(false);
        questItemRoot.gameObject.SetActive(true);
    }



    public void ReceiveAllQuestReward()
    {
        long totalCoin = 0;
        int totalExp = 0;
        bool isLevelUp = false;
        int receivedCount = 0;

        foreach (var item in questItemList)
        {
            if (item.gameObject.activeInHierarchy && item.IsReadyToComplete)
            {
                int earnCoinAmount = item.RewardCoin;
                int earnExpAmount = item.RewardExp;

                if (SaveDataManager.instance.playerData.finalQuizPlayData.playEndRoll)
                {
                    earnCoinAmount = (int)Math.Min((long)StaticGameData.MAX_COIN_VALUE, (long)earnCoinAmount * 50);
                    earnExpAmount *= 10;
                }

                totalCoin += earnCoinAmount;
                totalExp += earnExpAmount;

                QuestManager.instance.CompleteQuest(item.questId);
                item.SetCompleteState();
                receivedCount++;
            }
        }

        if (receivedCount > 0)
        {
            int finalCoin = (int)Math.Min(totalCoin, (long)StaticGameData.MAX_COIN_VALUE);

            SaveDataManager.instance.AddCoin(finalCoin);
            isLevelUp = SaveDataManager.instance.AddExp(totalExp);

            GameListView.instance.UpdateUserProfileProcess();
            LobbyUIManager.instance.ShowQuestRewardPopup(finalCoin, totalExp, isLevelUp);
        }
    }
}
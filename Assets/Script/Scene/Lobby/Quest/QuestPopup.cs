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
    private const int BATCH_SIZE = 10;

    private async UniTask SetQuestItems(QuestGame questGame, CancellationToken token)
    {
        var questMetaDataList = GameResourceManager.instance.GetQuestMetaData(questGame);

        // 필터링된 리스트 생성
        var visibleList = new List<QuestMetaData>();
        foreach (var quest in questMetaDataList)
        {
            QuestMetaData preQuest = null;
            // 1. Explicit PreQuestId
            if (quest.preQuestId > 0)
            {
                var target = GameResourceManager.instance.GetQuestMetaData(quest.detailType, quest.preQuestId);
                // detailType이 같은 경우에만 사전 퀘스트로 인정
                if (target != null && target.detailType == quest.detailType)
                {
                    preQuest = target;
                }
            }
            // 2. Implicit SubId (only if explicit is not set)
            else if (quest.subId > 1)
            {
                preQuest = questMetaDataList.Find(x => x.detailType == quest.detailType && x.subId == quest.subId - 1);
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

                if (i > 0 && i % BATCH_SIZE == 0)
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }

        // 모든 questItem 데이터 설정 및 활성화/비활성화
        for (int i = 0; i < questItemList.Count; ++i)
        {
            if (token.IsCancellationRequested) return;

            if (i < requiredCount)
            {
                questItemList[i].SetData(visibleList[i]);
                questItemList[i].gameObject.SetActive(true);
            }
            else
            {
                questItemList[i].gameObject.SetActive(false);
            }

            if (i > 0 && i % BATCH_SIZE == 0)
                await UniTask.Yield(PlayerLoopTiming.Update, token);
        }

        questItemPrefab.gameObject.SetActive(false);
        questItemRoot.gameObject.SetActive(false);
        questItemRoot.gameObject.SetActive(true);
    }
}
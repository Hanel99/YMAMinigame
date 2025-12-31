using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager instance { get; private set; }




    public QuestUserPlayData questUserPlayData => SaveDataManager.instance.playerData.questUserPlayData;






    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }


    public void InitQuestData()
    {
        // 플레이어 데이터에서 퀘스트 데이터 가져와서 초기화.
        // 저장할 때 퀘스트 데이터를 가져가게 해서 저장.

    }



    #region Quest Progress 

    public void AddQuestProgress(QuestDetailType detailType, QuestDetailType2 questDetailType2, int progress)
    {
        // 퀘스트 진행도 업데이트.


        switch (detailType)
        {

            //TODO Sample
            // case QuestDetailType.ReachTotalTouchCount:
            //     if (questDetailType2 == QuestDetailType2.CubeState_Fast)
            //         questUserPlayData.testData += progress;
            //     else
            //         questUserPlayData.testData += progress;
            //     break;

            default:
                break;
        }


        //TODO sample

        // questUserPlayData.testData += progress;
        // HLLogger.Log("@@@ Quest progress added. Current testData: " + questUserPlayData.testData);

    }


    public int GetQuestValue(QuestDetailType detailType, QuestDetailType2 questDetailType2)
    {
        switch (detailType)
        {
            //TODO Sample
            // case QuestDetailType.ReachTotalTouchCount:
            //     if (questDetailType2 == QuestDetailType2.CubeState_Fast)
            //         return questUserPlayData.testData;
            //     else
            //         return questUserPlayData.testData / 2;


            default:
                return 0;
        }
    }


    #endregion


    #region Quest Completion 

    public void CompleteQuest(int questId, System.Action onComplete = null)
    {
        // 퀘스트 완료 처리.
        if (!questUserPlayData.completedQuestIds.Contains(questId))
        {
            HLLogger.Log($"@@@ Quest completed: {questId}");
            questUserPlayData.completedQuestIds.Add(questId);
        }

        onComplete?.Invoke();
    }
    public bool IsQuestCompleted(int questId)
    {
        return questUserPlayData.completedQuestIds.Contains(questId);
    }



    #endregion

















}

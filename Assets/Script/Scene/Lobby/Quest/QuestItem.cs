using UnityEngine;
using UnityEngine.UI;

public class QuestItem : MonoBehaviour
{
    public int questId;

    public Image questGradeIcon;
    public Text indexText;
    public Text gameNameText;
    public Text descText;

    public Slider progressBar;
    public Text progressText;

    public GameObject borderBlink;
    public GameObject rewardButtonBorderBlink;
    public GameObject completeDim;
    public Text completeText;
    public GameObject unknownDim;


    private int progressValue = 0;
    private int progressMaxValue = 100;
    private float progress = 0f;

    private QuestState questState = QuestState.InProgress;
    private QuestMetaData questData;

    private bool isRewardProcessing = false;
    private int completeTouchCount = 0;



    public void SetData(QuestMetaData questMetaData)
    {
        questData = questMetaData;
        questId = questData.id;
        questState = QuestState.InProgress;



        //TODO 다 제대로 넣은게 아님.

        indexText.text = $"No.{questData.id.ToString("D2")}";
        questGradeIcon.sprite = GameResourceManager.instance.GetQuestGradeIcon(questData.grade);
        gameNameText.text = LocalizeManager.instance.GetString($"Quest.GameName.{questData.questGame}");

        SetDescText();
        progressValue = QuestManager.instance.GetQuestValue(questData.detailType, questData.detailType2);
        progressMaxValue = GetMaxProgressValue();
        UpdateProgress();
        SetDimCover();



    }


    private void SetDescText()
    {
        string desc = LocalizeManager.instance.GetString($"Quest.Desc.{questData.detailType}");
        string detail2 = "";
        if (questData.detailType2 != QuestDetailType2.None)
            detail2 = LocalizeManager.instance.GetString($"Quest.Detail2.{questData.detailType2}");

        //TODO detailtype에 맞춰서 format 조정하기
        string descMix = string.Format(desc, questData.reachCount, questData.tryCount);


        descText.text = descMix;
    }


    private int GetMaxProgressValue()
    {
        switch (questData.detailType)
        {
            case QuestDetailType.ReachTotalTouchCount:
            case QuestDetailType.ReachWingTtoDistance:
                return questData.reachCount;

            default:
                return questData.tryCount;
        }
    }


    public void UpdateProgress()
    {
        progressValue = QuestManager.instance.GetQuestValue(questData.detailType, questData.detailType2);
        progressValue = Mathf.Min(progressValue, progressMaxValue);

        progress = (float)progressValue / progressMaxValue;
        progress = Mathf.Max(0f, Mathf.Min(1f, progress));

        progressBar.value = progress;
        progressText.text = $"{progressValue} / {progressMaxValue}";

        // 퀘스트 상태 업데이트
        if (progressValue >= progressMaxValue)
        {
            if (QuestManager.instance.IsQuestCompleted(questId))
            {
                questState = QuestState.Complete;
                borderBlink.SetActive(false);
                rewardButtonBorderBlink.SetActive(false);
                return;
            }
            else
            {
                questState = QuestState.ReadyToComplete;
                borderBlink.SetActive(true);
                rewardButtonBorderBlink.SetActive(true);
                return;
            }
        }
        else
        {
            questState = QuestState.InProgress;
            borderBlink.SetActive(false);
            rewardButtonBorderBlink.SetActive(false);
        }
    }


    private void SetDimCover()
    {
        completeDim.SetActive(questState == QuestState.Complete);
        unknownDim.SetActive(questState == QuestState.InProgress && questData.hidden);
    }
















    public void OnClickReward()
    {
        if (isRewardProcessing)
            return;

        if (questState != QuestState.InProgress)
        {
            // 체크가 덜 된 경우, 재 체크 후 정상 진입
            // 퀘스트 매니저를 통해 체크할것
            UpdateProgress();
            if (questState == QuestState.Complete || progressValue >= progressMaxValue)
                questState = QuestState.Complete;
            else
                return;
        }

        isRewardProcessing = true;
        // 보상 수령 처리 및 퀘스트 완료 처리

        QuestManager.instance.CompleteQuest(questId, () =>
        {
            // 완료 후 처리
            SaveDataManager.instance.AddCoin(questData.coin);
            SaveDataManager.instance.AddExp(questData.exp);

            UpdateProgress();
            SetDimCover();

            isRewardProcessing = false;
        });




    }


    public void OnClickComplete()
    {
        if (string.IsNullOrEmpty(questData.redeem))
            return;

        completeTouchCount++;
        if (completeTouchCount >= 5)
        {
            completeText.text = questData.redeem;
            HLLogger.Log($"redeem Code : {questData.redeem}");
        }
    }





}

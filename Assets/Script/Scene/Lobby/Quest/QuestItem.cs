using System;
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
    public Text coinRewardText;
    public Text expRewardText;

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
        gameNameText.text = LocalizeManager.instance.GetString($"Quest.GameName.{questData.questGame}");

        // questGradeIcon.sprite = GameResourceManager.instance.GetQuestGradeIcon(questData.grade);
        questGradeIcon.sprite = GameResourceManager.instance.GetQuestGradeIcon(questData.questType, questData.hidden);
        // questGradeIcon.gameObject.SetActive(questData.hidden);

        SetDescText();
        coinRewardText.text = UnitKorean(questData.coin);
        expRewardText.text = questData.exp.ToString();

        progressValue = QuestManager.instance.GetQuestValue(questData.detailType, questData.detailType2);
        progressMaxValue = questData.tryCount;
        UpdateProgress();
        SetDimCover();

    }

    public void UpdateQuestUI()
    {
        progressValue = QuestManager.instance.GetQuestValue(questData.detailType, questData.detailType2);
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
        string descMix = string.Format(desc, UnitKorean(questData.tryCount), detail2);

        descText.text = descMix;
    }



    public void UpdateProgress()
    {
        progressValue = QuestManager.instance.GetQuestValue(questData.detailType, questData.detailType2);
        progressValue = Mathf.Min(progressValue, progressMaxValue);

        progress = (float)progressValue / progressMaxValue;
        progress = Mathf.Max(0f, Mathf.Min(1f, progress));

        progressBar.value = progress;
        progressText.text = $"{progressValue:N0} / {progressMaxValue:N0}";

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
        unknownDim.SetActive(SaveDataManager.instance.playerData.finalQuizPlayData.playEndRoll == false && questState == QuestState.InProgress && questData.hidden);
    }
















    public void OnClickReward()
    {
        if (isRewardProcessing)
            return;

        if (questState != QuestState.ReadyToComplete)
        {
            // 체크가 덜 된 경우, 재 체크 후 정상 진입
            // 퀘스트 매니저를 통해 체크할것
            UpdateProgress();
            if (questState == QuestState.ReadyToComplete || progressValue >= progressMaxValue)
                questState = QuestState.ReadyToComplete;
            else
                return;
        }

        isRewardProcessing = true;
        // 보상 수령 처리 및 퀘스트 완료 처리

        QuestManager.instance.CompleteQuest(questId, () =>
        {
            // 완료 후 처리
            int earnCoinAmount = questData.coin;
            int earnExpAmount = questData.exp;

            if (SaveDataManager.instance.playerData.finalQuizPlayData.playEndRoll)
            {
                earnCoinAmount = (int)Math.Min((long)StaticGameData.MAX_COIN_VALUE, (long)earnCoinAmount * 50);
                earnExpAmount *= 10;
            }

            SaveDataManager.instance.AddCoin(earnCoinAmount);
            bool isShowLevelUpPopup = SaveDataManager.instance.AddExp(earnExpAmount);

            UpdateProgress();
            SetDimCover();

            isRewardProcessing = false;
            LobbyUIManager.instance.ShowQuestRewardPopup(earnCoinAmount, earnExpAmount, isShowLevelUpPopup);
        });




    }


    // Consts
    private const int UNIT_HM = 100000000;
    private const int UNIT_M = 10000;
    private const int TOUCH_COUNT_FOR_REDEEM = 3;

    private const string TEXT_UNIT_HM = "억";
    private const string TEXT_UNIT_M = "만";

    public void OnClickComplete()
    {
        if (string.IsNullOrEmpty(questData.redeem))
            return;

        completeTouchCount++;
        if (completeTouchCount >= TOUCH_COUNT_FOR_REDEEM)
        {
            completeText.text = questData.redeem;
            GUIUtility.systemCopyBuffer = questData.redeem;

            HLLogger.Log($"redeem Code : {questData.redeem}");
        }
    }

    private string UnitKorean(int value)
    {
        if (value >= UNIT_HM)
            return $"{value / UNIT_HM}{TEXT_UNIT_HM}";
        else if (value >= UNIT_M)
            return $"{value / UNIT_M}{TEXT_UNIT_M}";
        else
            return value.ToString();
    }
}

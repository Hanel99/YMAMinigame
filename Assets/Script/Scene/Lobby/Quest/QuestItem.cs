using UnityEngine;
using UnityEngine.UI;

public class QuestItem : MonoBehaviour
{
    public int questId;

    public Text questGradeText;
    public Text indexText;
    public Text gameNameText;
    public Text descText;

    public Slider progressBar;
    public Text progressText;
    public Button rewardButton;

    public GameObject borderBlink;
    public GameObject rewardButtonBorderBlink;
    public GameObject completeDim;
    public GameObject unknownDim;


    private int progressValue = 0;
    private int progressMaxValue = 100;
    private float progress = 0f;

    private QuestState questState = QuestState.NotStarted;

    private bool isRewardProcessing = false;



    public void SetData()
    {
        // 퀘스트 데이터를 그대로 받아올듯? 그 데이터를 가공해서 내부 저장하거나 데이터 원본 그대로 저장하는걸로


        questState = QuestState.NotStarted;

    }


    public void UpdateProgress(int value)
    {
        progressValue = Mathf.Min(value, progressMaxValue);

        progress = (float)progressValue / progressMaxValue;
        progressBar.value = progress;
        progressText.text = $"{progressValue} / {progressMaxValue}";

        // 퀘스트 상태 업데이트
        if (progressValue >= progressMaxValue)
        {
            questState = QuestState.Complete;
            borderBlink.SetActive(true);
        }
        else
        {
            questState = QuestState.InProgress;
            borderBlink.SetActive(false);
        }
    }

    private void UpdateProgress()
    {
        // 퀘스트매니저에서 현재 진행도 받아오기
        int progress = 1;
        UpdateProgress(progress);
    }


    public void OnClickReward()
    {
        if (isRewardProcessing)
            return;

        if (questState != QuestState.Complete)
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




    }





}

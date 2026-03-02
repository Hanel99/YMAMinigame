using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 쥬얼 강화 확률을 등급별로 안내해주는 팝업 클래스입니다.
/// TowerGameJewelDetailPopup.cs의 구조를 따라 PopupBase를 상속받습니다.
/// </summary>
public class TowerGameJewelProbabilityPopup : PopupBase
{
    public static TowerGameJewelProbabilityPopup instance { get; private set; }

    [Header("UI Elements")]
    public Text gradeText;
    public Button leftButton;
    public Button rightButton;

    [Header("Item Elements")]
    public Text gradeUpText;
    public List<TowerGameJewelProbabilityItem> itemList = new List<TowerGameJewelProbabilityItem>();

    private TowerJewelGrade currentGrade;



    protected override void OnAwake()
    {
        instance = this;
    }

    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            UpdateUI();
            _OpenUI();
        }
        else
        {
            _CloseWindow();
        }
    }


    public void ShowPopup(TowerJewelGrade grade)
    {
        currentGrade = grade;
        ShowPopup(true);
    }

    private void UpdateUI()
    {
        if (gradeText != null)
        {
            gradeText.text = $"{LocalizeManager.instance.GetString($"grade.name.{currentGrade}")}";
        }

        float rankUpProb = currentGrade >= TowerJewelGrade.Black ? 0 : StaticGameData.rankUpRate[(int)currentGrade];
        if (gradeUpText != null)
        {
            gradeUpText.text = currentGrade < TowerJewelGrade.Black ? $"등급 업 확률 : {rankUpProb}%" : "";
        }
        leftButton?.gameObject.SetActive(currentGrade > TowerJewelGrade.Normal);
        rightButton?.gameObject.SetActive(currentGrade < TowerJewelGrade.Black);

        var metaDataList = GameResourceManager.instance.GetTowerJewelGradeList(currentGrade);
        int totalWeight = 0;
        if (metaDataList != null)
        {
            for (int i = 0; i < metaDataList.Count; i++)
            {
                totalWeight += metaDataList[i].weight;
            }
        }

        int dataCount = metaDataList != null ? metaDataList.Count : 0;


        for (int i = 0; i < dataCount; i++)
        {
            var meta = metaDataList[i];
            float probability = totalWeight > 0 ? ((float)meta.weight / totalWeight) * 100f : 0f;

            itemList[i].UpdateUI(meta, probability);
        }
    }

    public void OnClickLeft()
    {
        if (currentGrade > TowerJewelGrade.Normal)
        {
            currentGrade--;
            UpdateUI();
        }
    }

    public void OnClickRight()
    {
        if (currentGrade < TowerJewelGrade.Black)
        {
            currentGrade++;
            UpdateUI();
        }
    }
}

using System.Collections.Generic;
using System.Text;
using Coffee.UIExtensions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TowerGameWeaponJewel : MonoBehaviour
{
    public TowerGameWeaponImage weaponImage;
    public Button[] jewelSlots;
    public GameObject arrow;

    public Text gradeText;
    public Text typeText;
    public Text valueText;
    public Text rankUpRateText;
    public Text rollCoinText;

    public Toggle lockToggle;
    public Button enchantButton;

    public UIParticle rankUpParticle;

    //private 
    private List<TowerJewelUserData> jewelUserDataList;
    private TowerJewelUserData currentJewelData;
    public TowerJewelGrade CurrentGrade => currentJewelData.grade;
    private LongPressButton longPressButton;
    private bool isLongPress = false;
    private int currentJewelIndex;
    private bool isRankUp = false;
    private int rollCoinCost = 0;

    private readonly int[] rollCoin = new int[6] { 1500, 3150, 6620, 13910, 29220, 61370 };
    private readonly int[] rollLockCoin = new int[6] { 2, 2, 3, 3, 4, 4 };

    private PlayerData PlayerData => SaveDataManager.instance.playerData;
    private OtherPlayerData OtherPlayerData => SaveDataManager.instance.otherPlayerData;

    public void UpdateUIData()
    {
        jewelUserDataList = PlayerData.towerJewelUserDataList;
        for (int i = 0; i < jewelSlots.Length; i++)
        {
            jewelSlots[i].gameObject.SetActive(i < jewelUserDataList.Count);
        }
        weaponImage.UpdateWeaponImage();

        if (longPressButton == null)
        {
            longPressButton = enchantButton.GetComponent<LongPressButton>();
            longPressButton.SetLongPressAction(OnClickRoll);
        }

        currentJewelIndex = jewelUserDataList.Count > 0 ? 0 : -1;
        OnClickJewel(currentJewelIndex);
    }


    public void OnClickJewel(int index)
    {
        if (index < 0 || jewelUserDataList.Count <= index)
            return;

        currentJewelIndex = index;
        currentJewelData = jewelUserDataList[index];
        lockToggle.isOn = OtherPlayerData.isJewelLock[index];

        rankUpParticle.gameObject.SetActive(false);
        var jewelPosition = jewelSlots[currentJewelIndex].transform.localPosition;
        arrow.transform.localPosition = new Vector3(jewelPosition.x, jewelPosition.y - 150, jewelPosition.z);
        UpdateStatUI();
        UpdateRollCoinCost();
    }

    private void UpdateStatUI()
    {
        if (isRankUp)
        {
            UpdateRollCoinCost();
            SoundManager.instance.PlaySFX(SFXType.WeaponSuccess);

            rankUpParticle.gameObject.SetActive(true);
            rankUpParticle.Play();
            isRankUp = false;
        }

        rankUpRateText.text = currentJewelData.grade >= TowerJewelGrade.Black ? "" : $"등급 상승 확률 : {StaticGameData.rankUpRate[(int)currentJewelData.grade]}%";
        gradeText.text = $"{LocalizeManager.instance.GetString($"grade.name.{currentJewelData.grade}")} 등급";
        typeText.text = LocalizeManager.instance.GetString($"jewel.type.{currentJewelData.type}");

        valueText.text = GetJewelValueString(currentJewelData.type, currentJewelData.value);
        TowerGameWeaponEnchantPopup.instance.UpdateUI();
    }

    private void UpdateRollCoinCost()
    {
        rollCoinCost = lockToggle.isOn ? (rollCoin[(int)currentJewelData.grade] * rollLockCoin[(int)currentJewelData.grade])
                                        : rollCoin[(int)currentJewelData.grade];
        rollCoinText.text = rollCoinCost.ToString("N0");
    }


    public void OnClickRoll(bool isLongPress = false)
    {
        if (currentJewelIndex < 0 || jewelUserDataList.Count <= currentJewelIndex || PlayerData.coin < rollCoinCost)
            return;

        this.isLongPress = isLongPress;
        TowerJewelGrade targetGrade = currentJewelData.grade;
        TowerWeaponJewelMetaData selectedMeta = null;

        if (lockToggle.isOn == false)
        {
            // 등급업 체크
            int gradeIndex = (int)currentJewelData.grade;
            if (gradeIndex < StaticGameData.rankUpRate.Length && gradeIndex < (int)TowerJewelGrade.Black)
            {
                float rate = StaticGameData.rankUpRate[gradeIndex];
                int rand = UnityEngine.Random.Range(0, 10000);
                if (rand < rate * 100)
                {
                    isRankUp = true;
                    targetGrade = (TowerJewelGrade)(gradeIndex + 1);
                }
            }

            // 가중치 고려 타입 선정
            var jewelMetaList = GameResourceManager.instance.GetTowerJewelGradeList(targetGrade);
            if (jewelMetaList == null || jewelMetaList.Count == 0)
                return;

            int totalWeight = 0;
            for (int i = 0; i < jewelMetaList.Count; i++)
            {
                totalWeight += jewelMetaList[i].weight;
            }

            int weightRand = Random.Range(0, totalWeight);
            int currentWeight = 0;

            for (int i = 0; i < jewelMetaList.Count; i++)
            {
                currentWeight += jewelMetaList[i].weight;
                if (weightRand < currentWeight)
                {
                    selectedMeta = jewelMetaList[i];
                    break;
                }
            }
        }
        else
        {
            selectedMeta = GameResourceManager.instance.GetTowerJewelGradeList(targetGrade, currentJewelData.type);
        }

        // 이런 경우가 있을까 싶은데 예외처리...
        if (selectedMeta == null)
        {
            HLLogger.LogWarning("selectedMeta is null");
            return;
        }

        // 최종 수치 계산
        float finalValue = 0f;
        if (selectedMeta.type == TowerJewelType.CriRate || selectedMeta.type == TowerJewelType.CriDmg || selectedMeta.type == TowerJewelType.Avoid
            || selectedMeta.type == TowerJewelType.DefBreak || selectedMeta.type == TowerJewelType.DmgReduce || selectedMeta.type == TowerJewelType.AtkMul)
        {
            float randVal = UnityEngine.Random.Range(selectedMeta.min, selectedMeta.max);
            finalValue = (float)System.Math.Round(randVal, 2);
        }
        else
        {
            finalValue = UnityEngine.Random.Range((int)selectedMeta.min, (int)selectedMeta.max + 1);
        }

        SaveDataManager.instance.AddCoin(-rollCoinCost, false);
        SaveDataManager.instance.SetTowerUserJewelData(currentJewelIndex, targetGrade, selectedMeta.type, finalValue);
        SaveDataManager.instance.AddTowerJewelRollCount();
        UpdateStatUI();
    }

    public void OnLockToggle()
    {
        HLLogger.Log($"@@@ lockToggle Set : {lockToggle.isOn}");
        SaveDataManager.instance.otherPlayerData.isJewelLock[currentJewelIndex] = lockToggle.isOn;
        SaveDataManager.instance.SaveOtherPlayerData();

        UpdateRollCoinCost();
    }


    private string GetJewelValueString(TowerJewelType type, float value)
    {
        if (type == TowerJewelType.None)
            return "";

        return type switch
        {
            TowerJewelType.CriDmg => $"+{value * 100}%",
            TowerJewelType.CriRate or TowerJewelType.Avoid or TowerJewelType.DefBreak or TowerJewelType.DmgReduce or TowerJewelType.AtkMul => $"{value}%",
            _ => $"+{value}",
        };
    }


}



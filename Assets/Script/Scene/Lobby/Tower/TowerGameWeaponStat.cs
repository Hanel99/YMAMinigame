// using System;
using System.Text;
using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class TowerGameWeaponStat : MonoBehaviour
{
    public TowerGameWeaponImage weaponImage;

    public Text levelText;
    public Text atkValueText;
    public Text criDmgValueText;
    public Text requireCoinText;

    public GameObject rateGroup;
    public Text rankUpRateText;
    public Text rankStayRateText;
    public Text rankDownRateText;
    public Image rankUpGauge;
    public Image rankStayGauge;
    public Image rankDownGauge;

    public Button enchantButton;
    public Text enchantText;

    public GameObject emptyText;
    public GameObject weaponStatDetailGroup;
    public Toggle skipToggle;

    public UIParticle rankUpParticle;
    public UIParticle rankDownParticle;



    //private
    private int weaponLevel;
    private TowerWeaponLevelMetaData weaponMetaData;
    private TowerGameUserWeaponData userWeaponData;
    private LongPressButton longPressButton;

    private StringBuilder sb = new StringBuilder();
    private bool isLongPress = false;



    // Consts
    private const string TEXT_MAX = "MAX";
    private const string TEXT_BUY_WEAPON = "무기 구매";
    private const string TEXT_ENCHANT = "강화";


    public void UpdateUIData(int weaponLevel)
    {
        this.weaponLevel = weaponLevel;
        if (longPressButton == null)
        {
            longPressButton = enchantButton.GetComponent<LongPressButton>();
        }

        weaponMetaData = GameResourceManager.instance.GetTowerWeaponLevelMetaData(weaponLevel);
        userWeaponData = SaveDataManager.instance.playerData.towerGameUserWeaponData;

        emptyText.SetActive(weaponLevel == 0);
        weaponImage.UpdateWeaponImage();

        weaponStatDetailGroup.SetActive(weaponLevel != 0);
        skipToggle.isOn = SaveDataManager.instance.otherPlayerData.isTowerSkip;
        UpdateDetailText();

        longPressButton.SetLongPressAction(OnClickEnchant);

        // 1렙일 때 0.3초, 90렙일 때 0.05초로 선형 보간
        float t = Mathf.Clamp01((weaponLevel - 1f) / 89f);
        float interval = Mathf.Lerp(0.3f, 0.05f, t);
        longPressButton.SetRepeatInterval(interval);
    }

    public void HideParticle()
    {
        rankUpParticle.gameObject.SetActive(false);
        rankDownParticle.gameObject.SetActive(false);
    }

    private void UpdateDetailText()
    {
        levelText.text = $"Lv.{weaponMetaData.level}";
        atkValueText.text = $"{weaponMetaData.atk}";
        criDmgValueText.text = $"x{((1 + weaponMetaData.criDmg) * 100).ToString("F1")}%";

        requireCoinText.text = weaponMetaData.requireCoin <= 0 ? TEXT_MAX : weaponMetaData.requireCoin.ToString("N0");
        enchantButton.interactable = weaponMetaData.requireCoin > 0 && SaveDataManager.instance.playerData.coin >= weaponMetaData.requireCoin;
        rateGroup.SetActive(weaponLevel > 0 && weaponLevel < 1000);

        if (weaponLevel == 0)
        {
            enchantText.text = TEXT_BUY_WEAPON;
            return;
        }
        else if (weaponMetaData.requireCoin <= 0)
        {
            enchantText.text = TEXT_ENCHANT;
            return;
        }

        int upValue = weaponMetaData.up + userWeaponData.failCount * 10;
        int stayValue = weaponMetaData.stay + (userWeaponData.isDown || SaveDataManager.instance.playerData.finalQuizPlayData.playEndRoll ? weaponMetaData.down : 0);
        int downValue = (userWeaponData.isDown || SaveDataManager.instance.playerData.finalQuizPlayData.playEndRoll) ? 0 : weaponMetaData.down;
        int total = upValue + stayValue + downValue;

        float upRate = (float)upValue / total;
        float stayRate = (float)stayValue / total;
        float downRate = (float)downValue / total;

        enchantText.text = TEXT_ENCHANT;
        rankUpRateText.text = $"성공\n{(upRate * 100).ToString("F2")}%";
        rankStayRateText.text = $"유지\n{(stayRate * 100).ToString("F2")}%";
        rankDownRateText.text = $"하락\n{(downRate * 100).ToString("F2")}%";


        // 게이지 이미지 연출 (총 너비 500)
        float totalGaugeWidth = 496f;
        float upWidth = upRate * totalGaugeWidth;
        float stayWidth = stayRate * totalGaugeWidth;
        float downWidth = downRate * totalGaugeWidth;

        rankUpGauge.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, upWidth);
        rankDownGauge.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, downWidth);

        Vector2 stayPos = rankStayGauge.rectTransform.anchoredPosition;
        stayPos.x = (upWidth - downWidth) / 2;
        rankStayGauge.rectTransform.anchoredPosition = stayPos;
        rankStayGauge.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, stayWidth);

    }

    public void OnClickEnchant(bool isLongPress = false)
    {
        if (weaponMetaData.requireCoin <= 0 || SaveDataManager.instance.playerData.coin < weaponMetaData.requireCoin)
            return;

        SaveDataManager.instance.AddCoin(-weaponMetaData.requireCoin, false);
        this.isLongPress = isLongPress;
        WeaponEnchantProcess();
    }

    public void OnSkipToggle()
    {
        HLLogger.Log($"@@@ skipToggle Set : {skipToggle.isOn}");
        SaveDataManager.instance.otherPlayerData.isTowerSkip = skipToggle.isOn;
        SaveDataManager.instance.SaveOtherPlayerData();
    }

    private void WeaponEnchantProcess()
    {
        sb.Clear();

        int failCount = userWeaponData.failCount;
        int stayValue = weaponMetaData.stay;
        int downValue = weaponMetaData.down;
        int total = weaponMetaData.up + stayValue + downValue + failCount * 10;

        int rand = Random.Range(0, total);
        sb.AppendLine($"stay: {stayValue}, isDown? {userWeaponData.isDown}, down: {downValue}, up: {weaponMetaData.up} + failCount * 10: {failCount * 10} / total: {total}");

        if (rand < stayValue)
        {
            // 등급 유지
            sb.AppendLine($"rand: {rand} -> Stay");
            StayProcess();
        }
        else if (rand < stayValue + downValue)
        {
            if ((userWeaponData.isDown || SaveDataManager.instance.playerData.finalQuizPlayData.playEndRoll))
            {
                // 이미 하락 상태면 등급 유지
                sb.AppendLine($"rand: {rand} -> Stay (isDown)");
                StayProcess();
            }
            else
            {
                // 등급 하락
                sb.AppendLine($"rand: {rand} -> Down");
                DownProcess();
            }
        }
        else
        {
            // 등급 상승
            sb.AppendLine($"rand: {rand} -> Success");
            SuccessProcess();
        }
        // UpdateUIData(weaponLevel);
        HLLogger.Log(sb.ToString());
    }

    private void SuccessProcess()
    {
        weaponLevel++;
        SaveDataManager.instance.SetTowerUserWeaponFailCount(0);
        SaveDataManager.instance.SetTowerUserWeaponIsDown(false);
        SaveDataManager.instance.SetTowerUserWeaponLevel(weaponLevel);
        ShowResult(TowerGameResultType.Up, $"Lv.{weaponLevel - 1}", $"-> Lv.{weaponLevel}", () => UpdateUIData(weaponLevel));
    }
    private void StayProcess()
    {
        SaveDataManager.instance.AddTowerUserWeaponFailCount();
        ShowResult(TowerGameResultType.Stay, "", "", () => UpdateUIData(weaponLevel));
    }
    private void DownProcess()
    {
        weaponLevel--;
        SaveDataManager.instance.SetTowerUserWeaponFailCount(0);
        SaveDataManager.instance.SetTowerUserWeaponIsDown(true);
        SaveDataManager.instance.SetTowerUserWeaponLevel(weaponLevel);
        ShowResult(TowerGameResultType.Down, $"Lv.{weaponLevel + 1}", $"-> Lv.{weaponLevel}", () => UpdateUIData(weaponLevel));
    }

    private void ShowResult(TowerGameResultType type, string before, string after, System.Action callback)
    {
        QuestManager.instance.AddTowerEnchantData(type);

        if (skipToggle.isOn)
        {
            if (type == TowerGameResultType.Up)
            {
                if (rankUpParticle.IsActive() == false)
                    rankUpParticle.gameObject.SetActive(true);
                rankUpParticle.Play();
                SoundManager.instance.PlaySFX(SFXType.WeaponSuccess);
            }
            else if (type == TowerGameResultType.Down)
            {
                if (rankDownParticle.IsActive() == false)
                    rankDownParticle.gameObject.SetActive(true);
                rankDownParticle.Play();
                SoundManager.instance.PlaySFX(SFXType.WeaponFail);
            }
            else
            {
                // 유지
                if (isLongPress == false)
                    SoundManager.instance.PlaySFX(SFXType.WeaponStay);
            }

            UpdateUIData(weaponLevel);
            TowerGameWeaponEnchantPopup.instance.UpdateUI();
        }
        else
        {
            TowerGameWeaponEnchantPopup.instance.ShowEnchantResult(type, before, after, callback);
        }
    }



}

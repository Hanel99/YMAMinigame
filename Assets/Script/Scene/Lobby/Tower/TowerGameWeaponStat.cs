// using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TowerGameWeaponStat : MonoBehaviour
{
    public Text levelText;
    public Text atkValueText;
    public Text criDmgValueText;
    public Text requireCoinText;

    public GameObject RankUpText;
    public Text RankUpRateText;
    public Text RankStayRateText;
    public Text RankDownRateText;

    public Button enchantButton;
    public Text enchantText;

    public GameObject emptyText;
    public GameObject weaponStatDetailGroup;
    public Toggle skipToggle;

    //private
    private int weaponLevel;
    private TowerWeaponLevelMetaData weaponMetaData;
    private TowerGameUserWeaponData userWeaponData;

    private StringBuilder sb = new StringBuilder();



    public void UpdateUIData(int weaponLevel)
    {
        this.weaponLevel = weaponLevel;

        weaponMetaData = GameResourceManager.instance.GetTowerWeaponLevelMetaData(weaponLevel);
        userWeaponData = SaveDataManager.instance.playerData.towerGameUserWeaponData;

        emptyText.SetActive(weaponLevel == 0);
        weaponStatDetailGroup.SetActive(weaponLevel != 0);
        skipToggle.isOn = SaveDataManager.instance.otherPlayerData.isTowerSkip;
        UpdateDetailText();

        enchantButton.GetComponent<LongPressButton>().SetLongPressAction(OnClickEnchant);
    }

    private void UpdateDetailText()
    {
        levelText.text = $"Lv.{weaponMetaData.level}";
        atkValueText.text = $"{weaponMetaData.atk}";
        criDmgValueText.text = $"x{((1 + weaponMetaData.criDmg) * 100).ToString("F1")}%";

        requireCoinText.text = weaponMetaData.requireCoin <= 0 ? "MAX" : weaponMetaData.requireCoin.ToString();
        enchantButton.interactable = weaponMetaData.requireCoin > 0 && SaveDataManager.instance.playerData.coin >= weaponMetaData.requireCoin;

        if (weaponLevel == 0)
        {
            RankUpText.SetActive(false);
            enchantText.text = "무기 구매";
            return;
        }
        else if (weaponMetaData.requireCoin <= 0)
        {
            RankUpText.SetActive(false);
            enchantText.text = "강화";
            return;
        }

        int upValue = weaponMetaData.up + userWeaponData.failCount * 10;
        int stayValue = weaponMetaData.stay + (userWeaponData.isDown ? weaponMetaData.down : 0);
        int downValue = userWeaponData.isDown ? 0 : weaponMetaData.down;
        int total = upValue + stayValue + downValue;

        float upRate = (float)upValue / total;
        float stayRate = (float)stayValue / total;
        float downRate = (float)downValue / total;

        RankUpText.SetActive(true);
        enchantText.text = "강화";
        RankUpRateText.text = $"성공 : {(upRate * 100).ToString("F2")}%";
        RankStayRateText.text = $"유지 : {(stayRate * 100).ToString("F2")}%";
        RankDownRateText.text = $"하락 : {(downRate * 100).ToString("F2")}%";
    }

    public void OnClickEnchant()
    {
        if (weaponMetaData.requireCoin <= 0 || SaveDataManager.instance.playerData.coin < weaponMetaData.requireCoin)
            return;

        SaveDataManager.instance.AddCoin(-weaponMetaData.requireCoin, false);
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
            if (userWeaponData.isDown)
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
        ShowResult(TowerGameResultType.up, $"Lv.{weaponLevel - 1}", $"-> Lv.{weaponLevel}", () => UpdateUIData(weaponLevel));
    }
    private void StayProcess()
    {
        SaveDataManager.instance.AddTowerUserWeaponFailCount();
        ShowResult(TowerGameResultType.stay, "", "", () => UpdateUIData(weaponLevel));
    }
    private void DownProcess()
    {
        weaponLevel--;
        SaveDataManager.instance.SetTowerUserWeaponFailCount(0);
        SaveDataManager.instance.SetTowerUserWeaponIsDown(true);
        SaveDataManager.instance.SetTowerUserWeaponLevel(weaponLevel);
        ShowResult(TowerGameResultType.down, $"Lv.{weaponLevel + 1}", $"-> Lv.{weaponLevel}", () => UpdateUIData(weaponLevel));
    }

    private void ShowResult(TowerGameResultType type, string before, string after, System.Action callback)
    {
        if (skipToggle.isOn)
        {
            UpdateUIData(weaponLevel);
            TowerGameWeaponEnchantPopup.instance.UpdateUI();
        }
        else
        {
            TowerGameWeaponEnchantPopup.instance.ShowEnchantResult(type, before, after, callback);
        }
    }
}

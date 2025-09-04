using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class TowerGameWeaponStat : MonoBehaviour
{
    public Text levelText;
    public Text atkValueText;
    public Text criDmgValueText;
    public Text requireCoinText;

    public Text RankUpRateText;
    public Text RankStayRateText;
    public Text RankDownRateText;

    public Button enchantButton;

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

        levelText.text = $"Lv.{weaponMetaData.level}";
        atkValueText.text = $"{LocalizeManager.instance.GetString("Tower.StatType.atk")}: {weaponMetaData.atk}";
        criDmgValueText.text = $"{LocalizeManager.instance.GetString("Tower.StatType.criDmg")}: {(weaponMetaData.criDmg * 100).ToString("F1")}%";

        UpdateRateText();

        requireCoinText.text = weaponLevel >= 100 ? "MAX" : weaponMetaData.requireCoin.ToString();
        enchantButton.interactable = weaponLevel < 100 && SaveDataManager.instance.playerData.coin >= weaponMetaData.requireCoin;
    }

    private void UpdateRateText()
    {
        int upValue = weaponMetaData.up + userWeaponData.failCount * 10;
        int stayValue = weaponMetaData.stay;
        int downValue = userWeaponData.isDown ? 0 : weaponMetaData.down;
        int total = upValue + stayValue + downValue;

        float upRate = (float)upValue / total;
        float stayRate = (float)stayValue / total;
        float downRate = (float)downValue / total;

        RankUpRateText.text = $"{(upRate * 100).ToString("F1")}%";
        RankStayRateText.text = $"{(stayRate * 100).ToString("F1")}%";
        RankDownRateText.text = $"{(downRate * 100).ToString("F1")}%";
    }

    public void OnClickEnchant()
    {
        WeaponEnchantProcess();
    }

    private void WeaponEnchantProcess()
    {
        sb.Clear();

        int failCount = userWeaponData.failCount;
        int downValue = userWeaponData.isDown ? 0 : weaponMetaData.down;
        int total = weaponMetaData.up + weaponMetaData.stay + downValue + failCount * 10;

        int rand = Random.Range(0, total);
        sb.AppendLine($"stay: {weaponMetaData.stay}, isDown? {userWeaponData.isDown}, down: {downValue}, up: {weaponMetaData.up} + failCount * 10: {failCount * 10} / total: {total}");

        if (rand < weaponMetaData.stay)
        {
            // 등급 유지
            sb.AppendLine($"rand: {rand} -> Stay");
            StayProcess();
        }
        else if (rand < weaponMetaData.stay + downValue)
        {
            // 등급 하락
            sb.AppendLine($"rand: {rand} -> Down");
            DownProcess();
        }
        else
        {
            // 등급 상승
            sb.AppendLine($"rand: {rand} -> Success");
            SuccessProcess();
        }
        HLLogger.Log(sb.ToString());
    }

    private void SuccessProcess()
    {
        weaponLevel++;
        SaveDataManager.instance.SetTowerUserWeaponFailCount(0);
        SaveDataManager.instance.SetTowerUserWeaponIsDown(false);
        SaveDataManager.instance.SetTowerUserWeaponLevel(weaponLevel);
    }
    private void StayProcess()
    {
        SaveDataManager.instance.AddTowerUserWeaponFailCount();
    }
    private void DownProcess()
    {
        weaponLevel--;
        SaveDataManager.instance.SetTowerUserWeaponFailCount(0);
        SaveDataManager.instance.SetTowerUserWeaponIsDown(true);
        SaveDataManager.instance.SetTowerUserWeaponLevel(weaponLevel);
    }

    private T GetValue<T>(TowerUserStatType type)
    {
        return GameResourceManager.instance.GetTowerUserLevelStatValue<T>(type, weaponLevel);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerGameDetailStatPopup : PopupBase
{
    public static TowerGameDetailStatPopup instance { get; private set; }

    [Header("Title")]
    public Text playerNameText;
    public Text bossNameText;

    [Header("Stat Items")]
    public List<TowerGameDetailStatItem> statItemList = new();


    [Header("Boss Reward")]
    public Text rewardCoinText;
    public Text rewardExpText;

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

    private void UpdateUI()
    {
        var playerData = SaveDataManager.instance.playerData;
        var playerStatData = playerData.towerGameUserStatLevelData;
        var playerWeaponData = playerData.towerGameUserWeaponData;
        var floor = playerData.towerFloor;

        var weaponMetaData = GameResourceManager.instance.GetTowerWeaponLevelMetaData(playerWeaponData.weaponLevel);
        var bossMetaData = GameResourceManager.instance.GetTowerBossLevelMetaData(floor);

        if (playerNameText != null) playerNameText.text = playerData.name;

        int bossNumber = TowerGamePopup.instance.GetBossNumber(floor);
        if (bossNameText != null) bossNameText.text = LocalizeManager.instance.GetString($"Tower.Boss.Name.{bossNumber:D2}");

        // 1. 공격력
        statItemList[0]?.SetData(
            LocalizeManager.instance.GetString("Tower.StatType.Atk"),
            $"{playerData.level + GetValue<int>(TowerUserStatType.Atk, playerStatData.atkLevel)}",
            $"+{weaponMetaData.atk}",
            $"+{(int)SaveDataManager.instance.GetJewelValue(TowerJewelType.Atk)}",
            bossMetaData?.atk.ToString()
        );

        // 2. 방어력
        statItemList[1]?.SetData(
            LocalizeManager.instance.GetString("Tower.StatType.Def"),
            $"{playerData.level + GetValue<int>(TowerUserStatType.Def, playerStatData.defLevel)}",
            null,
            $"+{(int)SaveDataManager.instance.GetJewelValue(TowerJewelType.Def)}",
            bossMetaData?.def.ToString()
        );

        // 3. HP
        statItemList[2]?.SetData(
            LocalizeManager.instance.GetString("Tower.StatType.HP"),
            $"{playerData.level + GetValue<int>(TowerUserStatType.HP, playerStatData.hpLevel)}",
            null,
            null,
            bossMetaData?.hp.ToString()
        );

        // 4. 치명타 확률
        statItemList[3]?.SetData(
            LocalizeManager.instance.GetString("Tower.StatType.CriRate"),
            $"{(GetValue<float>(TowerUserStatType.CriRate, playerStatData.criRateLevel) * 100):F2}%",
            null,
            $"+{SaveDataManager.instance.GetJewelValue(TowerJewelType.CriRate):F2}%",
            $"{(bossMetaData?.criRate * 100):F2}%"
        );

        // 5. 치명타 데미지
        statItemList[4]?.SetData(
            LocalizeManager.instance.GetString("Tower.StatType.CriDmg"),
            $"x{((1 + GetValue<float>(TowerUserStatType.CriDmg, playerStatData.criDmgLevel)) * 100):F2}%",
            $"+{(weaponMetaData.criDmg * 100):F2}%",
            $"+{SaveDataManager.instance.GetJewelValue(TowerJewelType.CriDmg):F2}%",
            $"x{((1 + (bossMetaData?.criDmg ?? 0)) * 100):F2}%"
        );

        // 6. 회피율
        float baseAvoid = Mathf.Min(0.2f, playerData.level * 0.01f);
        statItemList[5]?.SetData(
            LocalizeManager.instance.GetString("Tower.StatType.Avoid"),
            $"{(baseAvoid * 100):F2}%",
            null,
            $"+{SaveDataManager.instance.GetJewelValue(TowerJewelType.Avoid):F2}%",
            $"{bossMetaData?.avoid * 100:F0}%"
        );

        // 7. 방어 무시
        statItemList[6]?.SetData(
            LocalizeManager.instance.GetString("Tower.StatType.DefBreak"),
            null,
            null,
            $"+{SaveDataManager.instance.GetJewelValue(TowerJewelType.DefBreak):F2}%",
            ""
        );

        // 8. 데미지 감소
        statItemList[7]?.SetData(
            LocalizeManager.instance.GetString("Tower.StatType.DmgReduce"),
            null,
            null,
            $"+{SaveDataManager.instance.GetJewelValue(TowerJewelType.DmgReduce):F2}%",
            ""
        );

        // 9. 공격력 배율
        statItemList[8]?.SetData(
            LocalizeManager.instance.GetString("Tower.StatType.AtkMul"),
            null,
            null,
            $"+{(SaveDataManager.instance.GetJewelValue(TowerJewelType.AtkMul) * 0.01f):F2}%",
            ""
        );

        // 보상 정보
        int earnCoinAmount = bossMetaData.rewardCoin;
        int earnExpAmount = bossMetaData.rewardExp;

        if (SaveDataManager.instance.playerData.finalQuizPlayData.playEndRoll)
        {
            earnCoinAmount = Math.Min(StaticGameData.MAX_COIN_VALUE, bossMetaData.rewardCoin * 50);
            earnExpAmount = bossMetaData.rewardExp * 10;
        }

        if (rewardCoinText != null) rewardCoinText.text = earnCoinAmount.ToString("N0");
        if (rewardExpText != null) rewardExpText.text = earnExpAmount.ToString("N0");
    }

    private T GetValue<T>(TowerUserStatType type, int level)
    {
        return GameResourceManager.instance.GetTowerUserLevelStatValue<T>(type, level);
    }
}

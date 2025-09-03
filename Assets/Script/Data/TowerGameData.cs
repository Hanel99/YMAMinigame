using System.Collections;
using System.Collections.Generic;
using System;


[Serializable]
public class TowerGameUserStatLevelData
{
    // 스탯 수치가 아닌 스탯의 레벨을 저장.
    public int atkLevel { get; set; }
    public int defLevel { get; set; }
    public int hpLevel { get; set; }
    public int criRateLevel { get; set; }
    public int criDmgLevel { get; set; }

    public TowerGameUserStatLevelData()
    {
        atkLevel = 1;
        defLevel = 1;
        criRateLevel = 1;
        criDmgLevel = 1;
        hpLevel = 1;
    }

    public int GetLevel(TowerUserStatType type)
    {
        return type switch
        {
            TowerUserStatType.atk => atkLevel,
            TowerUserStatType.def => defLevel,
            TowerUserStatType.hp => hpLevel,
            TowerUserStatType.criRate => criRateLevel,
            TowerUserStatType.criDmg => criDmgLevel,
            _ => 1,
        };
    }
}

[Serializable]
public class TowerGameUserWeaponData
{
    // 무기 스탯이 아닌 무기 레벨을 저장
    public int weaponLevel { get; set; }
    public int failCount { get; set; }
    public bool isDown { get; set; }


    public TowerGameUserWeaponData()
    {
        weaponLevel = 0;
        failCount = 0;
        isDown = false;
    }
}
using System.Collections;
using System.Collections.Generic;
using System;


[Serializable]
public class TowerGameUserStatLevelData
{
    // 스탯 수치가 아닌 스탯의 레벨을 저장.
    public int atkLevel;
    public int defLevel;
    public int hpLevel;
    public int criRateLevel;
    public int criDmgLevel;

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
    public int weaponLevel;
    public int failCount;
    public bool isDown;


    public TowerGameUserWeaponData()
    {
        weaponLevel = 0;
        failCount = 0;
        isDown = false;
    }
}
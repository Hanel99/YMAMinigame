using System.Collections;
using System.Collections.Generic;
using System;


[Serializable]
public class TowerGameUserStatData
{
    public int atk { get; set; }
    public int defense { get; set; }
    public int hp { get; set; }
    public int maxHp { get; set; }
    public float criRate { get; set; }
    public float criDmg { get; set; }

    public TowerGameUserStatData()
    {
        atk = 1;
        defense = 1;
        criRate = 0f;
        criDmg = 1.0f;
        hp = 100;
        maxHp = 100;
    }
}

[Serializable]
public class TowerGameUserWeaponData
{
    public int weaponLevel { get; set; }
    public int weaponAtk { get; set; }
    public float weaponCriDmg { get; set; }


    public TowerGameUserWeaponData()
    {
        weaponLevel = 0;
        weaponAtk = 0;
        weaponCriDmg = 0;
    }
}
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerGameWeaponImage : MonoBehaviour
{
    public GameObject weaponImage;
    public GameObject weaponImage2;
    public List<GameObject> jewels;

    private List<TowerJewelUserData> TowerJewelUserDataList => SaveDataManager.instance.playerData.towerJewelUserDataList;
    private int WeaponLevel => SaveDataManager.instance.playerData.towerGameUserWeaponData.weaponLevel;
    private readonly int UNLOCK_JEWEL_FLOOR = 200;
    private readonly int MAX_JEWEL_COUNT = 4;

    public void UpdateWeaponImage()
    {
        weaponImage.SetActive(WeaponLevel < UNLOCK_JEWEL_FLOOR);
        weaponImage2.SetActive(WeaponLevel >= UNLOCK_JEWEL_FLOOR);

        if (WeaponLevel >= UNLOCK_JEWEL_FLOOR)
        {
            for (int i = 0; i < MAX_JEWEL_COUNT; i++)
            {
                jewels[i].SetActive(TowerJewelUserDataList.Count > i);
            }
        }
    }
}

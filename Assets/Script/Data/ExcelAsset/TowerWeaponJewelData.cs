using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset(AssetPath = "AddressableResource/Data/ScriptableData")]
public class TowerWeaponJewelData : ScriptableObject
{
    public List<TowerWeaponJewelMetaData> Data;
}
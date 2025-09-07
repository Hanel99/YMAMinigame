using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExcelAsset(AssetPath = "AddressableResource/Data/ScriptableData")]
public class TowerBossLevelData : ScriptableObject
{
	public List<TowerBossLevelMetaData> Data; // Replace 'EntityType' to an actual type that is serializable.
}

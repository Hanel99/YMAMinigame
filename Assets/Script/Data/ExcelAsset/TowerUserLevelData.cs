using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset(AssetPath = "AddressableResource/Data/ScriptableData")]
public class TowerUserLevelData : ScriptableObject
{
	public List<TowerUserLevelMetaData> Data; // Replace 'EntityType' to an actual type that is serializable.
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset(AssetPath = "Resources/Data/ScriptableData")]
public class LevelData : ScriptableObject
{
    public List<LevelMetaData> Data; // Replace 'EntityType' to an actual type that is serializable.
}
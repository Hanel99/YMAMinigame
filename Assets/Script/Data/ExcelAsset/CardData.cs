using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset(AssetPath = "AddressableResource/Data/ScriptableData")]
public class CardData : ScriptableObject
{
    public List<CardMetaData> Data; // Replace 'EntityType' to an actual type that is serializable.
}

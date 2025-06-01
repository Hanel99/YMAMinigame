using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset(AssetPath = "AddressableResource/Data/ScriptableData")]
public class StringData : ScriptableObject
{
    public List<StringMetaData> Data; // Replace 'EntityType' to an actual type that is serializable.
}
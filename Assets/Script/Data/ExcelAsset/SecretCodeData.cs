using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExcelAsset(AssetPath = "AddressableResource/Data/ScriptableData")]
public class SecretCodeData : ScriptableObject
{
    public List<SecretCodeMetaData> Data;
}
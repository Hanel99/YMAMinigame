using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceData", menuName = "Scriptable Object/ResourceData", order = int.MaxValue)]
public class ResourceScriptableData : ScriptableObject
{
    [Header("- ExcelData")]
    public CardData cardData;
    public StringData stringData;
    public LevelData levelData;

    [Header("- Sprite")]
    public List<Sprite> cardImages = new();
    public List<Sprite> gameImages = new();
    public List<Sprite> masterImages = new();


    [Header("- Prefabs")]
    public GameObject cardPrefab;
    public List<GameObject> popups = new();




#if UNITY_EDITOR

    [Button(" Set Scriptable ExcelData ")]
    public void SetScriptableExcelData()
    {
        cardData = Resources.Load<CardData>("Data/ScriptableData/CardData");
        stringData = Resources.Load<StringData>("Data/ScriptableData/StringData");
        levelData = Resources.Load<LevelData>("Data/ScriptableData/LevelData");
    }


    [Button(" Set Sprites ")]
    public void SetSprites()
    {
        cardImages.Clear();
        gameImages.Clear();
        masterImages.Clear();



        DirectoryInfo di = new DirectoryInfo("Assets/Resources/Sprite/CardImages");

        foreach (FileInfo file in di.GetFiles())
        {
            if (file.Name.Contains("meta") || file.Name.Contains("mask")) continue;

            var fileName = file.Name.Split('.')[0];
            HLLogger.Log($"path ; {"Sprite/CardImages" + fileName}");

            var sprite = Resources.Load<Sprite>("Sprite/CardImages/" + fileName);
            cardImages.Add(sprite);
        }


        di = new DirectoryInfo("Assets/Resources/Sprite/GameImages");

        foreach (FileInfo file in di.GetFiles())
        {
            if (file.Name.Contains("meta")) continue;

            var fileName = file.Name.Split('.')[0];
            HLLogger.Log($"path ; {"Sprite/GameImages" + fileName}");

            var sprite = Resources.Load<Sprite>("Sprite/GameImages/" + fileName);
            gameImages.Add(sprite);
        }


        di = new DirectoryInfo("Assets/Resources/Sprite/MasterIcons");

        foreach (FileInfo file in di.GetFiles())
        {
            if (file.Name.Contains("meta")) continue;

            var fileName = file.Name.Split('.')[0];
            HLLogger.Log($"path ; {"Sprite/MasterIcons" + fileName}");

            var sprite = Resources.Load<Sprite>("Sprite/MasterIcons/" + fileName);
            masterImages.Add(sprite);
        }
    }


    [Button(" Set Prefabs ")]
    public void SetPrefabs()
    {
        cardPrefab = null;
        popups.Clear();


        DirectoryInfo di = new DirectoryInfo("Assets/Prefab/");
        FileInfo[] fileInfos = null;
        var prefabDirectories = di.GetDirectories();

        foreach (var dir in prefabDirectories)
        {
            fileInfos = dir.GetFiles("*.prefab");

            foreach (FileInfo file in fileInfos)
            {
                if (file.Name.Contains("meta")) continue;
                string filePath = $"{di}{dir.Name}/{file.Name}";
                HLLogger.Log($"path ; {filePath}");


                if (file.Name.Equals("Card.prefab"))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
                    cardPrefab = prefab;
                }
                if (file.Name.Contains("Popup.prefab"))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(filePath);
                    popups.Add(prefab);
                }
            }
        }
    }


    [Button(" Data Save ")]
    public void AssetDataRefresh()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
#endif
}

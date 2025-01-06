using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class LocalizeManager : MonoBehaviour
{
    public static LocalizeManager instance { get; private set; }
    private StringData stringData;


    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }


    public string GetString(string key)
    {
        if (stringData == null)
            stringData = ResourceManager.instance.resourceScriptableData.stringData;

        var findStr = stringData.Data.Find(x => x.key == key);

        if (findStr == null)
        {
            HLLogger.LogWarning($"@@@ string is null. empty key is : {key}");
            return $"Missing String {key}";
        }

        if (SaveDataManager.instance == null || SaveDataManager.instance.playerData == null)
            return findStr.ko;

        switch (SaveDataManager.instance.playerData.languageType)
        {
            case LanguageType.ko:
                return findStr.ko;
            // case LanguageType.jp:
            //     return findStr.jp;
            // case LanguageType.en:
            //     return findStr.en;
            default:
                return findStr.ko;
        }
    }
}

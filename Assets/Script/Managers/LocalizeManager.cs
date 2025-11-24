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
        if (stringData == null && GameResourceManager.instance != null && GameResourceManager.instance.stringData != null)
            stringData = GameResourceManager.instance.stringData;

        if (stringData == null)
        {
            HLLogger.LogError("StringData is null. Please load GameResourceManager first.");
            return $"Missing String {key}";
        }

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

    public string GetString(StringMetaData stringMetaData)
    {
        if (stringMetaData == null)
        {
            HLLogger.LogWarning($"@@@ stringMetaData is null.");
            return $"Missing String";
        }

        if (SaveDataManager.instance == null || SaveDataManager.instance.playerData == null)
            return stringMetaData.ko;

        switch (SaveDataManager.instance.playerData.languageType)
        {
            case LanguageType.ko:
                return stringMetaData.ko;
            // case LanguageType.jp:
            //     return stringMetaData.jp;
            // case LanguageType.en:
            //     return stringMetaData.en;
            default:
                return stringMetaData.ko;
        }
    }

    public (int, string) GetRandomAIWordString()
    {
        if (stringData == null && GameResourceManager.instance != null && GameResourceManager.instance.stringData != null)
            stringData = GameResourceManager.instance.stringData;

        if (stringData == null)
        {
            HLLogger.LogError("StringData is null. Please load GameResourceManager first.");
            return (-1, $"Missing String");
        }

        var aiWords = stringData.Data.FindAll(x => x.key.StartsWith("AIWord.Key."));
        if (aiWords.Count == 0)
        {
            return (-1, $"Missing String");
        }

        int index = Random.Range(0, aiWords.Count);
        var randomWord = aiWords[index];
        return (index, GetString(randomWord.key));
    }

    public List<string> GetAllAIWordString()
    {
        List<string> wordList = new List<string>();

        if (stringData == null && GameResourceManager.instance != null && GameResourceManager.instance.stringData != null)
            stringData = GameResourceManager.instance.stringData;

        if (stringData == null)
        {
            HLLogger.LogError("StringData is null. Please load GameResourceManager first.");
            return new List<string> { "Missing String" };
        }

        var aiWords = stringData.Data.FindAll(x => x.key.StartsWith("AIWord.Key."));

        for (int i = 0; i < aiWords.Count; ++i)
        {
            wordList.Add(GetString(aiWords[i].key));
        }

        return wordList;
    }
}

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

        // 보유하지 않은 단어 인덱스 목록 생성
        var ownedIndices = new HashSet<int>(SaveDataManager.instance.playerData.ownWordList);
        var notOwnedIndexes = new List<int>();
        for (int i = 0; i < aiWords.Count; ++i)
        {
            if (!ownedIndices.Contains(i))
            {
                notOwnedIndexes.Add(i);
            }
        }

        // 70% 확률로 보유하지 않은 단어 선택
        int index;
        if (Random.value < 0.7f && notOwnedIndexes.Count > 0)
        {
            index = notOwnedIndexes[Random.Range(0, notOwnedIndexes.Count)];
        }
        else
        {
            index = Random.Range(0, aiWords.Count);
        }

        return (index, GetString(aiWords[index].key));
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

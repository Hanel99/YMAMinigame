using System.Collections.Generic;
using System.IO;
using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class GeminiApiManager : MonoBehaviour
{
    public static GeminiApiManager instance { get; private set; }

    private string apiKey;

    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        DontDestroyOnLoad(this.gameObject);

        LoadApiKey();
    }

    private void LoadApiKey()
    {
        string path = Path.Combine(Application.dataPath, "Build/apikey.json");
        if (File.Exists(path))
        {
            apiKey = JsonUtility.FromJson<ApiKeyWrapper>(File.ReadAllText(path)).apiKey;
        }
        else
        {
            Debug.LogError("API Key file not found!");
        }
    }

    [System.Serializable]
    private class ApiKeyWrapper
    {
        public string apiKey;
    }

    /// <summary>
    /// 특정 키워드에 대한 10가지 힌트를 요청
    /// </summary>
    public async UniTask<List<string>> GetHintsForKeyword(string keyword)
    {
        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

        // 요청 데이터 간소화
        var requestData = new GeminiRequest
        {
            contents = new[]
            {
                new Content
                {
                    parts = new[]
                    {
                        new Part { text = $"키워드 '{keyword}'에 대해 맞추기 어려운 것부터 점점 쉬운 것까지 총 10개의 힌트를 순서대로 알려줘. 키워드 단어에만 국한된 힌트를 만들어줘.답변은 힌트 문구 단 10문장으로만 리스트 형식으로 출력해줘. 10문장 외에 다른 문구는 삭제해서 반환해." }
                    }
                }
            }
        };

        string json = JsonUtility.ToJson(requestData);
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + request.error);
                return null;
            }
            else
            {
                string responseJson = request.downloadHandler.text;
                HLLogger.Log(responseJson);


                var response = JsonUtility.FromJson<GeminiResponse>(responseJson);

                if (response.candidates != null && response.candidates.Length > 0)
                {
                    string fullText = response.candidates[0].content.parts[0].text;
                    List<string> hints = new List<string>(fullText.Split('\n'));
                    return hints;
                }
                else
                {
                    Debug.LogWarning("No candidates in response.");
                    return null;
                }
            }
        }
    }


    [Serializable]
    public class GeminiRequest
    {
        public Content[] contents;
    }


    [Serializable]
    public class GeminiResponse
    {
        public Candidate[] candidates;
        // public UsageMetadata usageMetadata;
        public string modelVersion;
        public string responseId;
    }

    [Serializable]
    public class Candidate
    {
        public Content content;
        public string finishReason;
        // public SafetyRating[] safetyRatings;
        public int index;
    }

    [Serializable]
    public class Content
    {
        public Part[] parts;
        public string role;
    }

    [Serializable]
    public class Part
    {
        public string text;
        // 필요시 확장:
        // public InlineData inline_data; // 이미지 등 바이너리 파트
        // public FileData fileData;
    }

    // [Serializable]
    // public class PromptFeedback
    // {
    //     public string blockReason;            // 차단 시 사유
    //     public SafetyRating[] safetyRatings;
    // }

    // [Serializable]
    // public class SafetyRating
    // {
    //     public string category;               // 예: HARM_CATEGORY_HARASSMENT
    //     public string probability;            // 예: LOW, MEDIUM...
    //     public bool blocked;                  // 해당 카테고리로 차단 여부
    // }

    // [Serializable]
    // public class UsageMetadata
    // {
    //     public int promptTokenCount;
    //     public int candidatesTokenCount;
    //     public int totalTokenCount;
    //     public int thoughtsTokenCount;
    // }
}

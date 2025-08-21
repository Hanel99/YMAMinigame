using UnityEngine;
using System;
using System.IO;

[CreateAssetMenu(fileName = "AppConfig", menuName = "Config/AppConfig")]
public class AppConfig : ScriptableObject
{
    [Header("API Keys")]
    [SerializeField] private string GeminiApiKey;
    [SerializeField] private string AesKey;
    [SerializeField] private string AesIV;

    // API Key 해독
    public string GetApiKey()
    {
        byte[] data = Convert.FromBase64String(GeminiApiKey);
        return System.Text.Encoding.UTF8.GetString(data);
    }

    // 외부 JSON 로딩 (예: balance.json)
    public T LoadJsonConfig<T>(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        Debug.LogError($"Config 파일 {fileName}을 찾을 수 없습니다.");
        return default;
    }
}

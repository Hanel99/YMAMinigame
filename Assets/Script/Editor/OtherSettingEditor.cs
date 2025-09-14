using UnityEngine;
using UnityEditor;
using System.IO;



#if UNITY_EDITOR
public class OtherSettingEditor : Editor
{
    [UnityEditor.MenuItem("Tools/Create Config File")]
    public static void CreateConfigFile()
    {
        string folderPath = "Assets/Resources/Config";
        string filePath = Path.Combine(folderPath, "AppConfig.json");

        // 폴더가 없으면 생성
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // 파일이 이미 존재하면 건드리지 않음
        if (File.Exists(filePath))
        {
            Debug.Log($"Config file already exists at: {filePath}");
        }
        else
        {
            var config = new GameConfig
            {
                geminiApiKey = "DEFAULT_API_KEY",
                playFabTitleId = "NONE",
                aesKey = "0123456789abcdef0123456789abcdef",
                aesIV = "0123456789abcdef"
            };

            string json = JsonUtility.ToJson(config, true);
            File.WriteAllText(filePath, json);

            Debug.Log($"Config file created at: {filePath}");
        }

        // 해당 경로 폴더 열기
        EditorUtility.RevealInFinder(folderPath);
    }
}
#endif
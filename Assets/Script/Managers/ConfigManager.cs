using System.IO;
using UnityEngine;

[System.Serializable]
public class GameConfig
{
    public string geminiApiKey;
    public string playFabTitleId;
    public string aesKey;
    public string aesIV;
    public string keystorePath;
    public string keystorePassword;
    public string keyAlias;
    public string keyPassword;
}

public class ConfigManager : MonoBehaviour
{
    public static ConfigManager instance { get; private set; }
    private GameConfig _config;
    public GameConfig Config => _config;

    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        DontDestroyOnLoad(this.gameObject);
        LoadConfig();
    }

    public void LoadConfig()
    {
        TextAsset configFile = Resources.Load<TextAsset>("Config/AppConfig");

        if (configFile != null)
        {
            _config = JsonUtility.FromJson<GameConfig>(configFile.text);
            Debug.Log("Config loaded successfully");
        }
        else
        {
            Debug.LogWarning("Config file not found! Using default values.");
            _config = GetDefaultConfig();
        }
    }

    // 개발자가 config 파일이 없을 때를 대비한 기본값
    private static GameConfig GetDefaultConfig()
    {
        return new GameConfig
        {
            geminiApiKey = "DEFAULT_API_KEY",
            playFabTitleId = "NONE",
            aesKey = "0123456789abcdef0123456789abcdef",
            aesIV = "0123456789abcdef",
            keystorePath = "keystore Path",
            keystorePassword = "your_keystore_password",
            keyAlias = "your_keyalias_name",
            keyPassword = "your_keyalias_password"
        };
    }

}

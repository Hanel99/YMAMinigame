using System.IO;
using Newtonsoft.Json;
using UnityEngine;

[System.Serializable]
public class SftpConfig
{
    public string host;
    public int port;
    public string username;
    public string password;
    public string remotePathRoot;

    public static SftpConfig Load(string path = "Assets/Build/SftpConfig.json")
    {
        if (!File.Exists(path))
        {
            Debug.LogError("❌ SFTP 설정 파일이 존재하지 않습니다: " + path);
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonConvert.DeserializeObject<SftpConfig>(json);
    }
}

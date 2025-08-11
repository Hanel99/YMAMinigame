using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Security.Cryptography;

static public class GameConfig
{
    private static string AesKey;
    private static string AesIV;

    private static string defaultAesKey = "DefaultAesKey16Bytes";
    private static string defaultAesIV = "DefaultAesIv16";

    static GameConfig()
    {
        LoadAESKeyAndIV();
    }

    static private void LoadAESKeyAndIV()
    {
        string filePath = Path.Combine(Application.dataPath, "Build/AESKeyConfig.json");

        if (File.Exists(filePath))
        {
            string jsonData = File.ReadAllText(filePath);
            AESKeyConfigData configData = JsonUtility.FromJson<AESKeyConfigData>(jsonData);

            if (configData != null)
            {
                AesKey = configData.AesKey;
                AesIV = configData.AesIV;
            }
            else
            {
                Debug.LogError("Failed to parse AESKeyConfig.json");
                // Handle the error appropriately, e.g., use default keys or disable encryption
                AesKey = defaultAesKey;
                AesIV = defaultAesIV;
            }
        }
        else
        {
            Debug.LogError("AESKeyConfig.json not found at " + filePath);
            // Handle the error appropriately
            AesKey = defaultAesKey;
            AesIV = defaultAesIV;
        }

        if (AesKey == defaultAesKey || AesIV == defaultAesIV)
        {
            Debug.LogWarning("Using default AES Key and IV. This is not secure!");
        }
    }
    static public void Set(string strName, string strValue)
    {
        string strEncodeName = EncodeSHA256(strName);
        string strEncodeValue = EncryptAES(strValue);
        PlayerPrefs.SetString(strEncodeName, strEncodeValue);
    }
    static public void Set(string strName, int iValue)
    {
        Set(strName, iValue.ToString());
    }

    static public void Set(string strName, float fValue)
    {
        Set(strName, fValue.ToString());
    }
    static public void Set(string strName, bool bValue)
    {
        Set(strName, bValue ? "True" : "False");
    }
    static public void Set(string strName, ulong ulValue)
    {
        Set(strName, ulValue.ToString());
    }
    static public void Set(string strName, long lValue)
    {
        Set(strName, lValue.ToString());
    }
    static public void Set(string strName, uint uiValue)
    {
        Set(strName, uiValue.ToString());
    }
    static public void Set(string strName, double dValue)
    {
        Set(strName, dValue.ToString());
    }

    static public void Save()
    {
        PlayerPrefs.Save();
    }

    static public void Delete(string strName)
    {
        string strEncodeName = EncodeSHA256(strName);
        if (PlayerPrefs.HasKey(strEncodeName))
        {
            PlayerPrefs.DeleteKey(strEncodeName);
        }
    }

    static public bool Get(string strName, ref string strValue)
    {
        string strEncodeName = EncodeSHA256(strName);
        if (PlayerPrefs.HasKey(strEncodeName))
        {
            string strEncodeValue = PlayerPrefs.GetString(strEncodeName);
            strValue = DecryptAES(strEncodeValue);
            return true;
        }
        return false;
    }

    static public bool Get(string strName, ref byte byteValue)
    {
        string strValue = string.Empty;
        if (Get(strName, ref strValue))
        {
            try
            {
                byteValue = System.Convert.ToByte(strValue);
            }
            catch
            {
                byteValue = 0;
            }
            return true;
        }
        return false;
    }

    static public bool Get(string strName, ref int iValue)
    {
        string strValue = string.Empty;
        if (Get(strName, ref strValue))
        {
            try
            {
                iValue = System.Convert.ToInt32(strValue);
            }
            catch
            {
                iValue = 0;
            }
            return true;
        }
        return false;
    }

    static public bool Get(string strName, ref float fValue)
    {
        string strValue = string.Empty;
        if (Get(strName, ref strValue))
        {
            try
            {
                fValue = System.Convert.ToSingle(strValue);
            }
            catch
            {
                fValue = 0.0f;
            }
            return true;
        }
        return false;
    }

    static public bool Get(string strName, ref bool bValue)
    {
        string strValue = string.Empty;
        if (Get(strName, ref strValue))
        {
            bValue = strValue == "True";
            return true;
        }
        return false;
    }

    static public bool Get(string strName, ref ulong ulValue)
    {
        string strValue = string.Empty;
        if (Get(strName, ref strValue))
        {
            try
            {
                ulValue = System.Convert.ToUInt64(strValue);
            }
            catch
            {
                ulValue = 0;
            }
            return true;
        }
        return false;
    }

    static public bool Get(string strName, ref long lValue)
    {
        string strValue = string.Empty;
        if (Get(strName, ref strValue))
        {
            try
            {
                lValue = System.Convert.ToInt64(strValue);
            }
            catch
            {
                lValue = 0;
            }
            return true;
        }
        return false;
    }

    static public bool Get(string strName, ref uint uiValue)
    {
        string strValue = string.Empty;
        if (Get(strName, ref strValue))
        {
            try
            {
                uiValue = System.Convert.ToUInt32(strValue);
            }
            catch
            {
                uiValue = 0;
            }
            return true;
        }
        return false;
    }

    static public bool Get(string strName, ref double dValue)
    {
        string strValue = string.Empty;
        if (Get(strName, ref strValue))
        {
            try
            {
                dValue = System.Convert.ToDouble(strValue);
            }
            catch
            {
                dValue = 0;
            }
            return true;
        }
        return false;
    }

    static public bool HasKey(string strName)
    {
        string strEncodeName = EncodeSHA256(strName);
        return PlayerPrefs.HasKey(strEncodeName);
    }

    static public string EncodeSHA256(string strValue)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(strValue));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    public static string EncryptAES(string plainText)
    {
        byte[] encrypted;
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Encoding.UTF8.GetBytes(AesKey);
            aesAlg.IV = Encoding.UTF8.GetBytes(AesIV);
            aesAlg.Mode = CipherMode.CBC; // Or CipherMode.CTR, but ensure proper IV handling
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }
        return Convert.ToBase64String(encrypted);
    }

    public static string DecryptAES(string cipherText)
    {
        try
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes(AesKey);
                aesAlg.IV = Encoding.UTF8.GetBytes(AesIV);
                aesAlg.Mode = CipherMode.CBC;
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Decryption failed: " + e.Message);
            return string.Empty; // Or handle the error as appropriate
        }
    }

    public static void Clear()
    {
        PlayerPrefs.DeleteAll();
    }
}

[System.Serializable]
public class AESKeyConfigData
{
    public string AesKey;
    public string AesIV;
}

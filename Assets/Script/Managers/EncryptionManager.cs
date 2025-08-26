using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class EncryptionManager : MonoBehaviour
{
    public static EncryptionManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    /// <summary>
    /// 문자열을 AES로 암호화
    /// </summary>
    /// <param name="plainText">암호화할 텍스트</param>
    /// <returns>암호화된 Base64 문자열</returns>
    public string EncryptString(string plainText)
    {
        try
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encryptedBytes = EncryptBytes(plainBytes);
            return Convert.ToBase64String(encryptedBytes);
        }
        catch (Exception ex)
        {
            Debug.LogError($"암호화 중 오류 발생: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// AES로 암호화된 문자열을 복호화
    /// </summary>
    /// <param name="encryptedText">암호화된 Base64 문자열</param>
    /// <returns>복호화된 원본 텍스트</returns>
    public string DecryptString(string encryptedText)
    {
        try
        {
            if (string.IsNullOrEmpty(encryptedText))
                return string.Empty;

            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] decryptedBytes = DecryptBytes(encryptedBytes);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
        catch (Exception ex)
        {
            Debug.LogError($"복호화 중 오류 발생: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// 바이트 배열을 AES로 암호화
    /// </summary>
    /// <param name="plainBytes">암호화할 바이트 배열</param>
    /// <returns>암호화된 바이트 배열</returns>
    public byte[] EncryptBytes(byte[] plainBytes)
    {
        try
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = GetAESKey();
                aes.IV = GetAESIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (MemoryStream msEncrypt = new MemoryStream())
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                    csEncrypt.FlushFinalBlock();
                    return msEncrypt.ToArray();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"바이트 암호화 중 오류 발생: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// AES로 암호화된 바이트 배열을 복호화
    /// </summary>
    /// <param name="encryptedBytes">암호화된 바이트 배열</param>
    /// <returns>복호화된 바이트 배열</returns>
    public byte[] DecryptBytes(byte[] encryptedBytes)
    {
        try
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = GetAESKey();
                aes.IV = GetAESIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (MemoryStream msDecrypt = new MemoryStream(encryptedBytes))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (MemoryStream result = new MemoryStream())
                {
                    csDecrypt.CopyTo(result);
                    return result.ToArray();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"바이트 복호화 중 오류 발생: {ex.Message}");
            return null;
        }
    }



    /// <summary>
    /// JSON 객체를 암호화하여 문자열로 변환
    /// </summary>
    /// <param name="obj">직렬화할 객체</param>
    /// <returns>암호화된 JSON 문자열</returns>
    public string EncryptJson<T>(T obj)
    {
        try
        {
            string json = JsonUtility.ToJson(obj);
            return EncryptString(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"JSON 암호화 중 오류 발생: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// 암호화된 JSON 문자열을 객체로 복호화
    /// </summary>
    /// <param name="encryptedJson">암호화된 JSON 문자열</param>
    /// <returns>복호화된 객체</returns>
    public T DecryptJson<T>(string encryptedJson)
    {
        try
        {
            string json = DecryptString(encryptedJson);
            if (string.IsNullOrEmpty(json))
                return default(T);

            return JsonUtility.FromJson<T>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"JSON 복호화 중 오류 발생: {ex.Message}");
            return default(T);
        }
    }

    /// <summary>
    /// ConfigManager에서 AES Key를 byte[]로 변환하여 가져오기
    /// </summary>
    private byte[] GetAESKey()
    {
        try
        {
            string keyString = ConfigManager.instance.Config.aesKey;
            // Base64 문자열인 경우
            return Convert.FromBase64String(keyString);

            // 또는 UTF8 인코딩인 경우 (키가 일반 문자열인 경우)
            // return Encoding.UTF8.GetBytes(keyString);
        }
        catch (Exception ex)
        {
            Debug.LogError($"AES Key 변환 중 오류: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// ConfigManager에서 AES IV를 byte[]로 변환하여 가져오기
    /// </summary>
    private byte[] GetAESIV()
    {
        try
        {
            string ivString = ConfigManager.instance.Config.aesIV;
            // Base64 문자열인 경우
            return Convert.FromBase64String(ivString);

            // 또는 UTF8 인코딩인 경우 (IV가 일반 문자열인 경우)
            // return Encoding.UTF8.GetBytes(ivString);
        }
        catch (Exception ex)
        {
            Debug.LogError($"AES IV 변환 중 오류: {ex.Message}");
            return null;
        }
    }
}
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;


static public class GameConfig
{
    // 암호화
    private static readonly string s_strEncodeKey = "59a48a2bdc1c93e7ce673cc695f8cfff";


    #region Key
    public const string MID = "mid";

    #endregion


    static public void Set(string strName, string strValue)
    {
        string strEncodeName = EncodeMD5(strName);
        string strEncodeValue = EncodeTripleDES(strValue);
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
        string strEncodeName = EncodeMD5(strName);
        if (PlayerPrefs.HasKey(strEncodeName))
            PlayerPrefs.DeleteKey(strEncodeName);
    }

    static public bool Get(string strName, ref string strValue)
    {
        string strEncodeName = EncodeMD5(strName);
        if (PlayerPrefs.HasKey(strEncodeName))
        {
            string strEncodeValue = PlayerPrefs.GetString(strEncodeName);
            strValue = DecodeTripleDES(strEncodeValue);
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
        string strEncodeName = EncodeMD5(strName);
        return PlayerPrefs.HasKey(strEncodeName);
    }

    static public string EncodeMD5(string strValue)
    {
        StringBuilder strBuilder = new StringBuilder();
        MD5 md5Hash = MD5.Create();
        byte[] hashData = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(strValue));
        foreach (byte hashValue in hashData)
        {
            strBuilder.Append(hashValue.ToString());
        }
        return strBuilder.ToString();
    }

    public static string EncodeTripleDES(string strValue)
    {
        MD5 md5Hash = new MD5CryptoServiceProvider();
        byte[] secret = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(s_strEncodeKey));

        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(strValue);

        TripleDES des = new TripleDESCryptoServiceProvider();
        des.Key = secret;
        des.Mode = CipherMode.ECB;
        ICryptoTransform xform = des.CreateEncryptor();
        byte[] encrypted = xform.TransformFinalBlock(bytes, 0, bytes.Length);

        return System.Convert.ToBase64String(encrypted);
    }

    public static string DecodeTripleDES(string strValue)
    {
        if (strValue.Length <= 0)
            return "";

        MD5 md5Hash = new MD5CryptoServiceProvider();
        byte[] secret = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(s_strEncodeKey));

        byte[] bytes = System.Convert.FromBase64String(strValue);
        TripleDES des = new TripleDESCryptoServiceProvider();

        des.Key = secret;
        des.Mode = CipherMode.ECB;
        ICryptoTransform xform = des.CreateDecryptor();
        byte[] decrypted = xform.TransformFinalBlock(bytes, 0, bytes.Length);

        return Encoding.UTF8.GetString(decrypted);
    }

    public static string EncodeTripleDES(string strValue, Encoding encodingType)
    {
        MD5 md5Hash = new MD5CryptoServiceProvider();
        byte[] secret = md5Hash.ComputeHash(encodingType.GetBytes(s_strEncodeKey));

        byte[] bytes = encodingType.GetBytes(strValue);

        TripleDES des = new TripleDESCryptoServiceProvider();
        des.Key = secret;
        des.Mode = CipherMode.ECB;
        ICryptoTransform xform = des.CreateEncryptor();
        byte[] encrypted = xform.TransformFinalBlock(bytes, 0, bytes.Length);

        return System.Convert.ToBase64String(encrypted);
    }

    public static string DecodeTripleDES(string strValue, Encoding encodingType)
    {
        MD5 md5Hash = new MD5CryptoServiceProvider();
        byte[] secret = md5Hash.ComputeHash(encodingType.GetBytes(s_strEncodeKey));

        byte[] bytes = System.Convert.FromBase64String(strValue);
        TripleDES des = new TripleDESCryptoServiceProvider();

        des.Key = secret;
        des.Mode = CipherMode.ECB;
        ICryptoTransform xform = des.CreateDecryptor();
        byte[] decrypted = xform.TransformFinalBlock(bytes, 0, bytes.Length);

        return encodingType.GetString(decrypted);
    }

    public static void SetClass<T>(string name, T instance)
    {
        using (var ms = new MemoryStream())
        {
            new BinaryFormatter().Serialize(ms, instance);
            GameConfig.Set(name, System.Convert.ToBase64String(ms.ToArray()));
        }
    }

    public static T GetClass<T>(string name) where T : new()
    {
        string getStr = string.Empty;
        GameConfig.Get(name, ref getStr);
        if (getStr == string.Empty)
            return default(T);

        byte[] bytes = System.Convert.FromBase64String(getStr);
        using (var ms = new MemoryStream(bytes))
        {
            object obj = new BinaryFormatter().Deserialize(ms);
            return (T)obj;
        }
    }


    public static void Clear()
    {
        PlayerPrefs.DeleteAll();
    }
}
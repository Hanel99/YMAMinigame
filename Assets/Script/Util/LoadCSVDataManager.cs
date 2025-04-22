using UnityEngine;
using System.IO;
using System.Collections;
using UnityEngine.Networking;
using System;


public class LoadCSVDataManager : MonoBehaviour
{
    public static LoadCSVDataManager Instance;

    private string sheetURL = "https://docs.google.com/spreadsheets/d/14c8e0u-_xwVo9sPLZqBPdYlJvGMqWiCH/export?format=csv";
    private string sheetGid = "&gid=1805016141";
    private string sheetRange = "&range=A1:AA46";
    private bool isUpdateNow = false;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }



    // 사용예시

    // var ta = LoadCSVDataManager.Instance.LoadDataFromResources("StringData");
    // List<Dictionary<string, object>> csv = CSVReader.Read(ta);
    // textData = TextMetaData.Create(csv);

    // LoadCSVDataManager.Instance.LoadDataFromStreamingAsset("StringData", (ta) =>
    // {
    //     List<Dictionary<string, object>> csv = CSVReader.Read(ta);
    //     textData = TextMetaData.Create(csv);
    // });

    // LoadCSVDataManager.Instance.LoadDataFromGoogleSheet((data) =>
    // {
    //     List<Dictionary<string, object>> csv = CSVReader.Read(data);
    //     textData = TextMetaData.Create(csv);
    // });



    // Resources.Load 방식
    public TextAsset LoadDataFromResources(string csvFileName)
    {
        TextAsset ta = Resources.Load<TextAsset>($"CSV/{csvFileName}");
        return ta;
    }



    //StreamingAsset 방식
    public void LoadDataFromStreamingAsset(string csvFileName, Action<string> callback)
    {
        if (isUpdateNow) return;
        isUpdateNow = true;

        string filePath = Path.Combine(Application.streamingAssetsPath, $"{csvFileName}.csv");
        string csvText = "";

        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            // Android, WebGL 등의 플랫폼의 경우. 스트리밍 에셋에 직접 접근할 수 없음
            StartCoroutine(Co_ReadCSV(filePath, callback));
        }
        else
        {
            // 그 외 PC 등의 경우
            csvText = File.ReadAllText(filePath);
            isUpdateNow = false;
            callback?.Invoke(csvText);
        }
    }


    // 구글 시트에서 데이터 업데이트 방식
    public void LoadDataFromGoogleSheet(Action<string> callback)
    {
        if (isUpdateNow) return;
        isUpdateNow = true;

        string url = $"{sheetURL}{sheetGid}{sheetRange}";

        StartCoroutine(Co_ReadCSV(url, callback));
    }


    IEnumerator Co_ReadCSV(string filePath, Action<string> callback)
    {
        string result = "";
        using (UnityWebRequest www = UnityWebRequest.Get(filePath))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                result = www.downloadHandler.text;
                HLLogger.Log($"FInish\nurl : {sheetURL}\ndata : {result}");

                isUpdateNow = false;
                callback?.Invoke(result);
            }
            else
            {
                Debug.LogError($"Failed to load CSV file: {www.error}");
                isUpdateNow = false;
                yield break;
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Networking;

public class ServerManager : MonoBehaviour
{
    //TODO @@@ url 분리해서 따로 관리하거나 시트 사용을 그냥 제거하기



    public static ServerManager instance { get; private set; }

    private string sheetURL = "https://docs.google.com/spreadsheets/d/1MiqVSvzW52aw-slBgJ6WbONVDFUbqfXJRNp5dkniHPs/export?format=tsv&range=";

    private int apiCount = 0;
    private Dictionary<int, Action<string>> apiCallbackDic = new Dictionary<int, Action<string>>();
    private List<Coroutine> coroutines = new List<Coroutine>();

    //@@@ 시트 데이터
    private string sheetRange = "O1";



    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnDestroy()
    {
        StopAllAPICoroutines();
    }

    public void StopAllAPICoroutines()
    {
        foreach (var cor in coroutines)
        {
            if (cor != null)
                StopCoroutine(cor);
        }
    }




    public void SendSheetAPI(Action<string> callback = null)
    {
        SendSheetAPI(sheetRange, callback);
    }


    public void SendSheetAPI(string sheetRange, Action<string> callback = null)
    {
        string url = $"{sheetURL}{sheetRange}";

        if (callback != null) apiCallbackDic.Add(apiCount, callback);

        Coroutine cor = StartCoroutine(GoogleSheetProcess(apiCount, url));
        coroutines.Add(cor);

        // HLLogger.Log($"@@@ send {apiCount} / {url}");
        apiCount++;
    }

    private IEnumerator GoogleSheetProcess(int apiNum, string url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.isDone)
            {
                var sheetData = www.downloadHandler.text;
                // HLLogger.Log($"FInish {apiNum}\nurl : {url}\ndate : {sheetData}");

                if (apiCallbackDic.ContainsKey(apiNum))
                    apiCallbackDic[apiNum]?.Invoke(sheetData);
            }
        }
    }





    public List<List<string>> SplitSheetData(string input)
    {
        var result = new List<List<string>>();

        var splitArray = input.Split("!e");
        foreach (var item in splitArray)
        {
            var innerList = item.Split("!a").ToList();
            result.Add(innerList);
        }

        return result;
    }

    public List<int> ConvertStringListToIntList(List<string> stringList)
    {
        List<int> intList = new List<int>();
        foreach (string str in stringList)
        {
            if (int.TryParse(str, out int number))
                intList.Add(number);
        }
        return intList;
    }
}

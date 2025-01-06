using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ServerManager : MonoBehaviour
{
    public static ServerManager instance { get; private set; }

    private string sheetURL = "https://docs.google.com/spreadsheets/d/1MiqVSvzW52aw-slBgJ6WbONVDFUbqfXJRNp5dkniHPs/export?format=tsv&range=";

    private int apiCount = 0;
    private Dictionary<int, Action<string>> apiCallbackDic = new Dictionary<int, Action<string>>();
    private List<Coroutine> coroutines = new List<Coroutine>();

    //@@@ 시트 데이터
    private List<string> sheetRangeList = new List<string>()
    {
        "B1",
        "B3:B4",
        "B6:B11",
        "B13",
        "C15:C17",
        "B19",
        "E1",
        "G14:G25",

    };



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




    public void SendSheetAPI(SheetRangeType rangeType, Action<string> callback = null)
    {
        SendSheetAPI(sheetRangeList[(int)rangeType], callback);
    }


    public void SendSheetAPI(string sheetRange, Action<string> callback = null)
    {
        string url = $"{sheetURL}{sheetRange}";

        if (callback != null) apiCallbackDic.Add(apiCount, callback);

        Coroutine cor = StartCoroutine(GoogleSheetProcess(apiCount, url));
        coroutines.Add(cor);

        HLLogger.Log($"@@@ send {apiCount} / {url}");
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
                HLLogger.Log($"FInish {apiNum}\nurl : {url}\ndate : {sheetData}");

                if (apiCallbackDic.ContainsKey(apiNum))
                    apiCallbackDic[apiNum]?.Invoke(sheetData);
            }
        }
    }





    public void CheckServerMaintenance(Action<string> callback)
    {
        SendSheetAPI(SheetRangeType.ServerMaintenance, (sheetData) =>
        {
            callback(sheetData);
        });
    }
}

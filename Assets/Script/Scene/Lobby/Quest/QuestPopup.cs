using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class QuestPopup : PopupBase
{
    public static QuestPopup instance { get; private set; }

    public QuestItem questItemPrefab;
    public List<QuestItem> questItemList = new List<QuestItem>();



    private CancellationTokenSource questSpawnCTS;




    protected override void OnAwake()
    {
        instance = this;
    }


    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            questSpawnCTS?.Cancel();
            questSpawnCTS = new CancellationTokenSource();

            _OpenUI();
        }
        else
        {
            questSpawnCTS?.Cancel();
            _CloseWindow();
        }
    }



}

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
    public Transform questItemRoot;
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

            SetQuestItems(questSpawnCTS.Token).Forget();
            _OpenUI();
        }
        else
        {
            questSpawnCTS?.Cancel();
            _CloseWindow();
        }
    }







    private async UniTask SetQuestItems(CancellationToken token)
    {
        //퀘스트 아이템들 생성.        
        questItemList.Clear();
        var questMetaDataList = GameResourceManager.instance.GetAllQuestMetaData();

        for (int i = 0; i < questMetaDataList.Count; ++i)
        {
            var questItem = questItemPrefab.Spawn(questItemRoot);
            questItem.SetData(questMetaDataList[i]);
            questItem.transform.localScale = Vector3.one;
            questItem.gameObject.SetActive(true);
            questItemList.Add(questItem);

            if (i % 10 == 9)
                await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
        questItemRoot.gameObject.SetActive(false);
        questItemRoot.gameObject.SetActive(true);
    }



}

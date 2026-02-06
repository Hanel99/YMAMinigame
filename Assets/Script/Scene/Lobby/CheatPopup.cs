using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class CheatPopup : PopupBase
{
    public static CheatPopup instance { get; private set; }
    public CheatItem cheatItemPrefab;
    public Transform cheatRoot;

    private List<CheatData> cheatList = new();
    private List<CheatItem> cheatItemList = new();


    private CancellationTokenSource cheatSpawnCTS;
    private const int BATCH_SIZE = 10;

    protected override void OnAwake()
    {
        instance = this;
    }

    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
#if DEV
            SetCheatData();
            _OpenUI();
            MakeCheatItem();
#else
            _OpenUI();
#endif
        }
        else
        {
            cheatSpawnCTS?.Cancel();
            _CloseWindow();
        }
    }

    private void MakeCheatItem()
    {
        cheatSpawnCTS?.Cancel();
        cheatSpawnCTS = new CancellationTokenSource();

        MakeCheatItems(cheatSpawnCTS.Token).Forget();
    }


    private async UniTask MakeCheatItems(CancellationToken token)
    {
        for (int i = 0; i < cheatList.Count; ++i)
        {
            if (token.IsCancellationRequested) return;

            var cheatItem = cheatItemPrefab.Spawn(cheatRoot);
            cheatItem.transform.localScale = Vector3.one;
            cheatItem.SetCheatData(cheatList[i]);

            cheatItemList.Add(cheatItem);

            if (i > 0 && i % BATCH_SIZE == 0)
                await UniTask.Yield(PlayerLoopTiming.Update, token);
        }


        cheatItemPrefab.gameObject.SetActive(false);
        cheatRoot.gameObject.SetActive(false);
        cheatRoot.gameObject.SetActive(true);
    }







    private void SetCheatData()
    {
        cheatList.Clear();

#if DEV

        cheatList.Add(new CheatData()
        {
            desc = "코인 지정",
            useInputfield = true,
            buttonAction = (value) =>
            {
                SaveDataManager.instance.SetCoin(value);
            },
        });

        cheatList.Add(new CheatData()
        {
            desc = "마일리지 지정",
            useInputfield = true,
            buttonAction = (value) =>
            {
                SaveDataManager.instance.SetMilage(value);
            },
        });

        cheatList.Add(new CheatData()
        {
            desc = "레벨 지정",
            useInputfield = true,
            buttonAction = (value) =>
            {
                value = Mathf.Clamp(value, 1, 300);
                SaveDataManager.instance.SetLevel(value);
                SaveDataManager.instance.SaveUnlockContentDate();
                GameListView.instance.UpdateUserProfileProcess();
                GameListView.instance.CheckUnlockContent();
            },
        });

        cheatList.Add(new CheatData()
        {
            desc = "경험치 지정",
            useInputfield = true,
            buttonAction = (value) =>
            {
                SaveDataManager.instance.SetExp(value);
                GameListView.instance.UpdateUserProfileProcess();
            },
        });

        cheatList.Add(new CheatData()
        {
            desc = "퀘스트 초기화",
            useInputfield = false,
            buttonAction = (value) =>
            {
                SaveDataManager.instance.playerData.questUserPlayData.completedQuestData.Clear();
                SaveDataManager.instance.playerData.questUserPlayData.completedQuestIds.Clear();
            },
        });

        cheatList.Add(new CheatData()
        {
            desc = "콜렉션 초기화",
            useInputfield = false,
            buttonAction = (value) =>
            {
                SaveDataManager.instance.playerData.ownCardList.Clear();
                SaveDataManager.instance.playerData.ownWordList.Clear();
            },
        });

        cheatList.Add(new CheatData()
        {
            desc = "카드 지급",
            useInputfield = true,
            buttonAction = (value) =>
            {
                for (int i = 0; i < value; i++)
                {
                    SaveDataManager.instance.playerData.ownCardList.Add(i);
                }
            },
        });

        cheatList.Add(new CheatData()
        {
            desc = "단어 지급",
            useInputfield = true,
            buttonAction = (value) =>
            {
                for (int i = 0; i < value; i++)
                {
                    SaveDataManager.instance.playerData.ownWordList.Add(i);
                }
            },
        });

        cheatList.Add(new CheatData()
        {
            desc = "꿀밤대회 층 지정",
            useInputfield = true,
            buttonAction = (value) =>
            {
                value = Mathf.Clamp(value, 1, 1000);
                SaveDataManager.instance.playerData.towerFloor = value;
            },
        });


        cheatList.Add(new CheatData()
        {
            desc = "꿀밤대회 스탯 초기화",
            useInputfield = false,
            buttonAction = (value) =>
            {
                SaveDataManager.instance.playerData.towerGameUserStatLevelData.ResetLevel();
            },
        });

        cheatList.Add(new CheatData()
        {
            desc = "꿀밤대회 무기 레벨 지정",
            useInputfield = true,
            buttonAction = (value) =>
            {
                value = Mathf.Clamp(value, 1, 1000);
                SaveDataManager.instance.playerData.towerGameUserWeaponData.weaponLevel = value;
            },
        });


        cheatList.Add(new CheatData()
        {
            desc = "큐브 게임 플레이카운트 수정",
            useInputfield = true,
            buttonAction = (value) =>
            {
                SaveDataManager.instance.playerData.questUserPlayData.cubeGame.playCount = value;
            },
        });


        cheatList.Add(new CheatData()
        {
            desc = "윙또 플레이카운트 수정",
            useInputfield = true,
            buttonAction = (value) =>
            {
                SaveDataManager.instance.playerData.questUserPlayData.wingTto.playCount = value;
            },
        });


        cheatList.Add(new CheatData()
        {
            desc = "최종 미션 재료 지급",
            useInputfield = true,
            buttonAction = (value) =>
            {
                value = Mathf.Clamp(value, 0, 4);
                SaveDataManager.instance.SetFinalQuizReferData(GameType.MatchCardGame, value > 0 ? FinalReferState.Completed : FinalReferState.Locked);
                SaveDataManager.instance.SetFinalQuizReferData(GameType.FindAIWordGame, value > 1 ? FinalReferState.Completed : FinalReferState.Locked);
                SaveDataManager.instance.SetFinalQuizReferData(GameType.CubeGame, value > 2 ? FinalReferState.Completed : FinalReferState.Locked);
                SaveDataManager.instance.SetFinalQuizReferData(GameType.WingTto, value > 3 ? FinalReferState.Completed : FinalReferState.Locked);

                GameListView.instance.CheckUnlockContent();
            },
        });

        cheatList.Add(new CheatData()
        {
            desc = "최종 미션 트라이 카운트 변경",
            useInputfield = true,
            buttonAction = (value) =>
            {
                value = Mathf.Clamp(value, 0, 10);
                SaveDataManager.instance.SetFinalQuizTryCount(value);
                GameListView.instance.CheckUnlockContent();
            },
        });

        cheatList.Add(new CheatData()
        {
            desc = "최종 미션 클리어 처리",
            useInputfield = true,
            buttonAction = (value) =>
            {
                value = Mathf.Clamp(value, 0, 1);
                SaveDataManager.instance.playerData.finalQuizPlayData.playEndRoll = value > 0;
                if (value > 0)
                    SaveDataManager.instance.playerData.finalQuizPlayData.completeTime = DateTime.Now;
            },
        });

#endif
    }




}

public class CheatData
{
    public string desc;
    public bool useInputfield;
    public UnityAction<int> buttonAction;

}
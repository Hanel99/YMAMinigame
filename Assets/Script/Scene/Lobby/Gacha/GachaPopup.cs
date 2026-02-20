using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class GachaPopup : PopupBase
{
    public static GachaPopup instance { get; private set; }

    public Button pick1Button;
    public Button pick10Button;
    public Button mileageButton;
    public Text pick1ValueText;
    public Text pick10ValueText;
    public Text mileageValueText;
    public Text userCoinValueText;
    public Text userMileageValueText;

    private bool isOnProcess = false;
    private List<int> gachaResultIDList = new List<int>();
    private List<int> newCardIDList = new List<int>();



    // Consts
    private const int PRICE_PICK_1 = 0; // Index in GachaPrice
    private const int PRICE_PICK_10 = 1;
    private const int PRICE_MILEAGE = 2;

    private const string MSG_COMPLETE_TITLE = "컴플리트!";
    private const string MSG_COMPLETE_DESC = "축하합니다!\n모든 카드를 획득하셨습니다.\n추가 카드 업데이트를 기다려주세요.";

    private const float DELAY_PROCESS_FINISH = 1f;


    protected override void OnAwake()
    {
        instance = this;
    }


    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            UpdateUI();
            _OpenUI();
            isOnProcess = false;
        }
        else
        {
            _CloseWindow();
        }
    }

    private void UpdateUI()
    {
        userCoinValueText.text = SaveDataManager.instance.playerData.coin.ToString("N0");
        userMileageValueText.text = SaveDataManager.instance.playerData.mileage.ToString("N0");

        int price1 = StaticGameData.GachaPrice[PRICE_PICK_1];
        int price10 = StaticGameData.GachaPrice[PRICE_PICK_10];
        int priceMileage = StaticGameData.GachaPrice[PRICE_MILEAGE];

        pick1Button.interactable = SaveDataManager.instance.playerData.coin >= price1;
        pick10Button.interactable = SaveDataManager.instance.playerData.coin >= price10;
        mileageButton.interactable = SaveDataManager.instance.playerData.mileage >= priceMileage;

        pick1ValueText.text = $"1회 {price1:N0}";
        pick10ValueText.text = $"10회 {price10:N0}";
        mileageValueText.text = $"1회 {priceMileage:N0}";
    }

    //@ 미획득 확정 1회 마일리지 가챠
    public void OnClickMileage()
    {
        if (isOnProcess || isOpenCloseAnimationActing) return;

        if (SaveDataManager.instance.GetNotOwnCardList().Count == 0)
        {
            LobbyUIManager.instance.ShowCommonPopup(MSG_COMPLETE_TITLE, MSG_COMPLETE_DESC, true, true, false);
            return;
        }

        ProcessGacha(true, 1).Forget();
    }

    //@ 코인 사용 일반 1회, 10회 가챠
    public void OnClickPick1()
    {
        if (isOnProcess || isOpenCloseAnimationActing) return;

        ProcessGacha(false, 1).Forget();
    }

    public void OnClickPick10()
    {
        if (isOnProcess || isOpenCloseAnimationActing) return;

        ProcessGacha(false, 10).Forget();
    }


    private async UniTask ProcessGacha(bool isMileage, int count)
    {
        isOnProcess = true;

        gachaResultIDList.Clear();
        newCardIDList.Clear();

        if (isMileage)
        {
            // 마일리지 가챠 로직
            var list = SaveDataManager.instance.GetNotOwnCardList();
            list.Shuffle();
            gachaResultIDList.Add(list[0]);

            HLLogger.Log($"@@@ mileage gacha Result : {list[0]}");
            QuestManager.instance.AddGachaData(true, count);

            // 재화 차감
            SaveDataManager.instance.AddMilage(-StaticGameData.GachaPrice[PRICE_MILEAGE]);
        }
        else
        {
            // 일반 가챠 로직
            var gradeList = GetRandomCardGradeList(count);
            for (int i = 0; i < gradeList.Count; ++i)
                gachaResultIDList.Add(GetRandomCardId(gradeList[i]));

            StringBuilder sb = new StringBuilder();
            foreach (var item in gachaResultIDList)
                sb.Append($"{item},");

            HLLogger.Log($"@@@ Coin {count} gacha Result : {sb}");
            QuestManager.instance.AddGachaData(false, count);

            // 재화 차감 및 마일리지 적립
            int priceIndex = count == 1 ? PRICE_PICK_1 : PRICE_PICK_10;
            SaveDataManager.instance.AddCoin(-StaticGameData.GachaPrice[priceIndex]);
            SaveDataManager.instance.AddMilage(count);
        }

        // 공통 결과 처리
        newCardIDList = SaveDataManager.instance.GetNotOwnCardList(gachaResultIDList);
        SaveDataManager.instance.AddOwnCardList(gachaResultIDList);

        UpdateUI();
        LobbyUIManager.instance.ShowGachaResultPopup(gachaResultIDList, newCardIDList);

        await UniTask.Delay(System.TimeSpan.FromSeconds(DELAY_PROCESS_FINISH));
        isOnProcess = false;
    }

    public void OnClickProbability()
    {
        if (isOpenCloseAnimationActing) return;

        LobbyUIManager.instance.ShowPopup<GachaProbabilityPopup>();
    }





    public List<CardGrade> GetRandomCardGradeList(int count = 1)
    {
        List<CardGrade> list = new();

        for (int i = 0; i < count; ++i)
        {
            var randomValue = UnityEngine.Random.Range(0, StaticGameData.TotalRandomValue);
            CardGrade grade = CardGrade.Normal;

            if (randomValue < StaticGameData.RandomValue[0])
                grade = CardGrade.Black;
            else if (randomValue < StaticGameData.RandomValue[1])
                grade = CardGrade.Gold;
            else if (randomValue < StaticGameData.RandomValue[2])
                grade = CardGrade.Silver;
            else if (randomValue < StaticGameData.RandomValue[3])
                grade = CardGrade.SuperRare;
            else if (randomValue < StaticGameData.RandomValue[4])
                grade = CardGrade.Rare;

            list.Add(grade);
        }

        HLLogger.Log($"Normal - {list.Count(x => x == CardGrade.Normal)}");
        HLLogger.Log($"Rare - {list.Count(x => x == CardGrade.Rare)}");
        HLLogger.Log($"SuperRare - {list.Count(x => x == CardGrade.SuperRare)}");
        HLLogger.Log($"Silver - {list.Count(x => x == CardGrade.Silver)}");
        HLLogger.Log($"Gold - {list.Count(x => x == CardGrade.Gold)}");
        HLLogger.Log($"Black - {list.Count(x => x == CardGrade.Black)}");
        return list;
    }

    public int GetRandomCardId(CardGrade grade)
    {
        var list = GameResourceManager.instance.GetCardIds(grade);
        list.Shuffle();

        HLLogger.Log($"@@@ Select [{grade}] grade card : {list[0]}");
        return list[0];
    }


}

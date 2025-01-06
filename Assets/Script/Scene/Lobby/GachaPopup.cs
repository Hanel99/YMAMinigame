using System.Collections;
using System.Collections.Generic;
using System.Text;
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
        userCoinValueText.text = SaveDataManager.instance.playerData.coin.ToString();
        userMileageValueText.text = SaveDataManager.instance.playerData.mileage.ToString();

        pick1Button.interactable = SaveDataManager.instance.playerData.coin >= StaticGameData.GachaPrice[0];
        pick10Button.interactable = SaveDataManager.instance.playerData.coin >= StaticGameData.GachaPrice[1];
        mileageButton.interactable = SaveDataManager.instance.playerData.mileage >= StaticGameData.GachaPrice[2];

        pick1ValueText.text = $"1회 {StaticGameData.GachaPrice[0].ToString()}";
        pick10ValueText.text = $"10회 {StaticGameData.GachaPrice[1].ToString()}";
        mileageValueText.text = $"1회 {StaticGameData.GachaPrice[2].ToString()}";
    }

    //@ 미획득 확정 1회 마일리지 가챠
    public void OnClickMileage()
    {
        if (isOnProcess || isOpenCloseAnimationActing) return;

        if (SaveDataManager.instance.GetNotOwnCardList().Count == 0)
        {
            LobbyUIManager.instance.ShowCommonPopup("컴플리트!", "축하합니다!\n모든 카드를 획득하셨습니다.\n추가 카드 업데이트를 기다려주세요.", true, true, false);
            return;
        }

        HLLogger.Log("@@@ 1 확정 gacha");

        isOnProcess = true;
        MileageGachaProcess();

        SaveDataManager.instance.AddMilage(-StaticGameData.GachaPrice[2]);
        UpdateUI();
        LobbyUIManager.instance.ShowGachaResultPopup(gachaResultIDList, newCardIDList);
        isOnProcess = false;
    }

    private void MileageGachaProcess()
    {
        // 확정 가챠의 경우는 미획득 카드 중 랜덤으로 1개 획득.
        // 등급 가중치 생각하지 말고 그냥 남은거 중 랜덤으로

        gachaResultIDList.Clear();
        newCardIDList.Clear();

        var list = SaveDataManager.instance.GetNotOwnCardList();
        list.Shuffle();
        gachaResultIDList.Add(list[0]);

        HLLogger.Log($"@@@ mileage gacha Result : {list[0]}");
        newCardIDList = SaveDataManager.instance.GetNotOwnCardList(gachaResultIDList);
        SaveDataManager.instance.AddOwnCardList(gachaResultIDList);
    }

    //@ 코인 사용 일반 1회, 10회 가챠
    public void OnClickPick1()
    {
        if (isOnProcess || isOpenCloseAnimationActing) return;

        HLLogger.Log("@@@ 1 gacha");

        isOnProcess = true;
        GachaProcess(1);
        SaveDataManager.instance.AddCoin(-StaticGameData.GachaPrice[0]);
        UpdateUI();
        LobbyUIManager.instance.ShowGachaResultPopup(gachaResultIDList, newCardIDList);
        DOVirtual.DelayedCall(1f, () => isOnProcess = false);
    }

    public void OnClickPick10()
    {
        if (isOnProcess || isOpenCloseAnimationActing) return;

        HLLogger.Log("@@@ 10 gacha");

        isOnProcess = true;
        GachaProcess(10);
        SaveDataManager.instance.AddCoin(-StaticGameData.GachaPrice[1]);
        UpdateUI();
        LobbyUIManager.instance.ShowGachaResultPopup(gachaResultIDList, newCardIDList);
        DOVirtual.DelayedCall(1f, () => isOnProcess = false);
    }

    private void GachaProcess(int count)
    {
        // 카드 등급을 우선 선정
        // 해당 등급의 카드 리스트를 셔플해 1개의 카드를 선택

        gachaResultIDList.Clear();
        newCardIDList.Clear();

        var gradeList = StaticGameData.GetRandomCardGradeList(count);
        for (int i = 0; i < gradeList.Count; ++i)
            gachaResultIDList.Add(StaticGameData.GetRandomCardId(gradeList[i]));

        StringBuilder sb = new StringBuilder();
        foreach (var item in gachaResultIDList)
        {
            sb.Append($"{item},");
        }
        HLLogger.Log($"@@@ Coin {count} gacha Result : {sb}");

        newCardIDList = SaveDataManager.instance.GetNotOwnCardList(gachaResultIDList);
        SaveDataManager.instance.AddOwnCardList(gachaResultIDList);
        SaveDataManager.instance.AddMilage(count);
    }

    public void OnClickProbability()
    {
        if (isOpenCloseAnimationActing) return;

        LobbyUIManager.instance.ShowGachaProbabilityPopup();
    }



}

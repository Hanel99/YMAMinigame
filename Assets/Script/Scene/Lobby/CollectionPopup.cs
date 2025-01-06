using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionPopup : PopupBase
{
    public static CollectionPopup instance { get; private set; }

    public Dropdown filter;
    public CollectionCard cardPrefab;
    public Transform cardRoot;


    public List<CollectionCard> cardObjectList = new();
    public List<int> viewCardIdList = new();
    private List<string> dropdownList = new();


    protected override void OnAwake()
    {
        instance = this;
    }


    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            SetDropdownFilter();
            CalcViewCardIdList();
            UpdateCardView();
            _OpenUI();
        }
        else
        {
            _CloseWindow();
        }
    }

    private void SetDropdownFilter()
    {
        filter.ClearOptions();
        dropdownList.Clear();

        //0 - all
        //1 ~ 6 - 카드 등급 
        //7 ~ masterCount - 카드 종류
        dropdownList.Add("ALL");
        for (CardGrade grade = CardGrade.Normal; grade < CardGrade.Count; ++grade)
        {
            dropdownList.Add($"{LocalizeManager.instance.GetString($"grade.name.{grade}")} 등급");
        }
        for (CardMaster master = CardMaster.Other; master < CardMaster.Count; ++master)
        {
            dropdownList.Add($"{LocalizeManager.instance.GetString($"master.name.{master}")} 카드");
        }
        filter.AddOptions(dropdownList);
        filter.value = 0;
    }



    private void CalcViewCardIdList()
    {
        viewCardIdList.Clear();

        //0 - all
        //1 ~ 6 - 카드 등급 
        //7 ~ masterCount - 카드 종류
        if (filter.value == 0)
        {
            //all
            HLLogger.Log("@@@ get all card data");
            viewCardIdList = ResourceManager.instance.GetAllCardIds();
        }
        else if (filter.value >= 1 && filter.value <= 6)
        {
            //카드 등급
            HLLogger.Log($"@@@ get {(CardGrade)(filter.value - 1)} card data");
            viewCardIdList = ResourceManager.instance.GetCardIds((CardGrade)(filter.value - 1));
        }
        else
        {
            //담당 캐릭터
            HLLogger.Log($"@@@ get {(CardMaster)(filter.value - 7)} card data");
            viewCardIdList = ResourceManager.instance.GetCardIds((CardMaster)(filter.value - 7));
        }
    }

    public void UpdateCardView()
    {
        // 우선 null인데 삭제 안된 오브젝트 검사를 우선 실행
        cardObjectList.RemoveAll(x => x == null);

        // 표시 카드 개수가 ui에 배치된 카드 수보다 많은 경우
        if (viewCardIdList.Count > cardObjectList.Count)
        {
            // 이미 있는 카드에 데이터 우선 셋팅
            for (int i = 0; i < cardObjectList.Count; ++i)
                cardObjectList[i].SetImage(viewCardIdList[i], SaveDataManager.instance.playerData.ownCardList.Contains(viewCardIdList[i]));

            // 추가 카드 생성 및 데이터 셋팅
            int startCount = cardObjectList.Count;
            for (int i = startCount; i < viewCardIdList.Count; ++i)
            {
                var go = cardPrefab.Spawn(cardRoot);
                go.transform.localScale = Vector3.one * 0.2f;
                go.SetImage(viewCardIdList[i], SaveDataManager.instance.playerData.ownCardList.Contains(viewCardIdList[i]));
                go.gameObject.SetActive(true);

                cardObjectList.Add(go);
            }
        }
        else
        {
            // ui에 배치된 카드보다 표시 카드 개수가 적음.
            for (int i = 0; i < cardObjectList.Count; ++i)
            {
                if (i >= viewCardIdList.Count)
                    // 불필요 카드 제거
                    cardObjectList[i].Recycle();
                else
                    cardObjectList[i].SetImage(viewCardIdList[i], SaveDataManager.instance.playerData.ownCardList.Contains(viewCardIdList[i]));
            }
        }
        cardRoot.gameObject.SetActive(false);
        cardRoot.gameObject.SetActive(true);

        // 여기서 null오브젝트 제거하려는데 제거가 안됨. recycle메소드가 비동기인걸로 추정.
        // cardObjectList.RemoveAll(x => x == null);
    }


    public void OnDropdownValueChanged(Dropdown change)
    {
        HLLogger.Log($"@@@ changed value is {change.value}");

        CalcViewCardIdList();
        UpdateCardView();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaResultPopup : PopupBase
{
    public static GachaResultPopup instance { get; private set; }

    public GameObject result1Root;
    public GachaCard resultCard;
    public GameObject result10Root;
    public List<GachaCard> resultCardList = new List<GachaCard>();

    private List<int> resultCardIDList = new List<int>();
    private List<int> newCardIdList = new List<int>();
    private Dictionary<int, bool> showNewDic = new Dictionary<int, bool>();


    protected override void OnAwake()
    {
        instance = this;
    }


    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            _OpenUI();
        }
        else
        {
            _CloseWindow();
        }
    }


    public void ShowPopup(List<int> cardIdList, List<int> newCardIdList)
    {
        resultCardIDList = cardIdList;
        this.newCardIdList = newCardIdList;

        if (newCardIdList != null)
            foreach (var item in newCardIdList)
                showNewDic.Add(item, false);

        SetResultCardImageList();
        ShowPopup();
    }

    private void SetResultCardImageList()
    {
        bool showNew = false;

        if (resultCardIDList.Count == 1)
        {
            //1연 결과
            result1Root.SetActive(true);
            result10Root.SetActive(false);

            resultCard.SetImage(resultCardIDList[0], newCardIdList == null ? false : newCardIdList.Contains(resultCardIDList[0]));
        }
        else
        {
            // 10연 결과
            result1Root.SetActive(false);
            result10Root.SetActive(true);

            for (int i = 0; i < resultCardList.Count; ++i)
            {
                if (i >= resultCardIDList.Count)
                {
                    resultCardList[i].gameObject.SetActive(false);
                }
                else
                {
                    resultCardList[i].gameObject.SetActive(true);

                    showNew = false;
                    if (newCardIdList != null && newCardIdList.Contains(resultCardIDList[i]) && showNewDic != null && showNewDic[resultCardIDList[i]] == false)
                    {
                        showNewDic[resultCardIDList[i]] = true;
                        showNew = true;
                    }

                    resultCardList[i].SetImage(resultCardIDList[i], showNew);
                }
            }
        }
    }
}

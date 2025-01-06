using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System;

public class CardGameCheckCardPopup : MonoBehaviour
{
    public static CardGameCheckCardPopup instance { get; private set; }

    public Text tryCountText;
    public List<Card> cardObject = new();

    //private
    private Coroutine cor;
    private List<Card> selectCardList = new();
    private Action closeCallback;


    private void Awake()
    {
        instance = this;
    }

    public void StopResultCoroutine()
    {
        if (cor != null)
            StopCoroutine(cor);
    }

    public void StartCheckProcess(List<Card> selectCardList, Action closeCallback = null)
    {
        HideResultObjects();
        this.selectCardList = selectCardList;
        this.closeCallback = closeCallback;
        cor = StartCoroutine(Co_CheckProcess());
    }

    private void HideResultObjects()
    {
        tryCountText.text = "";
    }


    private IEnumerator Co_CheckProcess()
    {
        cardObject[0].SetImage(selectCardList[0].cardId);
        cardObject[1].SetImage(selectCardList[1].cardId);

        yield return new WaitForSeconds(0.2f);
        cardObject[0].ShowCardImage(true);

        yield return new WaitForSeconds(0.2f);
        cardObject[1].ShowCardImage(true);

        yield return new WaitForSeconds(0.4f);
        tryCountText.text = selectCardList[0].cardId == selectCardList[1].cardId ? "통과!" : "실패!";

        yield return new WaitForSeconds(0.5f);
        cor = null;
        closeCallback?.Invoke();
        this.gameObject.SetActive(false);
    }
}


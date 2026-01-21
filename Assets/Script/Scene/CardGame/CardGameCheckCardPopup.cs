using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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


    // Consts
    private const float SHOW_DELAY = 0.2f;
    private const float RESULT_DELAY = 0.4f;
    private const float CLOSE_DELAY = 0.5f;

    private IEnumerator Co_CheckProcess()
    {
        cardObject[0].SetImage(selectCardList[0].cardId);
        cardObject[1].SetImage(selectCardList[1].cardId);

        yield return new WaitForSeconds(SHOW_DELAY);
        cardObject[0].ShowCardImage(true);

        yield return new WaitForSeconds(SHOW_DELAY);
        cardObject[1].ShowCardImage(true);

        yield return new WaitForSeconds(RESULT_DELAY);
        tryCountText.text = selectCardList[0].cardId == selectCardList[1].cardId ? "통과!" : "실패!";

        yield return new WaitForSeconds(CLOSE_DELAY);
        cor = null;
        closeCallback?.Invoke();
        this.gameObject.SetActive(false);
    }
}


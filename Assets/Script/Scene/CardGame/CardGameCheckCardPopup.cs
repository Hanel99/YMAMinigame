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

    public Image dimImage;
    public Text tryCountText;
    public List<Card> cardObject = new();

    // Private
    private Coroutine cor;
    private List<Card> selectCardList = new();
    private Action closeCallback;

    // 원본 트랜스폼 저장용
    private List<Vector3> originPositions = new();
    private List<Vector3> originScales = new();

    // Consts (템포 조절을 위해 대기 시간 증가)
    private const float SHOW_DELAY = 0.2f;
    private const float RESULT_DELAY = 0.5f;
    private const float CLOSE_DELAY = 0.8f;
    private const float DIM_FADE = 0.75f;
    private const float DIM_DURATION = 0.4f;

    private void Awake()
    {
        instance = this;
        // 초기 위치와 크기 저장
        foreach (var card in cardObject)
        {
            originPositions.Add(card.transform.localPosition);
            originScales.Add(card.transform.localScale);
        }
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

        dimImage.DOKill();
        dimImage.DOFade(DIM_FADE, DIM_DURATION).From(0);

        cor = StartCoroutine(Co_CheckProcess());
    }

    private void HideResultObjects()
    {
        tryCountText.text = "";
        ResetCardTransforms();
    }

    /// <summary>
    /// 카드의 트랜스폼을 초기 상태로 되돌리고 실행 중인 트윈을 종료합니다.
    /// </summary>
    private void ResetCardTransforms()
    {
        for (int i = 0; i < cardObject.Count; i++)
        {
            var card = cardObject[i];
            card.transform.DOKill(true);
            card.transform.localPosition = originPositions[i];
            card.transform.localScale = originScales[i];
            card.transform.localRotation = Quaternion.identity;
        }
    }

    private void OnDisable()
    {
        ResetCardTransforms();
    }

    private IEnumerator Co_CheckProcess()
    {
        cardObject[0].SetImage(selectCardList[0].cardId);
        cardObject[1].SetImage(selectCardList[1].cardId);

        yield return new WaitForSeconds(SHOW_DELAY);
        cardObject[0].Flip(true, playSound: false);

        yield return new WaitForSeconds(SHOW_DELAY);
        cardObject[1].Flip(true, playSound: false);

        yield return new WaitForSeconds(RESULT_DELAY);

        bool isMatch = selectCardList[0].cardId == selectCardList[1].cardId;
        tryCountText.text = isMatch ? "통과!" : "실패!";

        if (isMatch)
        {
            SoundManager.instance.PlaySFX(SFXType.O);
            for (int i = 0; i < cardObject.Count; i++)
            {
                var card = cardObject[i];
                card.transform.DOKill(true);
                card.transform.localScale = originScales[i];
                // 1.2배 정도로 살짝 튀어 오르는 간결한 연출 (0.4초간 2번 반동)
                card.transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 0.4f, 2, 1f);
            }
        }
        else
        {
            SoundManager.instance.PlaySFX(SFXType.X);
            for (int i = 0; i < cardObject.Count; i++)
            {
                var card = cardObject[i];
                card.transform.DOKill(true);
                card.transform.localPosition = originPositions[i];
                card.transform.DOShakePosition(0.4f, 25f, 35);
            }
        }

        yield return new WaitForSeconds(CLOSE_DELAY - DIM_DURATION);
        yield return dimImage.DOFade(0, DIM_DURATION).WaitForCompletion();

        cor = null;
        closeCallback?.Invoke();
        ResetCardTransforms(); // 닫히기 전 한 번 더 리셋
        this.gameObject.SetActive(false);
    }
}


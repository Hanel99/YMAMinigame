using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FinalQuizGameResultPanel : MonoBehaviour
{

    public Image bg;
    public Text gameOverText;

    public Text titleText;
    public Text wrongCountText;
    public Text playTimeText;
    public Text resultTitleText;
    public Text resultText;
    public Text descText;

    bool isFail = false;





    public void SetColorAndData(bool isFail)
    {
        this.isFail = isFail;

        bg.color = isFail ? Color.black : Color.white;
        titleText.color = isFail ? Color.white : Color.black;
        wrongCountText.color = isFail ? Color.white : Color.black;
        playTimeText.color = isFail ? Color.white : Color.black;
        resultTitleText.color = isFail ? Color.white : Color.black;
        resultText.color = isFail ? new Color32(200, 0, 0, 255) : new Color32(0, 2, 168, 255);
        descText.color = isFail ? Color.white : Color.black;


        wrongCountText.text = $"오답 {FinalQuizManager.instance.WrongCount}회";
        playTimeText.text = $"진행 정도 {FinalQuizManager.instance.CurrentQuizIndex * 10}%";
        resultText.text = isFail ? "불합격" : "합격";
        descText.text = isFail ? "준비를 더 한 뒤 시험에 응시하세요" : "당신은 이제 연모아 벽반의 일원입니다";
    }



    public async UniTask ResultAnimation()
    {
        titleText.gameObject.SetActive(false);
        wrongCountText.gameObject.SetActive(false);
        playTimeText.gameObject.SetActive(false);
        resultTitleText.gameObject.SetActive(false);
        resultText.gameObject.SetActive(false);
        descText.gameObject.SetActive(false);

        this.gameObject.SetActive(true);

        if (isFail)
        {
            gameOverText.gameObject.SetActive(true);
            bg.gameObject.SetActive(true);
            bg.DOFade(1, 5f).From(0).SetEase(Ease.OutCubic);
            await UniTask.Delay(5000);

            gameOverText.DOFade(0, 2f).From(1).SetEase(Ease.Linear);
            await UniTask.Delay(2000);
            gameOverText.gameObject.SetActive(false);
        }
        else
        {
            gameOverText.gameObject.SetActive(false);
            bg.gameObject.SetActive(true);
            bg.DOFade(1, 0.1f).From(0).SetEase(Ease.Linear);
            await UniTask.Delay(200);
        }

        SoundManager.instance.PlaySFX(SFXType.FinalFlip);
        titleText.gameObject.SetActive(true);
        await UniTask.Delay(1000);

        wrongCountText.gameObject.SetActive(true);
        await UniTask.Delay(1000);

        playTimeText.gameObject.SetActive(true);
        await UniTask.Delay(1000);

        resultTitleText.gameObject.SetActive(true);
        await UniTask.Delay(2000);

        resultText.gameObject.SetActive(true);
        await UniTask.Delay(2000);

        descText.gameObject.SetActive(true);
        await UniTask.Delay(5000);

        SoundManager.instance.PlaySFX(SFXType.DiscordLeave);
        FinalQuizManager.instance.MoveLobbyScene();
    }




}

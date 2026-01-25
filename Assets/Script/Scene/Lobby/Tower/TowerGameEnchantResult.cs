using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TowerGameEnchantResult : MonoBehaviour
{
    public Text resultText;

    public Text beforeText;
    public Text afterText;

    public Text touchToCloseText;
    public Button closeButton;

    public async UniTask ActResultAnimation(TowerGameResultType type, string before, string after, Action UIRefreshAction = null)
    {
        // 초기화
        beforeText.text = "";
        afterText.text = "";
        resultText.text = "";
        touchToCloseText.DOKill();
        touchToCloseText.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() =>
        {
            TowerGameWeaponEnchantPopup.instance.CloseEnchantResult();
            UIRefreshAction?.Invoke();
        });

        if (type != TowerGameResultType.stat)
        {
            resultText.text = ".";
            await UniTask.Delay(200);
            resultText.text = ". .";
            await UniTask.Delay(200);
            resultText.text = ". . .";
            await UniTask.Delay(300);
        }

        // 결과 텍스트 설정
        switch (type)
        {
            case TowerGameResultType.stat:
                resultText.text = "스탯 레벨업!";
                SoundManager.instance.PlaySFX(SFXType.WeaponSuccess);
                PopEffect(resultText.transform);
                break;
            case TowerGameResultType.up:
                resultText.text = "무기 강화 성공!!";
                SoundManager.instance.PlaySFX(SFXType.WeaponSuccess);
                PopEffect(resultText.transform);
                break;
            case TowerGameResultType.stay:
                resultText.text = "무기 등급 유지";
                SoundManager.instance.PlaySFX(SFXType.WeaponStay);
                break;
            case TowerGameResultType.down:
                resultText.text = "강화 실패...\n\n무기 등급 하락";
                SoundManager.instance.PlaySFX(SFXType.WeaponFail);
                break;
        }

        await UniTask.Delay(type == TowerGameResultType.stay ? 0 : 300);

        beforeText.DOFade(1f, 1f).From(0).SetEase(Ease.Linear);
        afterText.DOFade(1f, 1f).From(0).SetEase(Ease.Linear);
        beforeText.text = before;
        afterText.text = after;

        await UniTask.Delay(600);

        touchToCloseText.gameObject.SetActive(true);
        touchToCloseText.DOFade(0.5f, 0.5f).SetEase(Ease.Linear).From(0f).SetLoops(-1, LoopType.Yoyo);
        closeButton.gameObject.SetActive(true);
    }

    public void PopEffect(Transform target, float scaleFactor = 1.2f, float duration = 0.1f)
    {
        if (target == null) return;

        Vector3 originalScale = target.localScale;

        Sequence seq = DOTween.Sequence();
        seq.Append(target.DOScale(originalScale * scaleFactor, duration).SetEase(Ease.OutQuad));
        seq.Append(target.DOScale(originalScale, duration).SetEase(Ease.InQuad));
    }

    private void OnDisable()
    {
        beforeText.text = "";
        afterText.text = "";
        resultText.text = "";
        touchToCloseText.DOKill();
        touchToCloseText.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
    }

}

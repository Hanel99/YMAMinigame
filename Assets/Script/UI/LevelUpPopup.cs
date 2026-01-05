using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpPopup : PopupBase
{
    public static LevelUpPopup instance { get; private set; }
    public Text level;
    public Text unlockContentText;
    public Slider expSlider;
    public Text expText;

    private int targetLevel;
    private int targetExp;
    private int targetMaxExp;


    protected override void OnAwake()
    {
        instance = this;
    }

    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            UpdateData();
            _OpenUI();
            ShowAnimation();
        }
        else
        {
            _CloseWindow();
        }
    }

    private void UpdateData()
    {
        targetLevel = SaveDataManager.instance.playerData.level;
        targetExp = SaveDataManager.instance.playerData.exp;
        targetMaxExp = SaveDataManager.instance.playerData.maxExp;

        int unlockContent = GameResourceManager.instance.GetLevelUnlockValue(targetLevel);
        bool showUnlockContentText = SaveDataManager.instance.playerData.unlockContent != unlockContent;

        unlockContentText.text = "";
        if (showUnlockContentText)
        {
            unlockContentText.text = LocalizeManager.instance.GetString($"levelup.unlock.{unlockContent}");
            SaveDataManager.instance.SaveUnlockContentDate();
        }

        expText.text = targetMaxExp < 0 ? "MAX" : $"{targetExp} / {targetMaxExp}";
    }

    private void ShowAnimation()
    {
        expSlider.value = 0f;

        int startValue = 0;
        DOTween.To(() => startValue, x =>
        {
            startValue = x;
            level.text = startValue.ToString();
        }, targetLevel, 1.3f).SetDelay(0.2f).SetEase(Ease.OutCubic);
        expSlider.DOValue(1f, 1.3f).SetDelay(0.2f).From(0).SetEase(Ease.OutCubic);

        if (targetMaxExp > 0)
        {
            float sliderValue = (float)targetExp / targetMaxExp;
            expSlider.DOValue(sliderValue, 0.4f).SetDelay(1.6f).From(0).SetEase(Ease.OutCubic);
        }
        level.transform.DOScale(1.5f, 0.2f).SetDelay(1.6f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            level.transform.DOScale(1f, 0.2f).SetEase(Ease.InQuad);
        });

        unlockContentText.DOFade(1f, 1f).SetDelay(1.6f).From(0f).SetEase(Ease.OutCubic);
        expText.DOFade(1f, 1f).SetDelay(1.6f).From(0f).SetEase(Ease.OutCubic);
    }




}

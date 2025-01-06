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

    private int targetLevel;


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

        int unlockContent = ResourceManager.instance.GetLevelUnlockValue(targetLevel);
        unlockContentText.text = unlockContent == 0 ? "" : LocalizeManager.instance.GetString($"levelup.unlock.{unlockContent}");
    }

    private void ShowAnimation()
    {
        int startValue = 0;
        DOTween.To(() => startValue, x =>
        {
            startValue = x;
            level.text = startValue.ToString();
        }, targetLevel, 1.5f).SetDelay(0.2f).SetEase(Ease.OutCubic);

        unlockContentText.DOFade(1f, 1f).SetDelay(1f).From(0f).SetEase(Ease.OutCubic);
    }




}

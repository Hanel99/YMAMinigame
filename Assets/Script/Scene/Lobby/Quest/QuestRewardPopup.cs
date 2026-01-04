using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class QuestRewardPopup : PopupBase
{
    public static QuestRewardPopup instance { get; private set; }

    public Text coinText;
    public Text expText;

    private bool isShowLevelUpPopup = false;
    private CancellationTokenSource showLevelUpPopupCts;


    protected override void OnAwake()
    {
        instance = this;
    }

    protected override void _OnDestroy()
    {
        showLevelUpPopupCts?.Cancel();
        showLevelUpPopupCts?.Dispose();
    }

    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            showLevelUpPopupCts?.Cancel();
            showLevelUpPopupCts = new CancellationTokenSource();

            _OpenUI();

            if (isShowLevelUpPopup)
                ShowLevelUpPopup(showLevelUpPopupCts.Token).Forget();
        }
        else
        {
            showLevelUpPopupCts?.Cancel();
            _CloseWindow();
        }
    }

    public void ShowPopup(int coin, int exp, bool isShowLevelUpPopup = false)
    {
        this.isShowLevelUpPopup = isShowLevelUpPopup;
        SetReward(coin, exp);
        ShowPopup();
    }

    public void SetReward(int coin, int exp)
    {
        coinText.text = UnitKorean(coin);
        expText.text = exp.ToString();
    }

    private async UniTaskVoid ShowLevelUpPopup(CancellationToken token)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(1.5f), cancellationToken: token);
            LobbyUIManager.instance.ShowPopup<LevelUpPopup>();
        }
        catch (OperationCanceledException)
        {
            // The popup was closed before the delay finished.
        }
    }

    private string UnitKorean(int value)
    {
        if (value >= 100000000)
            return $"{value / 100000000}억";
        else if (value >= 100000)
            return $"{value / 10000}만";
        else
            return value.ToString();
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class FinalQuizReferPopup : PopupBase
{
    public static FinalQuizReferPopup instance { get; private set; }

    public Sprite[] spriteList;
    public Image referBGImage;
    public Image referImage;
    public Text infoText;


    protected override void OnAwake()
    {
        instance = this;
    }

    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            dimAlphaValue = 0.9f;
            _OpenUI();
        }
        else
        {
            _CloseWindow();
        }
    }


    public void ShowPopup(GameType gameType)
    {
        SetReferImage(gameType);
        ShowPopup();
    }

    public void SetReferImage(GameType gameType)
    {
        bool isGet = FinalReferManager.instance.GetFinalReferState(gameType) == FinalReferState.Completed;
        referBGImage.sprite = spriteList[(int)gameType];
        referImage.sprite = spriteList[(int)gameType];
        referImage.gameObject.SetActive(isGet);

        infoText.text = isGet ? "동의서를 획득했습니다." : "동의서가 발견되었습니다.\n게임을 플레이해서 동의서를 획득하세요.";

        SoundManager.instance.PlaySFX(isGet ? SFXType.FinalRefer2 : SFXType.FinalRefer1);
    }
}
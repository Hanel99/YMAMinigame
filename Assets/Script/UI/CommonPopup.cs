using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommonPopup : PopupBase
{
    public static CommonPopup instance { get; private set; }
    public Button closeBtn;
    public Text title;
    public Text desc;
    public Button OKBtn;
    public Button cancelBtn;

    private Action OKCallback;



    protected override void OnAwake()
    {
        instance = this;
    }



    public void ShowPopup(string titleText, string descText, bool showClose, bool showOK, bool showNo, Action closeCallback = null, Action OKCallback = null)
    {
        title.text = titleText;
        desc.text = descText;
        closeBtn.gameObject.SetActive(showClose);
        OKBtn.gameObject.SetActive(showOK);
        cancelBtn.gameObject.SetActive(showNo);

        SetCloseCallBack(closeCallback);
        this.OKCallback = OKCallback;

        SetPopupSize();
        ShowPopup();
    }


    private void SetPopupSize()
    {
        Canvas.ForceUpdateCanvases();
        bgTransform.sizeDelta = new Vector2(bgTransform.rect.size.x, 450 + desc.rectTransform.rect.height);
    }


    /// <summary>
    /// commonPopup은 이 메소드 사용 금지
    /// </summary>
    /// <param name="enable"></param>
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


    public void OnClickOK()
    {
        if (isOpenCloseAnimationActing) return;

        if (OKCallback != null)
            OKCallback?.Invoke();
        else
            ShowPopup(false);
    }
}

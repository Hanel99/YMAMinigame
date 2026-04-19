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

    [Header("스크롤 뷰 (긴 텍스트 처리용)")]
    public GameObject scrollDescGroup; // 스크롤 뷰가 포함된 부모 또는 전체 오브젝트 (ScrollRect)
    public Text scrollDesc; // 스크롤 뷰 내부에 배치될 텍스트
    public int maxLineCount = 6; // 스크롤 뷰로 전환할 기준 라인 수

    private Action OKCallback;



    protected override void OnAwake()
    {
        instance = this;
    }



    public void ShowPopup(string titleText, string descText, bool showClose, bool showOK, bool showNo, Action closeCallback = null, Action OKCallback = null)
    {
        title.text = titleText;
        desc.text = descText;
        
        // 기본 텍스트 활성화, 스크롤 그룹 비활성화
        desc.gameObject.SetActive(true);
        if (scrollDescGroup != null) scrollDescGroup.SetActive(false);

        // 텍스트 제네레이터를 활용하여 자동 줄바꿈(Wrap)이 포함된 실제 표시 라인 수 계산
        TextGenerationSettings settings = desc.GetGenerationSettings(desc.rectTransform.rect.size);
        desc.cachedTextGeneratorForLayout.Populate(descText, settings);
        
        int layoutLineCount = desc.cachedTextGeneratorForLayout.lineCount;
        int explicitLineCount = string.IsNullOrEmpty(descText) ? 0 : descText.Split('\n').Length;
        int finalLineCount = Mathf.Max(layoutLineCount, explicitLineCount);

        // 설정된 최대 라인 수를 초과하면 스크롤뷰 모드로 전환
        if (finalLineCount > maxLineCount)
        {
            desc.gameObject.SetActive(false);
            if (scrollDescGroup != null)
            {
                scrollDescGroup.SetActive(true);
                if (scrollDesc != null) scrollDesc.text = descText;
            }
        }

        closeBtn.gameObject.SetActive(showClose);
        OKBtn.gameObject.SetActive(showOK);
        cancelBtn.gameObject.SetActive(showNo);

        SetCloseCallBack(closeCallback);
        this.OKCallback = OKCallback;

        // SetPopupSize();
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

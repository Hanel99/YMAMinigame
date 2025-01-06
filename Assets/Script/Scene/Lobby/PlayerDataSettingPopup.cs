using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDataSettingPopup : PopupBase
{
    public static PlayerDataSettingPopup instance { get; private set; }

    public InputField inputName;
    public Dropdown inputMaster;
    public Dropdown inputLanguage;


    List<string> languageList = new List<string>();
    List<string> masterList = new List<string>();


    protected override void OnAwake()
    {
        instance = this;
    }


    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            SetPopupData();
            _OpenUI();
        }
        else
        {
            _CloseWindow();
        }
    }


    private void SetPopupData()
    {
        inputName.text = SaveDataManager.instance.playerData.name;

        inputMaster.ClearOptions();
        masterList.Clear();
        for (CardMaster master = CardMaster.Other; master < CardMaster.Count; ++master)
        {
            masterList.Add(LocalizeManager.instance.GetString($"master.name.{master}"));
        }
        inputMaster.AddOptions(masterList);
        inputMaster.value = (int)SaveDataManager.instance.playerData.master;

        inputLanguage.ClearOptions();
        languageList.Clear();
        for (LanguageType language = LanguageType.ko; language < LanguageType.Count; ++language)
        {
            languageList.Add(LocalizeManager.instance.GetString($"language.name.{language}"));
        }

        inputLanguage.AddOptions(languageList);
        inputLanguage.value = (int)SaveDataManager.instance.playerData.languageType;
    }

    public override void OnClickClose()
    {
        if (UserProfilePopup.instance != null && UserProfilePopup.instance.isActiveAndEnabled)
            UserProfilePopup.instance.UpdateDataText();

        GameListView.instance.UpdateUserProfileProcess();
        ShowPopup(false);
    }

    public void OnClickConfirm()
    {
        if (isOpenCloseAnimationActing) return;

        SaveDataManager.instance.playerData.name = inputName.text;
        SaveDataManager.instance.playerData.master = (CardMaster)inputMaster.value;
        SaveDataManager.instance.playerData.languageType = (LanguageType)inputLanguage.value;
        SaveDataManager.instance.SavePlayerData();

        HLLogger.Log("@@@ Confirm Action");
        OnClickClose();
    }




    public enum ValidationError
    {
        Error0,     // 입력된 내용이 없습니다.
        Error1,     // 변경된 내용이 없습니다.
        Error2,     // 너무 짧습니다.
        Error3,     // 12글자 이하로 입력하세요.
        Error4,     // 특수문자와 띄어쓰기는 입력하실 수 없습니다.
        Error5,     // 띄어쓰기는 입력하실 수 없습니다.
        Error6,     // 특수문자는 입력하실 수 없습니다.

        Success,
    }
    public ValidationError CheckValidation(string value)
    {
        //HLLogger.Log($"value = [ {value} ]", LogColor.cyan);

        //? ############ 입력 내용 없음. ############
        if (string.IsNullOrEmpty(value))
            return ValidationError.Error0;

        //? ############ 변경 사항 없음. ############
        if (SaveDataManager.instance.playerData.name.Equals(value))
            return ValidationError.Error1;

        //? ############ 입력 제한 - 4 ~ 24 byte (2 ~ 12자) ############
        int bytecount = Encoding.Unicode.GetByteCount(value);
        int minByte = 4, maxByte = 24;
        if (bytecount < minByte)
            return ValidationError.Error2;
        else if (bytecount > maxByte)
        {
            inputName.text = Encoding.Unicode.GetString(Encoding.Unicode.GetBytes(value), 0, maxByte);
            return ValidationError.Error3;
        }

        //? ############ 입력 제한 - 특수 문자 ############
        /*
            숫자          @[^0-9]
            엉어          @[^a-zA-Z]
            한글          @[^가-힣]
            일본어        @[^ぁ-ゔァ-ヴー々〆〤一-龥]
            띄어쓰기      @[^\s]
         */

        bool isSpace = Regex.Replace(value, @"[^\s]{1,24}", string.Empty, RegexOptions.Singleline).Length > 0;                            //? 공백 체크
        bool isSpecial = !string.IsNullOrEmpty(Regex.Replace(value, @"[ ^0-9a-zA-Z가-힣]{1,24}", string.Empty, RegexOptions.Singleline)); //? 특수문자
        if (isSpace || isSpecial)
        {
            inputName.text = Regex.Replace(value, @"[^0-9a-zA-Z가-힣]{1,24}", string.Empty, RegexOptions.Singleline);
            if (isSpace && isSpecial)
                return ValidationError.Error4;
            else if (isSpace)
                return ValidationError.Error5;
            else if (isSpecial)
                return ValidationError.Error6;
        }

        // ############ 정상 입력 ############
        return ValidationError.Success;
    }





}

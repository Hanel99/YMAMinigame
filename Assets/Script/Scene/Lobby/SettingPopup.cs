using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SettingPopup : PopupBase
{
    public enum SettingTab
    {
        None,
        Visual,
        Sound,
    }

    public static SettingPopup instance { get; private set; }
    public Text appVersionText;
    public Text dataVersionText;

    [Header("Tab Group")]
    public GameObject VisualTab;
    public GameObject SoundTab;


    [Header("Visual Settings")]
    public Toggle fullScreen;
    public Dropdown resolutionDropdown;
    public InputField inputRedeem;
    private Resolution[] resolutions;
    private List<(int, int)> resolutionList = new List<(int, int)>();


    [Header("Sound Setting")]
    public Slider bgmSlider;
    public Toggle bgmMuteToggle;
    public Slider sfxSlider;
    public Toggle sfxMuteToggle;

    private CommonPopup serverDataUpdatePopup = null;

    protected override void OnAwake()
    {
        instance = this;
    }


    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            SetData();
            _OpenUI();
        }
        else
        {
            SoundManager.instance.SaveSoundSettings();
            _CloseWindow();
        }
    }


    private void SetData()
    {
        appVersionText.text = $"App Version - {Application.version}";

        //@@@ temp
        if (SaveDataManager.instance == null || SaveDataManager.instance.playerData == null)
            dataVersionText.text = "server data is null";
        else
            dataVersionText.text = $"Data Version - {SaveDataManager.instance.playerData.serverDataVersion}";

        SetTab(SettingTab.Visual);
    }

    [VisibleEnum(typeof(SettingTab))]
    public void OnClickTab(int tabIndex)

    {
        if (isOpenCloseAnimationActing) return;

        SettingTab tab = (SettingTab)tabIndex;
        SetTab(tab);
    }
    private void SetTab(SettingTab tab)
    {
        if (tab == SettingTab.Visual)
        {
            VisualTab.SetActive(true);
            SoundTab.SetActive(false);

            inputRedeem.text = "";
            SetResolutionOption();
        }
        else if (tab == SettingTab.Sound)
        {
            VisualTab.SetActive(false);
            SoundTab.SetActive(true);

            // 사운드 설정 초기화
            bgmSlider.value = SoundManager.instance.bgmVolume;
            bgmMuteToggle.isOn = SoundManager.instance.isBgmMute;

            sfxSlider.value = SoundManager.instance.sfxVolume;
            sfxMuteToggle.isOn = SoundManager.instance.isSfxMute;
        }
    }




    #region Video Setting

    public void OnClickFullScreenToggle()
    {
        if (isOpenCloseAnimationActing) return;

        HLLogger.Log($"@@@ fullScreen Set : {fullScreen.isOn}");
        Screen.fullScreen = fullScreen.isOn;
    }

    public void ResolutionDropdownChanged(Dropdown change)
    {
        var resolutionData = resolutionList[change.value];

        var tempList = resolutions.ToList();
        Resolution resolution = tempList.FindAll(x => x.width == resolutionData.Item1 && x.height == resolutionData.Item2).OrderByDescending(x => x.refreshRateRatio).First();

        HLLogger.Log($"@@@ Resolution : {resolution.width}/{resolution.height} ({resolution.refreshRateRatio} Hz)");
        Screen.SetResolution(resolution.width, resolution.height, fullScreen.isOn);
    }

    private void SetResolutionOption()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        //apk 빌드. 불필요. 아예 꺼버림
        fullScreen.gameObject.SetActive(false);
        resolutionDropdown.gameObjsect.SetActive(false);
#else
        //exe 빌드. 버튼과 드롭다운을 켬.
        fullScreen.gameObject.SetActive(true);
        resolutionDropdown.gameObject.SetActive(true);

        resolutions = Screen.resolutions;
        resolutionList.Clear();
        resolutionDropdown.ClearOptions();

        HashSet<string> options = new HashSet<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            if (options.Contains(option) == false)
            {
                options.Add(option);
                resolutionList.Add((resolutions[i].width, resolutions[i].height));
            }
        }

        resolutionDropdown.AddOptions(new List<string>(options));
        resolutionDropdown.RefreshShownValue();

        fullScreen.isOn = Screen.fullScreen;
#endif
    }

    public void OnClickRedeemCheck()
    {
        if (isOpenCloseAnimationActing) return;

        string code = inputRedeem.text.ToLower();

        // 코드에 들어있는지 확인
        if (StaticGameData.RedeemCodes.Contains(code) == false)
        {
            LobbyUIManager.instance.ShowCommonPopup("실패", "잘못된 리딤 코드입니다.", true, true, false);
            return;
        }

        // 쓴 리딤인지 우선 확인
        if (SaveDataManager.instance.IsUseRedeemCode(code))
        {
            LobbyUIManager.instance.ShowCommonPopup("실패", $"이미 사용된 리딤 코드입니다.\n{inputRedeem.text}", true, true, false);
            return;
        }

        // 리딤코드에 맞춰 수행

        //TODO @@@ 왜 이따구로 만들었을까 추후수정필요
        switch (code)
        {
            case "getmile":
                SaveDataManager.instance.AddMilage(3000);
                break;
            case "getgold":
                SaveDataManager.instance.AddCoin(140000);
                break;
            case "getallcard":
                SaveDataManager.instance.AddOwnCardList(GameResourceManager.instance.GetAllCardIds());
                break;
            case "devtestopen":
                StaticGameData.showDevTestText = true;
                break;
        }

        SaveDataManager.instance.AddUsingRedeemCode(code);
        LobbyUIManager.instance.ShowCommonPopup("성공", $"{inputRedeem.text}\n리딤 코드 입력이 완료되었습니다.", true, true, false);
        inputRedeem.text = "";
    }


    #endregion




    #region Sound Setting

    public void OnBgmSliderChanged(Slider change)
    {
        HLLogger.Log($"@@@ BGM Volume Set : {change.value}");
        SoundManager.instance.SetBGMVolume(change.value);
    }

    public void OnSfxSliderChanged(Slider change)
    {
        HLLogger.Log($"@@@ SFX Volume Set : {change.value}");
        SoundManager.instance.SetSFXVolume(change.value);
    }

    public void OnBgmMuteToggle()
    {
        HLLogger.Log($"@@@ BGM Mute Set : {bgmMuteToggle.isOn}");
        SoundManager.instance.isBgmMute = bgmMuteToggle.isOn;
        if (SoundManager.instance.isBgmMute)
            SoundManager.instance.StopBGM();
        else
            SoundManager.instance.PlayBGM(BGMType.Lobby);
    }

    public void OnSfxMuteToggle()
    {
        HLLogger.Log($"@@@ SFX Mute Set : {sfxMuteToggle.isOn}");
        SoundManager.instance.isSfxMute = sfxMuteToggle.isOn;
    }

    #endregion




    public void OnClickRemoveData()
    {
        if (isOpenCloseAnimationActing) return;

        var popup = LobbyUIManager.instance.ShowCommonPopup("주의", "정말로 삭제하시겠습니까?\n삭제한 데이터는 복구되지 않습니다.", true, true, true, null,
        () =>
        {
            SaveDataManager.instance.RemovePlayerData();
            var popup2 = LobbyUIManager.instance.ShowCommonPopup("데이터 삭제", "데이터가 삭제되었습니다.", false, true, false, null, () =>
            {
                Application.Quit();
            });
            popup2.isActBackKey = false;
        });
        popup.isActBackKey = false;
    }

    public void OnClickCheckServerData()
    {
        if (isOpenCloseAnimationActing) return;

        serverDataUpdatePopup = LobbyUIManager.instance.ShowCommonPopup("데이터 업데이트", $"서버 데이터 업데이트 중입니다...", false, false, false);
        serverDataUpdatePopup.isActBackKey = false;

        ServerManager.instance.CheckServerMaintenance((sheetData) =>
        {
            CommonPopup popup = null;
            switch (sheetData)
            {
                case "0":
                    //이상 없음. 접속 가능
                    StartCoroutine(nameof(UpdateServerData));
                    break;

                case "1":
                    //DEV만 입장 가능
#if DEV
                    StartCoroutine(nameof(UpdateServerData));
#else
                    popup = LobbyUIManager.instance.ShowCommonPopup("공지", $"서버 점검 중입니다.\nCode.{sheetData}", false, true, false, null, () =>
                    {
                        Application.Quit();
                    });
                    popup.isActBackKey = false;
#endif
                    break;

                case "2":
                    //Editor만 입장 가능
#if UNITY_EDITOR
                    StartCoroutine(nameof(UpdateServerData));
#else
                    popup = LobbyUIManager.instance.ShowCommonPopup("공지", $"서버 점검 중입니다.\nCode.{sheetData}", false, true, false, null, () =>
                    {
                        Application.Quit();
                    });
                    popup.isActBackKey = false;

#endif
                    break;

                default:
                    popup = LobbyUIManager.instance.ShowCommonPopup("공지", $"서버 점검 중입니다.\nCode.{sheetData}", false, true, false, null, () =>
                    {
                        Application.Quit();
                    });
                    popup.isActBackKey = false;
                    break;
            }
        });
    }

    private IEnumerator UpdateServerData()
    {
        bool apiComplete = true;
        yield return null;

        //@ 1. 데이터 버전 체크
        apiComplete = false;
        ServerManager.instance.SendSheetAPI(SheetRangeType.DataVersion, (sheetData) =>
        {
            if (SaveDataManager.instance.playerData.serverDataVersion.Equals(sheetData) == false)
                SaveDataManager.instance.playerData.serverDataVersion = sheetData;
            apiComplete = true;
        });
        yield return new WaitUntil(() => apiComplete);

        //@ 2. 이벤트 타임 검증
        apiComplete = false;
        ServerManager.instance.SendSheetAPI(SheetRangeType.EventDateTimeRange, (sheetData) =>
        {
            StaticGameData.UpdateEventDateTimeFromServer(sheetData);
            apiComplete = true;
        });
        yield return new WaitUntil(() => apiComplete);

        //@ 3. 카드 등급 랜덤 가중치 업데이트
        apiComplete = false;
        ServerManager.instance.SendSheetAPI(SheetRangeType.RandomValue, (sheetData) =>
        {
            StaticGameData.UpdateRandomValueFromServer(sheetData);
            apiComplete = true;
        });
        yield return new WaitUntil(() => apiComplete);

        //@ 5. 가챠 가격
        apiComplete = false;
        ServerManager.instance.SendSheetAPI(SheetRangeType.GachaPrice, (sheetData) =>
        {
            StaticGameData.UpdateGachaPriceFromServer(sheetData);
            apiComplete = true;
        });
        yield return new WaitUntil(() => apiComplete);

        serverDataUpdatePopup?.ShowPopup(false);
        LobbyUIManager.instance.ShowCommonPopup("완료", "서버 데이터 업데이트가 완료되었습니다.", true, true, false);
    }


}
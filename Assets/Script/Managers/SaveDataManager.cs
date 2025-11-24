using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager instance { get; private set; }
    private PlayerData _playerData;
    public PlayerData playerData => _playerData;
    private OtherPlayerData _otherPlayerData;
    public OtherPlayerData otherPlayerData => _otherPlayerData;

    private CancellationTokenSource saveCts; // SavePlayerData 호출을 제어하는 CancellationTokenSource


    #region Convert

    //playfab에 저장할 용도
    public string JsonPlayerData => JsonUtility.ToJson(_playerData);
    public PlayerData PlayerDataFromJson(string json) => JsonUtility.FromJson<PlayerData>(json);

    #endregion


    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }


    public void Init()
    {
        _playerData = new PlayerData();
        _otherPlayerData = new OtherPlayerData();
    }

    private void OnApplicationQuit()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != SceneName.IntroScene.ToString())
            SavePlayerData(true);
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != SceneName.IntroScene.ToString())
            SavePlayerData(true);
    }





    #region CollectionWordDataLogic



    public void AddOwnCardList(int card)
    {
        List<int> cardList = new List<int>() { card };
        AddOwnCardList(cardList);
    }

    public void AddOwnCardList(List<int> cardList)
    {
        foreach (var id in cardList)
        {
            if (_playerData.ownCardList.Contains(id) == false)
                _playerData.ownCardList.Add(id);
        }
        _playerData.ownCardList.Sort();
        SavePlayerData();
    }



    public void RemoveOwnCardList()
    {
#if DEV
        _playerData.ownCardList.Clear();
        SavePlayerData();
#endif
    }

    public void RemoveNotUseCardList()
    {
        var allCardList = GameResourceManager.instance.GetAllCardIds();
        _playerData.ownCardList.Intersect(allCardList);
        SavePlayerData();
    }



    public bool IsOwnCard(int cardID)
    {
        return _playerData.ownCardList.Contains(cardID);
    }

    /// <summary>
    /// 해당 등급의 카드 중 획득한 카드 id만 반환
    /// </summary>
    /// <param name="grade"></param>
    /// <returns></returns>
    public List<int> GetOwnCardList(CardGrade grade)
    {
        var list = GameResourceManager.instance.GetCardIds(grade);
        return list.Intersect(_playerData.ownCardList).ToList();
    }

    /// <summary>
    /// 해당 등급의 카드 중 미획득인 카드 id만 반환
    /// </summary>
    /// <param name="grade"></param>
    /// <returns></returns>
    public List<int> GetNotOwnCardList(CardGrade grade)
    {
        var list = GameResourceManager.instance.GetCardIds(grade);
        return list.Except(_playerData.ownCardList).ToList();
    }
    public List<int> GetNotOwnCardList()
    {
        var list = GameResourceManager.instance.GetAllCardIds();
        return list.Except(_playerData.ownCardList).ToList();
    }
    public List<int> GetNotOwnCardList(List<int> list)
    {
        return list.Except(_playerData.ownCardList).ToList();
    }


    #endregion




    #region CollectionWordDataLogic

    public void AddOwnWordList(int index)
    {
        List<int> wordList = new List<int>() { index };
        AddOwnWordList(wordList);
    }

    public void AddOwnWordList(List<int> wordList)
    {
        foreach (var id in wordList)
        {
            if (_playerData.ownWordList.Contains(id) == false)
                _playerData.ownWordList.Add(id);
        }
        _playerData.ownWordList.Sort();
        SavePlayerData();
    }


    public bool IsOwnWord(int index)
    {
        return _playerData.ownWordList.Contains(index);
    }

    #endregion



    #region playerDataLogic

    public void AddCoin(int amount, bool isSave = true)
    {
        _playerData.coin += amount;

        if (_playerData.coin > StaticGameData.MAX_COIN_VALUE)
            _playerData.coin = StaticGameData.MAX_COIN_VALUE;
        if (_playerData.coin < 0)
            _playerData.coin = 0;

        if (isSave)
            SavePlayerData();
    }
    public void SetCoin(int amount)
    {
        _playerData.coin = amount;

        if (_playerData.coin > StaticGameData.MAX_COIN_VALUE)
            _playerData.coin = StaticGameData.MAX_COIN_VALUE;
        if (_playerData.coin < 0)
            _playerData.coin = 0;

        SavePlayerData();
    }
    public void AddMilage(int amount)
    {
        _playerData.mileage += amount;

        if (_playerData.mileage > StaticGameData.MAX_MILEAGE_VALUE)
            _playerData.mileage = StaticGameData.MAX_MILEAGE_VALUE;
        if (_playerData.mileage < 0)
            _playerData.mileage = 0;

        SavePlayerData();
    }
    public void SetMilage(int amount)
    {
        _playerData.mileage = amount;

        if (_playerData.mileage > StaticGameData.MAX_MILEAGE_VALUE)
            _playerData.mileage = StaticGameData.MAX_MILEAGE_VALUE;
        if (_playerData.mileage < 0)
            _playerData.mileage = 0;

        SavePlayerData();
    }

    public bool IsTodayFirstLogin()
    {
        return _playerData.savedTime.Year != DateTime.Now.Year
            || _playerData.savedTime.Month != DateTime.Now.Month
            || _playerData.savedTime.Day != DateTime.Now.Day;
    }

    public bool IsUseRedeemCode(string code)
    {
        return _playerData.usingRedeemCode.Contains(code);
    }

    public void AddUsingRedeemCode(string code)
    {
        if (_playerData.usingRedeemCode.Contains(code) == false)
            _playerData.usingRedeemCode.Add(code);
    }

    public void AddLevel(int amount = 1)
    {
        _playerData.level += amount;
        HLLogger.Log($"Player Level Up! {_playerData.level}");
    }

    public bool AddExp(int amount, bool isSave = true)
    {
        bool isShowLevelUpPopup = false;
        bool needLevelUp = false;
        _playerData.exp += amount;

        if (_playerData.maxExp < 0)
        {
            //max level
            _playerData.exp = 0;
            if (isSave)
                SavePlayerData();
            return false;
        }

        HLLogger.Log($"Add {amount} Player Exp -> {_playerData.exp}/{_playerData.maxExp}");

        if (_playerData.maxExp >= 0 && _playerData.exp >= _playerData.maxExp)
            isShowLevelUpPopup = needLevelUp = true;

        while (needLevelUp && _playerData.maxExp >= 0)
        {
            _playerData.exp -= _playerData.maxExp;
            AddLevel();

            if (_playerData.maxExp < 0)
            {
                //max level
                _playerData.exp = 0;
                needLevelUp = false;
            }
            else
            {
                HLLogger.Log($"Now Player Exp -> {_playerData.exp}/{_playerData.maxExp}");
                needLevelUp = _playerData.exp >= _playerData.maxExp;
            }
        }

        if (_playerData.maxExp < 0)
            _playerData.exp = 0;

        if (isSave)
            SavePlayerData();
        return isShowLevelUpPopup;
    }



    public void SetIDPW(string id, string pw, bool isAutoLogin)
    {
        _playerData.playFabLoginID = id;
        _playerData.playFabLoginPW = pw;
        _playerData.autoLogin = isAutoLogin;

        SavePlayerData();
    }
    public (string, string) GetIDPW()
    {
        return (_playerData.playFabLoginID, _playerData.playFabLoginPW);
    }

    public void SetPlayerName()
    {
        PlayFabManager.instance.SetDisplayName();
    }

    public void SetWingTtoHighScore(float score)
    {
        if (_playerData.wingTtoHighScore < score)
            _playerData.wingTtoHighScore = score;
    }

    #endregion




    #region TowerGameDataLogic

    public void SetTowerUserStatLevel(TowerUserStatType type, int level)
    {
        switch (type)
        {
            case TowerUserStatType.atk:
                _playerData.towerGameUserStatLevelData.atkLevel = level;
                break;
            case TowerUserStatType.def:
                _playerData.towerGameUserStatLevelData.defLevel = level;
                break;
            case TowerUserStatType.hp:
                _playerData.towerGameUserStatLevelData.hpLevel = level;
                break;
            case TowerUserStatType.criRate:
                _playerData.towerGameUserStatLevelData.criRateLevel = level;
                break;
            case TowerUserStatType.criDmg:
                _playerData.towerGameUserStatLevelData.criDmgLevel = level;
                break;
        }

        // SavePlayerData 호출 예약
        ScheduleSavePlayerData();
    }


    public void AddTowerFloor()
    {
        _playerData.towerFloor++;

        ScheduleSavePlayerData();
    }



    public void SetTowerUserWeaponLevel(int level)
    {
        _playerData.towerGameUserWeaponData.weaponLevel = level;

        ScheduleSavePlayerData();
    }

    public void AddTowerUserWeaponFailCount()
    {
        _playerData.towerGameUserWeaponData.failCount++;

        ScheduleSavePlayerData();
    }

    public void SetTowerUserWeaponFailCount(int value)
    {
        _playerData.towerGameUserWeaponData.failCount = value;

        ScheduleSavePlayerData();
    }

    public void SetTowerUserWeaponIsDown(bool value)
    {
        _playerData.towerGameUserWeaponData.isDown = value;

        ScheduleSavePlayerData();
    }


    // 세이브데이터가 너무 많이 들어올 경우, 1초간 중복 메소드를 기다린 뒤 실행.
    private void ScheduleSavePlayerData()
    {
        // 이전 작업 취소
        saveCts?.Cancel();
        saveCts = new CancellationTokenSource();
        HLLogger.Log("@@@ SavePlayerData 예약됨.");

        // 1초 대기 후 SavePlayerData 호출
        WaitAndSavePlayerData(saveCts.Token).Forget();
    }

    private async UniTaskVoid WaitAndSavePlayerData(CancellationToken token)
    {
        try
        {
            await UniTask.Delay(1000, cancellationToken: token); // 1초 대기
            SavePlayerData(); // SavePlayerData 호출
        }
        catch (OperationCanceledException)
        {
            // 작업이 취소된 경우 처리 (필요 시 추가 로직 작성 가능)
            HLLogger.Log("SavePlayerData 호출이 취소되었습니다.");
        }
    }



    #endregion







    #region SaveDataInDevice

    public void SavePlayerData(bool forceSave = false)
    {
        _playerData.savedTime = DateTime.Now;
        ES3.Save(StaticGameData.SAVE_PLAYER_DATA_KEY, _playerData);
        HLLogger.Log("Save Date Complete");

        PlayFabManager.instance.SavePlayerData(forceSave);
    }


    public PlayerData LoadPlayerData()
    {
        if (ES3.KeyExists(StaticGameData.SAVE_PLAYER_DATA_KEY))
        {
            HLLogger.Log("Load Date Complete");
            ES3.LoadInto(StaticGameData.SAVE_PLAYER_DATA_KEY, _playerData);
        }
        else
        {
            HLLogger.Log("save date is null. create new playerData");
            _playerData = new PlayerData();
            _playerData.mid = UnityEngine.Random.Range(0, 10000);
            _playerData.name = $"Player{_playerData.mid.ToString("D4")}";

            SavePlayerData();
        }



        return _playerData;
    }

    public OtherPlayerData LoadOtherPlayerData()
    {
        //local data load
        if (ES3.KeyExists(StaticGameData.SAVE_PLAYER_OTHER_DATA_KEY))
        {
            HLLogger.Log("Load Other Date Complete");
            ES3.LoadInto(StaticGameData.SAVE_PLAYER_OTHER_DATA_KEY, _otherPlayerData);
        }
        else
        {
            HLLogger.Log("save date is null. create new playerData");
            _otherPlayerData = new OtherPlayerData();

            SaveOtherPlayerData();
        }

        return _otherPlayerData;
    }


    public void SaveJsonPlayerData(string json)
    {
        var serverData = PlayerDataFromJson(json);
        if (StaticGameData.useServerData)
        {
            HLLogger.Log("Force Use Server PlayerData");
            _playerData = serverData;
            _playerData.serverDataVersion = StaticGameData.serverVersion;

            SavePlayerData();
            return;
        }
        else if (serverData.mid == _playerData.mid &&
            (_playerData.level > serverData.level ||
            (_playerData.level == serverData.level && _playerData.exp > serverData.exp)))
        {
            // 로컬과 서버의 mid가 일치하고, 로컬 데이터의 레벨 또는 exp가 더 높으면 로컬 데이터 유지
            // 로컬 데이터가 더 높으므로 아무것도 하지 않음(로컬 데이터 유지)
            HLLogger.Log("Use Local PlayerData");
        }
        else
        {
            // 그 외의 경우 playFabData를 적용
            HLLogger.Log("Use Server PlayFabData PlayerData");
            _playerData = serverData;
            SavePlayerData();
        }
    }



    public void RemovePlayerData()
    {
        ES3.DeleteKey(StaticGameData.SAVE_PLAYER_DATA_KEY);
        ES3.DeleteKey(StaticGameData.SAVE_PLAYER_OTHER_DATA_KEY);
    }

    public void RemoveLoginData()
    {
        playerData.autoLogin = false;
        playerData.playFabLoginID = "";
        playerData.playFabLoginPW = "";

        SavePlayerData(true);
    }




    public void SaveOtherPlayerData()
    {
        ES3.Save(StaticGameData.SAVE_PLAYER_OTHER_DATA_KEY, _otherPlayerData);
        HLLogger.Log("Save Local Player Data Complete");
    }

    #endregion


}

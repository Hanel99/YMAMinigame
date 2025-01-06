using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SaveDataManager : MonoBehaviour
{
    public static SaveDataManager instance { get; private set; }
    private PlayerData _playerData;
    public PlayerData playerData => _playerData;


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
    }






    #region CardDataLogic



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
#if DevTest
        _playerData.ownCardList.Clear();
        SavePlayerData();
#endif
    }

    public void RemoveNotUseCardList()
    {
        var allCardList = ResourceManager.instance.GetAllCardIds();
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
        var list = ResourceManager.instance.GetCardIds(grade);
        return list.Intersect(_playerData.ownCardList).ToList();
    }

    /// <summary>
    /// 해당 등급의 카드 중 미획득인 카드 id만 반환
    /// </summary>
    /// <param name="grade"></param>
    /// <returns></returns>
    public List<int> GetNotOwnCardList(CardGrade grade)
    {
        var list = ResourceManager.instance.GetCardIds(grade);
        return list.Except(_playerData.ownCardList).ToList();
    }
    public List<int> GetNotOwnCardList()
    {
        var list = ResourceManager.instance.GetAllCardIds();
        return list.Except(_playerData.ownCardList).ToList();
    }
    public List<int> GetNotOwnCardList(List<int> list)
    {
        return list.Except(_playerData.ownCardList).ToList();
    }


    #endregion



    #region playerDataLogic

    public void AddCoin(int amount)
    {
        _playerData.coin += amount;

        if (_playerData.coin > StaticGameData.MAX_COIN_VALUE)
            _playerData.coin = StaticGameData.MAX_COIN_VALUE;
        if (_playerData.coin < 0)
            _playerData.coin = 0;

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

    public bool AddExp(int amount)
    {
        bool isShowLevelUpPopup = false;
        bool needLevelUp = false;
        _playerData.exp += amount;

        HLLogger.Log($"Add {amount} Player Exp -> {_playerData.exp}/{_playerData.maxExp}");

        if (_playerData.maxExp >= 0 && _playerData.exp >= _playerData.maxExp)
            isShowLevelUpPopup = needLevelUp = true;

        while (needLevelUp)
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
        SavePlayerData();
        return isShowLevelUpPopup;
    }




    #endregion







    #region SaveDataInDevice

    public void SavePlayerData()
    {
        _playerData.savedTime = DateTime.Now;
        ES3.Save(StaticGameData.SAVE_PLAYER_DATA_KEY, _playerData);
        HLLogger.Log("Save Date Complete");
    }


    public PlayerData LoadPlayerData()
    {
        if (ES3.KeyExists(StaticGameData.SAVE_PLAYER_DATA_KEY))
        {
            HLLogger.Log("Load Date Complete");
            ES3.LoadInto(StaticGameData.SAVE_PLAYER_DATA_KEY, _playerData);

            StaticGameData.introData.isNewUser = false;
        }
        else
        {
            HLLogger.Log("save date is null. create new playerData");
            _playerData = new PlayerData();
            _playerData.mid = UnityEngine.Random.Range(0, 10000);
            _playerData.name = $"Player{_playerData.mid.ToString("D4")}";

            StaticGameData.introData.isNewUser = true;
            SavePlayerData();
        }

        return _playerData;
    }

    public void RemovePlayerData()
    {
        ES3.DeleteKey(StaticGameData.SAVE_PLAYER_DATA_KEY);
    }

    #endregion

}

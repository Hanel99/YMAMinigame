using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSelectIcon : MonoBehaviour
{
    public Image gameImage;
    public Text gameNameText;

    public Button button;
    public GameObject lockObject;
    public Text unlockText;


    private GameType gameType;


    public void SetGameData(GameType game)
    {
        gameType = game;
        gameImage.sprite = GameResourceManager.instance.GetGameImage(gameType);
        gameNameText.text = LocalizeManager.instance.GetString($"Quest.GameName.{gameType}");

        CheckUnlockContent();
    }

    public void CheckUnlockContent()
    {
        bool gameLock = SaveDataManager.instance.playerData.unlockContent < (int)gameType;

        button.interactable = !gameLock;
        lockObject.SetActive(gameLock);
        unlockText.text = $"Lv.{GameResourceManager.instance.GetContentUnlockLevel((int)gameType)}";
    }


    public void OnClickGameButton()
    {
        LobbyUIManager.instance.ShowGameSelectPopup(gameType);
    }
}

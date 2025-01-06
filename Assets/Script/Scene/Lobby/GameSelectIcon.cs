using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSelectIcon : MonoBehaviour
{
    public Image gameImage;
    public Text tempGameId;


    private GameType gameType;




    public void SetGameData(GameType game)
    {
        gameType = game;

        gameImage.sprite = ResourceManager.instance.GetGameImage(gameType);

        //@@@ temp
        tempGameId.text = StaticGameData.showDevTestText ? gameType.ToString() : "";
    }


    public void OnClickGameButton()
    {
        HLLogger.Log($"@@@ onClick Game {gameType}");

        LobbyUIManager.instance.ShowGameSelectPopup(gameType);
    }

}

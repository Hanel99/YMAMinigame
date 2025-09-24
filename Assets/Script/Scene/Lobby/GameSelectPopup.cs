using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSelectPopup : PopupBase
{
    public static GameSelectPopup instance { get; private set; }

    public Text titleText;
    public Text descText;
    public Button gameStartButton;
    public Image gameImage;
    public GameObject descViewport;


    private GameType gameType;




    protected override void OnAwake()
    {
        instance = this;
    }


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


    public void ShowPopup(GameType type)
    {
        SetGameData(type);
        ShowPopup();

        StartCoroutine(nameof(Co_RefreshDesc));
    }

    public void SetGameData(GameType type)
    {
        gameType = type;
        gameImage.sprite = GameResourceManager.instance.GetGameImage(gameType);

        titleText.text = LocalizeManager.instance.GetString($"game.name.{gameType.ToString()}");
        descText.text = LocalizeManager.instance.GetString($"game.desc.{gameType.ToString()}");
    }

    private IEnumerator Co_RefreshDesc()
    {
        yield return null;

        descViewport.transform.parent.gameObject.SetActive(false);
        descViewport.transform.parent.gameObject.SetActive(true);
    }


    public void OnClickGameStart()
    {
        if (isOpenCloseAnimationActing) return;

        HLLogger.Log($"@@@ GameStart - {gameType}");

        switch (gameType)
        {
            case GameType.MatchCardGame:
                SceneMoveManager.instance.MoveScene(SceneName.YMAMatch2CardGame);
                break;

            case GameType.FindAIWordGame:
                SceneMoveManager.instance.MoveScene(SceneName.YMAFindAIWordGame);
                break;

            case GameType.CubeGame:
                SceneMoveManager.instance.MoveScene(SceneName.YMACubeGame);
                break;

            case GameType.WingTto:
                LobbyUIManager.instance.ShowCommonPopup("알림", "개발중입니다.", true, true, false, null, null);
                // SceneMoveManager.instance.MoveScene(SceneName.YMAWingTto);
                break;

            default:
                HLLogger.Log("@@@ gametype is error");
                break;
        }
    }
}

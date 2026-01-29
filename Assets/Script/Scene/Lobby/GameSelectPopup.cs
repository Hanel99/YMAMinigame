using System.Collections;
using System.Collections.Generic;
using ChocDino.UIFX;
using DG.Tweening;
using UnityEngine;
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
    private LongPressButton gameImageButton;




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
            StaticGameData.useServerSeed = false;
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

        if (type == GameType.CubeGame || type == GameType.WingTto)
        {
            gameImageButton = gameImage.GetComponent<LongPressButton>();
            gameImageButton.SetLongPressTime(3f);
            gameImageButton.SetLongPressAction((isLongPress) =>
            {
                if (StaticGameData.useServerSeed) return;

                StaticGameData.useServerSeed = true;
                HLLogger.Log($"@@@ Long Press! {StaticGameData.useServerSeed}, {StaticGameData.serverGameSeed}");
                SoundManager.instance.PlaySFX(SFXType.TowerStart);

                DOVirtual.Float(0f, 1f, 0.3f, (val) =>
                {
                    gameImage.GetComponent<GlowFilter>().Strength = val;
                });
            });
        }
        else
        {
            gameImageButton = null;
        }
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
                SceneMoveManager.instance.MoveScene(SceneName.YMAWingTto);
                break;

            default:
                HLLogger.Log("@@@ gametype is error");
                break;
        }
    }
}

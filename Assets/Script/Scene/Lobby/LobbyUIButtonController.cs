using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LobbyButton : MonoBehaviour
{
    public Button button;
    public GameObject lockObject;
    public Text unlockText;

    public void SetButtonData(LobbyIconType iconType, UnityAction buttonAction)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(buttonAction);

        var playerData = SaveDataManager.instance.playerData;
        if (iconType == LobbyIconType.FinalQuiz)
        {
            if (playerData.playEndRoll || playerData.finalQuizTryCount > 0
                || playerData.dawnMemberRefer.matchCardGame || playerData.dawnMemberRefer.findAIWordGame
                || playerData.dawnMemberRefer.cubeGame || playerData.dawnMemberRefer.wingTto)
                gameObject.SetActive(true);
            else
            {
                gameObject.SetActive(false);
                return;
            }
        }

        UnlockContent unlockContent = (iconType) switch
        {
            LobbyIconType.Gacha => UnlockContent.MatchCardGame,
            LobbyIconType.Tower => UnlockContent.TowerGame,
            LobbyIconType.Ranking => UnlockContent.WingTto,
            LobbyIconType.Collection => UnlockContent.FindAIWordGame,
            LobbyIconType.Quest => UnlockContent.EveryThing,
            LobbyIconType.FinalQuiz => UnlockContent.EveryThing,

            _ => UnlockContent.MatchCardGame,
        };

        bool isLock = SaveDataManager.instance.playerData.unlockContent < (int)unlockContent;
        lockObject.SetActive(isLock);
        button.interactable = !isLock;
        unlockText.text = $"Lv.{GameResourceManager.instance.GetContentUnlockLevel(unlockContent)}";

    }
}

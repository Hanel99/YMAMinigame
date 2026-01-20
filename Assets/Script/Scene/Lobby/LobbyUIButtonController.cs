using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LobbyButton : MonoBehaviour
{
    public Button button;
    public GameObject lockObject;
    public Text unlockText;
    LobbyIconType iconType;

    public void SetButtonData(LobbyIconType iconType, UnityAction buttonAction)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(buttonAction);
        this.iconType = iconType;

        CheckUnlockContent();
    }

    public void CheckUnlockContent()
    {
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

        if (iconType == LobbyIconType.FinalQuiz)
        {
            var finalQuizPlayData = SaveDataManager.instance.playerData.finalQuizPlayData;
            if (finalQuizPlayData.playEndRoll || finalQuizPlayData.tryCount > 0
                || finalQuizPlayData.matchCardGame >= FinalReferState.Unlocked || finalQuizPlayData.findAIWordGame >= FinalReferState.Unlocked
                || finalQuizPlayData.cubeGame >= FinalReferState.Unlocked || finalQuizPlayData.wingTto >= FinalReferState.Unlocked)
                gameObject.SetActive(true);
            else
            {
                gameObject.SetActive(false);
                return;
            }
        }

        bool isLock = SaveDataManager.instance.playerData.unlockContent < (int)unlockContent;
        lockObject.SetActive(isLock);
        button.interactable = !isLock;
        unlockText.text = $"Lv.{GameResourceManager.instance.GetContentUnlockLevel(unlockContent)}";
    }
}

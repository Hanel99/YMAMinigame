using UnityEngine;

public class FinalReferManager : MonoBehaviour
{
    public static FinalReferManager instance { get; private set; }

    private PlayerData playerData => SaveDataManager.instance.playerData;
    private QuestUserPlayData questUserPlayData => playerData.questUserPlayData;
    private FinalQuizPlayData finalQuizPlayData => playerData.finalQuizPlayData;


    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }



    public bool IsFinalReferUnlock(GameType gameType)
    {
        // 카드맞추기와 단어 맞추기는 획득한 카드와 단어가 33개 이상일 때
        // 큐브게임과 윙또는 플레이한 횟수가 15회 이상일 때

        return gameType switch
        {
            GameType.MatchCardGame => finalQuizPlayData.matchCardGame == FinalReferState.Locked && playerData.ownCardList.Count >= 33,
            GameType.FindAIWordGame => finalQuizPlayData.findAIWordGame == FinalReferState.Locked && playerData.ownWordList.Count >= 33,
            GameType.CubeGame => finalQuizPlayData.cubeGame == FinalReferState.Locked && questUserPlayData.cubeGame.playCount >= 15,
            GameType.WingTto => finalQuizPlayData.wingTto == FinalReferState.Locked && questUserPlayData.wingTto.playCount >= 15,
            _ => false
        };
    }

    public bool IsFinalReferStateUnlocked(GameType gameType)
    {
        return gameType switch
        {
            GameType.MatchCardGame => finalQuizPlayData.matchCardGame == FinalReferState.Unlocked,
            GameType.FindAIWordGame => finalQuizPlayData.findAIWordGame == FinalReferState.Unlocked,
            GameType.CubeGame => finalQuizPlayData.cubeGame == FinalReferState.Unlocked,
            GameType.WingTto => finalQuizPlayData.wingTto == FinalReferState.Unlocked,
            _ => false
        };
    }

    public FinalReferState GetFinalReferState(GameType gameType)
    {
        return gameType switch
        {
            GameType.MatchCardGame => finalQuizPlayData.matchCardGame,
            GameType.FindAIWordGame => finalQuizPlayData.findAIWordGame,
            GameType.CubeGame => finalQuizPlayData.cubeGame,
            GameType.WingTto => finalQuizPlayData.wingTto,
            _ => FinalReferState.Locked
        };
    }
}

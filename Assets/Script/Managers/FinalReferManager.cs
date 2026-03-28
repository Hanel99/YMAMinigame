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
        // 모든 컨텐츠 언락이 되지 않았거나 엔딩을 봤으면 이 메소드는 작동하지 않음
        if (SaveDataManager.instance.playerData.finalQuizPlayData.playEndRoll || SaveDataManager.instance.playerData.unlockContent < (int)UnlockContent.EveryThing) return false;

        // 1.0.0 기준
        // 카드맞추기와 단어 맞추기는 획득한 카드와 단어가 33개 이상일 때
        // 큐브게임과 윙또는 플레이한 횟수가 15회 이상일 때
        // return gameType switch
        // {
        //     GameType.MatchCardGame => finalQuizPlayData.matchCardGame == FinalReferState.Locked && playerData.ownCardList.Count >= 33,
        //     GameType.FindAIWordGame => finalQuizPlayData.findAIWordGame == FinalReferState.Locked && playerData.ownWordList.Count >= 33,
        //     GameType.CubeGame => finalQuizPlayData.cubeGame == FinalReferState.Locked && questUserPlayData.cubeGame.playCount >= 15,
        //     GameType.WingTto => finalQuizPlayData.wingTto == FinalReferState.Locked && questUserPlayData.wingTto.playCount >= 15,
        //     _ => false
        // };

        // 1.1.0 난이도 완화
        // 동의서 발견 조건을 게임 플레이 3회로 통일
        return gameType switch
        {
            GameType.MatchCardGame => finalQuizPlayData.matchCardGame == FinalReferState.Locked && questUserPlayData.matchCardGame.playCount >= 3,
            GameType.FindAIWordGame => finalQuizPlayData.findAIWordGame == FinalReferState.Locked && questUserPlayData.findAIWordGame.playCount >= 3,
            GameType.CubeGame => finalQuizPlayData.cubeGame == FinalReferState.Locked && questUserPlayData.cubeGame.playCount >= 3,
            GameType.WingTto => finalQuizPlayData.wingTto == FinalReferState.Locked && questUserPlayData.wingTto.playCount >= 3,
            _ => false
        };
    }

    public bool IsFinalReferStateUnlocked(GameType gameType)
    {
        // 모든 컨텐츠 언락이 되지 않았거나 엔딩을 봤으면 이 메소드는 작동하지 않음
        if (SaveDataManager.instance.playerData.finalQuizPlayData.playEndRoll || SaveDataManager.instance.playerData.unlockContent < (int)UnlockContent.EveryThing) return false;

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

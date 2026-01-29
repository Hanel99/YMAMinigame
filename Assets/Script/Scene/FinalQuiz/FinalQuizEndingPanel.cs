using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FinalQuizEndingPanel : MonoBehaviour
{
    public Image ymaLogo;
    public Text endingTitle;
    public Text endingCredit;


    public void InitUIObject()
    {
        ymaLogo.gameObject.SetActive(false);
        endingTitle.gameObject.SetActive(false);
        endingCredit.gameObject.SetActive(false);
    }



    public async UniTask ShowEnding()
    {
        // 엔딩 크레딧. 총 61초 사용
        SoundManager.instance.PlayBGM(BGMType.Ending);

        // 하얀 배경화면에 로고 먼저 페이드 인
        ymaLogo.gameObject.SetActive(true);
        ymaLogo.DOFade(1, 1f).From(0).SetEase(Ease.Linear);
        await UniTask.Delay(1000);

        // 엔딩 크레딧 텍스트 페이드 인
        endingTitle.gameObject.SetActive(true);
        endingTitle.DOFade(1, 1f).From(0).SetEase(Ease.Linear);
        await UniTask.Delay(1000);

        endingCredit.gameObject.SetActive(true);
        endingCredit.DOFade(1, 1f).From(0).SetEase(Ease.Linear);
        await UniTask.Delay(1000);

        // 두개 다 페이드아웃되고 게임 정보 노출
        Sequence seq = DOTween.Sequence();
        seq.Append(ymaLogo.DOFade(0, 1f).From(1).SetEase(Ease.Linear));
        seq.Join(endingCredit.DOFade(0, 1f).From(1).SetEase(Ease.Linear));
        await seq.Play().AsyncWaitForCompletion();

        // 카드 맞추기 게임 정보
        string matchCardText = $"<size=70>[ 짝 맞추기 게임 ]</size>\n\n" +
            $"플레이 횟수 : {QuestManager.instance.GetQuestValue(QuestDetailType.PlayMatchCardGame, QuestDetailType2.None):N0} 회\n" +
            $"능력자 인증 횟수 : {QuestManager.instance.GetQuestValue(QuestDetailType.FinishCardGameUnderCount, QuestDetailType2.None):N0} 회";
        await ShowCreditSection(matchCardText, 6f);


        // 단어 맞추기 게임 정보
        string wordGameText = $"<size=70>[ AI 그림 단어 맞추기 ]</size>\n\n" +
            $"플레이 횟수 : {QuestManager.instance.GetQuestValue(QuestDetailType.PlayFindAIWordGame, QuestDetailType2.None):N0} 회\n" +
            $"능력자 인증 횟수 : {QuestManager.instance.GetQuestValue(QuestDetailType.FinishWordGameUnderCount, QuestDetailType2.None):N0} 회";
        await ShowCreditSection(wordGameText, 6f);


        // 큐브 게임 게임 정보
        string cubeGameText = $"<size=70>[ 리듬 큐브 ]</size>\n\n" +
            $"플레이 횟수 : {QuestManager.instance.GetQuestValue(QuestDetailType.PlayCubeGame, QuestDetailType2.None):N0} 회\n" +
            $"최고 점수 : {QuestManager.instance.GetQuestValue(QuestDetailType.ReachCubeScore, QuestDetailType2.None):N0} 점\n" +
            $"퍼펙트 횟수 : {QuestManager.instance.GetQuestValue(QuestDetailType.ReachCubeStateTouchCount, QuestDetailType2.CubeState_Perfect):N0} 회";
        await ShowCreditSection(cubeGameText, 6f);


        // 윙또 게임 정보
        string wingTtoText = $"<size=70>[ 달려라 윙또 ]</size>\n\n" +
            $"플레이 횟수 : {QuestManager.instance.GetQuestValue(QuestDetailType.PlayWingTto, QuestDetailType2.None):N0} 회\n" +
            $"최장 비행 거리 : {QuestManager.instance.GetQuestValue(QuestDetailType.ReachWingTtoDistance, QuestDetailType2.None):N0} M\n" +
            $"김밥 획득 : {QuestManager.instance.GetQuestValue(QuestDetailType.CollectWingTtoItem, QuestDetailType2.WingTtoItem_Gimbab):N0} 개";
        await ShowCreditSection(wingTtoText, 6f);


        // 타워 게임 정보
        string towerGameText = $"<size=70>[ 타워 오르기 ]</size>\n\n" +
            $"무기 강화 레벨 : {QuestManager.instance.GetQuestValue(QuestDetailType.ReachWeaponLevel, QuestDetailType2.None):N0} 강\n" +
            $"최고 도달 층수 : {QuestManager.instance.GetQuestValue(QuestDetailType.ReachTowerFloor, QuestDetailType2.None):N0} 층\n" +
            $"회피 승리 횟수 : {QuestManager.instance.GetQuestValue(QuestDetailType.S_WinWithAvoid, QuestDetailType2.None):N0} 회";
        await ShowCreditSection(towerGameText, 6f);


        // 파이널 퀴즈 게임 정보
        var finalData = SaveDataManager.instance.playerData.finalQuizPlayData;
        string finalQuizText = $"<size=70>[ 연모아 입부 테스트 ]</size>\n\n" +
            $"최종 도달 단계 : {finalData.enterQuizIndex + 1} 단계\n" +
            $"짝 맞추기 해금 : {finalData.matchCardGame}\n" +
            $"AI 단어 맞추기 해금 : {finalData.findAIWordGame}\n" +
            $"리듬 큐브 해금 : {finalData.cubeGame}\n" +
            $"달려라 윙또 해금 : {finalData.wingTto}";

        await ShowCreditSection(finalQuizText, 6f);


        // 기타 계정 정보(레벨, 콜렉션, 퀘스트, 사용 코인 등)
        string accountText = $"<size=70>[ 플레이어 기록 ]</size>\n\n" +
            $"레벨 : {SaveDataManager.instance.playerData.level}\n" +
            $"획득 경험치 : {SaveDataManager.instance.playerData.exp:N0}\n" +
            $"사용 코인 : {QuestManager.instance.GetQuestValue(QuestDetailType.UseCoin, QuestDetailType2.None):N0}\n" +
            $"수집 카드 : {QuestManager.instance.GetQuestValue(QuestDetailType.CollectCard, QuestDetailType2.None):N0} 개";
        await ShowCreditSection(accountText, 6f);


        // 개발자 소개
        // 나, 





        // 결과 화면 연출 시작
        SoundManager.instance.StopBGM();
        FinalQuizUIManager.instance.ShowCompleteAnimation();

        await UniTask.Delay(100);
        this.gameObject.SetActive(false);
    }

    private async UniTask ShowCreditSection(string text, float duration)
    {
        endingCredit.text = text;
        endingCredit.DOFade(1f, 1f).From(0f).SetEase(Ease.Linear);
        await UniTask.Delay(TimeSpan.FromSeconds(duration - 2f)); // 나타나고 1초, 사라지고 1초를 뺀 시간만큼 대기
        endingCredit.DOFade(0f, 1f).From(1f).SetEase(Ease.Linear);
        await UniTask.Delay(1000);
    }
}

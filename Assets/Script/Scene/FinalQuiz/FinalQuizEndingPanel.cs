using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FinalQuizEndingPanel : MonoBehaviour
{
    public CanvasGroup ymaLogo;
    public Text endingTitle;
    public Text[] endingCredits;
    public Text centerText;

    private int currentCreditIndex = 0;

    public void InitUIObject()
    {
        ymaLogo.gameObject.SetActive(false);
        endingTitle.gameObject.SetActive(false);
        foreach (var credit in endingCredits)
            credit.gameObject.SetActive(false);
        centerText.gameObject.SetActive(false);

        currentCreditIndex = 0;
    }

    public async UniTask ShowEnding()
    {
        // 엔딩 크레딧. 총 61초 사용
        SoundManager.instance.PlaySFX(SFXType.Ending);
        await UniTask.Delay(500);

        // 하얀 배경화면에 로고 먼저 페이드 인
        ymaLogo.gameObject.SetActive(true);
        ymaLogo.DOFade(1, 1f).From(0).SetEase(Ease.Linear);
        await UniTask.Delay(2000);

        // 엔딩 크레딧 텍스트 페이드 인
        endingTitle.gameObject.SetActive(true);
        endingTitle.DOFade(1, 1f).From(0).SetEase(Ease.Linear);
        await UniTask.Delay(4000);

        // 두개 다 페이드아웃
        Sequence seq = DOTween.Sequence();
        seq.Append(ymaLogo.DOFade(0, 1f).From(1).SetEase(Ease.Linear));
        seq.Join(endingTitle.DOFade(0, 1f).From(1).SetEase(Ease.Linear));
        await seq.Play().AsyncWaitForCompletion();

        ymaLogo.gameObject.SetActive(false);
        endingTitle.gameObject.SetActive(false);

        // 데이터 캐싱 후 게임 플레이 내역 노출
        var playerData = SaveDataManager.instance.playerData;
        var questData = playerData.questUserPlayData;
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // 카드 맞추기 게임 정보
        sb.Clear();
        sb.Append("<size=60>[ 카드 맞추기 ]</size>\n\n");
        sb.Append($"플레이 횟수 : {questData.matchCardGame.playCount:N0} 회\n");
        sb.Append($"획득한 카드 갯수 : {playerData.ownCardList.Count:N0} 장");
        await ShowCreditSection(sb.ToString(), 5f);


        // 단어 맞추기 게임 정보
        sb.Clear();
        sb.Append("<size=60>[ 단어 맞추기 ]</size>\n\n");
        sb.Append($"플레이 횟수 : {questData.findAIWordGame.playCount:N0} 회\n");
        sb.Append($"발견한 단어 수 : {playerData.ownWordList.Count:N0} 개");
        await ShowCreditSection(sb.ToString(), 5f);


        // 큐브 게임 게임 정보
        sb.Clear();
        sb.Append("<size=60>[ 큐브 게임 ]</size>\n\n");
        sb.Append($"플레이 횟수 : {questData.cubeGame.playCount:N0} 회\n");
        sb.Append($"최고 점수 : {playerData.cubeGameHighScore:N0} 점\n");
        sb.Append($"퍼펙트 횟수 : {questData.cubeGame.touchCount_Perfect:N0} 회\n");
        sb.Append($"총 터치 횟수 : {questData.cubeGame.totalTouchCount:N0} 회");
        await ShowCreditSection(sb.ToString(), 5f);


        // 윙또 게임 정보
        sb.Clear();
        sb.Append("<size=60>[ 윙또 ]</size>\n\n");
        sb.Append($"플레이 횟수 : {questData.wingTto.playCount:N0} 회\n");
        sb.Append($"최장 비행 거리 : {playerData.wingTtoHighScore:N0}m\n");
        sb.Append($"삼김 먹은 횟수 : {questData.wingTto.collectCount_Gimbab:N0} 개\n");
        sb.Append($"벽에 부딪힌 횟수 : {questData.wingTto.crashCount:N0} 회");
        await ShowCreditSection(sb.ToString(), 5f);


        // 타워 게임 정보
        sb.Clear();
        sb.Append("<size=60>[ 천하제일 꿀밤대회 ]</size>\n\n");
        sb.Append($"최고 도달 층수 : {playerData.towerFloor:N0} 층\n");
        sb.Append($"무기 강화 레벨 : {playerData.towerGameUserWeaponData.weaponLevel:N0} 강\n");
        sb.Append($"무기 강화 시도 횟수 : {questData.towerGame.playCount:N0} 회\n");
        sb.Append($"무기가 터진 횟수 : {questData.towerGame.weaponEnchantCount_Down:N0} 회");
        await ShowCreditSection(sb.ToString(), 5f);


        // 파이널 퀴즈 게임 정보
        var finalData = playerData.finalQuizPlayData;
        sb.Clear();
        sb.Append("<size=60>[ 연모아 새벽반 입부 시험 ]</size>\n\n");
        sb.Append($"시험 본 횟수 : {finalData.tryCount:N0} 회\n");
        sb.Append($"맞춘 문제 수 : {finalData.OCount:N0} 개\n");
        sb.Append($"틀린 문제 수 : {finalData.XCount:N0} 개");
        await ShowCreditSection(sb.ToString(), 5f);


        // 기타 계정 정보(레벨, 콜렉션, 퀘스트, 사용 코인 등)
        sb.Clear();
        sb.Append("<size=60>[ 플레이어 기록 ]</size>\n\n");
        sb.Append($"레벨 : {playerData.level} +({playerData.exp}exp)\n");
        sb.Append($"총 사용한 코인 : {questData.common.useCoin:N0} 개\n");
        sb.Append($"가챠 돌린 횟수 : {questData.gacha.playCount:N0} 회");
        await ShowCreditSection(sb.ToString(), 5f);
        await FadeOutAllCredit();

        // 개발자 소개
        sb.Clear();
        sb.Append("<size=60>[ YMA MINI GAME ]</size>\n\n");
        sb.Append($"기획 : Hanel\n");
        sb.Append($"프로그래밍 : Hanel\n");
        sb.Append($"리소스 제작 : Hanel + AI\n");
        sb.Append($"YMA Bot 제작 : Hanel\n");
        sb.Append($"QA : Hanel\n");
        sb.Append($"기타 등등 : Hanel");
        await ShowCenterCredit(sb.ToString(), 5f);

        sb.Clear();
        sb.Append("<size=60>[ Special Thanks ]</size>\n\n");
        sb.Append($"노래 만들어준 가슬이\n");
        sb.Append($"그림 그려준 삼사, 멍개, 만타, 니모 등등\n");
        sb.Append($"게임 테스트해준 연모아 매니저들\n");
        sb.Append($"모두모두 고맙습니다.");
        await ShowCenterCredit(sb.ToString(), 5f);


        sb.Clear();
        sb.Append("플레이 해주셔서 감사합니다.\n");
        sb.Append("Thank you for playing.");
        await ShowCenterCredit(sb.ToString(), 8f, 2f);


        // 결과 화면 연출 시작
        SoundManager.instance.StopBGM();
        FinalQuizUIManager.instance.ShowCompleteAnimation();

        await UniTask.Delay(100);
        this.gameObject.SetActive(false);
    }

    private async UniTask ShowCreditSection(string text, float duration)
    {
        if (endingCredits == null || endingCredits.Length == 0) return;

        // 다음 인덱스 계산 (순차 순회)
        int nextIndex = (currentCreditIndex + 1) % endingCredits.Length;
        int prevIndex = currentCreditIndex;

        // 교차 페이드 실행
        FadeInCredit(endingCredits[nextIndex], text);
        // if (currentCreditIndex > 0)
        FadeOutCredit(endingCredits[prevIndex]);

        // 인덱스 업데이트
        currentCreditIndex = nextIndex;

        // duration 만큼 대기 (1초 페이드 시간은 duration 내에 포함됨)
        await UniTask.Delay(TimeSpan.FromSeconds(duration));
    }


    private async UniTask FadeOutAllCredit()
    {
        foreach (var credit in endingCredits)
        {
            if (!credit.gameObject.activeSelf) continue;
            credit.DOFade(0f, 1f).SetEase(Ease.Linear);
        }
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
    }

    private async UniTask ShowCenterCredit(string text, float duration, float fadeDuration = 1f)
    {
        centerText.text = text;
        centerText.gameObject.SetActive(true);
        centerText.DOFade(1f, fadeDuration).From(0f).SetEase(Ease.Linear);
        await UniTask.Delay(TimeSpan.FromSeconds(duration - fadeDuration));

        centerText.DOFade(0f, fadeDuration).From(1f).SetEase(Ease.Linear);
        await UniTask.Delay(TimeSpan.FromSeconds(fadeDuration));
    }

    private void FadeInCredit(Text target, string text, float duration = 1f)
    {
        target.text = text;
        target.gameObject.SetActive(true);
        target.DOFade(1f, duration).From(0f).SetEase(Ease.Linear);
    }

    private void FadeOutCredit(Text target, float duration = 1f)
    {
        if (!target.gameObject.activeSelf) return;

        target.DOFade(0f, duration).From(1f).SetEase(Ease.Linear).OnComplete(() =>
        {
            target.gameObject.SetActive(false);
        });
    }
}

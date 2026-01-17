using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FinalQuizEndingPanel : MonoBehaviour
{
    public Image ymaLogo;
    public Text endingCredit;


    public void InitUIObject()
    {
        ymaLogo.gameObject.SetActive(false);
        endingCredit.gameObject.SetActive(false);
    }



    public async UniTask ShowEnding()
    {
        // 엔딩 크레딧
        // 하얀 배경화면에 로고 먼저 페이드 인
        // 엔딩 크레딧 텍스트 페이드 인
        // 두개 다 페이드아웃되고 아래에서 위로 스크립트 스크롤
        // 스크린 위치 무시하고 9:18에 맞춰 구현
        // 그 아래에서 페이드인 후 알파가 1이 된 시점엔 화면에 보이게
        // 화면 밖으로 넘어가고 페이드아웃
        // 결과 화면 연출 시작(합격)

        await UniTask.Delay(1000);

        SoundManager.instance.PlayBGM(BGMType.Ending);
        ymaLogo.gameObject.SetActive(true);
        ymaLogo.DOFade(1, 1f).From(0).SetEase(Ease.Linear);





        await UniTask.Delay(1000);
        SoundManager.instance.StopBGM();
        FinalQuizUIManager.instance.ShowCompleteAnimation();
    }
}

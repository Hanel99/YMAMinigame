using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CubeGameCube : MonoBehaviour
{
    public Image image;
    public Text devText;
    public Text keyText;
    public KeyCode keyCode;
    public Image guideBarL;
    public Image guideBarR;
    public Image guideBarU;
    public Image guideBarD;

    private CubeState _state;
    public CubeState state => _state;


    //private
    private float waitTime = 1f;
    private float tooFastTime = 1f;
    private float fastTime = 1f;
    private float perfectTime = 1f;
    private float slowTime = 1f;
    private float tooSlowTime = 1f;
    private float barMoveTime = 0f;
    private CancellationTokenSource cts;
    private Sequence barSequence;



    public void SetKeyCode(KeyCode code)
    {
        guideBarL.gameObject.SetActive(false);
        guideBarR.gameObject.SetActive(false);
        guideBarU.gameObject.SetActive(false);
        guideBarD.gameObject.SetActive(false);

        keyText.text = string.Empty;

        if (devText != null)
            devText.text = "";

#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        keyCode = code;
        keyText.text = code.ToString();
#endif
    }

    private void Update()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
        if (CubeGameManager.instance.inGameState == InGameState.Play)
        {
            if (Input.GetKeyDown(keyCode))
            {
                OnClickCube();
            }
        }
#endif
    }

    private void OnDestroy()
    {
        StopCube();

        barSequence?.Kill(true);
        barSequence = null;
    }


    public void StartCube(float wait, float tooFast, float fast, float perfect, float slow, float tooSlow)
    {
        // 이전 실행 중인 토큰 취소
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();

        waitTime = wait;
        tooFastTime = tooFast;
        fastTime = fast;
        perfectTime = perfect;
        slowTime = slow;
        tooSlowTime = tooSlow;

        barMoveTime = tooFastTime + fastTime;

        RunStateMachine(cts.Token).Forget();
    }

    public void StopCube()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;

        barSequence?.Kill();
        barSequence = null;

        guideBarL.gameObject.SetActive(false);
        guideBarR.gameObject.SetActive(false);
        guideBarU.gameObject.SetActive(false);
        guideBarD.gameObject.SetActive(false);

        _state = CubeState.Wait;
        ApplyColor(_state);
    }

    private async UniTaskVoid RunStateMachine(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && CubeGameManager.instance?.inGameState == InGameState.Play && this != null)
            {
                // #if DEV
                barSequence?.Kill();
                barSequence = DOTween.Sequence();

                guideBarL.gameObject.SetActive(true);
                guideBarR.gameObject.SetActive(true);
                guideBarU.gameObject.SetActive(true);
                guideBarD.gameObject.SetActive(true);

                guideBarU.transform.localPosition = new Vector3(guideBarU.transform.localPosition.x, -100f, guideBarU.transform.localPosition.z);
                guideBarD.transform.localPosition = new Vector3(guideBarD.transform.localPosition.x, 100f, guideBarD.transform.localPosition.z);

                barSequence.AppendInterval(waitTime);

                // L과 R이 좌우로 닫히는 연출
                barSequence.Append(guideBarL.transform.DOLocalMoveX(0, barMoveTime).SetEase(Ease.InQuad).From(-100f))
                        .Join(guideBarR.transform.DOLocalMoveX(0, barMoveTime).SetEase(Ease.InQuad).From(100f));

                // L과 R 연출이 끝난 뒤 U와 D가 위아래로 움직이는 연출
                barSequence.Append(guideBarU.transform.DOLocalMoveY(0, perfectTime).SetEase(Ease.InQuad).From(-100f))
                        .Join(guideBarD.transform.DOLocalMoveY(0, perfectTime).SetEase(Ease.InQuad).From(100f));

                barSequence.Play();
                // #endif


                // 한 사이클 실행
                bool cycleCompleted = await RunSingleCycle(token);
                if (!cycleCompleted) break; // 사이클이 취소된 경우(클릭 등)

                // Miss까지 완료된 경우 (클릭하지 않음)
                CubeGameManager.instance.MissProcess();
                // 자동 재시작 
            }
        }
        catch (OperationCanceledException)
        {
            HLLogger.Log($"@@@ RunStateMachine 취소됨: {gameObject.name}");
        }
    }

    private async UniTask<bool> RunSingleCycle(CancellationToken token)
    {
        try
        {
            await SetState(CubeState.Wait, waitTime, token);
            await SetState(CubeState.TooFast, tooFastTime, token);
            await SetState(CubeState.Fast, fastTime, token);
            await SetState(CubeState.Perfect, perfectTime, token);
            await SetState(CubeState.Slow, slowTime, token);
            await SetState(CubeState.TooSlow, tooSlowTime, token);

            return true; // 사이클 완료
        }
        catch (OperationCanceledException)
        {
            return false; // 사이클 취소됨
        }
    }

    // Colors
    private static readonly Color COLOR_WAIT = new Color(1f, 1f, 1f, 0.3f);
    private static readonly Color32 COLOR_TOO_FAST = new Color32(94, 19, 19, 255);
    private static readonly Color32 COLOR_FAST = new Color32(198, 40, 40, 255);
    private static readonly Color32 COLOR_PERFECT = new Color32(0, 230, 118, 255);
    private static readonly Color32 COLOR_SLOW = new Color32(25, 118, 210, 255);
    private static readonly Color32 COLOR_TOO_SLOW = new Color32(11, 53, 118, 255);


    // ... (Omitted fields)

    private async UniTask SetState(CubeState newState, float duration, CancellationToken token)
    {
        if (this == null || gameObject == null) return;

        _state = newState;
        ApplyColor(_state);

        try
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();

                // 게임이 일시정지 상태인지 체크
                if (CubeGameManager.instance.inGameState == InGameState.Pause)
                {
                    // #if DEV
                    // 일시정지 중에는 가이드라인(DOTween)도 멈춤
                    if (barSequence != null && barSequence.IsActive() && barSequence.IsPlaying())
                        barSequence.Pause();

                    if (devText != null)
                        devText.text = $"{newState}\n{elapsed:F2}\n{duration:F2}  [PAUSED]";
                    // #endif

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                    continue;
                }

                // #if DEV
                // 일시정지 해제 시 다시 재생
                if (barSequence != null && barSequence.IsActive() && !barSequence.IsPlaying())
                    barSequence.Play();
                // #endif

                elapsed += Time.deltaTime;

#if DEV
                if (devText != null)
                    devText.text = $"{newState}\n{elapsed:F2}\n{duration:F2}";
#endif

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        }
        catch (OperationCanceledException)
        {
            throw; // 상위로 예외 전파
        }
    }


    private void ApplyColor(CubeState state)
    {
        if (image == null) return;

        switch (state)
        {
            case CubeState.Wait:
                image.color = COLOR_WAIT;
                break;

            case CubeState.TooFast:
                image.color = COLOR_TOO_FAST;
                break;

            case CubeState.Fast:
                image.color = COLOR_FAST;
                break;

            case CubeState.Perfect:
                image.color = COLOR_PERFECT;
                break;

            case CubeState.Slow:
                image.color = COLOR_SLOW;
                break;

            case CubeState.TooSlow:
                image.color = COLOR_TOO_SLOW;
                break;
        }

    }

    public void OnClickCube()
    {
        // 큐브 터치 연출: 살짝 작아졌다 커짐
        transform.DOKill();
        transform.DOScale(0.95f, 0.05f).OnComplete(() => transform.DOScale(1f, 0.05f));

        // 조건 수정: Idle 상태에서는 클릭 무시
        if (_state == CubeState.Wait)
            return;

        // 판정에 따른 사운드 시스템 (주석 처리된 곳을 추후 수정하여 사용)
        PlayJudgeSound(_state);

        // 게임 매니저에 클릭 처리 요청
        CubeGameManager.instance.CubeClickProcess(this, _state);
        HLLogger.Log($"{this.gameObject.name} 클릭됨: {_state} / {perfectTime} - {fastTime} - {tooFastTime}");

        // 현재 사이클 재시작
        RestartCycle();
    }

    private void PlayJudgeSound(CubeState state)
    {
        switch (state)
        {
            case CubeState.Perfect:
                SoundManager.instance.PlaySFX(SFXType.CubePerfect);
                break;
            case CubeState.Fast:
            case CubeState.Slow:
                SoundManager.instance.PlaySFX(SFXType.CubeGreat);
                break;
            case CubeState.TooFast:
            case CubeState.TooSlow:
                SoundManager.instance.PlaySFX(SFXType.CubeBad);
                break;
        }
    }

    private void RestartCycle()
    {
        // 현재 사이클 취소
        cts?.Cancel();
        cts?.Dispose();

        // 새로운 사이클 시작
        cts = new CancellationTokenSource();
        RunStateMachine(cts.Token).Forget();
    }


}

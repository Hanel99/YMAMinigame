using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CubeGameCube : MonoBehaviour
{
    public Image image;
    public Text devText;
    public Text keyText;
    public KeyCode keyCode;

    private CubeState _state;
    public CubeState state => _state;


    //private
    private float idleTime = 1f;
    private float goodTime = 1f;
    private float greatTime = 1f;
    private float perfectTime = 1f;
    private float badTime = 1f;
    private float missTime = 1f;
    private CancellationTokenSource cts;



    public void SetKeyCode(KeyCode code)
    {
        keyText.text = string.Empty;
#if UNITY_STANDALONE_WIN
        keyCode = code;
        keyText.text = code.ToString();
#endif
    }

    private void Update()
    {
#if UNITY_STANDALONE_WIN
        if (CubeGameManager.instance.inGameState == InGameState.Play)
        {
            if (Input.GetKeyDown(keyCode))
            {
                OnClickCube();
            }
        }
#endif
    }


    public void StartCube(float idle, float good, float great, float perfect, float bad, float miss)
    {
        // 이전 실행 중인 토큰 취소
        cts?.Cancel();
        cts?.Dispose();
        cts = new CancellationTokenSource();

        idleTime = idle;
        goodTime = good;
        greatTime = great;
        perfectTime = perfect;
        badTime = bad;
        missTime = miss;

        RunStateMachine(cts.Token).Forget();
    }

    public void StopCube()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
        _state = CubeState.Idle;
        ApplyColor(_state);
    }

    private async UniTaskVoid RunStateMachine(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && CubeGameManager.instance.inGameState == InGameState.Play)
            {
                // 한 사이클 실행
                bool cycleCompleted = await RunSingleCycle(token);

                if (!cycleCompleted) break; // 사이클이 취소된 경우

                // Miss까지 완료된 경우 3초 대기 후 재시작
                HLLogger.Log("@@@ miss lost...");
                await UniTask.Delay(3000, cancellationToken: token);

                HLLogger.Log("@@@ reStart");
                // 자동 재시작 (OnClickCube 호출 제거)
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
            await SetState(CubeState.Idle, idleTime, token);
            await SetState(CubeState.Good, goodTime, token);
            await SetState(CubeState.Great, greatTime, token);
            await SetState(CubeState.Perfect, perfectTime, token);
            await SetState(CubeState.Bad, badTime, token);
            await SetState(CubeState.Miss, missTime, token);

            return true; // 사이클 완료
        }
        catch (OperationCanceledException)
        {
            return false; // 사이클 취소됨
        }
    }

    private async UniTask SetState(CubeState newState, float duration, CancellationToken token)
    {
        _state = newState;
        ApplyColor(_state);
        Debug.Log($"상태 변경: {newState} (유지 {duration}초)");

        try
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested();

                // 게임이 일시정지 상태인지 체크
                if (CubeGameManager.instance.inGameState == InGameState.Pause)
                {
#if DEV
                    // 일시정지 중에는 시간을 멈춤
                    if (devText != null)
                        devText.text = $"{newState}\n{elapsed:F2}\n{duration:F2}  [PAUSED]";
#endif

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                    continue;
                }

                elapsed += Time.deltaTime;

#if DEV
                if (devText != null)
                    devText.text = $"{newState}\n{elapsed:F2}\n{duration:F2}";
#endif

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            HLLogger.Log($"@@@ Finish {gameObject.name} State : {_state}");
        }
        catch (OperationCanceledException)
        {
            HLLogger.Log($"@@@ SetState 취소됨 : {gameObject.name}, State={_state}");
            throw; // 상위로 예외 전파
        }
    }


    private void ApplyColor(CubeState state)
    {
        if (image == null) return;

        switch (state)
        {
            case CubeState.Idle:
                image.color = new Color(1f, 1f, 1f, 0.3f); // 흰색 반투명
                break;
            case CubeState.Good:
                image.color = new Color(1f, 0.5f, 0f, 1f); // 주황
                break;
            case CubeState.Great:
                image.color = Color.green; // 초록
                break;
            case CubeState.Perfect:
                image.color = Color.blue; // 파랑
                break;
            case CubeState.Bad:
                image.color = new Color(1f, 0.4f, 0.7f, 1f); // 분홍
                break;
            case CubeState.Miss:
                image.color = Color.red; // 빨강
                break;
        }
    }

    public void OnClickCube()
    {
        // 조건 수정: Idle 상태에서는 클릭 무시
        if (_state == CubeState.Idle)
            return;

        HLLogger.Log($"@@@ OnClickCube - Current State: {_state}");

        // 게임 매니저에 클릭 처리 요청
        CubeGameManager.instance.CubeClickProcess(this, _state);

        // 현재 사이클 재시작
        RestartCycle();
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

    // 현재 상태 반환 (외부에서 상태 확인용)
    public CubeState GetCurrentState() => _state;

    // 큐브가 활성 상태인지 확인
    public bool IsActive() => cts != null && !cts.Token.IsCancellationRequested;

    private void OnDestroy()
    {
        StopCube();
    }


}

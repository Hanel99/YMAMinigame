using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class CubeGameCube : MonoBehaviour
{
    public Image image;
    public Text devText;

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

        HLLogger.Log("startCube");
        RunStateMachine(cts.Token).Forget();
    }

    public void StopCube()
    {
        cts?.Cancel();
        cts?.Dispose();
        cts = null;
    }

    private async UniTaskVoid RunStateMachine(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await SetState(CubeState.Idle, idleTime, token);
            await SetState(CubeState.Good, goodTime, token);
            await SetState(CubeState.Great, greatTime, token);
            await SetState(CubeState.Perfect, perfectTime, token);
            await SetState(CubeState.Bad, badTime, token);
            await SetState(CubeState.Miss, missTime, token);

            HLLogger.Log("@@@ miss lost");
            await UniTask.Delay(3000);
            OnClickCube();
        }
    }

    private async UniTask SetState(CubeState newState, float duration, CancellationToken token)
    {
        _state = newState;
        ApplyColor(_state);
        Debug.Log($"상태 변경: {newState} (유지 {duration}초)");

        float elapsed = 0f;
        try
        {
            while (elapsed < duration)
            {
                token.ThrowIfCancellationRequested(); // 토큰 취소 체크
                elapsed += Time.deltaTime;

                if (devText != null)
                    devText.text = $"{newState}\n{elapsed:F2}\n{duration:F2}";

                await UniTask.Yield(PlayerLoopTiming.Update, token); // 매 프레임 대기
            }

            HLLogger.Log($"@@@ Finish {gameObject.name} State : {_state}");
        }
        catch (OperationCanceledException)
        {
            HLLogger.Log($"@@@ SetState 취소됨 : {gameObject.name}, State={_state}");
        }
    }


    private void ApplyColor(CubeState state)
    {
        // 연출용 이미지로 바꿀 예정
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
        if (_state >= CubeState.Idle)
            return;

        HLLogger.Log("@@@ OnClickCube");
        // StopCube();
        CubeGameManager.instance.CubeClickProcess(this, _state);
    }


}

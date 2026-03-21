using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class WingTtoFlyer : MonoBehaviour
{
    private float speedRatio = 0.2f;
    private float speedUpMultiplier = 1.5f;
    private float despawnLeftX = -15f;
    private float despawnRightX = 15f;
    private float smoothFactor = 6f;

    private float verticalSpeed = 1.2f;
    private float verticalCheckInterval = 3f;
    private int verticalChance = 60;
    private float verticalDurationMin = 0.8f;
    private float verticalDurationMax = 2f;
    private float yMin = -2f;
    private float yMax = 3f;

    public SpriteRenderer iconRenderer;

    private bool isMoving = false;
    private float currentSpeed = 0f;
    private float verticalDir = 0f;
    private float verticalTimer = 0f;

    public CardMaster AssignedMaster { get; private set; } = CardMaster.Count;

    // 크래시 모드 플래그 — GameManager가 직접 전환, playerState에 의존하지 않음
    private bool _isCrashMode = false;

    private CancellationTokenSource _cts;
    private WingTtoGameManager GameMgr => WingTtoGameManager.instance;


    public void Activate(Vector3 spawnPosition)
    {
        transform.position = spawnPosition;
        isMoving = true;
        currentSpeed = 0f;
        verticalDir = 0f;
        verticalTimer = 0f;
        _isCrashMode = false;
        gameObject.SetActive(true);

        _cts = new CancellationTokenSource();
        VerticalMoveLoop(_cts.Token).Forget();
    }

    public void SetMasterIcon(CardMaster master)
    {
        AssignedMaster = master;

        if (iconRenderer == null) return;

        iconRenderer.sprite = GameResourceManager.instance.GetMasterIcon(master);
    }

    public void SetPause(bool pause)
    {
        // if (_isCrashMode) return; // 크래시 모드 중에는 무시
        isMoving = !pause;
    }

    // 크래시 시작/종료 시 GameManager에서 직접 호출
    public void SetCrashMode(bool crash)
    {
        _isCrashMode = crash;
        isMoving = true; // 크래시 중에도, 종료 후에도 계속 이동
    }

    public void Despawn()
    {
        CancelVerticalLoop();

        isMoving = false;
        _isCrashMode = false;
        verticalDir = 0f;
        verticalTimer = 0f;
        AssignedMaster = CardMaster.Count;
        gameObject.SetActive(false);
    }


    private void FixedUpdate()
    {
        if (!isMoving) return;

        float baseSpeed = GameMgr.normalSpeed * speedRatio;
        float speedMulti = GameMgr.IsSpeedPressed ? speedUpMultiplier : 1f;

        if (_isCrashMode)
        {
            currentSpeed = baseSpeed; // 크래시 중 즉시 오른쪽으로
        }
        else
        {
            float targetHorizontal = -baseSpeed * speedMulti;
            currentSpeed = Mathf.Lerp(currentSpeed, targetHorizontal, Time.deltaTime * smoothFactor);
        }

        float verticalMove = 0f;
        if (verticalTimer > 0f)
        {
            verticalTimer -= Time.deltaTime;

            float newY = transform.position.y + verticalDir * verticalSpeed * Time.deltaTime;

            if (newY <= yMin || newY >= yMax)
            {
                newY = Mathf.Clamp(newY, yMin, yMax);
                verticalTimer = 0f;
                verticalDir = 0f;
            }

            verticalMove = newY - transform.position.y;
        }

        transform.position += new Vector3(currentSpeed * Time.deltaTime, verticalMove, 0f);

        float posX = transform.position.x;
        if (posX < despawnLeftX || posX > despawnRightX)
            GameMgr.DespawnFlyer(this);
    }


    private async UniTaskVoid VerticalMoveLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            bool cancelled = await UniTask
                .Delay(System.TimeSpan.FromSeconds(verticalCheckInterval), cancellationToken: ct)
                .SuppressCancellationThrow();

            if (cancelled) break;
            if (!isMoving) continue;
            if (Random.Range(0, 100) >= verticalChance) continue;

            verticalDir = Random.value < 0.5f ? 1f : -1f;

            float posY = transform.position.y;
            if (posY >= yMax) verticalDir = -1f;
            else if (posY <= yMin) verticalDir = 1f;

            verticalTimer = Random.Range(verticalDurationMin, verticalDurationMax);
        }
    }

    private void CancelVerticalLoop()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private void OnDestroy()
    {
        CancelVerticalLoop();
    }
}

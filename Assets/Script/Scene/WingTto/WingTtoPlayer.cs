using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class WingTtoPlayer : MonoBehaviour
{
    [Header("playerData")]
    private int hp = 100;
    private int maxHp = 100;

    [Header("UI")]
    public SpriteRenderer playerIcon;
    public Text hpText;
    public Slider hpSlider;



    [Header("Movement Settings")]
    private float upwardAcceleration = 35f;   // 상승 가속도
    private float fallAcceleration = 35f;     // 낙하 가속도
    private float maxUpSpeed = 5f;
    private float maxDownSpeed = 5f;

    [Header("Natural Movement")]
    private float releaseFloatDuration = 0.15f; // 놓았을 때 여운 시간
    private float releaseFloatStrength = 0.1f; // 놓았을 때 여운 강도


    private Rigidbody2D rb;
    private float currentVerticalVelocity;

    private bool wasClickingLastFrame;
    private float floatTimer;
    private float velocityAtRelease;

    [Header("other")]
    private float crashTime = 2f;
    private WingTtoPlayerState playerState;

    void Start()
    {
        playerIcon.sprite = GameResourceManager.instance.GetMasterIcon(SaveDataManager.instance.playerData.master);
        playerState = WingTtoPlayerState.Ready;
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        wasClickingLastFrame = false;
        currentVerticalVelocity = 0;
        UpdateHPUI(0);
    }

    public void StartProcess()
    {
        playerState = WingTtoPlayerState.Fly;
    }

    public void SetPause(bool pause)
    {
        if (pause)
        {
            playerState = WingTtoPlayerState.Pause;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        else
        {
            playerState = WingTtoPlayerState.Fly;
        }
    }

    void Update()
    {
        if (playerState == WingTtoPlayerState.Fly || playerState == WingTtoPlayerState.Invincible)
            HandleMovement();
    }




    #region 상하 움직임 처리

    void HandleMovement()
    {
        bool isClickLeft = false;

        // PC - 마우스 입력
        if (Input.GetMouseButton(0))
        {
            isClickLeft = true;
        }

        // 모바일 - 터치 입력 (마우스 입력 덮어쓰기)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began ||
                touch.phase == TouchPhase.Stationary ||
                touch.phase == TouchPhase.Moved)
            {
                // 화면 절반 기준으로 좌우 구분
                float screenHalfWidth = Screen.width / 2f;

                if (touch.position.x < screenHalfWidth)
                {
                    // 왼쪽 화면 
                    isClickLeft = true;
                }
                // else
                // {
                //     // 오른쪽 화면 - 빠르게
                //     targetSpeedMultiplier = fastSpeedMultiplier;
                // }
            }
        }



        // 클릭을 뗐을 때
        if (!isClickLeft && wasClickingLastFrame)
        {
            floatTimer = releaseFloatDuration;
            velocityAtRelease = currentVerticalVelocity;
        }

        // 클릭 중 - 부드럽게 가속하며 상승
        if (isClickLeft)
        {
            floatTimer = 0;
            currentVerticalVelocity += upwardAcceleration * Time.deltaTime;
            currentVerticalVelocity = Mathf.Min(currentVerticalVelocity, maxUpSpeed);
        }
        // 클릭을 뗀 직후 - 약간 더 상승하다가 감속
        else if (floatTimer > 0)
        {
            floatTimer -= Time.deltaTime;
            float floatProgress = floatTimer / releaseFloatDuration;

            // 현재 속도에서 부드럽게 감소
            float targetFloat = velocityAtRelease * releaseFloatStrength * floatProgress;
            currentVerticalVelocity = Mathf.Lerp(currentVerticalVelocity, targetFloat, Time.deltaTime * 5f);

            // 여운이 끝나면 낙하 시작
            if (floatTimer <= 0)
            {
                // 자연스럽게 낙하로 전환
            }
        }
        // 낙하 - 부드럽게 가속
        else
        {
            currentVerticalVelocity -= fallAcceleration * Time.deltaTime;
            currentVerticalVelocity = Mathf.Max(currentVerticalVelocity, -maxDownSpeed);
        }

        rb.linearVelocity = new Vector2(0, currentVerticalVelocity);

        wasClickingLastFrame = isClickLeft;
    }

    #endregion



    #region 충돌 처리


    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("WingTtoGimbab") && playerState == WingTtoPlayerState.Fly)
        {
            // HP 회복
            UpdateHPUI(50);
            WingTtoObjectPool.instance.ReturnObject(other);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("WingTtoStone") && playerState == WingTtoPlayerState.Fly)
        {
            CrashProcess().Forget();
        }
    }


    private async UniTaskVoid CrashProcess()
    {
        HLLogger.Log("CrashProcess");

        playerState = WingTtoPlayerState.Crash;
        WingTtoGameManager.instance.SetPause(true);

        UpdateHPUI(-25);
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        Color originalColor = playerIcon.color;
        Color alphaColor = playerIcon.color;
        alphaColor = Color.red * 0.5f;
        alphaColor.a = 0.7f;
        playerIcon.color = alphaColor;

        this.gameObject.layer = LayerMask.NameToLayer("WingTtoInvincible");

        await UniTask.WaitForSeconds(0.3f);

        if (hp <= 0)
        {
            WingTtoGameManager.instance.SetGameOver();
            return;
        }
        transform.localPosition = new Vector3(-7f, transform.localPosition.y, transform.localPosition.z);
        playerIcon.DOColor(originalColor, crashTime).From(alphaColor).SetEase(Ease.InCubic);
        playerState = WingTtoPlayerState.Invincible;
        WingTtoGameManager.instance.SetPause(false);
        await UniTask.Delay(TimeSpan.FromSeconds(crashTime));

        this.gameObject.layer = LayerMask.NameToLayer("Default");
        playerState = WingTtoPlayerState.Fly;
    }



#if UNITY_EDITOR && DEV
    public void CollisionOnOff(bool isOn)
    {
        this.gameObject.layer = LayerMask.NameToLayer(isOn ? "WingTtoInvincible" : "Default");
    }
#endif

    #endregion




    #region  UI

    private void UpdateHPUI(int value)
    {
        hp += value;
        hp = Math.Min(maxHp, hp);

        float barValue = 0.01f * hp;
        hpSlider.value = barValue;
        hpText.text = $"{barValue * 100}%";
    }


    #endregion


}

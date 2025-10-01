using UnityEngine;

public class WingTtoPlayer : MonoBehaviour
{
    public WingTtoPlayerState playerState;


    public int hp = 0;

    [Header("Movement Settings")]
    private float upwardAcceleration = 35f;   // 상승 가속도
    private float fallAcceleration = 35f;     // 낙하 가속도
    private float maxUpSpeed = 7f;
    private float maxDownSpeed = 7f;

    [Header("Natural Movement")]
    private float releaseFloatDuration = 0.1f; // 놓았을 때 여운 시간
    private float releaseFloatStrength = 0.1f; // 놓았을 때 여운 강도

    private Rigidbody2D rb;
    private float currentVerticalVelocity;

    private bool wasClickingLastFrame;
    private float floatTimer;
    private float velocityAtRelease;

    void Start()
    {
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
    }

    public void StartProcess()
    {
        playerState = WingTtoPlayerState.Fly;
    }

    void Update()
    {
        if (playerState == WingTtoPlayerState.Fly)
            HandleMovement();
    }

    void HandleMovement()
    {
        bool isClicking = Input.GetMouseButton(0);

        // 클릭을 뗐을 때
        if (!isClicking && wasClickingLastFrame)
        {
            floatTimer = releaseFloatDuration;
            velocityAtRelease = currentVerticalVelocity;
        }

        // 클릭 중 - 부드럽게 가속하며 상승
        if (isClicking)
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

        wasClickingLastFrame = isClicking;
    }

    public void OnClickMoveUp()
    {

    }
}

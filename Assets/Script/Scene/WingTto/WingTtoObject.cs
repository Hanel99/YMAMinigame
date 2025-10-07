using System;
using UnityEngine;

public class WingTtoObject : MonoBehaviour
{
    public WingTtoObjectType objectType;
    public SpriteRenderer image;


    private float speedUpMultiplier = 1.5f;
    private float currentSpeed;
    private float normalSpeed = 6f;
    private float targetSpeed = 0f;
    private float currentSpeedSmooth = 0f;

    private bool isMoving = false;
    private bool isClickRight = false;
    private WingTtoObjectPool pool => WingTtoObjectPool.instance;
    private WingTtoGameManager gameManager => WingTtoGameManager.instance;

    // 플레이어 충돌 이벤트
    public event Action<WingTtoObject, GameObject> OnPlayerCollision;


    public void SetData(WingTtoObjectType type)
    {
        objectType = type;
        UpdateSpeed();
    }

    public void SetPause(bool pause)
    {
        isMoving = !pause;
    }

    public void UpdateSpeed()
    {
        normalSpeed = gameManager.normalSpeed;
    }

    private void Update()
    {
        if (!isMoving) return;


        // PC - 마우스 입력
        if (Input.GetMouseButton(1))
        {
            isClickRight = true;
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

                if (touch.position.x > screenHalfWidth)
                {
                    // 오른쪽 화면 
                    isClickRight = true;
                }
            }
        }



        // 속도 결정 (우클릭 여부에 따라)
        currentSpeed = normalSpeed * (isClickRight ? speedUpMultiplier : 1f);

        // 목표 속도 설정
        targetSpeed = normalSpeed * (isClickRight ? speedUpMultiplier : 1f);

        // 부드럽게 보간 (0.1f 값을 조절해서 블렌딩 속도 변경)
        currentSpeedSmooth = Mathf.Lerp(currentSpeedSmooth, targetSpeed, Time.deltaTime * 8f);



        // 왼쪽으로 이동
        transform.position += Vector3.left * currentSpeedSmooth * Time.deltaTime;
    }

    // 오브젝트 활성화 및 이동 시작
    public void Activate(Vector3 spawnPosition)
    {
        transform.position = spawnPosition;
        isMoving = true;
        gameObject.SetActive(true);
    }

    // 풀로 반환
    private void ReturnToPool()
    {
        if (pool != null)
        {
            pool.ReturnObject(this);
        }
    }

    // 오브젝트가 풀로 반환될 때 초기화
    public void OnReturnToPool()
    {
        isMoving = false;
        currentSpeed = normalSpeed;
        gameObject.SetActive(false);
    }


    public void HandlePlayerCollision(GameObject player)
    {
        // 이벤트 발생
        OnPlayerCollision?.Invoke(this, player);
        HLLogger.Log("HandlePlayerCollision");

        // 충돌 후 오브젝트 반환
        ReturnToPool();
    }

}

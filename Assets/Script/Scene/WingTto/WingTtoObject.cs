using System;
using UnityEngine;

public class WingTtoObject : MonoBehaviour
{
    public WingTtoObjectType objectType;


    private float speedUpMultiplier = 1.5f;
    private float normalSpeed = 6f;
    private float currentSpeedSmooth = 0f;

    private bool isMoving = false;
    private WingTtoObjectPool pool;
    private WingTtoGameManager gameManager;

    // 플레이어 충돌 이벤트
    public event Action<WingTtoObject, GameObject> OnPlayerCollision;

    private void Awake()
    {
        pool = WingTtoObjectPool.instance;
        gameManager = WingTtoGameManager.instance;
    }

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

    private void FixedUpdate()
    {
        if (!isMoving) return;

        // SpeedUp 배율 적용 후 보간
        float targetSpeed = normalSpeed * (gameManager.IsSpeedPressed ? speedUpMultiplier : 1f);
        currentSpeedSmooth = Mathf.Lerp(currentSpeedSmooth, targetSpeed, Time.deltaTime * 8f);

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
        currentSpeedSmooth = normalSpeed;
        gameObject.SetActive(false);
    }


    public void HandlePlayerCollision(GameObject player)
    {
        // 이벤트 발생
        OnPlayerCollision?.Invoke(this, player);

        // 충돌 후 오브젝트 반환
        ReturnToPool();
    }

}

using System;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class WingTtoObject : MonoBehaviour
{
    public WingTtoObjectType objectType;
    public Image image;







    private float speedUpMultiplier = 1.5f;
    private float currentSpeed;
    private float normalSpeed = 6f;
    private float maxSpeed = 15f;

    private bool isMoving = false;
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
        maxSpeed = gameManager.maxSpeed;
    }

    private void Update()
    {
        if (!isMoving) return;

        // 속도 결정 (우클릭 여부에 따라)
        currentSpeed = normalSpeed * (Input.GetMouseButton(1) ? speedUpMultiplier : 1f);
        currentSpeed = Mathf.Min(maxSpeed, currentSpeed);

        // 왼쪽으로 이동
        transform.position += Vector3.left * currentSpeed * Time.deltaTime;
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

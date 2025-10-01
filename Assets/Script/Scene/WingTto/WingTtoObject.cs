using System;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class WingTtoObject : MonoBehaviour
{
    public WingTtoObjectType objectType;
    public Image image;

    [Header("Movement Settings")]
    [SerializeField] private float normalSpeed = 5f;
    [SerializeField] private float specialSpeed = 10f;
    // [SerializeField] private float despawnXPosition = -10f;

    private float currentSpeed;
    private bool isMoving = false;
    private WingTtoObjectPool pool => WingTtoObjectPool.instance;

    // 플레이어 충돌 이벤트
    public event Action<WingTtoObject, GameObject> OnPlayerCollision;


    public void SetData(WingTtoObjectType type)
    {
        objectType = type;

        HLLogger.Log($"@@@ WingTtoObject Init {name}");


    }

    private void Update()
    {
        if (!isMoving) return;

        // 속도 결정 (우클릭 여부에 따라)
        currentSpeed = Input.GetMouseButton(1) ? specialSpeed : normalSpeed;

        // 왼쪽으로 이동
        transform.position += Vector3.left * currentSpeed * Time.deltaTime;

        // 특정 좌표를 넘으면 자동 소멸
        // if (transform.position.x < despawnXPosition)
        // {
        //     ReturnToPool();
        // }
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
        HLLogger.Log($"@@@ OnReturnToPool {name}");
        isMoving = false;
        currentSpeed = normalSpeed;
        gameObject.SetActive(false);
    }

    // 플레이어와 충돌 처리
    private void OnTriggerEnter2D(Collider2D other)
    {
        HLLogger.Log("trigger enter 2d");

        if (other.CompareTag("WingTtoBorder"))
        {
            ReturnToPool();
        }

        if (other.CompareTag("WingTtoPlayer"))
        {
            HandlePlayerCollision(other.gameObject);
            ReturnToPool();
        }
    }

    public void HandlePlayerCollision(GameObject player)
    {
        // 이벤트 발생
        OnPlayerCollision?.Invoke(this, player);
        HLLogger.Log("HandlePlayerCollision");

        // 충돌 후 오브젝트 반환
        ReturnToPool();
    }

    // Inspector에서 설정값 변경을 위한 public 메서드
    public void SetSpeeds(float normal, float special)
    {
        normalSpeed = normal;
        specialSpeed = special;
    }

    // public void SetDespawnPosition(float xPosition)
    // {
    //     despawnXPosition = xPosition;
    // }

}

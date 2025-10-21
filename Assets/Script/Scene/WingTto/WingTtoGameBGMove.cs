using UnityEngine;

public class WingTtoGameBGMove : MonoBehaviour
{
    [SerializeField] private float originalScrollSpeed = 1f;

    private float calcSpeed = 1f;
    private float spriteWidth = 24f; // Sprite의 실제 너비
    private SpriteRenderer spriteRenderer;
    private GameObject clone;
    private bool isMoving = true;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Sprite 크기 자동 계산
        spriteWidth = spriteRenderer.bounds.size.x;

        // 복제본 생성 (오른쪽에 배치)
        clone = new GameObject(gameObject.name + "_Clone");
        SpriteRenderer cloneRenderer = clone.AddComponent<SpriteRenderer>();
        cloneRenderer.sprite = spriteRenderer.sprite;
        cloneRenderer.sortingLayerName = spriteRenderer.sortingLayerName;
        cloneRenderer.sortingOrder = spriteRenderer.sortingOrder;
        cloneRenderer.color = spriteRenderer.color;
        cloneRenderer.drawMode = SpriteDrawMode.Sliced;
        cloneRenderer.size = new Vector2(24, 13.5f);

        clone.transform.position = transform.position + Vector3.right * spriteWidth;
        clone.transform.parent = transform.parent;
        UpdateSpeed(1);
    }

    void Update()
    {
        if (!isMoving) return;

        // 왼쪽으로 이동
        transform.position += Vector3.left * calcSpeed * Time.deltaTime;
        clone.transform.position += Vector3.left * calcSpeed * Time.deltaTime;

        // 화면 밖으로 나가면 재배치
        if (transform.position.x <= -spriteWidth)
        {
            transform.position += Vector3.right * (spriteWidth * 2);
        }

        if (clone.transform.position.x <= -spriteWidth)
        {
            clone.transform.position += Vector3.right * (spriteWidth * 2);
        }
    }

    public void UpdateSpeed(float value)
    {
        calcSpeed = originalScrollSpeed * value;
    }

    public void SetPause(bool isPause)
    {
        this.isMoving = !isPause;
    }


    void OnDestroy()
    {
        if (clone != null)
            Destroy(clone);
    }
}



using UnityEngine;

public class WingTtoBGLooping : MonoBehaviour
{
    [System.Serializable]
    public class Layer
    {
        public Transform[] sprites; // 이미지 2개 이상 (A, B)
        public float speed = 1f;    // 이동 속도
    }

    public Layer[] layers;
    public float backgroundWidth = 24f; // 한 이미지의 가로 크기
    public bool isMove = false;


    void Update()
    {
        if (isMove)
        {
            foreach (var layer in layers)
            {
                foreach (var sprite in layer.sprites)
                {
                    // 왼쪽으로 이동
                    sprite.position += Vector3.left * layer.speed * Time.deltaTime;

                    // 일정 거리 넘어가면 오른쪽 끝으로 이동시켜 반복
                    if (sprite.position.x <= -backgroundWidth)
                    {
                        float rightMostX = GetRightMostX(layer.sprites);
                        sprite.position = new Vector3(rightMostX + backgroundWidth, sprite.position.y, sprite.position.z);
                    }
                }
            }
        }
    }

    public void StartMove(bool isMove)
    {
        this.isMove = isMove;
    }



    float GetRightMostX(Transform[] sprites)
    {
        float maxX = float.MinValue;
        foreach (var s in sprites)
        {
            if (s.position.x > maxX)
                maxX = s.position.x;
        }
        return maxX;
    }
}

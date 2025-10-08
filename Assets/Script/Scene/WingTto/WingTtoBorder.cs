using UnityEngine;

public class WingTtoBorder : MonoBehaviour
{





    void OnTriggerEnter2D(Collider2D other)
    {
        WingTtoObjectPool.instance.ReturnObject(other);
    }
}

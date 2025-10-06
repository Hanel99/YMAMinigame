using UnityEngine;

public class WingTtoBorder : MonoBehaviour
{





    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("WingTtoStone") || other.CompareTag("WingTtoGimbab"))
        {
            WingTtoObjectPool.instance.ReturnObject(other);
        }
    }
}

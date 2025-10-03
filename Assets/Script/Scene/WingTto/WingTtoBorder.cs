using UnityEngine;

public class WingTtoBorder : MonoBehaviour
{





    void OnTriggerEnter2D(Collider2D other)
    {
        HLLogger.Log($"{other.name} coming");

        if (other.CompareTag("WingTtoStone") || other.CompareTag("WingTtoGimbab"))
        {
            WingTtoObjectPool.instance.ReturnObject(other);
        }
    }
}

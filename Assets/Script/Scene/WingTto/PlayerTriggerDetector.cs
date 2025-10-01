using UnityEngine;


public class PlayerTriggerDetector : MonoBehaviour
{
    [SerializeField] private GameObject parentPlayer;

    private void Awake()
    {
        if (parentPlayer == null)
        {
            parentPlayer = transform.parent.gameObject;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        WingTtoObject wingObj = other.GetComponent<WingTtoObject>();
        if (wingObj != null)
        {
            wingObj.HandlePlayerCollision(parentPlayer);
        }
    }
}
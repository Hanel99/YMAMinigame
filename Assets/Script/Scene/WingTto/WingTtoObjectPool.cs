using UnityEngine;
using System.Collections.Generic;


public class WingTtoObjectPool : MonoBehaviour
{
    [System.Serializable]
    public class PoolSettings
    {
        public WingTtoObjectType type;
        public GameObject prefab;
        public int initialPoolSize;
    }
    public static WingTtoObjectPool instance;


    [SerializeField] private List<PoolSettings> poolSettings = new List<PoolSettings>();

    public Dictionary<WingTtoObjectType, Queue<WingTtoObject>> poolDictionary;
    public Dictionary<WingTtoObjectType, GameObject> prefabDictionary;
    public Dictionary<WingTtoObjectType, Transform> parentDictionary;

    private void Awake()
    {
        instance = this;
        InitializePool();
    }

    private void InitializePool()
    {
        poolDictionary = new Dictionary<WingTtoObjectType, Queue<WingTtoObject>>();
        prefabDictionary = new Dictionary<WingTtoObjectType, GameObject>();
        parentDictionary = new Dictionary<WingTtoObjectType, Transform>();

        foreach (var setting in poolSettings)
        {
            // 타입별 부모 오브젝트 생성
            Transform parent = new GameObject($"Pool_{setting.type}").transform;
            parent.SetParent(transform);
            parentDictionary[setting.type] = parent;

            // 큐 초기화
            Queue<WingTtoObject> objectQueue = new Queue<WingTtoObject>();

            // 프리팹 저장
            prefabDictionary[setting.type] = setting.prefab;

            // 초기 오브젝트 생성
            for (int i = 0; i < setting.initialPoolSize; i++)
            {
                WingTtoObject obj = CreateNewObject(setting.type, parent);
                objectQueue.Enqueue(obj);
            }

            poolDictionary[setting.type] = objectQueue;

            Debug.Log($"Pool initialized: {setting.type} with {setting.initialPoolSize} objects");
        }
    }

    private WingTtoObject CreateNewObject(WingTtoObjectType type, Transform parent)
    {
        GameObject obj = Instantiate(prefabDictionary[type], parent);
        WingTtoObject wingTtoObj = obj.GetComponent<WingTtoObject>();

        if (wingTtoObj == null)
        {
            wingTtoObj = obj.AddComponent<WingTtoObject>();
        }

        wingTtoObj.SetData(type);
        obj.SetActive(false);

        return wingTtoObj;
    }

    // 오브젝트 가져오기
    public WingTtoObject GetObject(WingTtoObjectType type, Vector3 position)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogWarning($"Pool for type {type} does not exist!");
            return null;
        }

        WingTtoObject obj;

        // 풀에 사용 가능한 오브젝트가 있으면 가져오기
        if (poolDictionary[type].Count > 0)
        {
            obj = poolDictionary[type].Dequeue();
        }
        else
        {
            // 없으면 새로 생성
            obj = CreateNewObject(type, parentDictionary[type]);
            Debug.Log($"Pool expanded: {type}");
        }

        obj.transform.position = position;
        obj.SetData(type);
        obj.gameObject.SetActive(true);
        obj.Activate(position);


        HLLogger.Log($"@@@ Spawn Object {obj.name}");

        return obj;
    }

    // 오브젝트를 풀로 반환
    public void ReturnObject(WingTtoObject obj)
    {
        if (obj == null) return;

        obj.OnReturnToPool();

        if (poolDictionary.ContainsKey(obj.objectType))
        {
            poolDictionary[obj.objectType].Enqueue(obj);
        }
        else
        {
            Debug.LogWarning($"Trying to return object of unknown type: {obj.objectType}");
            Destroy(obj.gameObject);
        }
    }

    // 모든 활성 오브젝트 반환
    public void ReturnAllObjects()
    {
        foreach (var parent in parentDictionary.Values)
        {
            foreach (Transform child in parent)
            {
                if (child.gameObject.activeSelf)
                {
                    WingTtoObject obj = child.GetComponent<WingTtoObject>();
                    if (obj != null)
                    {
                        ReturnObject(obj);
                    }
                }
            }
        }
    }
}

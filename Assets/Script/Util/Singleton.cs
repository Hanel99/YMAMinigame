using UnityEngine;
using System;
using System.Collections;

public class SingletonMono<T> : MonoBehaviour where T : class, new()
{
    private static T _instance = null;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                var obj = new GameObject(typeof(T).Name);
                obj.AddComponent(typeof(T));
            }

            return _instance;
        }
    }

    protected bool _isDontDestroy = true;

    protected virtual void Awake()
    {
        // var check = GameObject.FindObjectsOfType(typeof(T));
        var check = GameObject.FindObjectsByType(typeof(T), FindObjectsSortMode.None);

        if (check != null && check.Length > 1)
        {
            //Debug.LogError(string.Format("Can't Use [{0}]  already made", typeof(T).Name));
            GameObject.DestroyImmediate(this.gameObject);
            return;
        }

        _instance = this as T;
        if (_isDontDestroy)
            _SetDontDestroyOnLoad();

        OnAwake();
    }

    protected virtual void OnAwake()
    { }

    void _SetDontDestroyOnLoad()
    {
        DontDestroyOnLoad(this);
    }
}

public class Singleton<T> where T : class, new()
{
    private static readonly T _instance = new T();
    public static T Instance { get { return _instance; } }

}
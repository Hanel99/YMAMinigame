using UnityEngine;

namespace Singleton
{
    /// <summary>
    /// InfiniTree behaviour that extends <see cref="UnityEngine.MonoBehaviour"/>.
    /// Use this instead of <see cref="UnityEngine.MonoBehaviour"/> strictly.
    /// </summary>
    public class SingletonBehaviour : MonoBehaviour
    {
    }

    /// <summary>
    /// <see cref="SingletonBehaviour"/> which is in Singleton design.
    /// Use this instead of the regular <see cref="SingletonBehaviour"/>
    /// whenever possible.
    /// </summary>
    public class SingletonBehaviour<T> : SingletonBehaviour where T : SingletonBehaviour
    {
        private static T _instance;
        private static object _lock = new object();

        public static T instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    //Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                    //    "' already destroyed on application quit." +
                    //    " Won't create again - returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = (T)FindObjectOfType(typeof(T));

                        if (FindObjectsOfType(typeof(T)).Length > 1)
                        {
                            //Debug.LogError("[Singleton] Something went really wrong " +
                            //    " - there should never be more than 1 singleton!" +
                            //    " Reopenning the scene might fix it.");
                            return _instance;
                        }

                        if (_instance == null)
                        {
                            GameObject singleton = new GameObject();
                            _instance = singleton.AddComponent<T>();
#if UNITY_EDITOR
                            singleton.name = "(singleton) " + typeof(T).ToString();
#else
                            singleton.name = typeof(T).ToString();
#endif
                            DontDestroyOnLoad(singleton);

                            /*Debug.Log("[Singleton] An instance of " + typeof(T) +
                                " is needed in the scene, so '" + singleton +
                                "' was created with DontDestroyOnLoad.");*/
                        }
                        else
                        {
                            //  Debug.Log("[Singleton] Using instance already created: " +
                            //     instance.gameObject.name);
                        }
                    }
                    return _instance;
                }
            }
        }

        private static bool applicationIsQuitting = false;

        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed,
        /// it will create a buggy ghost object that will stay on the Editor scene
        /// even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        public void OnDestroy()
        {
            applicationIsQuitting = true;
        }
    }
}
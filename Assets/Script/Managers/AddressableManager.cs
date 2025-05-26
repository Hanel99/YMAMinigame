using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;

public class AddressableManager : MonoBehaviour
{
    public static AddressableManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }


    public void TestFunction()
    {
        AsyncOperationHandle handle = Addressables.LoadAssetAsync<Texture2D>("Cursor_Mining");
        handle.Completed += (op) =>
        {
            Cursor.SetCursor(handle.Result as Texture2D, Vector2.zero, CursorMode.Auto);
            Debug.Log("Complete");
        };

        Addressables.Release(handle);
    }



    // 프리팹 로드 및 인스턴스화
    public void LoadPrefab(string address, Transform parent, Action<GameObject> onComplete)
    {
        var temp = Addressables.LoadAssetAsync<GameObject>(address);
        temp.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject instance = Instantiate(handle.Result, parent);
                onComplete?.Invoke(instance);
            }
            else
            {
                Debug.LogError($"Failed to load prefab: {address}");
            }
        };

        temp.Release();
    }

    // 스프라이트 로드
    public void LoadSprite(string folder, string imageName, Action<Sprite> onComplete)
    {
        string addressName = $"Assets/AddressableResource/Sprite/{folder}/{imageName}.png";
        var temp = Addressables.LoadAssetAsync<Sprite>(addressName);
        temp.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
                onComplete?.Invoke(handle.Result);
            else
                Debug.LogError($"Failed to load sprite: {imageName}");
        };

        temp.Release();
    }
}

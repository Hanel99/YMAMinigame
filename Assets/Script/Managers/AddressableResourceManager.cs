using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using System.Threading.Tasks;


/// <summary>
/// AddressableResourceManager는 Addressables를 호출해 리소스를 가져옴. 불러온 리소스는 GameResourceManager에서 관리
/// </summary>
public class AddressableResourceManager : MonoBehaviour
{
    public static AddressableResourceManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }


    #region ScriptableObject

    public async Task<List<ScriptableObject>> LoadAllScriptableDataAsync(string label)
    {
        var handle = Addressables.LoadAssetsAsync<ScriptableObject>(label, null);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return new List<ScriptableObject>(handle.Result);
        }

        Debug.LogError($"Failed to load ScriptableObjects with label {label}");
        return new List<ScriptableObject>();
    }


    #endregion



    #region images  




    // 개별 스프라이트 로드
    public void LoadSprite(string imageName, Action<Sprite> onComplete)
    {
        var temp = Addressables.LoadAssetAsync<Sprite>(imageName);
        temp.Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
                onComplete?.Invoke(handle.Result);
            else
                Debug.LogError($"Failed to load sprite: {imageName}");
        };

        temp.Release();
    }

    public async Task<List<Sprite>> LoadSpritesByNamesAsync(List<string> imageNames)
    {
        var handles = new List<AsyncOperationHandle<Sprite>>();
        var sprites = new List<Sprite>();

        foreach (var imageName in imageNames)
        {
            var handle = Addressables.LoadAssetAsync<Sprite>(imageName);
            handles.Add(handle);
        }

        foreach (var handle in handles)
        {
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
                sprites.Add(handle.Result);
            else
                Debug.LogError($"Failed to load sprite: {handle.DebugName}");
        }

        // 핸들 릴리즈
        foreach (var handle in handles)
        {
            Addressables.Release(handle);
        }

        return sprites;
    }



    public async void LoadSpritesByLabel(string label, Action<List<Sprite>> onComplete)
    {
        var handle = Addressables.LoadAssetsAsync<Sprite>(label, null);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
            onComplete?.Invoke(new List<Sprite>(handle.Result));
        else
            Debug.LogError($"Failed to load sprites with label: {label}");

        handle.Release();
    }

    #endregion





    #region prefabs 

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

    #endregion





}

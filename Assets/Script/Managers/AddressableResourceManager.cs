using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using Cysharp.Threading.Tasks;


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

    // ScriptableObject 전체 로드
    public async UniTask<List<ScriptableObject>> LoadAllScriptableDataAsync(string label)
    {
        var handle = Addressables.LoadAssetsAsync<ScriptableObject>(label, null);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
            return new List<ScriptableObject>(handle.Result);

        Debug.LogError($"Failed to load ScriptableObjects with label {label}");
        return new List<ScriptableObject>();
    }


    #endregion



    #region images  



    //TODO @@@ 릴리즈 하니까 저장하기 전에 메모리에서 해제됨. 다른 방식으로 생각해보기

    // 개별 스프라이트 로드
    public async UniTask<Sprite> LoadSpriteAsync(string imageName)
    {
        var handle = Addressables.LoadAssetAsync<Sprite>(imageName);
        await handle.Task;

        Sprite result = null;
        if (handle.Status == AsyncOperationStatus.Succeeded)
            result = handle.Result;
        else
            Debug.LogError($"Failed to load sprite: {imageName}");

        // Addressables.Release(handle);
        return result;
    }

    public async UniTask<List<Sprite>> LoadSpritesByNamesAsync(List<string> imageNames)
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

        foreach (var handle in handles)
        {
            Addressables.Release(handle);
        }

        return sprites;
    }



    public async UniTask<List<Sprite>> LoadSpritesByLabelAsync(string label)
    {
        var handle = Addressables.LoadAssetsAsync<Sprite>(label, null);
        await handle.Task;

        List<Sprite> result = null;
        if (handle.Status == AsyncOperationStatus.Succeeded)
            result = new List<Sprite>(handle.Result);
        else
            Debug.LogError($"Failed to load sprites with label: {label}");

        // handle.Release();
        return result;
    }

    #endregion

    #region sound

    public async UniTask<List<AudioClip>> LoadAudioClipsByLabelAsync(string label)
    {
        var handle = Addressables.LoadAssetsAsync<AudioClip>(label, null);
        await handle.Task;

        List<AudioClip> result = null;
        if (handle.Status == AsyncOperationStatus.Succeeded)
            result = new List<AudioClip>(handle.Result);
        else
            Debug.LogError($"Failed to load audio clips with label: {label}");

        // handle.Release();
        return result;
    }

    #endregion





    #region prefabs 

    // 프리팹 로드 및 인스턴스화
    public async UniTask<GameObject> LoadPrefabAsync(string name, Transform parent)
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(name);
        await handle.Task;

        GameObject instance = null;
        if (handle.Status == AsyncOperationStatus.Succeeded)
            instance = Instantiate(handle.Result, parent);
        else
            Debug.LogError($"Failed to load prefab: {name}");

        // Addressables.Release(handle);
        return instance;
    }

    public async UniTask<List<GameObject>> LoadPrefabsByLabelAsync(string label)
    {
        var handle = Addressables.LoadAssetsAsync<GameObject>(label, null);
        await handle.Task;

        List<GameObject> result = null;
        if (handle.Status == AsyncOperationStatus.Succeeded)
            result = new List<GameObject>(handle.Result);
        else
            Debug.LogError($"Failed to load prefabs with label: {label}");

        // handle.Release();
        return result;
    }


    public async UniTask<GameObject> LoadPrefabByNameAsync(string assetName)
    {
        // 해당 이름이 정확히 일치하는 오브젝트만 비동기로 로드
        var handle = Addressables.LoadAssetAsync<GameObject>(assetName);
        await handle.Task;

        GameObject result = null;
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            if (handle.Result != null)
                result = handle.Result;
            else
                Debug.LogError($"Loaded object is not a PopupBase: {assetName}");
        }
        else
        {
            Debug.LogError($"Failed to load popup prefab: {assetName}");
        }

        // Addressables.Release(handle);
        return result;
    }

    public async UniTask<T> LoadPopupAsync<T>(string popupName) where T : PopupBase
    {
        var popup = await LoadPrefabByNameAsync(popupName);
        return popup.GetComponent<T>();
    }


    #endregion





}

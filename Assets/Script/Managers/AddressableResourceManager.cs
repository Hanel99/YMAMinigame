using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


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


    #region Generic & Handles

    // 핸들 관리를 위한 딕셔너리 (Key: Label or Name, Value: Handle)
    private Dictionary<string, AsyncOperationHandle> _handles = new Dictionary<string, AsyncOperationHandle>();


    public async UniTask<List<T>> LoadAssetsByLabelAsync<T>(string label) where T : UnityEngine.Object
    {
        var handle = Addressables.LoadAssetsAsync<T>(label, null);

        if (_handles.ContainsKey(label))
        {
            // 이미 로드된 핸들이 있으면 해제하고 다시 로드할지, 아니면 기존 핸들을 쓸지 결정해야 함.
            // 여기서는 단순화를 위해 기존 핸들 제거 후 새로 로드 (혹은 중복 로드 방지 로직 필요)
            // 하지만 LoadAssetsAsync는 매번 새로운 핸들을 줄 수 있으므로, 관리가 복잡할 수 있음.
            // 일단은 새로운 핸들을 저장.
            _handles.Remove(label);
        }
        _handles.Add(label, handle);

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return new List<T>(handle.Result);
        }
        else
        {
            Debug.LogError($"Failed to load assets with label: {label}");
            return null;
        }
    }

    public async UniTask<T> LoadAssetAsync<T>(string name) where T : UnityEngine.Object
    {
        var handle = Addressables.LoadAssetAsync<T>(name);

        if (_handles.ContainsKey(name))
            _handles.Remove(name);
        _handles.Add(name, handle);

        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }
        else
        {
            Debug.LogError($"Failed to load asset: {name}");
            return null;
        }
    }


    public void ReleaseAsset(string key)
    {
        if (_handles.TryGetValue(key, out var handle))
        {
            Addressables.Release(handle);
            _handles.Remove(key);
        }
    }

    #endregion


    #region Specific Wrappers (Optional, for backward compatibility if needed, but we will update callers)

    // 이전에 있던 개별 메서드들을 제네릭 메서드로 대체합니다. 
    // 호출부 수정을 위해 기존 메서드 시그니처와 유사하게 유지할 수도 있지만, 
    // 리팩토링 목적상 호출부를 수정하는 것이 더 깔끔합니다.

    public async UniTask<GameObject> LoadPrefabAsync(string name, Transform parent)
    {
        var result = await LoadAssetAsync<GameObject>(name);
        if (result != null)
        {
            return Instantiate(result, parent);
        }
        return null;
    }

    public async UniTask<T> LoadPopupAsync<T>(string popupName) where T : PopupBase
    {
        var result = await LoadAssetAsync<GameObject>(popupName);
        if (result != null)
        {
            if (result.TryGetComponent<T>(out var component))
                return component;
            else
                Debug.LogError($"Loaded object is not a {typeof(T).Name}: {popupName}");
        }
        return null;
    }

    #endregion





}

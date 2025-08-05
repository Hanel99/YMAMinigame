using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using System.Threading.Tasks;

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

    public async void InitScriptableData()
    {
        var scriptableDatas = await LoadAllScriptableDataAsync(StaticGameData.AddressLabels.SOData);

        foreach (var sData in scriptableDatas)
        {
            switch (sData)
            {
                case CardData data:
                    ResourceManager.instance.cardData = data;
                    break;

                case LevelData data:
                    ResourceManager.instance.levelData = data;
                    break;

                case StringData data:
                    ResourceManager.instance.stringData = data;
                    break;

                default:
                    Debug.LogWarning($"Unknown config type: {sData.name}");
                    break;
            }
        }
    }



    #endregion




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
        // string addressName = $"Assets/AddressableResource/Sprite/{folder}/{imageName}.png";
        // var temp = Addressables.LoadAssetAsync<Sprite>(addressName);
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
}

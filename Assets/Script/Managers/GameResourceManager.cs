using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameResourceManager
{
    private static GameResourceManager _instance;
    public static GameResourceManager Instance => _instance ??= new GameResourceManager();

    private bool _isLoaded = false;

    public CardData cardData;
    public StringData stringData;
    public LevelData levelData;

    public List<Sprite> cardImages = new();
    public List<Sprite> gameImages = new();
    public List<Sprite> masterImages = new();

    public GameObject cardPrefab;
    public List<GameObject> popups = new();








    public async Task LoadAsync()
    {
        if (_isLoaded) return;

        // ScriptableObject
        var dataList = await Addressables.LoadAssetsAsync<ScriptableObject>(StaticGameData.AddressLabels.gameData, null).Task;
        foreach (var data in dataList)
        {
            switch (data)
            {
                case CardData cd: cardData = cd; break;
                case StringData sd: stringData = sd; break;
                case LevelData ld: levelData = ld; break;
            }
        }

        // Images
        cardImages = new List<Sprite>(await Addressables.LoadAssetsAsync<Sprite>(StaticGameData.AddressLabels.cardImage, null).Task);
        gameImages = new List<Sprite>(await Addressables.LoadAssetsAsync<Sprite>(StaticGameData.AddressLabels.gameImage, null).Task);
        masterImages = new List<Sprite>(await Addressables.LoadAssetsAsync<Sprite>(StaticGameData.AddressLabels.masterImage, null).Task);

        // Prefabs
        var popups = await Addressables.LoadAssetsAsync<GameObject>(StaticGameData.AddressLabels.popup, null).Task;
        this.popups = new List<GameObject>(popups);

        var gamePrefabList = await Addressables.LoadAssetsAsync<GameObject>(StaticGameData.AddressLabels.gamePrefab, null).Task;
        cardPrefab = gamePrefabList.Count > 0 ? gamePrefabList[0] : null;

        _isLoaded = true;
    }
}

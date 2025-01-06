using UnityEditor;
using UnityEngine;


#region Editor
#if UNITY_EDITOR
public static class MissingScriptFinder
{
    private static int missingScriptCount;

    [MenuItem("Tools/Find Missing Scripts Recursively")]
    private static void FindMissingScriptsRecursively()
    {
        missingScriptCount = 0;
        foreach (GameObject gameObject in
            Resources.FindObjectsOfTypeAll<GameObject>())
            FindMissingScriptsInGameObject(gameObject);
        Debug.Log("Found " + missingScriptCount + " missing scripts.");
    }

    private static void FindMissingScriptsInGameObject(GameObject gameObject)
    {
        Component[] components = gameObject.GetComponents<Component>();
        for (int i = 0; i < components.Length; ++i)
            if (!components[i])
            {
                ++missingScriptCount;
                Debug.LogWarning("'" + GetPath(gameObject) +
                    "' has an empty script attached in position: " + i,
                    gameObject);
            }
        foreach (Transform childTransform in gameObject.transform)
            FindMissingScriptsInGameObject(childTransform.gameObject);
    }

    private static string GetPath(GameObject gameObject)
    {
        return AssetDatabase.Contains(gameObject) ?
            AssetDatabase.GetAssetPath(gameObject) :
            GetSceneGameObjectPath(gameObject);
    }

    private static string GetSceneGameObjectPath(GameObject gameObject)
    {
        string path = gameObject.name;
        for (Transform transform = gameObject.transform; transform.parent;
            transform = transform.parent)
            path = transform.parent.name + "/" + path;
        return path;
    }
}
#endif
#endregion Editor
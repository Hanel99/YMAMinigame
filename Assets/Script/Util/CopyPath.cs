using System.Text;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public static class CopyPath
{
    [MenuItem("GameObject/Copy Path", false, -1)]
    public static void CopyGameObjectPathToClipboard()
    {
        GameObject obj = Selection.activeGameObject;
        if (obj == null)
            return;

        string fullPath = GetFullPath(obj.transform);

        Debug.Log(fullPath);

        GUIUtility.systemCopyBuffer = fullPath;
    }

    private static StringBuilder tempSb = new StringBuilder();

    public static string GetFullPath(Transform tm)
    {
        if (tm == null)
            return string.Empty;

        tempSb.Length = 0;

        Transform currentTm = tm;
        while (currentTm)
        {
            tempSb.Insert(0, "/" + currentTm.name);
            currentTm = currentTm.parent;
        }

        return tempSb.ToString();
    }
}
#endif
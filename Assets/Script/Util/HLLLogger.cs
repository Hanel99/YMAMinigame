using UnityEngine;

public enum LogColor
{
    none,
    // black,
    // blue,
    brown,
    cyan,
    // darkblue,
    green,
    grey,
    lightblue,
    lime,
    magenta,
    // maroon,
    // navy,
    olive,
    orange,
    // purple,
    red,
    silver,
    teal,
    white,
    yellow,
}


public static class HLLogger
{
    public static void Log(string log, LogColor color = LogColor.none, Transform tr = null)
    {
#if !Live
        if (color == LogColor.none)
            Debug.Log($"{log}", tr);
        else
            Debug.Log($"<color={color}>{log}</color>", tr);
#endif
    }

    public static void LogWarning(string log, LogColor color = LogColor.none, Transform tr = null)
    {
#if !Live
        if (color == LogColor.none)
            Debug.LogWarning($"{log}", tr);
        else
            Debug.LogWarning($"<color={color}>{log}</color>", tr);
#endif
    }

    public static void LogError(string log, LogColor color = LogColor.none, Transform tr = null)
    {
#if !Live
        if (color == LogColor.none)
            Debug.LogError($"{log}", tr);
        else
            Debug.LogError($"<color={color}>{log}</color>", tr);
#endif
    }
}
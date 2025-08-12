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
    // Custom colors
    Error,
    Warning,
}


public static class HLLogger
{
    [HideInCallstack]
    public static void Log(string log, LogColor color = LogColor.none, Object context = null)
    {
#if !LIVE
        if (color == LogColor.none)
            Debug.Log(log, context);
        else
            Debug.Log($"<color={color}>{log}</color>", context);
#endif
    }

    [HideInCallstack]
    public static void LogWarning(string log, LogColor color = LogColor.yellow, Object context = null)
    {
#if !LIVE
        string formattedLog = $"<color=yellow><b>[Warning]</b></color> <color={color}>{log}</color>";
        Debug.LogWarning(formattedLog, context);
#endif
    }

    [HideInCallstack]
    public static void LogError(string log, LogColor color = LogColor.red, Object context = null)
    {
#if !LIVE
        string formattedLog = $"<color=red><b>[Error]</b></color> <color={color}>{log}</color>";
        Debug.LogError(formattedLog, context);
#endif
    }
}
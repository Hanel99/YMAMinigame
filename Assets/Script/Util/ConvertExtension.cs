using DG.Tweening;
using System;
using System.Globalization;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public static class ConvertExtension
{

    public static string ReplaceNewLine(this object obj) => obj.ToString().Replace("\\n", System.Environment.NewLine).Replace("\n", System.Environment.NewLine);

    private static bool IsEmpty(object obj) => string.IsNullOrEmpty(obj.ToString());

    public static bool ToBool(this object obj) => !IsEmpty(obj) && Convert.ToBoolean(obj);

    public static int ToInt(this object obj) => IsEmpty(obj) ? -1 : Convert.ToInt32(obj);

    public static float ToFloat(this object obj)
    {
        if (IsEmpty(obj)) return -1f;
        if (decimal.TryParse(obj.ToString().ToString(CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return (float)result;
        return -1f;
    }

    public static double ToDouble(this object obj)
    {
        if (IsEmpty(obj)) return -1;
        if (decimal.TryParse(obj.ToString().ToString(CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return (double)result;
        return -1f;
    }

    public static double ToDoubleFullLength(this object obj)
    {
        if (IsEmpty(obj)) return -1;
        if (double.TryParse(obj.ToString().ToString(CultureInfo.InvariantCulture), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return (double)result;
        return -1f;
    }

}
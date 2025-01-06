using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;


public static class ShuffleList
{
    public static IList<T> Shuffle<T>(this IList<T> list)
    {
        // System.Random rng = new System.Random(StaticGameData.seedNumber + ((GameManager.Instance.phase - 1) * 10000));
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }
}
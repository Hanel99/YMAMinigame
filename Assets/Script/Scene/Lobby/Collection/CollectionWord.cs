using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionWord : MonoBehaviour
{
    private int index;
    private bool isShow = false;
    public bool IsShow => isShow;
    public GameObject border;

    public Text indexText;
    public Text wordText;

    public void SetData(int id, bool showWord)
    {
        index = id;
        isShow = showWord;

        indexText.text = $"No.{(index + 1).ToString("D2")}";
        wordText.text = isShow ? LocalizeManager.instance.GetString($"AIWord.Key.{index}") : "???";
        border?.gameObject.SetActive(isShow);
    }
}
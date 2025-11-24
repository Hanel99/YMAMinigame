using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionWord : MonoBehaviour
{
    private int index;
    private bool isShow = false;
    public bool IsShow => isShow;
    public BorderBlink borderBlink;

    public Text indexText;
    public Text wordText;

    public void SetData(int index, bool showWord = true)
    {
        this.index = index;
        isShow = showWord;

        indexText.text = (index + 1).ToString("D2");
        wordText.text = isShow ? LocalizeManager.instance.GetString($"AIWord.Key.{index}") : "???";

        transform.localScale = Vector3.one;
        gameObject.SetActive(true);

        if (borderBlink != null)
            borderBlink.gameObject.SetActive(isShow);
    }
}
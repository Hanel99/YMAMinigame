using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Text))]
public class LocalizeText : MonoBehaviour
{
    public string Key;
    private Text label;

    private void Awake()
    {
        label = GetComponent<Text>();
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(Key))
            SetText(Key);
    }

    private void SetText(string key)
    {
        if (LocalizeManager.instance)
            label.text = LocalizeManager.instance.GetString(key);
    }
}

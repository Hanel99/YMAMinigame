using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CheatItem : MonoBehaviour
{
    public Text desc;
    public InputField inputField;
    public Button button;

    public UnityAction buttonAction;


    public void SetCheatData(CheatData cheatData)
    {
        desc.text = cheatData.desc;
        inputField.gameObject.SetActive(cheatData.useInputfield);
        inputField.text = "";

        button.onClick.AddListener(() =>
        {
            cheatData.buttonAction(inputField.text.ToInt());
            inputField.text = "";
            SaveDataManager.instance.SavePlayerData();
        });
    }


}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PausePopup : PopupBase
{
    public static PausePopup instance { get; private set; }

    protected override void OnAwake()
    {
        instance = this;
    }

    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            _OpenUI();
        }
        else
        {
            _CloseWindow();
        }
    }


    public void OnClickLobby()
    {
        if (isOpenCloseAnimationActing) return;

        SceneMoveManager.instance.MoveScene(SceneName.LobbyScene);
    }
}

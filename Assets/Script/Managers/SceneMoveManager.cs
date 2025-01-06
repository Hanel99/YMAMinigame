using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMoveManager : MonoBehaviour
{
    public static SceneMoveManager instance { get; private set; }


    private void Awake()
    {
        if (instance != null && instance != this)
            return;

        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void MoveScene(SceneName sceneName, bool showMoveAni = true)
    {
        if (showMoveAni == false)
        {
            // 애니메이션 스킵하고 바로 이동
            SceneManager.LoadScene(sceneName.ToString());
            return;
        }

        // 애니메이션 연출 후 이동
        SceneName currentSceneName = (SceneName)Enum.Parse(typeof(SceneName), SceneManager.GetActiveScene().name, true);


        switch (currentSceneName)
        {
            case SceneName.IntroScene:
                IntroUIManager.instance.ShowSceneMoveAnimation(false, () => SceneManager.LoadScene(sceneName.ToString()));
                break;

            case SceneName.LobbyScene:
                LobbyUIManager.instance.ShowSceneMoveAnimation(false, () => SceneManager.LoadScene(sceneName.ToString()));
                break;

            case SceneName.YMAMatch2CardGame:
                CardGameUIManager.instance.ShowSceneMoveAnimation(false, () => SceneManager.LoadScene(sceneName.ToString()));
                break;
        }
    }





}

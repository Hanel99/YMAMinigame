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
        SoundManager.instance.FadeOutBGM(0.4f);
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
                IntroUIManager.instance.ShowSceneMoveAnimation(false, () => AnimationCompleteProcess(sceneName));
                break;

            case SceneName.LobbyScene:
                LobbyUIManager.instance.ShowSceneMoveAnimation(false, () => AnimationCompleteProcess(sceneName));
                break;

            case SceneName.YMAMatch2CardGame:
                CardGameUIManager.instance.ShowSceneMoveAnimation(false, () => AnimationCompleteProcess(sceneName));
                break;

            case SceneName.YMAFindAIWordGame:
                FindAIWordGameUIManager.instance.ShowSceneMoveAnimation(false, () => AnimationCompleteProcess(sceneName));
                break;

            case SceneName.YMACubeGame:
                CubeGameUIManager.instance.ShowSceneMoveAnimation(false, () => AnimationCompleteProcess(sceneName));
                break;

            case SceneName.YMAWingTto:
                WingTtoGameUIManager.instance.ShowSceneMoveAnimation(false, () => AnimationCompleteProcess(sceneName));
                break;
        }
    }

    private void SetScreenRotate(SceneName name)
    {
        if (name == SceneName.YMAWingTto)
            SetLandscape();
        else
            SetPortrait();
    }

    private void AnimationCompleteProcess(SceneName sceneName)
    {
        SetScreenRotate(sceneName);
        SceneManager.LoadScene(sceneName.ToString());
    }





    // 가로 화면으로 고정
    public void SetLandscape()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        // 또는 자동 회전 허용
        // Screen.orientation = ScreenOrientation.AutoRotation;
        // Screen.autorotateToLandscapeLeft = true;
        // Screen.autorotateToLandscapeRight = true;
        // Screen.autorotateToPortrait = false;
        // Screen.autorotateToPortraitUpsideDown = false;
    }

    // 세로 화면으로 고정
    public void SetPortrait()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        // 또는 자동 회전 허용
        // Screen.orientation = ScreenOrientation.AutoRotation;
        // Screen.autorotateToPortrait = true;
        // Screen.autorotateToPortraitUpsideDown = true;
        // Screen.autorotateToLandscapeLeft = false;
        // Screen.autorotateToLandscapeRight = false;
    }

}

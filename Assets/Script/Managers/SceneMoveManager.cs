using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMoveManager : MonoBehaviour
{
    public static SceneMoveManager instance { get; private set; }
    private ScreenOrientation screenOrientation = ScreenOrientation.Portrait;


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
                IntroUIManager.instance.ShowSceneMoveAnimation(false, () => AnimationCompleteProcess(sceneName).Forget());
                break;

            case SceneName.LobbyScene:
                LobbyUIManager.instance.ShowSceneMoveAnimation(false, () => AnimationCompleteProcess(sceneName).Forget());
                break;

            case SceneName.YMAMatch2CardGame:
                CardGameUIManager.instance.ShowSceneMoveAnimation(false, () => AnimationCompleteProcess(sceneName).Forget());
                break;

            case SceneName.YMAFindAIWordGame:
                FindAIWordGameUIManager.instance.ShowSceneMoveAnimation(false, () => AnimationCompleteProcess(sceneName).Forget());
                break;

            case SceneName.YMACubeGame:
                CubeGameUIManager.instance.ShowSceneMoveAnimation(false, () => AnimationCompleteProcess(sceneName).Forget());
                break;

            case SceneName.YMAWingTto:
                WingTtoGameUIManager.instance.ShowSceneMoveAnimation(false, () => AnimationCompleteProcess(sceneName).Forget());
                break;
        }
    }

    private bool SetScreenRotate(SceneName name)
    {
        if (name == SceneName.YMAWingTto)
            return SetLandscape();
        else
            return SetPortrait();
    }

    private async UniTask AnimationCompleteProcess(SceneName sceneName)
    {
#if UNITY_ANDROID
    if (SetScreenRotate(sceneName))
        await UniTask.WaitForSeconds(1f);
#else
        await UniTask.Yield();
#endif

        SceneManager.LoadScene(sceneName.ToString());
    }






    // 가로 화면으로 고정
    public bool SetLandscape()
    {
        bool isRotate = false;
        if (screenOrientation != ScreenOrientation.LandscapeLeft)
        {
            screenOrientation = ScreenOrientation.LandscapeLeft;
            Screen.orientation = screenOrientation;
            isRotate = true;
        }
        return isRotate;

        // 또는 자동 회전 허용
        // Screen.orientation = ScreenOrientation.AutoRotation;
        // Screen.autorotateToLandscapeLeft = true;
        // Screen.autorotateToLandscapeRight = true;
        // Screen.autorotateToPortrait = false;
        // Screen.autorotateToPortraitUpsideDown = false;
    }

    // 세로 화면으로 고정
    public bool SetPortrait()
    {
        bool isRotate = false;
        if (screenOrientation != ScreenOrientation.Portrait)
        {
            screenOrientation = ScreenOrientation.Portrait;
            Screen.orientation = screenOrientation;
            isRotate = true;
        }
        return isRotate;

        // 또는 자동 회전 허용
        // Screen.orientation = ScreenOrientation.AutoRotation;
        // Screen.autorotateToPortrait = true;
        // Screen.autorotateToPortraitUpsideDown = true;
        // Screen.autorotateToLandscapeLeft = false;
        // Screen.autorotateToLandscapeRight = false;
    }

}

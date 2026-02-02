using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GlitchText : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Text textComponent;

    [Header("Settings")]
    private float glitchSpeed = 0.015f;
    private int glitchIterations = 20;
    private float revealDuration = 0.3f;

    private string glitchCharacters = "!@#$%^&*()█▓▒░<>?/\\|[]{}";

    private bool isGlitching = false;

    void Start()
    {
        if (textComponent == null)
            textComponent = GetComponent<Text>();
    }

    public void ChangeTextWithGlitch(string newText)
    {
        if (!isGlitching)
            GlitchTransition(textComponent.text, newText).Forget();
    }

    private async UniTaskVoid GlitchTransition(string fromText, string toText)
    {
        isGlitching = true;
        var token = this.GetCancellationTokenOnDestroy();

        int maxLength = Mathf.Max(fromText.Length, toText.Length);

        // 1. 기존 텍스트가 글리치되는 연출
        for (int iteration = 0; iteration < glitchIterations; iteration++)
        {
            string currentDisplayText = "";

            for (int i = 0; i < maxLength; i++)
            {
                if (i < fromText.Length && fromText[i] == ' ')
                {
                    currentDisplayText += ' ';
                }
                else if (i < maxLength)
                {
                    currentDisplayText += glitchCharacters[Random.Range(0, glitchCharacters.Length)];
                }
            }

            textComponent.text = currentDisplayText;
            await UniTask.Delay(System.TimeSpan.FromSeconds(glitchSpeed), cancellationToken: token);
        }

        // 2. 새로운 텍스트가 점차 드러나는 연출 (시간 기반)
        float startTime = Time.time;
        while (true)
        {
            float elapsedTime = Time.time - startTime;
            if (elapsedTime >= revealDuration) break;
            // 시간 비율 (0 ~ 1)
            float t = Mathf.Clamp01(elapsedTime / revealDuration);

            // 현재 보여질 글자 수 계산
            int revealed = (int)(t * toText.Length);

            string currentDisplayText = "";
            bool isShrinking = fromText.Length > toText.Length;

            // 문자열 길이 계산 (Lerp로 자연스럽게 변화)
            int currentLength;
            if (isShrinking)
            {
                currentLength = (int)Mathf.Lerp(maxLength, toText.Length, t);
            }
            else
            {
                currentLength = maxLength;
            }

            for (int i = 0; i < currentLength; i++)
            {
                if (i < toText.Length)
                {
                    if (i < revealed)
                        currentDisplayText += toText[i];
                    else
                        currentDisplayText += glitchCharacters[Random.Range(0, glitchCharacters.Length)];
                }
                else
                {
                    currentDisplayText += glitchCharacters[Random.Range(0, glitchCharacters.Length)];
                }
            }

            textComponent.text = currentDisplayText;

            // 너무 잦은 업데이트 방지 및 글리치 느낌 유지를 위해 딜레이
            // glitchSpeed가 너무 느리면 연출이 끊기므로, 프레임 단위로 하거나 최소 딜레이를 줌.
            await UniTask.Delay(System.TimeSpan.FromSeconds(glitchSpeed), cancellationToken: token);

        }

        textComponent.text = toText;
        isGlitching = false;
    }
}
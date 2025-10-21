using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CubeGameCountUI : MonoBehaviour
{
    public Image image;
    public Text stateInfoText;
    public Text detailText;
    public Text countText;
    public CubeState state;


    public void UpdateCountText(int count)
    {
        countText.text = count.ToString();
    }

    public void InitUI(CubeState cubeState)
    {
        state = cubeState;
        stateInfoText.text = cubeState.ToString();
        image.gameObject.SetActive(true);
        countText.text = "0";
        detailText.text = "";

        switch (state)
        {
            case CubeState.TooFast:
                image.color = new Color32(94, 19, 19, 255);   // #8B1E1E (아주빠름 - 더 어두운 붉은색)
                detailText.text = "-100점, -0.5초";
                break;

            case CubeState.Fast:
                image.color = new Color32(198, 40, 40, 255);   // #C62828 (빠름 - 살짝 어두운 붉은색)
                detailText.text = "100점";
                break;

            case CubeState.Perfect:
                image.color = new Color32(0, 230, 118, 255);   // #00E676 (정확 - 형광초록/시원한 느낌)
                detailText.text = "300점, +0.5초";
                break;

            case CubeState.Slow:
                image.color = new Color32(25, 118, 210, 255);  // #1976D2 (느림 - 살짝 어두운 푸른색)
                detailText.text = "100점";
                break;

            case CubeState.TooSlow:
                image.color = new Color32(11, 53, 118, 255);   // #0D47A1 (아주느림 - 진한 남색)
                detailText.text = "-100점, -0.5초";
                break;

            default:
                image.gameObject.SetActive(false);
                break;
        }
    }
}

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

    // Consts (Colors matched with CubeGameCube to keep consistency, but defined here for UI independence or could be shared)
    private static readonly Color32 COLOR_TOO_FAST = new Color32(94, 19, 19, 255);
    private static readonly Color32 COLOR_FAST = new Color32(198, 40, 40, 255);
    private static readonly Color32 COLOR_PERFECT = new Color32(0, 230, 118, 255);
    private static readonly Color32 COLOR_SLOW = new Color32(25, 118, 210, 255);
    private static readonly Color32 COLOR_TOO_SLOW = new Color32(11, 53, 118, 255);

    private const string TEXT_TOO_FAST_SLOW = "-100점, -0.5초";
    private const string TEXT_FAST_SLOW = "100점";
    private const string TEXT_PERFECT = "300점, +0.5초";

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
                image.color = COLOR_TOO_FAST;
                detailText.text = TEXT_TOO_FAST_SLOW;
                break;

            case CubeState.Fast:
                image.color = COLOR_FAST;
                detailText.text = TEXT_FAST_SLOW;
                break;

            case CubeState.Perfect:
                image.color = COLOR_PERFECT;
                detailText.text = TEXT_PERFECT;
                break;

            case CubeState.Slow:
                image.color = COLOR_SLOW;
                detailText.text = TEXT_FAST_SLOW;
                break;

            case CubeState.TooSlow:
                image.color = COLOR_TOO_SLOW;
                detailText.text = TEXT_TOO_FAST_SLOW;
                break;

            default:
                image.gameObject.SetActive(false);
                break;
        }
    }
}

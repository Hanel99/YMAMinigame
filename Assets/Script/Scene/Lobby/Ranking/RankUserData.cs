using UnityEngine;
using UnityEngine.UI;

public class RankUserData : MonoBehaviour
{
    public Text playerRankText;
    public Text playerNameText;
    public Text playerLevelText;
    public Text playerExpText;
    public Text playerSingleText;

    public void UpdateData(int rank, string name, int level, int exp, float single, string appendString = "")
    {
        playerRankText.text = rank.ToString();
        playerNameText.text = name;
        playerLevelText.text = level < 0 ? "" : $"{level}{appendString}";
        playerExpText.text = exp < 0 ? "" : $"{exp}{appendString}";
        playerSingleText.text = single < 0 ? "" : $"{single}{appendString}";
    }
}

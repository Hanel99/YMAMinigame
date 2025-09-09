using UnityEngine;
using UnityEngine.UI;

public class RankUserData : MonoBehaviour
{
    public Text playerRankText;
    public Text playerNameText;
    public Text playerLevelText;
    public Text playerExpText;

    public void UpdateData(int rank, string name, int level, int exp)
    {
        playerRankText.text = rank.ToString();
        playerNameText.text = name;
        playerLevelText.text = level < 0 ? "" : level.ToString();
        playerExpText.text = exp < 0 ? "" : exp.ToString();
    }
}

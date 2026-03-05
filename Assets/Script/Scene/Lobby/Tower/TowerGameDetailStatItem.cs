using UnityEngine;
using UnityEngine.UI;

public class TowerGameDetailStatItem : MonoBehaviour
{
    [Header("Title")]
    public Text statNameText;

    [Header("Player Stat")]
    public Text playerBaseText;
    public Text playerWeaponText;
    public Text playerJewelText;

    [Header("Boss Stat")]
    public Text bossValueText;

    /// <summary>
    /// 스탯 아이템의 데이터를 설정합니다.
    /// </summary>
    /// <param name="name">스탯 명칭</param>
    /// <param name="pBase">플레이어 기본 수치 (레벨 + 스탯)</param>
    /// <param name="pWeapon">무기 강화 수치 (null 또는 빈 문자열이면 미구현/미표시)</param>
    /// <param name="pJewel">보석 강화 수치 (null 또는 빈 문자열이면 미구현/미표시)</param>
    /// <param name="bValue">보스 수치 또는 보상 정보</param>
    public void SetData(string name, string pBase, string pWeapon, string pJewel, string bValue)
    {
        if (statNameText != null) statNameText.text = name;
        if (playerBaseText != null) playerBaseText.text = pBase;
        if (playerWeaponText != null) playerWeaponText.text = pWeapon ?? "";
        if (playerJewelText != null) playerJewelText.text = pJewel ?? "";
        if (bossValueText != null) bossValueText.text = bValue ?? "";
    }
}

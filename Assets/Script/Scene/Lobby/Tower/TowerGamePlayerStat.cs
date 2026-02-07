using Coffee.UIExtensions;
using UnityEngine;
using UnityEngine.UI;

public class TowerGamePlayerStat : MonoBehaviour
{
    public Text nameText;
    public Text levelText;
    public Text valueText;
    public Text requireCoinText;
    public Button enchantButton;
    public UIParticle uiParticle;


    private TowerUserStatType statType;
    private int statLevel;
    private int requireCoin;
    private bool isLongPress = false;
    private LongPressButton longPressButton;

    void OnDisable()
    {
        uiParticle.gameObject.SetActive(false);
    }

    public void UpdateUIData(TowerUserStatType type, int statLevel)
    {
        statType = type;
        this.statLevel = statLevel;

        nameText.text = LocalizeManager.instance.GetString($"Tower.StatType.{type}");
        this.levelText.text = $"Lv.{statLevel}";

        if (type == TowerUserStatType.CriRate)
            valueText.text = $"{(GetValue<float>(type) * 100).ToString("F1")}%";
        else if (type == TowerUserStatType.CriDmg)
            valueText.text = $"x{((1 + GetValue<float>(type)) * 100).ToString("F1")}%";
        else
            valueText.text = GetValue<int>(type).ToString();

        requireCoin = GameResourceManager.instance.GetTowerUserLevelRequireCoin(type, statLevel);

        requireCoinText.text = requireCoin <= 0 ? "MAX" : requireCoin.ToString("N0");
        enchantButton.interactable = requireCoin > 0 && SaveDataManager.instance.playerData.coin >= requireCoin;

        if (longPressButton == null)
        {
            longPressButton = enchantButton.GetComponent<LongPressButton>();
            longPressButton.SetLongPressAction(OnClickEnchant);
            longPressButton.SetAccelerateAction(OnClickEnchant);
        }
    }

    public void UpdateButtonInteractable()
    {
        enchantButton.interactable = requireCoin > 0 && SaveDataManager.instance.playerData.coin >= requireCoin;
    }


    public void OnClickEnchant(bool isLongPress = false)
    {
        this.isLongPress = isLongPress;
        requireCoin = GameResourceManager.instance.GetTowerUserLevelRequireCoin(statType, statLevel);
        if (requireCoin <= 0 || SaveDataManager.instance.playerData.coin < requireCoin)
            return;

        if (isLongPress == false)
            SoundManager.instance.PlaySFX(SFXType.WeaponSuccess);
        SaveDataManager.instance.AddCoin(-GameResourceManager.instance.GetTowerUserLevelRequireCoin(statType, statLevel), false);
        statLevel++;
        SaveDataManager.instance.SetTowerUserStatLevel(statType, statLevel);

        UpdateUIData(statType, statLevel);
        TowerGameWeaponEnchantPopup.instance.UpdateUI();
        //TODO 강화 파티클

        if (uiParticle.IsActive() == false)
            uiParticle.gameObject.SetActive(true);
        uiParticle?.Play();


        // #if UNITY_EDITOR && DEV
        //         UpdateUIData(statType, statLevel);
        //         TowerGameWeaponEnchantPopup.instance.UpdateUI();
        // #else
        //         TowerGameWeaponEnchantPopup.instance.ShowEnchantResult(TowerGameResultType.stat, $"Lv.{statLevel - 1}", $"-> Lv.{statLevel}", () => UpdateUIData(statType, statLevel));
        // #endif
    }

    private T GetValue<T>(TowerUserStatType type)
    {
        return GameResourceManager.instance.GetTowerUserLevelStatValue<T>(type, statLevel);
    }


}

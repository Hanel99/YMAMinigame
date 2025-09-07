using UnityEngine.UI;

public class TowerGamePopup : PopupBase
{
    public static TowerGamePopup instance { get; private set; }

    public Text bossTitle;
    public Image bossImage;
    public Text bossDesc;



    //private
    private bool isOnCombat = false;

    private TowerGameUserStatLevelData towerGameUserStatLevelData;
    private TowerGameUserWeaponData towerGameUserWeaponData;


    protected override void OnAwake()
    {
        instance = this;
    }


    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            UpdateGameData();
            UpdateUI();
            _OpenUI();
            isOnCombat = false;
        }
        else
        {
            _CloseWindow();
        }
    }

    private void UpdateUI()
    {
        bossTitle.text = "";
        bossImage.sprite = null;
        bossImage.SetNativeSize();

        bossDesc.text = "";
    }

    private void UpdateGameData()
    {

        var level = SaveDataManager.instance.playerData.level;
        var floor = SaveDataManager.instance.playerData.towerFloor;
        var userStatData = SaveDataManager.instance.playerData.towerGameUserStatLevelData;
        var weaponMetaData = GameResourceManager.instance.GetTowerWeaponLevelMetaData(level);
        var monsterMetaData = GameResourceManager.instance.GettowerBossLevelMetaData(floor);
        var userMetaData = GameResourceManager.instance.GetTowerUserLevelMetaData(level);


        towerGameUserStatLevelData = SaveDataManager.instance.playerData.towerGameUserStatLevelData;
        towerGameUserWeaponData = SaveDataManager.instance.playerData.towerGameUserWeaponData;
    }

    public void OnClickStartCombat()
    {
        if (isOpenCloseAnimationActing || isOnCombat) return;

        // isOnProcess = true;



    }


    public void OnClickShowEnchantPopup()
    {
        if (isOpenCloseAnimationActing) return;

        LobbyUIManager.instance.ShowPopup<TowerGameWeaponEnchantPopup>();
    }

    public void OnClickShowDetailStatPopup()
    {
        if (isOpenCloseAnimationActing) return;

        LobbyUIManager.instance.ShowPopup<TowerGameDetailStatPopup>();
    }












    private int GetRandomValue(int n)
    {
        // n=0일 때는 바로 반환
        if (n == 0)
            return (int)((uint)(n * 2654435761) % 3);

        // 이전값과 현재값 계산
        int prevValue = (int)((uint)((n - 1) * 2654435761) % 3);
        int currValue = (int)((uint)(n * 2654435761) % 3);

        // 연속되면 다른 값으로 변경
        if (currValue == prevValue)
        {
            currValue = (currValue + 1 + (int)((uint)(n * 1234567) % 2)) % 3;
        }

        return currValue;
    }
}

using UnityEngine.UI;

public class TowerGamePopup : PopupBase
{
    public static TowerGamePopup instance { get; private set; }

    public Text bossTitle;
    public Image bossImage;
    public Text bossDesc;



    //private
    private bool isOnCombat = false;

    private PlayerData playerData;
    private int floor;


    protected override void OnAwake()
    {
        instance = this;
    }


    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            UpdatePopupData();
            _OpenUI();
        }
        else
        {
            _CloseWindow();
        }
    }


    public void UpdatePopupData()
    {
        UpdateGameData();
        UpdateUI();
        isOnCombat = false;
    }


    private void UpdateGameData()
    {
        playerData = SaveDataManager.instance.playerData;
        floor = SaveDataManager.instance.playerData.towerFloor;
    }

    private void UpdateUI()
    {
        bossTitle.text = $"{floor} 층 보스";
        bossImage.sprite = GameResourceManager.instance.GetTowerBossImage(GetRandomValue(floor), false, false);
        bossImage.SetNativeSize();

        bossDesc.text = "보스 설명 텍스트";
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






    public void OnClickStartCombat()
    {
        if (isOpenCloseAnimationActing || isOnCombat) return;

        isOnCombat = true;
        LobbyUIManager.instance.ShowPopup<TowerGameCombatPopup>();
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











}

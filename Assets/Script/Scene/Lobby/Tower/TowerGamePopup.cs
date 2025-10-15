using DG.Tweening;
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
    private TowerBossLevelMetaData bossMetaData;
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
        bossMetaData = GameResourceManager.instance.GetTowerBossLevelMetaData(floor);

        if (bossMetaData == null || bossMetaData.atk < 0)
        {
            bossTitle.text = $"{floor} 층";
            bossImage.sprite = null;
            bossImage.gameObject.SetActive(false);
            bossImage.transform.DOKill();

            bossDesc.text = LocalizeManager.instance.GetString("Tower.game.Desc.empty");
            return;
        }

        int bossNumber = GetBossNumber(floor);
        bossTitle.text = $"{floor} 층\n{LocalizeManager.instance.GetString($"Tower.Boss.Name.{bossNumber.ToString("D2")}")}";
        bossImage.gameObject.SetActive(true);
        bossImage.sprite = GameResourceManager.instance.GetTowerBossImage(bossNumber, false, false);
        bossImage.SetNativeSize();
        bossImage.transform.DOKill();
        bossImage.transform.DOLocalMoveY(105f, 2f).From(115f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);

        bossDesc.text = LocalizeManager.instance.GetString($"Tower.Boss.Desc.{bossNumber.ToString("D2")}");
    }

    public int GetBossNumber(int n)
    {
        int count = 9;
        // n=0일 때는 바로 반환
        if (n == 0)
            return (int)((uint)(n * 2654435761) % count);

        // 이전값과 현재값 계산
        int prevValue = (int)((uint)((n - 1) * 2654435761) % count);
        int currValue = (int)((uint)(n * 2654435761) % count);

        // 연속되면 다른 값으로 변경
        if (currValue == prevValue)
        {
            currValue = (currValue + 1 + (int)((uint)(n * 1234567) % 2)) % count;
        }

        return currValue;
    }






    public void OnClickStartCombat()
    {
        if (isOpenCloseAnimationActing || isOnCombat) return;

        if (bossMetaData == null || bossMetaData.atk < 0)
        {
            LobbyUIManager.instance.ShowCommonPopup("알림", LocalizeManager.instance.GetString("Tower.game.Desc.empty"), true, true, false, null, null);
        }
        else
        {
            isOnCombat = true;
            LobbyUIManager.instance.ShowPopup<TowerGameCombatPopup>();
        }
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

    public void OnClickHowToPlay()
    {
        LobbyUIManager.instance.ShowHowToPlayPopup($"game.desc.InfinityTower");
    }

}

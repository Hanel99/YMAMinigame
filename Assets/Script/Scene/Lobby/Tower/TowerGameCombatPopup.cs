using System.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TowerGameCombatPopup : PopupBase
{
    public static TowerGameCombatPopup instance { get; private set; }

    public ScrollRect scrollRect;
    public Text combatText;
    public Button speedButton;
    public Button retryButton;
    public Button confirmButton;

    //private 
    PlayerData playerData = null;
    private int turnCount = 0;


    private combatStatData playerCombatData = new();
    private combatStatData bossCombatData = new();
    private int rewardCoin;
    private int rewardExp;
    private StringBuilder sb = new StringBuilder();

    private int speedMode = 0;
    private int delayTime = 800;






    protected override void OnAwake()
    {
        instance = this;
    }


    public override void ShowPopup(bool enable = true)
    {
        if (enable)
        {
            SetCombatData();
            UpdateUI();
            _OpenUI();
            CombatProcess();
        }
        else
        {
            _CloseWindow();
        }
    }

    private void SetCombatData()
    {
        turnCount = 0;
        sb.Clear();


        playerData = SaveDataManager.instance.playerData;
        var playerStatData = playerData.towerGameUserStatLevelData;
        var playerWeaponData = playerData.towerGameUserWeaponData;
        var floor = playerData.towerFloor;

        var weaponMetaData = GameResourceManager.instance.GetTowerWeaponLevelMetaData(playerWeaponData.weaponLevel);
        var bossMetaData = GameResourceManager.instance.GetTowerBossLevelMetaData(floor);

        playerCombatData.atk = playerData.level + GetValue<int>(TowerUserStatType.atk, playerStatData.atkLevel) + weaponMetaData.atk;
        playerCombatData.def = playerData.level + GetValue<int>(TowerUserStatType.def, playerStatData.defLevel);
        playerCombatData.hp = playerData.level + GetValue<int>(TowerUserStatType.hp, playerStatData.hpLevel);
        playerCombatData.maxHp = playerCombatData.hp;
        playerCombatData.criRate = GetValue<float>(TowerUserStatType.criRate, playerStatData.criRateLevel);
        playerCombatData.criDmg = GetValue<float>(TowerUserStatType.criDmg, playerStatData.criDmgLevel) + weaponMetaData.criDmg;
        playerCombatData.avoidance = Mathf.Min(0.2f, playerData.level * 0.01f);


        bossCombatData.atk = bossMetaData.atk;
        bossCombatData.def = bossMetaData.def;
        bossCombatData.hp = bossMetaData.hp;
        bossCombatData.maxHp = bossMetaData.hp;
        bossCombatData.criRate = bossMetaData.criRate;
        bossCombatData.criDmg = bossMetaData.criDmg;
        bossCombatData.avoidance = Mathf.Min(0.2f, floor * 0.001f);
        rewardCoin = bossMetaData.rewardCoin;
        rewardExp = bossMetaData.rewardExp;
    }
    private T GetValue<T>(TowerUserStatType type, int level)
    {
        return GameResourceManager.instance.GetTowerUserLevelStatValue<T>(type, level);
    }

    public void UpdateUI()
    {
        retryButton.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        UpdateCombatText();
    }

    public async void CombatProcess()
    {
        bool isCritical;
        await UniTask.Delay(1000);

        while (playerCombatData.hp > 0 && bossCombatData.hp > 0 && turnCount < 20)
        {
            turnCount++;
            sb.AppendLine("");
            sb.AppendLine("---------------------------------");
            sb.AppendLine($"{turnCount}번째 턴!");
            UpdateCombatText();
            await UniTask.Delay(delayTime);


            // 플레이어가 보스에게 공격
            sb.AppendLine($"{playerData.name}의 공격!").AppendLine();
            UpdateCombatText();
            await UniTask.Delay(delayTime);

            if (!IsAvoided(bossCombatData.avoidance))
            {
                int damage = CalculateDamage(playerCombatData, bossCombatData, out isCritical);
                if (isCritical)
                    sb.AppendLine($"{playerData.name} 혼신의 일격!");
                else
                    sb.AppendLine($"{playerData.name}이/가 보스를 공격!");
                UpdateCombatText();
                await UniTask.Delay(delayTime);

                bossCombatData.hp -= damage;
                bossCombatData.hp = Mathf.Max(0, bossCombatData.hp);

                sb.AppendLine($"{damage} 데미지를 입혔다! ({bossCombatData.hp}/{bossCombatData.maxHp})");
                UpdateCombatText();
                await UniTask.Delay(delayTime);

                if (bossCombatData.hp <= 0)
                {
                    sb.AppendLine("보스를 처치했습니다!");
                    UpdateCombatText();
                    EndCombat(true);
                    return;
                }
            }
            else
            {
                sb.AppendLine($"보스가 {playerData.name}의 공격을 회피했습니다!");
                UpdateCombatText();
                await UniTask.Delay(delayTime);
            }


            // 보스가 플레이어에게 공격
            sb.AppendLine($"보스의 공격!").AppendLine();
            UpdateCombatText();
            await UniTask.Delay(delayTime);

            if (!IsAvoided(playerCombatData.avoidance))
            {
                int damage = CalculateDamage(bossCombatData, playerCombatData, out isCritical);
                if (isCritical)
                    sb.AppendLine($"보스 혼신의 일격!");
                else
                    sb.AppendLine($"보스가 {playerData.name}을/를 공격!");
                UpdateCombatText();
                await UniTask.Delay(delayTime);

                playerCombatData.hp -= damage;
                playerCombatData.hp = Mathf.Max(0, playerCombatData.hp);

                sb.AppendLine($"{damage} 데미지를 입었다! ({playerCombatData.hp}/{playerCombatData.maxHp})");
                UpdateCombatText();
                await UniTask.Delay(delayTime);

                if (playerCombatData.hp <= 0)
                {
                    sb.AppendLine($"{playerData.name}이/가 사망했습니다...");
                    UpdateCombatText();
                    EndCombat(false);
                    return;
                }
            }
            else
            {
                sb.AppendLine($"{playerData.name}이/가 보스의 공격을 회피했습니다!");
                UpdateCombatText();
                await UniTask.Delay(delayTime);
            }
        }

        sb.AppendLine($"{playerData.name}은/는 너무 길어진 전투에 지쳐버렸다...");
        UpdateCombatText();

        await UniTask.Delay(delayTime);
        EndCombat(false);
    }

    private void UpdateCombatText()
    {
        combatText.text = sb.ToString(); // StringBuilder 내용을 UI에 반영

        // Canvas.ForceUpdateCanvases(); // 즉시 레이아웃 업데이트
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private bool IsAvoided(float avoidanceRate)
    {
        // 만분위 체크: 0~100000 범위의 랜덤 값 생성
        int randomValue = Random.Range(0, 10000); // 0부터 100000까지 포함
        int threshold = Mathf.RoundToInt(avoidanceRate * 10000); // 회피율을 0~100000 범위로 변환
        HLLogger.Log($"회피 판정 - Random: {randomValue}, Threshold: {threshold} / 회피? {randomValue < threshold}");

        return randomValue < threshold;
    }

    private int CalculateDamage(combatStatData attacker, combatStatData defender, out bool isCritical)
    {
        // 크리티컬 판정
        int randomValue = Random.Range(0, 10000); // 0부터 100000까지 포함
        int threshold = Mathf.RoundToInt(attacker.criRate * 10000); // 회피율을 0~100000 범위로 변환

        isCritical = randomValue < threshold;
        float criticalMultiplier = isCritical ? 1 + attacker.criDmg : 1;
        HLLogger.Log($"크리티컬 판정 - Random: {randomValue}, Threshold: {threshold} / 크리티컬? {isCritical}");

        // 데미지 계산
        int rawDamage = Mathf.Max(0, Mathf.RoundToInt(attacker.atk * criticalMultiplier) - defender.def);
        HLLogger.Log($"기본 {attacker.atk}, 크뎀 {Mathf.RoundToInt(attacker.atk * criticalMultiplier)}, 상대 방어력 {defender.def}, 최종뎀 {rawDamage}");
        return Mathf.Max(1, rawDamage); // 최소 데미지는 1
    }

    private async void EndCombat(bool playerWon)
    {
        if (playerWon)
        {
            sb.AppendLine("");
            sb.AppendLine("---------------------------------");
            sb.AppendLine("전투에서 승리했습니다!");
            sb.AppendLine($"{rewardCoin}코인, {rewardExp}경험치 획득!");
            UpdateCombatText();

            SaveDataManager.instance.AddCoin(rewardCoin);
            if (SaveDataManager.instance.AddExp(rewardExp))
            {
                await UniTask.Delay(1000);
                LobbyUIManager.instance.ShowPopup<LevelUpPopup>();
            }
            GameListView.instance.UpdateUserProfileProcess();
            SaveDataManager.instance.AddTowerFloor();
        }
        else
        {
            sb.AppendLine("");
            sb.AppendLine("---------------------------------");
            sb.AppendLine("전투에서 패배했습니다...");
            UpdateCombatText();
        }

        retryButton.interactable = !playerWon;
        retryButton.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(true);
        TowerGamePopup.instance.UpdatePopupData();
    }



    private class combatStatData
    {
        public int atk;
        public int def;
        public int hp;
        public float maxHp;
        public float criRate;
        public float criDmg;
        public float avoidance; //회피율
    }



    public void OnClickSpeedButton()
    {
        speedMode = (speedMode + 1) % 3;

        delayTime = speedMode switch
        {
            0 => 800,
            1 => 400,
            2 => 200,
            _ => 800
        };

        HLLogger.Log($"Mode {speedMode} : {delayTime}ms");
    }

    public void OnClickRetryButton()
    {
        SetCombatData();
        UpdateUI();
        CombatProcess();
    }
}
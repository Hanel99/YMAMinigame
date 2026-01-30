using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TowerGameCombatPopup : PopupBase
{
    public static TowerGameCombatPopup instance { get; private set; }

    public TowerGameCombatEntity playerEntity;
    public TowerGameCombatEntity bossEntity;


    public ScrollRect scrollRect;
    public Text combatText;
    public Button skipButton;
    public Button retryButton;
    public Button confirmButton;

    //private 
    PlayerData playerData = null;
    private int turnCount = 0;
    private int playerAvoidCount = 0;


    private CombatStatData playerCombatData = new();
    private CombatStatData bossCombatData = new();
    private int bossNumber;
    private int rewardCoin;
    private int rewardExp;
    private StringBuilder sb = new StringBuilder();

    private int delayTime = 200;
    private int lineCount = 0;
    private int combatDataCount = 0;
    private bool isSkip = false;




    private bool @combatTest = false;


    // Consts
    private const int SCALE_PROBABILITY = 10000;
    private const int MAX_TURN_COUNT = 20;
    private const int DELAY_TURN_DEFAULT = 200;
    private const int DELAY_RESULT_DEFAULT = 800;
    private const int DELAY_SKIPPED = 1;


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
            CombatProcess().Forget();
        }
        else
        {
            _CloseWindow();
        }
    }

    private void SetCombatData()
    {
        isSkip = false;
        delayTime = DELAY_TURN_DEFAULT;
        turnCount = 0;
        lineCount = 0;
        combatDataCount = 0;
        playerAvoidCount = 0;
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

        bossNumber = TowerGamePopup.instance.GetBossNumber(playerData.towerFloor);
        playerEntity.SetData(playerCombatData.hp, true);
        bossEntity.SetData(bossCombatData.hp, false, bossNumber);
    }
    private T GetValue<T>(TowerUserStatType type, int level)
    {
        return GameResourceManager.instance.GetTowerUserLevelStatValue<T>(type, level);
    }

    public void UpdateUI()
    {
        skipButton.gameObject.SetActive(false);
        retryButton.gameObject.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        UpdateCombatText().Forget();
    }

    public async UniTask CombatProcess()
    {
        SoundManager.instance.PlaySFX(SFXType.TowerStart);
        string bossName = LocalizeManager.instance.GetString($"Tower.Boss.Name.{bossNumber.ToString("D2")}");

        // 전투 시뮬레이션을 먼저 모두 실행해서 로그와 결과를 얻음
        var (lines, combatDetails, playerWon) = await SimulateCombat(bossName);
        await UniTask.Delay(DELAY_RESULT_DEFAULT);

        // 시뮬레이션 결과(라인들)를 순차적으로 UI에 표시
        sb.Clear();
        foreach (var line in lines)
        {
            sb.AppendLine(line);

            if (line.StartsWith("-") || line == "")
            {
                // 구분선이나 빈 줄은 바로 업데이트                
            }
            else
            {
                lineCount++;
                UpdateCombatText().Forget();

                if (line.Contains("데미지"))
                {
                    var detail = combatDetails[combatDataCount];
                    combatDataCount++;

                    if (isSkip == false)
                        SoundManager.instance.PlaySFX(SFXType.TowerHit2);
                    if (detail.isPlayerAttack)
                    {
                        bossEntity.GetDamage(detail.damage, isSkip);
                    }
                    else
                    {
                        playerEntity.GetDamage(detail.damage, isSkip);
                    }
                }
                else if (line.Contains("회피"))
                {
                    var detail = combatDetails[combatDataCount];
                    combatDataCount++;
                    if (isSkip == false)
                    {
                        SoundManager.instance.PlaySFX(SFXType.TowerHit);
                        if (detail.isPlayerAttack)
                        {
                            bossEntity.AvoidAnimation();
                        }
                        else
                        {
                            playerEntity.AvoidAnimation();
                        }
                    }
                }

                await UniTask.Delay(delayTime);
            }

            if (lineCount >= 7 && isSkip == false)
            {
                skipButton.gameObject.SetActive(true);
            }
        }

        // 결과 처리
        await UniTask.Delay(delayTime);
        await EndCombat(playerWon);
    }

    private async UniTask UpdateCombatText()
    {
        combatText.text = sb.ToString(); // StringBuilder 내용을 UI에 반영

        await UniTask.DelayFrame(1);
        scrollRect.verticalNormalizedPosition = 0f;
        combatText.gameObject.SetActive(false);
        combatText.gameObject.SetActive(true);
    }

    private bool IsAvoided(float avoidanceRate)
    {
        // 만분위 체크: 0~100000 범위의 랜덤 값 생성
        int randomValue = UnityEngine.Random.Range(0, SCALE_PROBABILITY); // 0부터 10000까지 포함
        int threshold = Mathf.RoundToInt(avoidanceRate * SCALE_PROBABILITY); // 회피율을 0~10000 범위로 변환
        HLLogger.Log($"회피 판정 - Random: {randomValue}, Threshold: {threshold} / 회피? {randomValue < threshold}");

        return randomValue < threshold;
    }

    private int CalculateDamage(CombatStatData attacker, CombatStatData defender, out bool isCritical)
    {
        // 크리티컬 판정
        int randomValue = UnityEngine.Random.Range(0, SCALE_PROBABILITY); // 0부터 10000까지 포함
        int threshold = Mathf.RoundToInt(attacker.criRate * SCALE_PROBABILITY); // 크리티컬율을 0~10000 범위로 변환

        isCritical = randomValue < threshold;
        float criticalMultiplier = isCritical ? 1 + attacker.criDmg : 1;
        HLLogger.Log($"크리티컬 판정 - Random: {randomValue}, Threshold: {threshold} / 크리티컬? {isCritical}");

        // 데미지 계산
        int rawDamage = Mathf.Max(0, Mathf.RoundToInt(attacker.atk * criticalMultiplier) - defender.def);
        HLLogger.Log($"기본 {attacker.atk}, 크뎀 {Mathf.RoundToInt(attacker.atk * criticalMultiplier)}, 상대 방어력 {defender.def}, 최종뎀 {rawDamage}");
        return Mathf.Max(1, rawDamage); // 최소 데미지는 1
    }

    private async UniTask EndCombat(bool playerWon)
    {
        if (playerWon)
        {
            int earnCoinAmount = rewardCoin;
            int earnExpAmount = rewardExp;

            if (SaveDataManager.instance.playerData.finalQuizPlayData.playEndRoll)
            {
                earnCoinAmount = Math.Min(StaticGameData.MAX_COIN_VALUE, earnCoinAmount * 50);
                earnExpAmount *= 10;
            }

            bossEntity.DieAnimation();
            sb.AppendLine("");
            sb.AppendLine("---------------------------------");
            sb.AppendLine("전투에서 승리했습니다!");
            sb.AppendLine($"{earnCoinAmount:N0}코인, {earnExpAmount}경험치 획득!");
            UpdateCombatText().Forget();

            if (playerAvoidCount >= 3)
                QuestManager.instance.AddSpecialMission(QuestDetailType.S_WinWithAvoid, 0);

            SaveDataManager.instance.AddCoin(earnCoinAmount, false);
            if (SaveDataManager.instance.AddExp(earnExpAmount, false))
            {
                await UniTask.Delay(1000);
                LobbyUIManager.instance.ShowPopup<LevelUpPopup>();
            }
            GameListView.instance.UpdateUserProfileProcess();
            SaveDataManager.instance.AddTowerFloor();
        }
        else
        {
            playerEntity.DieAnimation();
            sb.AppendLine("");
            sb.AppendLine("---------------------------------");
            sb.AppendLine("전투에서 패배했습니다...");
            UpdateCombatText().Forget();
        }

        QuestManager.instance.AddTowerCombatData(1);

        skipButton.gameObject.SetActive(false);
        if (playerWon)
            retryButton.transform.Find("text").GetComponent<Text>().text = "다음 도전";
        if (SaveDataManager.instance.playerData.towerFloor < 10)
            retryButton.interactable = false;

        retryButton.gameObject.SetActive(true);
        confirmButton.gameObject.SetActive(true);
        TowerGamePopup.instance.UpdatePopupData();
    }



    private class CombatStatData
    {
        public int atk;
        public int def;
        public int hp;
        public float maxHp;
        public float criRate;
        public float criDmg;
        public float avoidance; //회피율
    }

    private class CombatDetailData
    {
        public bool isPlayerAttack = false;
        public bool isAvoided = false;
        public bool isCritical = false;
        public int damage = 0;
    }


    public void OnClickSkipButton()
    {
        if (isSkip) return;

        isSkip = true;
        skipButton.gameObject.SetActive(false);
        delayTime = DELAY_SKIPPED;
    }

    public void OnClickRetryButton()
    {
        SetCombatData();
        UpdateUI();
        CombatProcess().Forget();
    }

    // 전투를 실제로 계산만 하고, 텍스트(라인) 리스트와 승패를 반환
    private async UniTask<(List<string> lines, List<CombatDetailData> combatDetails, bool playerWon)> SimulateCombat(string bossName)
    {
        var lines = new List<string>();
        var combatDetails = new List<CombatDetailData>();

        // 로컬 복사본으로 시뮬레이션 (원본 데이터는 변경하지 않음)
        var p = new CombatStatData
        {
            atk = playerCombatData.atk,
            def = playerCombatData.def,
            hp = playerCombatData.hp,
            maxHp = playerCombatData.maxHp,
            criRate = playerCombatData.criRate,
            criDmg = playerCombatData.criDmg,
            avoidance = playerCombatData.avoidance
        };

        var b = new CombatStatData
        {
            atk = bossCombatData.atk,
            def = bossCombatData.def,
            hp = bossCombatData.hp,
            maxHp = bossCombatData.maxHp,
            criRate = bossCombatData.criRate,
            criDmg = bossCombatData.criDmg,
            avoidance = bossCombatData.avoidance
        };

        turnCount = 0;
        while (p.hp > 0 && b.hp > 0 && turnCount < MAX_TURN_COUNT)
        {
            await UniTask.Delay(1); // 시뮬레이션 속도 조절용 딜레이

            turnCount++;
            lines.Add("");
            lines.Add("---------------------------------");
            lines.Add($"{turnCount}번째 턴!");

            // 플레이어 공격
            bool bossDefeated = ProcessAttack(
                p, b,
                attackerName: playerData.name,
                defenderName: bossName,
                isPlayerAttacking: true,
                lines, combatDetails
            );
            if (bossDefeated) return (lines, combatDetails, true);


            // 보스 공격
            bool playerDefeated = ProcessAttack(
                b, p,
                attackerName: bossName,
                defenderName: playerData.name,
                isPlayerAttacking: false,
                lines, combatDetails
            );
            if (playerDefeated) return (lines, combatDetails, false);

        }

        lines.Add("");
        lines.Add($"{playerData.name.En_Nun()} 너무 길어진 전투에 지쳐버렸다...");
        return (lines, combatDetails, false);
    }

    /// <summary>
    /// 공격 처리 공통 로직
    /// </summary>
    /// <returns>방어자가 죽었는지 여부</returns>
    private bool ProcessAttack(
        CombatStatData attacker,
        CombatStatData defender,
        string attackerName,
        string defenderName,
        bool isPlayerAttacking,
        List<string> lines,
        List<CombatDetailData> combatDetails)
    {
        lines.Add("");
        lines.Add($"{attackerName}의 공격!");

        if (!IsAvoided(defender.avoidance))
        {
            bool isCritical;
            int damage = CalculateDamage(attacker, defender, out isCritical);

            if (isCritical)
                lines.Add($"{attackerName} 혼신의 일격!");
            else
                lines.Add($"{attackerName.E_Ga()} {defenderName.Eul_Reul()} 공격!");

            if (@combatTest) damage = 10;

            defender.hp -= damage;
            defender.hp = Mathf.Max(0, defender.hp);

            if (isPlayerAttacking)
                lines.Add($"{damage} 데미지를 입혔다! ({defender.hp}/{defender.maxHp})");
            else
                lines.Add($"{damage} 데미지를 입었다! ({defender.hp}/{defender.maxHp})");


            combatDetails.Add(new CombatDetailData { isPlayerAttack = isPlayerAttacking, isCritical = isCritical, damage = damage });

            if (defender.hp <= 0)
            {
                lines.Add("");
                if (isPlayerAttacking)
                    lines.Add($"{defenderName.Eul_Reul()} 물리쳤습니다!");
                else
                    lines.Add($"{defenderName.E_Ga()} 쓰러졌습니다...");

                return true;
            }
        }
        else
        {
            // 회피 발생
            if (!isPlayerAttacking) playerAvoidCount++; // 보스가 공격했는데 플레이어가 피함 -> 플레이어 회피 카운트 증가

            lines.Add($"{defenderName.E_Ga()} {attackerName}의 공격을 회피했습니다!");
            combatDetails.Add(new CombatDetailData { isPlayerAttack = isPlayerAttacking, isAvoided = true });
        }

        return false;
    }

    #region Combat Animation




    #endregion
}
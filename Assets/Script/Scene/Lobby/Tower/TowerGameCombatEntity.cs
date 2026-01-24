using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TowerGameCombatEntity : MonoBehaviour
{
    public Image image;
    public Image punch;
    public Slider hpBar;
    public Text hpText;
    public Text damageText;


    //private
    private bool isPlayer;
    private int currentHp;
    private int maxHp;



    // Consts
    private const float DURATION_HP_BAR = 0.3f;
    private const float DURATION_DAMAGE_COLOR = 0.3f;
    private const float DURATION_DAMAGE_TEXT_MOVE = 0.5f;
    private const float DURATION_DAMAGE_TEXT_FADE = 0.5f;

    private const float POS_Y_DAMAGE_TEXT_FROM = -80f;
    private const float POS_Y_DAMAGE_TEXT_TO = -95f;

    private const float DURATION_AVOID_MOVE = 0.15f;
    private const float DELAY_AVOID_RETURN = 0.1f;
    private const float POS_X_AVOID_PLAYER = -45f;
    private const float POS_X_AVOID_BOSS = 45f;

    private const float DURATION_PUNCH_MOVE = 0.2f;
    private const float POS_X_PUNCH_PLAYER_FROM = 70f;
    private const float POS_X_PUNCH_PLAYER_TO = 45f;
    private const float POS_X_PUNCH_BOSS_FROM = -80f;
    private const float POS_X_PUNCH_BOSS_TO = -55f;
    private const float POS_Y_PUNCH_FROM = 50f;
    private const float POS_Y_PUNCH_TO = 23f;

    private const float DURATION_DIE_FADE = 0.5f;


    void OnDisable()
    {
        image.DOKill();
        punch.DOKill();
        damageText.DOKill();
        damageText.transform.DOKill();
    }

    public void SetData(int maxHp, bool isPlayer, int bossNumber = -1)
    {
        //이미지 셋팅, hp 셋팅
        this.isPlayer = isPlayer;

        if (isPlayer)
            image.sprite = GameResourceManager.instance.GetMasterIcon(SaveDataManager.instance.playerData.master);
        else
            image.sprite = GameResourceManager.instance.GetTowerBossImage(bossNumber, false, false);

        image.color = Color.white;

        currentHp = maxHp;
        this.maxHp = maxHp;
        UpdateHpBar(true);
    }


    void UpdateHpBar(bool immediate = false)
    {
        float ratio = (float)currentHp / maxHp;
        hpText.text = $"{currentHp} / {maxHp}";

        if (immediate)
            hpBar.value = ratio;
        else
            hpBar.DOValue(ratio, DURATION_HP_BAR).SetEase(Ease.Linear);
    }

    public void GetDamage(int damage, bool immediate = false)
    {
        currentHp -= damage;
        currentHp = Mathf.Max(0, currentHp);
        UpdateHpBar(immediate);

        HLLogger.Log($"GetDamage: {damage} -> left {currentHp}, immediate: {immediate}");

        if (immediate == false)
        {
            //애니메이션으로 데미지 표시
            PunchAnimation();
            image.DOKill();
            image.DOColor(Color.white, DURATION_DAMAGE_COLOR).From(Color.red).SetEase(Ease.Linear);
            ShowDamageText(damage);
        }
    }

    private void ShowDamageText(int damage)
    {
        damageText.text = $"-{damage}";
        damageText.gameObject.SetActive(true);

        damageText.DOKill();
        damageText.transform.DOKill();

        damageText.transform.DOLocalMoveY(POS_Y_DAMAGE_TEXT_TO, DURATION_DAMAGE_TEXT_MOVE).From(POS_Y_DAMAGE_TEXT_FROM).SetEase(Ease.OutCubic);
        damageText.DOFade(0, DURATION_DAMAGE_TEXT_FADE).From(1).SetEase(Ease.Linear).onComplete = () =>
        {
            damageText.gameObject.SetActive(false);
        };
    }


    public void AvoidAnimation()
    {
        //회피 애니메이션
        image.DOKill();
        float movePositionX = isPlayer ? POS_X_AVOID_PLAYER : POS_X_AVOID_BOSS;

        PunchAnimation();
        image.transform.DOLocalMoveX(movePositionX, DURATION_AVOID_MOVE).From(0f).SetEase(Ease.InOutSine).SetDelay(0f).OnComplete(() =>
        {
            DOVirtual.DelayedCall(DELAY_AVOID_RETURN, () =>
            {
                image.transform.DOLocalMoveX(0f, DURATION_AVOID_MOVE).SetEase(Ease.InOutSine);
            });
        });
    }

    public void PunchAnimation()
    {
        //공격 애니메이션
        float fromX = isPlayer ? POS_X_PUNCH_PLAYER_FROM : POS_X_PUNCH_BOSS_FROM;
        float toX = isPlayer ? POS_X_PUNCH_PLAYER_TO : POS_X_PUNCH_BOSS_TO;
        Vector3 from = new Vector3(fromX, POS_Y_PUNCH_FROM, 0f);
        Vector3 to = new Vector3(toX, POS_Y_PUNCH_TO, 0f);

        punch.DOKill();
        punch.gameObject.SetActive(true);
        punch.transform.DOLocalMove(to, DURATION_PUNCH_MOVE).From(from).SetEase(Ease.OutBack).OnComplete(() =>
        {
            punch.gameObject.SetActive(false);
        });
    }

    public void DieAnimation()
    {
        //죽는 애니메이션
        SoundManager.instance.PlaySFX(SFXType.TowerDie);
        image.DOKill();
        image.DOFade(0, DURATION_DIE_FADE).From(1).SetEase(Ease.Linear);
    }
}



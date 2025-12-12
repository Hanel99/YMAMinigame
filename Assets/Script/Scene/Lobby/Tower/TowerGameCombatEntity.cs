using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TowerGameCombatEntity : MonoBehaviour
{
    public Image image;
    public Slider hpBar;
    public Text hpText;
    public Text damageText;


    //private
    private bool isPlayer;
    private int currentHp;
    private int maxHp;



    void OnDisable()
    {
        image.DOKill();
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
            hpBar.DOValue(ratio, 0.3f).SetEase(Ease.Linear);
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
            image.DOKill();
            image.DOColor(Color.white, 0.3f).From(Color.red).SetEase(Ease.Linear);
            ShowDamageText(damage);
        }
    }

    private void ShowDamageText(int damage)
    {
        damageText.text = $"-{damage}";
        damageText.gameObject.SetActive(true);

        damageText.DOKill();
        damageText.transform.DOKill();

        damageText.transform.DOLocalMoveY(80f, 0.5f).From(65f).SetEase(Ease.OutCubic);
        damageText.DOFade(0, 0.5f).From(1).SetEase(Ease.Linear).onComplete = () =>
        {
            damageText.gameObject.SetActive(false);
        };
    }


    public void AvoidAnimation()
    {
        //회피 애니메이션
        image.DOKill();
        float movePositionX = isPlayer ? -45f : 45f;

        image.transform.DOLocalMoveX(movePositionX, 0.15f).From(0f).SetEase(Ease.InOutSine).SetDelay(0f).OnComplete(() =>
        {
            DOVirtual.DelayedCall(0.1f, () =>
            {
                image.transform.DOLocalMoveX(0f, 0.15f).SetEase(Ease.InOutSine);
            });
        });
    }

    public void DieAnimation()
    {
        //죽는 애니메이션
        image.DOKill();
        image.DOFade(0, 0.5f).From(1).SetEase(Ease.Linear);
    }


}

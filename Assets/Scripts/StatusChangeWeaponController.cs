using UnityEngine;
using System.Collections;

public class StatusChangeWeaponController : WeaponController
{
    [SerializeField]
    private int effectType;
    [SerializeField]
    private float effectTime;
    [SerializeField]
    private float effectRate;
    [SerializeField]
    private GameObject effect;

    const int EFFECT_ATTACK = 1;
    const int EFFECT_RECOVER_SP = 2;
    const int EFFECT_AVOID = 3;
    const int EFFECT_SPEED = 4;

    protected override void Awake()
    {
        base.Awake();

        if (effect != null) effect.SetActive(false);
    }

    protected override void Action()
    {
        if (playerStatus == null) return;

        switch (effectType)
        {
            case EFFECT_ATTACK:
                //攻撃力
                playerStatus.ChangeAttackRate(effectRate, effectTime, effect);
                break;

            case EFFECT_RECOVER_SP:
                //SP回復量
                playerStatus.AccelerateRecoverSp(effectRate, effectTime, effect);
                break;

            case EFFECT_AVOID:
                //回避時間
                playerStatus.AvoidBurst(effectRate, effectTime, effect);
                break;

            case EFFECT_SPEED:
                //移動速度
                playerStatus.AccelerateRunSpeed(effectRate, effectTime, effect);
                break;
        }

        base.StartReload(effectTime);
    }
}

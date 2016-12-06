using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class BulletLevelController : EffectLevelController
{
    //##### 強化System #####

    //発射数増加
    const int CUSTOM_SYSTEM_RAPID_COUNT = 101;
    //発射間隔
    const int CUSTOM_SYSTEM_RAPID_INTERVAL = 102;
    //拡散数
    const int CUSTOM_SYSTEM_SPREAD_COUNT = 103;
    //発射角
    const int CUSTOM_SYSTEM_SPREAD_DIFF = 104;
    //ブレ
    const int CUSTOM_SYSTEM_FOCUS_DIFF = 105;
    //弾交換(AddObject使用)
    const int CUSTOM_SYSTEM_CHANGE_BULLET = 106;

    //ダメージアップ
    const int CUSTOM_SYSTEM_DAMAGE = 131;
    //旋回速度UP
    const int CUSTOM_SYSTEM_TURN_SPEED = 132;
    //判定拡大
    const int CUSTOM_SYSTEM_COLLIDER = 133;
    //ActiveTime
    const int CUSTOM_SYSTEM_ACTIVE_TIME = 134;
    //ActiveDistance
    const int CUSTOM_SYSTEM_ACTIVE_DISTANCE = 135;
    //StuckTime
    const int CUSTOM_SYSTEM_STUCK_TIME = 136;
    //Knockback
    const int CUSTOM_SYSTEM_KNOCKBACK = 137;
    //ChangeHitEffect
    const int CUSTOM_SYSTEM_HIT_EFFECT = 138;
    //ChangeBreakEffect
    const int CUSTOM_SYSTEM_BREAK_EFFECT = 139;
    //Speed
    const int CUSTOM_SYSTEM_SPEED = 140;
    //Scale
    const int CUSTOM_SYSTEM_SCALE = 141;
    //LockStartTime
    const int CUSTOM_SYSTEM_LOCK_TIME = 142;
    //LockedSpeedRate
    const int CUSTOM_SYSTEM_LOCKED_SPEED_RATE = 143;
    //LockedTurnSpeedRate
    const int CUSTOM_SYSTEM_LOCKED_TURN_RATE = 144;
    //IsPhysicsBulletBreak
    const int CUSTOM_SYSTEM_PHYSICS_BREAK = 145;
    //IsEnergyBulletBreak
    const int CUSTOM_SYSTEM_ENERGY_BREAK = 146;
    //IsHitBreak
    const int CUSTOM_SYSTEM_HIT_BREAK = 147;


    //状態異常：ATK
    const int CUSTOM_SYSTEM_DEBUFF_ATTACK = 151;
    //状態異常：SP
    const int CUSTOM_SYSTEM_DEBUFF_SP = 152;
    //状態異常：AVD
    const int CUSTOM_SYSTEM_DEBUFF_AVOID = 153;
    //状態異常：SPD
    const int CUSTOM_SYSTEM_DEBUFF_SPEED = 154;
    //状態異常：DEF
    const int CUSTOM_SYSTEM_DEBUFF_DEFENCE = 155;
    //状態異常：Time
    const int CUSTOM_SYSTEM_DEBUFF_TIME = 156;


    private BulletWeaponController _bulletWeaponCtrl;
    protected BulletWeaponController bulletWeaponCtrl
    {
        get { return _bulletWeaponCtrl ? _bulletWeaponCtrl : _bulletWeaponCtrl = GetComponent<BulletWeaponController>(); }
    }

    //武器強化実行
    protected override void WeaponCustomExe(int customSystem, float effectValue)
    {
        switch (customSystem)
        {
            case CUSTOM_SYSTEM_RAPID_COUNT:
                //発射数増加
                bulletWeaponCtrl.CustomRapidCount((int)effectValue);
                break;

            case CUSTOM_SYSTEM_RAPID_INTERVAL:
                //発射間隔
                bulletWeaponCtrl.CustomRapidInterval(effectValue);
                break;

            case CUSTOM_SYSTEM_SPREAD_COUNT:
                //同時発射数
                bulletWeaponCtrl.CustomSpreadCount((int)effectValue);
                break;

            case CUSTOM_SYSTEM_SPREAD_DIFF:
                //発射角
                bulletWeaponCtrl.CustomSpreadDiff((int)effectValue);
                break;

            case CUSTOM_SYSTEM_FOCUS_DIFF:
                //ブレ抑制
                bulletWeaponCtrl.CustomFocusDiff(effectValue);
                break;

            case CUSTOM_SYSTEM_CHANGE_BULLET:
                //弾変更
                bulletWeaponCtrl.CustomChangeBullet(addObject);
                break;

            default:
                base.WeaponCustomExe(customSystem, effectValue);
                break;
        }
    }

    //弾丸強化
    public void BulletCustom(BulletController bulletCtrl)
    {
        UnityAction callback = () =>
        {
            for (int i = 0; i < customSystemList.Count; i++)
            {
                int customSystem = customSystemList[i];
                float effectValue = effectValueList[i] * myCustomLevel;
                BulletCustomExe(bulletCtrl, customSystem, effectValue);
            }
        };
        WaitCustomReady(callback);
    }

    //弾丸強化実行
    protected virtual void BulletCustomExe(BulletController bulletCtrl, int customSystem, float effectValue)
    {
        switch (customSystem)
        {
            case CUSTOM_SYSTEM_DAMAGE:
                //ダメージアップ
                bulletCtrl.CustomDamage((int)effectValue);
                break;

            case CUSTOM_SYSTEM_TURN_SPEED:
                //旋回速度UP
                bulletCtrl.CustomTurnSpeed(effectValue);
                break;

            case CUSTOM_SYSTEM_COLLIDER:
                //判定拡大
                bulletCtrl.CustomCollider(effectValue);
                break;

            case CUSTOM_SYSTEM_ACTIVE_TIME:
                //ActiveTime
                bulletCtrl.CustomActiveTime(effectValue);
                break;

            case CUSTOM_SYSTEM_ACTIVE_DISTANCE:
                //ActiveDistance
                bulletCtrl.CustomActiveDistance(effectValue);
                break;

            case CUSTOM_SYSTEM_STUCK_TIME:
                //StuckTime
                bulletCtrl.CustomStuckTime(effectValue);
                break;

            case CUSTOM_SYSTEM_KNOCKBACK:
                //Knockback
                bulletCtrl.CustomKnockBack(effectValue);
                break;

            case CUSTOM_SYSTEM_HIT_EFFECT:
                //HitEffect
                bulletCtrl.CustomHitEffect(addObject);
                break;

            case CUSTOM_SYSTEM_BREAK_EFFECT:
                //BreakEffect
                bulletCtrl.CustomBreakEffect(addObject);
                break;

            case CUSTOM_SYSTEM_SPEED:
                //Speed
                bulletCtrl.CustomSpeed(effectValue);
                break;

            case CUSTOM_SYSTEM_SCALE:
                //Scale
                bulletCtrl.CustomScale(effectValue);
                break;

            case CUSTOM_SYSTEM_LOCK_TIME:
                //Scale
                bulletCtrl.CustomLockTime(effectValue);
                break;

            case CUSTOM_SYSTEM_LOCKED_SPEED_RATE:
                //Scale
                bulletCtrl.CustomLockedSpeedRate(effectValue);
                break;

            case CUSTOM_SYSTEM_LOCKED_TURN_RATE:
                //Scale
                bulletCtrl.CustomLockedTurnRate(effectValue);
                break;

            case CUSTOM_SYSTEM_PHYSICS_BREAK:
                bulletCtrl.CustomPhysicsBreak(effectValue == 1);
                break;

            case CUSTOM_SYSTEM_ENERGY_BREAK:
                bulletCtrl.CustomEnergyBreak(effectValue == 1);
                break;

            case CUSTOM_SYSTEM_HIT_BREAK:
                bulletCtrl.CustomHitBreak(effectValue == 1);
                break;
                
            case CUSTOM_SYSTEM_DEBUFF_ATTACK:
                //debuff:attack
                bulletCtrl.CustomDebuffAttack(effectValue);
                break;

            case CUSTOM_SYSTEM_DEBUFF_SP:
                //debuff:sp
                bulletCtrl.CustomDebuffSp(effectValue);
                break;

            case CUSTOM_SYSTEM_DEBUFF_AVOID:
                //debuff:avoid
                bulletCtrl.CustomDebuffAvoid(effectValue);
                break;

            case CUSTOM_SYSTEM_DEBUFF_SPEED:
                //debuff:speed
                bulletCtrl.CustomDebuffSpeed(effectValue);
                break;

            case CUSTOM_SYSTEM_DEBUFF_DEFENCE:
                //debuff:defence
                bulletCtrl.CustomDebuffDefence(effectValue);
                break;

            case CUSTOM_SYSTEM_DEBUFF_TIME:
                //debuff:time
                bulletCtrl.CustomDebuffTime(effectValue);
                break;
        }
    }
}
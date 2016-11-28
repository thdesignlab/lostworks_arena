﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class BulletLevelController : WeaponLevelController
{
    [SerializeField, TooltipAttribute("101:RpdCnt,102:RpdInt,103:SpreadCnt,104:SpreadDiff,105:FocusDiff,106:changeBullet")]
    protected List<int> powerCustomSystemList;
    [SerializeField]
    protected List<float> powerEffectValueDiffList;
    [SerializeField, TooltipAttribute("131:Dmg,132:TurnSpd,133:Collider,134:ActTime,135:ActDistance,136:Stuck,137:Knockback,138:HitEffect,139:BreakEffect,140:speed")]
    protected List<int> technicCustomSystemList;
    [SerializeField]
    protected List<float> technicEffectValueDiffList;
    [SerializeField, TooltipAttribute("(debuff)151:atk,152:sp,153:avd,154:spd,155:def,156:time")]
    protected List<int> uniqueCustomSystemList;
    [SerializeField]
    protected List<float> uniqueEffectValueDiffList;

    [SerializeField]
    protected GameObject AddObject;     //追加用オブジェクト

    private BulletWeaponController _bulletWeaponCtrl;
    protected BulletWeaponController bulletWeaponCtrl
    {
        get { return _bulletWeaponCtrl ? _bulletWeaponCtrl : _bulletWeaponCtrl = GetComponent<BulletWeaponController>(); }
    }

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


    //カスタムSystemセットアップ
    protected override void SetCustomSystem()
    {
        switch (myCustomType)
        {
            case Common.Weapon.CUSTOM_TYPE_POWER:
                customSystemList = powerCustomSystemList;
                effectValueList = powerEffectValueDiffList;
                break;

            case Common.Weapon.CUSTOM_TYPE_TECHNIC:
                customSystemList = technicCustomSystemList;
                effectValueList = technicEffectValueDiffList;
                break;

            case Common.Weapon.CUSTOM_TYPE_UNIQUE:
                customSystemList = uniqueCustomSystemList;
                effectValueList = uniqueEffectValueDiffList;
                break;
        }
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
                bulletWeaponCtrl.CustomChangeBullet(AddObject);
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
    protected void BulletCustomExe(BulletController bulletCtrl, int customSystem, float effectValue)
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
                bulletCtrl.CustomHitEffect(AddObject);
                break;

            case CUSTOM_SYSTEM_BREAK_EFFECT:
                //BreakEffect
                bulletCtrl.CustomBreakEffect(AddObject);
                break;

            case CUSTOM_SYSTEM_SPEED:
                //Speed
                bulletCtrl.CustomSpeed(effectValue);
                break;

            case CUSTOM_SYSTEM_DEBUFF_ATTACK:
                //debuff:attack
                bulletCtrl.CustomDebuffAttack(effectValue);
                break;

            case CUSTOM_SYSTEM_DEBUFF_SP:
                //debuff:sp

                break;

            case CUSTOM_SYSTEM_DEBUFF_AVOID:
                //debuff:avoid

                break;

            case CUSTOM_SYSTEM_DEBUFF_SPEED:
                //debuff:speed

                break;

            case CUSTOM_SYSTEM_DEBUFF_DEFENCE:
                //debuff:defence

                break;

            case CUSTOM_SYSTEM_DEBUFF_TIME:
                //debuff:time
                bulletCtrl.CustomDebuffTime(effectValue);
                break;
        }
    }
}
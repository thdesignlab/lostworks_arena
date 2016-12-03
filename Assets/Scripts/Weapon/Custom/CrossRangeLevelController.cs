using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class CrossRangeLevelController : EffectLevelController
{
    //##### 強化System #####

    //Blade変更
    const int CUSTOM_SYSTEM_CHANGE_BLADE = 501;
    //AttackTime
    const int CUSTOM_SYSTEM_ATTACKE_TIME = 502;
    //AttackWaitTime
    const int CUSTOM_SYSTEM_ATTACK_WAIT_TIME = 503;
    //BoostSpeed
    const int CUSTOM_SYSTEM_BOOST_SPEED = 504;
    //BoostTime
    const int CUSTOM_SYSTEM_BOOST_TIME = 505;
    //BoostWaitTime
    const int CUSTOM_SYSTEM_BOOST_WAIT_TIME = 506;
    //BoostCost
    const int CUSTOM_SYSTEM_BOOST_COST = 507;
    //追撃
    const int CUSTOM_SYSTEM_SECOND_ATTACK = 508;

    //BladeDamage
    const int CUSTOM_SYSTEM_BLADE_DAMAGE = 531;
    //IsPhysicsBulletBreak
    const int CUSTOM_SYSTEM_BLADE_PHYSICS_BREAK = 532;
    //IsEnergyBulletBreak
    const int CUSTOM_SYSTEM_BLADE_ENERGY_BREAK = 533;
    //BladeEndScale
    const int CUSTOM_SYSTEM_BLADE_END_SCALE = 534;
    //BladeHitEffect
    const int CUSTOM_SYSTEM_BLADE_HIT_EFFECT = 535;



    private CrossRangeWeaponController _crossRangeWeaponCtrl;
    protected CrossRangeWeaponController crossRangeWeaponCtrl
    {
        get { return _crossRangeWeaponCtrl ? _crossRangeWeaponCtrl : _crossRangeWeaponCtrl = GetComponent<CrossRangeWeaponController>(); }
    }


    //武器強化実行
    protected override void WeaponCustomExe(int customSystem, float effectValue)
    {
        switch (customSystem)
        {
            case CUSTOM_SYSTEM_CHANGE_BLADE:
                crossRangeWeaponCtrl.CustomChangeBlade(addObject);
                break;

            case CUSTOM_SYSTEM_ATTACKE_TIME:
                crossRangeWeaponCtrl.CustomAttackTime(effectValue);
                break;

            case CUSTOM_SYSTEM_ATTACK_WAIT_TIME:
                crossRangeWeaponCtrl.CustomAttackWaitTime(effectValue);
                break;

            case CUSTOM_SYSTEM_BOOST_SPEED:
                crossRangeWeaponCtrl.CustomBoostSpeed(effectValue);
                break;

            case CUSTOM_SYSTEM_BOOST_TIME:
                crossRangeWeaponCtrl.CustomBoostTime(effectValue);
                break;

            case CUSTOM_SYSTEM_BOOST_WAIT_TIME:
                crossRangeWeaponCtrl.CustomBoostWaitTime(effectValue);
                break;

            case CUSTOM_SYSTEM_BOOST_COST:
                crossRangeWeaponCtrl.CustomBoostCost((int)effectValue);
                break;

            case CUSTOM_SYSTEM_SECOND_ATTACK:
                crossRangeWeaponCtrl.CustomSecondAttack((int)effectValue);
                break;

            case CUSTOM_SYSTEM_BLADE_DAMAGE:
                crossRangeWeaponCtrl.CustomDamage((int)effectValue);
                break;

            case CUSTOM_SYSTEM_BLADE_PHYSICS_BREAK:
                crossRangeWeaponCtrl.CustomPhysicsBreak();
                break;

            case CUSTOM_SYSTEM_BLADE_ENERGY_BREAK:
                crossRangeWeaponCtrl.CustomEnergyBreak();
                break;

            case CUSTOM_SYSTEM_BLADE_END_SCALE:
                crossRangeWeaponCtrl.CustomEndScale(effectValue);
                break;

            case CUSTOM_SYSTEM_BLADE_HIT_EFFECT:
                crossRangeWeaponCtrl.CustomChangeHitEffect(addObject);
                break;

            default:
                base.WeaponCustomExe(customSystem, effectValue);
                break;
        }
    }
}
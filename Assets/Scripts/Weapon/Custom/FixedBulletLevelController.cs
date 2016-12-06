using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class FixedBulletLevelController : BulletLevelController
{
    //##### 強化System #####

    //固定している時間
    const int CUSTOM_SYSTEM_FIXED_TIME = 1101;
    //解除差分時間
    const int CUSTOM_SYSTEM_FIXED_DIFF_TIME = 1102;

    //固定するまでの時間
    const int CUSTOM_SYSTEM_BULLET_FIX_TIME = 1111;
    //固定解除後の弾速
    const int CUSTOM_SYSTEM_FIXED_SPEED = 1112;
    //固定解除後の旋回速度
    const int CUSTOM_SYSTEM_FIXED_TURN_SPEED = 1113;
    //自動固定解除時間
    const int CUSTOM_SYSTEM_AUTO_FIRE_TIME = 1114;
    //自動固定解除差分時間
    const int CUSTOM_SYSTEM_AUTO_FIRE_DIFF = 1115;


    private FixedBulletWeaponController _fixedBulletWeaponCtrl;
    protected FixedBulletWeaponController fixedBulletWeaponCtrl
    {
        get { return _fixedBulletWeaponCtrl ? _fixedBulletWeaponCtrl : _fixedBulletWeaponCtrl = GetComponent<FixedBulletWeaponController>(); }
    }

    //武器強化実行
    protected override void WeaponCustomExe(int customSystem, float effectValue)
    {
        switch (customSystem)
        {
            case CUSTOM_SYSTEM_FIXED_TIME:
                fixedBulletWeaponCtrl.CustomFixedTime(effectValue);
                break;

            case CUSTOM_SYSTEM_FIXED_DIFF_TIME:
                fixedBulletWeaponCtrl.CustomFixedDiffTime(effectValue);
                break;

            default:
                base.WeaponCustomExe(customSystem, effectValue);
                break;
        }
    }

    //弾丸強化実行
    protected override void BulletCustomExe(BulletController bulletCtrl, int customSystem, float effectValue)
    {
        FixedTrackingBulletController fixedBulletCtrl = bulletCtrl.GetComponent<FixedTrackingBulletController>();
        switch (customSystem)
        {
            case CUSTOM_SYSTEM_BULLET_FIX_TIME:
                fixedBulletCtrl.CustomFixTime(effectValue);
                break;

            case CUSTOM_SYSTEM_FIXED_SPEED:
                fixedBulletCtrl.CustomFixSpeed(effectValue);
                break;

            case CUSTOM_SYSTEM_FIXED_TURN_SPEED:
                fixedBulletCtrl.CustomFixTurnSpeed(effectValue);
                break;

            case CUSTOM_SYSTEM_AUTO_FIRE_TIME:
                fixedBulletCtrl.CustomAutoFireTime(effectValue);
                break;

            case CUSTOM_SYSTEM_AUTO_FIRE_DIFF:
                fixedBulletCtrl.CustomAutoFireDiff(effectValue);
                break;

            default:
                base.BulletCustomExe(bulletCtrl, customSystem, effectValue);
                break;
        }
    }
}
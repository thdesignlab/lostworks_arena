using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class ChargeBulletLevelController : BulletLevelController
{
    //##### 強化System #####

    //MaxChargeTime
    const int CUSTOM_SYSTEM_CHARGE_TIME = 1001;
    //MaxSpeedRate
    const int CUSTOM_SYSTEM_CHARGE_SPEED_RATE = 1002;
    //MaxDamageRate
    const int CUSTOM_SYSTEM_CHARGE_DAMAGE_RATE = 1003;
    //MaxSizeRate
    const int CUSTOM_SYSTEM_CHARGE_SIZE_RATE = 1004;
    //LimitTime
    const int CUSTOM_SYSTEM_CHARGE_LIMIT_TIME = 1005;


    //チャージ弾丸強化実行
    protected override void BulletCustomExe(BulletController bulletCtrl, int customSystem, float effectValue)
    {
        ChargeBulletController chargeBulletCtrl = bulletCtrl.transform.GetComponent<ChargeBulletController>();
        switch (customSystem)
        {
            case CUSTOM_SYSTEM_CHARGE_TIME:
                chargeBulletCtrl.CustomChargeTime(effectValue);
                break;
            case CUSTOM_SYSTEM_CHARGE_SPEED_RATE:
                chargeBulletCtrl.CustomSpeedRate(effectValue);
                break;
            case CUSTOM_SYSTEM_CHARGE_DAMAGE_RATE:
                chargeBulletCtrl.CustomDamageRate(effectValue);
                break;
            case CUSTOM_SYSTEM_CHARGE_SIZE_RATE:
                chargeBulletCtrl.CustomSizeRate(effectValue);
                break;
            case CUSTOM_SYSTEM_CHARGE_LIMIT_TIME:
                chargeBulletCtrl.CustomLimitTime(effectValue);
                break;
            default:
                base.BulletCustomExe(bulletCtrl, customSystem, effectValue);
                break;
        }
    }
}
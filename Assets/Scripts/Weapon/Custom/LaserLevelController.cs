using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class LaserLevelController : BulletLevelController
{
    //##### 強化System #####

    //LaserObject変更
    const int CUSTOM_SYSTEM_CHANGE_LASER_OBJECT = 901;
    //射程
    const int CUSTOM_SYSTEM_EFFECTIVE_LENGTH = 902;
    //幅
    const int CUSTOM_SYSTEM_EFFECTIVE_WIDTH = 903;
    //照射時間
    const int CUSTOM_SYSTEM_EFFECTIVE_TIME = 904;
    //最大射程までの時間
    const int CUSTOM_SYSTEM_EFFECTIVE_LENGTH_TIME = 905;
    //最大幅までの時間
    const int CUSTOM_SYSTEM_EFFECTIVE_WIDTH_TIME = 906;
    //回転速度
    const int CUSTOM_SYSTEM_TURN_SPEED_RATE = 907;

    protected override void WeaponCustomExe(int customSystem, float effectValue)
    {
        LaserWeaponController laserWeaponCtrl = GetComponent<LaserWeaponController>();
        switch (customSystem)
        {
            case CUSTOM_SYSTEM_CHANGE_LASER_OBJECT:
                laserWeaponCtrl.CustomChangeLaserObject(addObject);
                break;

            case CUSTOM_SYSTEM_EFFECTIVE_LENGTH:
                laserWeaponCtrl.CustomEffectiveLength(effectValue);
                break;

            case CUSTOM_SYSTEM_EFFECTIVE_WIDTH:
                laserWeaponCtrl.CustomEffectiveWidth(effectValue);
                break;

            case CUSTOM_SYSTEM_EFFECTIVE_TIME:
                laserWeaponCtrl.CustomEffectiveTime(effectValue);
                break;

            case CUSTOM_SYSTEM_EFFECTIVE_LENGTH_TIME:
                laserWeaponCtrl.CustomEffectiveLengthTime(effectValue);
                break;

            case CUSTOM_SYSTEM_EFFECTIVE_WIDTH_TIME:
                laserWeaponCtrl.CustomEffectiveWidthTime(effectValue);
                break;

            case CUSTOM_SYSTEM_TURN_SPEED_RATE:
                laserWeaponCtrl.CustomTurnSpeedRate(effectValue);
                break;

            default:
                base.WeaponCustomExe(customSystem, effectValue);
                break;
        }
    }
}
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class LaserLevelController : BulletLevelController
{
    //##### 強化System #####

    //BeamObject変更
    const int CUSTOM_SYSTEM_CHANGE_LASER_OBJECT = 901;
    const int CUSTOM_SYSTEM_EFFECTIVE_LENGTH = 902;
    const int CUSTOM_SYSTEM_EFFECTIVE_WIDTH = 903;
    const int CUSTOM_SYSTEM_EFFECTIVE_TIME = 904;
    const int CUSTOM_SYSTEM_EFFECTIVE_LENGTH_TIME = 905;
    const int CUSTOM_SYSTEM_EFFECTIVE_WIDTH_TIME = 906;

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

            default:
                base.WeaponCustomExe(customSystem, effectValue);
                break;
        }
    }
}
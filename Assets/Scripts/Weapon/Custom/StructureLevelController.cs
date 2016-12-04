using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class StructureLevelController : BulletLevelController
{
    //##### 強化System #####

    //HP
    const int CUSTOM_SYSTEM_STRUCTURE_HP = 801;
    //反射
    const int CUSTOM_SYSTEM_REFLECTION = 802;
    //BreakEffect
    const int CUSTOM_SYSTEM_CHANGE_BREAK_EFFECT = 803;


    //Structure強化実行
    protected override void BulletCustomExe(BulletController bulletCtrl, int customSystem, float effectValue)
    {
        StructureController structureCtrl = bulletCtrl.transform.GetComponent<StructureController>();
        switch (customSystem)
        {
            case CUSTOM_SYSTEM_STRUCTURE_HP:
                structureCtrl.CustomHp((int)effectValue);
                break;

            case CUSTOM_SYSTEM_REFLECTION:
                structureCtrl.CustomReflection(effectValue == 1);
                break;

            case CUSTOM_SYSTEM_CHANGE_BREAK_EFFECT:
                structureCtrl.CustomChangeBreakEffect(addObject);
                break;

            default:
                base.BulletCustomExe(bulletCtrl, customSystem, effectValue);
                break;
        }
    }
}
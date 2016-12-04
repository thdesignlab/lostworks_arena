using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class ChainBulletLevelController : BulletLevelController
{
    //##### 強化System #####

    //ChainObject変更
    const int CUSTOM_SYSTEM_CHANGE_CHAIN_OBJECT = 601;
    //Chain数
    const int CUSTOM_SYSTEM_CHAIN_COUNT = 602;
    //ChainObject変更
    const int CUSTOM_SYSTEM_CHAIN_TIME = 603;
    //ChainObject変更
    const int CUSTOM_SYSTEM_CHAIN_DISTANCE = 604;


    //連鎖弾丸強化実行
    protected override void BulletCustomExe(BulletController bulletCtrl, int customSystem, float effectValue)
    {
        ChainBulletController chainBulletCtrl = bulletCtrl.transform.GetComponent<ChainBulletController>();
        switch (customSystem)
        {
            case CUSTOM_SYSTEM_CHANGE_CHAIN_OBJECT:
                chainBulletCtrl.CustomChangeChainObject(addObject);
                break;

            case CUSTOM_SYSTEM_CHAIN_COUNT:
                chainBulletCtrl.CustomChainCount((int)effectValue);
                break;

            case CUSTOM_SYSTEM_CHAIN_TIME:
                chainBulletCtrl.CustomChainTime(effectValue);
                break;

            case CUSTOM_SYSTEM_CHAIN_DISTANCE:
                chainBulletCtrl.CustomChainDistance(effectValue);
                break;

            default:
                base.BulletCustomExe(bulletCtrl, customSystem, effectValue);
                break;
        }
    }
}
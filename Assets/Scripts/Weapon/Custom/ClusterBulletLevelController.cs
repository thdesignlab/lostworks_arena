using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class ClusterBulletLevelController : BulletLevelController
{
    //##### 強化System #####

    //ChildBullet変更
    const int CUSTOM_SYSTEM_CHANGE_CHILD_BULLET = 701;
    //ChildBullet数
    const int CUSTOM_SYSTEM_CHANGE_CHILD_COUNT = 702;
    //パージ距離
    const int CUSTOM_SYSTEM_CHANGE_PURGE_DISTANCE = 703;
    //パージ秒数
    const int CUSTOM_SYSTEM_CHANGE_PURGE_TIME = 704;
    //パージDIFF
    const int CUSTOM_SYSTEM_CHANGE_PURGE_DIFF = 705;
    //パージDestroy
    const int CUSTOM_SYSTEM_CHANGE_PURGE_DESTROY = 706;


    //拡散弾丸強化実行
    protected override void BulletCustomExe(BulletController bulletCtrl, int customSystem, float effectValue)
    {
        ClusterBulletController clusterBulletCtrl = bulletCtrl.transform.GetComponent<ClusterBulletController>();
        switch (customSystem)
        {
            case CUSTOM_SYSTEM_CHANGE_CHILD_BULLET:
                clusterBulletCtrl.CustomChangeChildBullet(addObject);
                break;
            case CUSTOM_SYSTEM_CHANGE_CHILD_COUNT:
                clusterBulletCtrl.CustomChildCount((int)effectValue);
                break;
            case CUSTOM_SYSTEM_CHANGE_PURGE_DISTANCE:
                clusterBulletCtrl.CustomPurgeDistance(effectValue);
                break;
            case CUSTOM_SYSTEM_CHANGE_PURGE_TIME:
                clusterBulletCtrl.CustomPurgeTime(effectValue);
                break;
            case CUSTOM_SYSTEM_CHANGE_PURGE_DIFF:
                clusterBulletCtrl.CustomPurgeDiff(effectValue);
                break;
            case CUSTOM_SYSTEM_CHANGE_PURGE_DESTROY:
                clusterBulletCtrl.CustomPurgeDestroy(effectValue == 1);
                break;
            default:
                base.BulletCustomExe(bulletCtrl, customSystem, effectValue);
                break;
        }
    }
}
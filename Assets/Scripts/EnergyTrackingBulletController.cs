using UnityEngine;
using System.Collections;

public class EnergyTrackingBulletController : EnergyBulletController
{
    [SerializeField]
    private float turnSpeed;  //旋回速度
    [SerializeField]
    private bool isNeedLock = false;    //誘導に要ロック(画面に捕らえている)

    private bool enableSetAngle = true;
    private PlayerStatus targetStatus;

    protected override void Update()
    {
        base.Update();

        //ロック可能チェック
        enableSetAngle = true;
        if (isNeedLock)
        {
            enableSetAngle = false;
            if (targetStatus != null)
            {
                enableSetAngle = targetStatus.IsLocked();
            }
        }

        //向き調整
        if (enableSetAngle)
        {
            base.SetAngle(base.targetTran, turnSpeed);
        }
    }

    [PunRPC]
    protected override void SetTargetRPC(string targetName)
    {
        GameObject targetObj = GameObject.Find(targetName);
        if (targetObj != null)
        {
            base.targetTran = targetObj.transform;
            targetStatus = targetObj.GetComponent<PlayerStatus>();
        }
    }
}

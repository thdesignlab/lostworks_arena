using UnityEngine;
using System.Collections;

public class ClusterLaserController : EnergyBulletController
{
    [SerializeField]
    private GameObject childBullet;
    [SerializeField]
    private int childeBulletCount;
    [SerializeField]
    private float purgeDistance;

    [SerializeField]
    private float turnSpeed;  //旋回速度
    [SerializeField]
    private bool isNeedLock = false;    //誘導に要ロック(画面に捕らえている)

    private bool enableSetAngle = true;
    private PlayerStatus targetStatus;

    void FixedUpdate()
    {
        if (base.targetTran != null && childBullet != null && base.activeTime >= 1.0f)
        {
            if (Vector3.Distance(myTran.position, base.targetTran.position) <= purgeDistance)
            {
                Purge();
            }
        }

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

    private void Purge()
    {
        //発射口
        Transform muzzle = null;
        foreach (Transform child in myTran)
        {
            if (child.tag == Common.CO.TAG_MUZZLE)
            {
                muzzle = child;
                break;
            }
        }
        if (muzzle != null)
        {
            //子供生成
            float moveAngle = 360 / childeBulletCount;
            for (int i = 0; i < childeBulletCount; i++)
            {
                myTran.Rotate(Vector3.forward, moveAngle);
                GameObject ob = PhotonNetwork.Instantiate(Common.CO.RESOURCE_BULLET+childBullet.name, muzzle.position, muzzle.rotation, 0);
                ob.GetComponent<ClusterLaserController>().SetTarget(targetTran);
            }
        }

        //本体破棄
        base.DestoryObject();
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

using UnityEngine;
using System.Collections;

public class ClusterLaserController : TrackingBulletController
{
    [SerializeField]
    private GameObject childBullet;
    [SerializeField]
    private int childeBulletCount;
    [SerializeField]
    private float purgeDistance;

    protected override void Update()
    {
        base.Update();

        if (photonView.isMine)
        {
            if (base.targetTran != null && childBullet != null && base.activeTime >= 1.0f)
            {
                if (Vector3.Distance(myTran.position, base.targetTran.position) <= purgeDistance)
                {
                    Purge();
                }
            }
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
                base.myTran.Rotate(Vector3.forward, moveAngle);
                GameObject ob = PhotonNetwork.Instantiate(Common.Func.GetResourceBullet(childBullet.name), muzzle.position, muzzle.rotation, 0);
                ob.GetComponent<BulletController>().SetTarget(base.targetTran);
            }
        }

        //本体破棄
        base.DestoryObject();
    }
}

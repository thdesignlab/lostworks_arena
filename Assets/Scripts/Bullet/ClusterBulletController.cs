using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClusterBulletController : TrackingBulletController
{
    [SerializeField]
    private GameObject childBullet;
    [SerializeField]
    private int childeBulletCount;
    [SerializeField]
    private float purgeDistance;
    [SerializeField]
    private float purgeTime;
    [SerializeField]
    private float purgeDiff = 0;
    [SerializeField]
    private bool isPurgeDestroy = true;
    //private bool isPurge = false;
    protected float prePurgeTime = 0;

    protected override void Update()
    {
        base.Update();

        prePurgeTime += Time.deltaTime;
        if (photonView.isMine)
        {
            if (CheckPurge()) Purge();
        }
    }

    protected bool CheckPurge()
    {
        bool purge = false;
        //if (!isPurge && childBullet != null && activeTime >= safetyTime)
        if (childBullet != null && prePurgeTime >= safetyTime)
        {
            if (purgeDistance > 0 && targetTran != null)
            {
                //ターゲットまでの距離チェック
                if (Vector3.Distance(myTran.position, targetTran.position) <= purgeDistance)
                {
                    purge = true;
                }

            }
            if (purgeTime > 0)
            {
                //経過時間チェック
                if (prePurgeTime >= purgeTime) purge = true;
            }
        }
        return purge;
    }


    protected void Purge()
    {
        //isPurge = true;

        //発射口
        //Transform muzzle = null;
        List<Transform> muzzles = new List<Transform>();
        foreach (Transform child in myTran)
        {
            if (child.tag == Common.CO.TAG_MUZZLE) muzzles.Add(child);
            //{
                //muzzle = child;
                //break; 
            //}
        }
        //if (muzzle == null) muzzle = myTran;
        if (muzzles.Count == 0) muzzles.Add(myTran);

        //子供生成
        float moveAngle = 360 / childeBulletCount;
        for (int i = 0; i < childeBulletCount; i++)
        {
            int muzzleIndex = i % muzzles.Count;
            Transform muzzle = muzzles[muzzleIndex];
            Vector3 muzzlePos = muzzle.position;
            Quaternion muzzleRot = muzzle.rotation;
            if (purgeDiff > 0)
            {
                muzzleRot *= Quaternion.AngleAxis(Random.Range(-purgeDiff, purgeDiff), Vector3.up);
                muzzleRot *= Quaternion.AngleAxis(Random.Range(-purgeDiff, purgeDiff), Vector3.right);
            }
            else
            {
                if (muzzles.Count == 1)
                {
                    myTran.Rotate(Vector3.forward, moveAngle);
                }
            }
            GameObject ob = PhotonNetwork.Instantiate(Common.Func.GetResourceBullet(childBullet.name), muzzlePos, muzzleRot, 0);
            int bulletNo = Common.Func.GetBulletNo(myTran.root.name);
            ob.name = ob.name + "_" + bulletNo.ToString();
            BulletController bulletCtrl = ob.GetComponent<BulletController>();
            bulletCtrl.BulletSetting(ownerTran, targetTran, weaponTran);
        }
        prePurgeTime = 0;

        if (isPurgeDestroy)
        {
            //本体破棄
            DestoryObject();
        }
    }
}

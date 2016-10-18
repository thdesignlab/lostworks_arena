using UnityEngine;
using System.Collections;

public class ChildBulletController : ClusterBulletController
{
    private BulletController parentBulletCtrl;
    //private Vector3 defaultScale;

    protected override void Start()
    {
        base.Start();

        //defaultScale = myTran.lossyScale;

        //ターゲット取得
        parentBulletCtrl = myTran.root.GetComponent<BulletController>();
        if (parentBulletCtrl != null)
        {
            Transform target = parentBulletCtrl.GetTarget();
            SetTarget(target);
        }
    }

    protected override void Update()
    {
        activeTime += Time.deltaTime;

        ////スケール維持
        //Vector3 lossScale = myTran.lossyScale;
        //Vector3 localScale = myTran.localScale;
        //myTran.localScale = new Vector3(
        //        localScale.x / lossScale.x * defaultScale.x,
        //        localScale.y / lossScale.y * defaultScale.y,
        //        localScale.z / lossScale.z * defaultScale.z
        //);
        if (photonView.isMine)
        {
            if (base.CheckPurge())
            {
                base.Purge();
            }
        }
    }

    //private void Purge()
    //{
    //    myTran.parent = null;
    //    if (photonView.isMine)
    //    {
    //        Debug.Log("Mine");
    //        //photonView.RPC("PurgeRPC", PhotonTargets.All);
    //    }
    //}

    //[PunRPC]
    //private void PurgeRPC()
    //{
    //    myTran.parent = null;
    //}
}

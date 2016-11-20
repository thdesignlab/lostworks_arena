using UnityEngine;
using System.Collections;

public class ChildBulletController : ClusterBulletController
{
    private BulletController parentBulletCtrl;

    protected override void Start()
    {
        base.Start();

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
        prePurgeTime += Time.deltaTime;
        if (photonView.isMine)
        {
            if (CheckPurge()) Purge();
        }
    }
}

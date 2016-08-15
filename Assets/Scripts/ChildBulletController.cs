using UnityEngine;
using System.Collections;

public class ChildBulletController : TrackingBulletController
{
    [SerializeField]
    private float purgeDistance;
    [SerializeField]
    private float purgeTime;

    private float childTime = 0;
    private float childDistance = 0;

    private BulletController parentBulletCtrl;

    protected override void Start()
    {
        base.Start();

        parentBulletCtrl = myTran.root.GetComponent<BulletController>();
    }

    protected override void Update()
    {
        if (myTran.parent != null)
        {
            if (purgeDistance > 0)
            {
                //進んだ距離チェック
                if (parentBulletCtrl != null)
                {
                    Vector3 moveVector = parentBulletCtrl.GetMoveDiff();
                    childDistance += moveVector.magnitude;
                    if (childDistance >= purgeDistance) Purge();
                }
            }
            else if (purgeTime > 0)
            {
                //経過時間チェック
                childTime += Time.deltaTime;
                if (childTime >= purgeTime) Purge();
            }
        }
        else
        {
            base.Update();
        }
    }

    private void Purge()
    {
        if (parentBulletCtrl != null)
        {
            Transform target = parentBulletCtrl.GetTarget();
            SetTarget(target);
        }
        myTran.parent = null;
    }
}

using UnityEngine;
using System.Collections;

public class ChildBulletController : TrackingBulletController
{
    [SerializeField]
    private float purgeTargetDistance;
    [SerializeField]
    private float purgeDistance;
    [SerializeField]
    private float purgeTime;

    private float childTime = 0;
    private float childDistance = 0;

    private BulletController parentBulletCtrl;
    private Vector3 defaultScale;

    protected override void Start()
    {
        base.Start();

        defaultScale = myTran.lossyScale;
        parentBulletCtrl = myTran.root.GetComponent<BulletController>();
        if (parentBulletCtrl != null)
        {
            Transform target = parentBulletCtrl.GetTarget();
            SetTarget(target);
        }
    }

    protected override void Update()
    {
        childTime += Time.deltaTime;

        //スケール維持
        Vector3 lossScale = myTran.lossyScale;
        Vector3 localScale = myTran.localScale;
        myTran.localScale = new Vector3(
                localScale.x / lossScale.x * defaultScale.x,
                localScale.y / lossScale.y * defaultScale.y,
                localScale.z / lossScale.z * defaultScale.z
        );

        if (myTran.parent != null)
        {
            if (purgeTargetDistance > 0 && targetTran != null)
            {
                //ターゲットとの距離チェック
                if (Vector3.Distance(myTran.position, targetTran.position) <= purgeTargetDistance)
                {
                    Purge();
                }
            }
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
            if (purgeTime > 0)
            {
                //経過時間チェック
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
        myTran.parent = null;
    }
}

using UnityEngine;
using System.Collections;

public class PhysicsTrackingBulletController : PhysicsBulletController
{
    [SerializeField]
    private float speed;  //巡航速度
    [SerializeField]
    private float turnSpeed;  //旋回速度
    [SerializeField]
    private float lockStartTime; //発射後待機時間
    [SerializeField]
    private bool isNeedLock = false;    //誘導に要ロック(画面に捕らえている)

    private bool enableSetAngle = true;

    protected override void Awake()
    {
        base.Awake();

        base.safetyTime = 0.3f;
    }
    protected override void Update()
    {
        base.Update();

        //推進力
        base.Move(Vector3.forward, speed);

        //初回ターゲット待機時間
        if (base.activeTime >= lockStartTime)
        {
            //ロック可能チェック
            enableSetAngle = true;
            if (isNeedLock)
            {
                enableSetAngle = false;
                if (base.targetStatus != null)
                {
                    enableSetAngle = base.targetStatus.IsLocked();
                }
            }

            //向き調整
            if (enableSetAngle)
            {
                base.SetAngle(base.targetTran, turnSpeed);
            }
        }
    }

    //衝突時処理
    protected override void OnCollisionEnter(Collision other)
    {
        GameObject otherObj = other.gameObject;
        if (IsSafety(otherObj)) return;

        //同じタグはスルー
        if (otherObj.tag == myTran.tag) return;
        isHit = true;

        //ダメージを与える
        base.AddDamage(otherObj);

        base.DestoryObject();
    }
}

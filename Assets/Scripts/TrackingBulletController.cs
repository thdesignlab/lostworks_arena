using UnityEngine;
using System.Collections;

public class TrackingBulletController : BulletController
{
    [SerializeField]
    protected float turnSpeed;  //旋回速度
    [SerializeField]
    protected float lockStartTime; //発射後待機時間
    [SerializeField]
    protected float lockedSpeedRate = 1; //ロック時のスピードRate
    [SerializeField]
    protected bool isNeedLock = false;    //誘導に要ロック(画面に捕らえている)

    protected bool enableSetAngle = true;
    protected float defaultSpeed;

    protected override void Awake()
    {
        base.Awake();

        defaultSpeed = base.speed;
    }

    protected override void Update()
    {
        base.Update();

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
                if (base.SetAngle(base.targetTran, turnSpeed))
                {
                    //ロック時スピード
                    base.speed = defaultSpeed * lockedSpeedRate;
                }
            }
        }
    }
}

﻿using UnityEngine;
using System.Collections;

public class TrackingBulletController : BulletController
{
    [SerializeField]
    protected float turnSpeed;  //旋回速度
    [SerializeField]
    protected float lockStartTime; //発射後待機時間
    [SerializeField]
    protected float lockedSpeedRate = 1; //ロック後のスピードRate
    [SerializeField]
    protected float lockedTurnSpeedRate = 1; //ロック後の旋回速度Rate
    [SerializeField]
    protected bool isNeedLock = false;    //誘導に要ロック(画面に捕らえている)
    [SerializeField]
    protected bool isLockFlat = false;    //平面ロックFLG

    protected LaserPointerController pointCtrl;    //ロック後ポインター

    protected bool enableSetAngle = true;
    protected float defaultSpeed;
    protected float defaultTurnSpeed;
    protected Vector3 lockVector = Vector3.one;

    protected override void Awake()
    {
        base.Awake();

        defaultSpeed = base.speed;
        defaultTurnSpeed = turnSpeed;
        if (isLockFlat) lockVector = new Vector3(1, 0, 1);
        pointCtrl = myTran.GetComponentInChildren<LaserPointerController>();
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
                if (base.SetAngle(base.targetTran, turnSpeed, lockVector))
                {
                    //ロック時スピード
                    base.speed = defaultSpeed * lockedSpeedRate;
                    turnSpeed = defaultTurnSpeed * lockedTurnSpeedRate;
                    if (pointCtrl != null) pointCtrl.SetOn();
                }
            }
        }
    }
}
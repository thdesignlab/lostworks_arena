﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LaserWeaponController : WeaponController
{
    [SerializeField]
    private GameObject laserPrefab;

    [SerializeField]
    private float effectiveLength;   //射程距離
    [SerializeField]
    private float effectiveWidth;   //幅
    [SerializeField]
    private float effectiveTime;   //最大幅照射時間
    [SerializeField]
    private float effectiveLengthTime;   //最大長になるまでの時間
    [SerializeField]
    private float effectiveWidthTime;   //最大幅になるまでの時間
    //[SerializeField]
    //private float runSpeedRate;   //移動速度制限
    [SerializeField]
    private float turnSpeedRate;   //回転速度制限
    [SerializeField]
    private float laserMazzleMaxScale;  //発射口スケール

    private float laserSwitchTime = 0;

    private Transform muzzle;
    

    protected override void Awake()
    {
        base.Awake();

        if (photonView.isMine)
        {
            //発射口取得
            foreach (Transform child in myTran)
            {
                if (child.tag == Common.CO.TAG_MUZZLE)
                {
                    muzzle = child;
                    break;
                }
            }

            //Bit移動用
            bitToPos = muzzle.localPosition;
            radius = Vector3.Distance(base.bitFromPos, base.bitToPos) / 2;
        }
    }

    protected override void Action()
    {
        //base.Action();
        isAction = true;

        //移動制限
        if (isStopInAttack && atkMotionTime > 0)
        {
            playerStatus.InterfareMove(atkMotionTime, null, false);
        }

        //ステータスUP
        if (playerStatus != null && statusChangeCtrl != null)
        {
            if (statusChangeCtrl.GetEffectTime() <= 0) statusChangeCtrl.AddEffectTime(atkMotionTime);
            statusChangeCtrl.Action(playerStatus);
        }
        
        //発射
        StartCoroutine(LaserShoot());
    }

    IEnumerator LaserShoot()
    {
        //Bit移動
        StartBitMove(bitFromPos, bitToPos);
        yield return new WaitForSeconds(bitMoveTime);
        laserSwitchTime = 0;

        //モーション開始
        StartMotion();

        if (playerStatus != null)
        {
            //移動・回転制限
            //playerStatus.AccelerateRunSpeed(runSpeedRate, effectiveTime, null, false);
            playerStatus.InterfareTurn(turnSpeedRate, effectiveTime);
            if (aimingCtrl != null) aimingCtrl.SetAimSpeed(turnSpeedRate);
        }

        //レーザー生成
        PlayVoice();
        //PlayAudio();
        GameObject laser = PhotonNetwork.Instantiate(Common.Func.GetResourceBullet(laserPrefab.name), muzzle.position, muzzle.rotation, 0);
        BulletSetting(laser);
        Transform laserTran = laser.transform;
        Transform laserEndTran = null;
        Transform laserMuzzle = null;
        foreach (Transform child in laserTran)
        {
            switch (child.tag)
            {
                case "LaserEnd":
                    laserEndTran = child;
                    break;

                case Common.CO.TAG_MUZZLE:
                    laserMuzzle = child;
                    break;
            }
        }
        CapsuleCollider laserCollider = laser.GetComponent<CapsuleCollider>();

        float nowWidth = 0;
        float nowLength = 0;
        for (;;)
        {
            laserSwitchTime += Time.deltaTime;

            //発射位置固定
            if (laserTran == null) break;
            laserTran.position = muzzle.position;
            laserTran.rotation = muzzle.rotation;

            //長さ決定
            if (effectiveLengthTime == 0)
            {
                nowLength = effectiveLength;
            }
            else
            {
                nowLength += effectiveLength * Time.deltaTime / effectiveLengthTime;
                if (nowLength >= effectiveLength)
                {
                    nowLength = effectiveLength;
                }

            }
            nowLength = GetLaserLength(nowLength, laserMuzzle);

            //レーザーの長さ設定
            if (laserEndTran != null)
            {
                laserEndTran.localPosition = new Vector3(0, 0, nowLength);
            }

            //コライダーの長さ設定
            if (laserCollider != null)
            {
                laserCollider.height = nowLength;
                laserCollider.center = new Vector3(0, 0, nowLength / 2);
            }


            //幅決定
            if (effectiveWidthTime == 0)
            {
                nowWidth = effectiveWidth;
                if (nowWidth == 0) nowWidth = laserTran.localScale.z;
            }
            else
            {
                //effectiveTime
                if (laserSwitchTime <= effectiveWidthTime)
                {
                    //太くする
                    nowWidth += effectiveWidth * Time.deltaTime / effectiveWidthTime;
                    if (nowWidth >= effectiveWidth) nowWidth = effectiveWidth;
                }
                else if (laserSwitchTime >= effectiveTime - effectiveWidthTime)
                {
                    //細くする
                    nowWidth -= effectiveWidth * Time.deltaTime / effectiveWidthTime;
                    if (nowWidth <= 0) nowWidth = 0;
                }
            }
            
            //レーザー幅設定
            laserTran.localScale = new Vector3(nowWidth, nowWidth, laserTran.localScale.z);
            if (laserMuzzle != null && laserMazzleMaxScale > 0)
            {
                laserMuzzle.localScale = Vector3.Lerp(Vector3.one, Vector3.one * laserMazzleMaxScale, nowWidth / effectiveWidth);
            }

            //照射時間チェック
            if (laserSwitchTime >= effectiveTime) break;

            yield return null;
        }

        //照射終了
        laserTran.GetComponent<ObjectController>().DestoryObject();

        if (aimingCtrl != null) aimingCtrl.SetAimSpeed();

        StopAudio();
        base.EndAction();
    }

    //private int hitCnt = 0;
    private float GetLaserLength(float length, Transform laserMuzzle)
    {
        RaycastHit hit;
        int layerNo = LayerMask.NameToLayer(Common.CO.LAYER_STRUCTURE);
        int layerMask = 1 << layerNo;
        if (laserMuzzle == null) laserMuzzle = muzzle;
        Ray ray = new Ray(laserMuzzle.position, laserMuzzle.forward);
        if (Physics.Raycast(ray, out hit, length, layerMask))
        {
            length = Vector3.Distance(laserMuzzle.position, hit.transform.position);
        }
        return length;
    }

    public override bool IsEnableFire()
    {
        if (!base.IsEnableFire()) return false;
        if (laserPrefab == null) return false;
        if (effectiveTime <= 0) return false;
        return true;
    }

    protected void BulletSetting(GameObject bulletObj)
    {
        BulletController bulletCtrl = bulletObj.GetComponent<BulletController>();
        if (bulletCtrl != null) bulletCtrl.BulletSetting(playerTran, targetTran, myTran);
    }

    protected override float GetAtkMotionTime()
    {
        float time = effectiveTime + bitMoveTime;
        return (time >= 0) ? time : 0;
    }


    //##### CUSTOM #####

    public void CustomChangeLaserObject(GameObject obj)
    {
        laserPrefab = obj;
    }
    public void CustomEffectiveLength(float value)
    {
        effectiveLength += value;
    }
    public void CustomEffectiveWidth(float value)
    {
        effectiveWidth += value;
    }
    public void CustomEffectiveTime(float value)
    {
        effectiveTime += value;
        atkMotionTime = -1;
    }
    public void CustomEffectiveLengthTime(float value)
    {
        effectiveLengthTime += value;
    }
    public void CustomEffectiveWidthTime(float value)
    {
        effectiveWidthTime += value;
    }
    public void CustomTurnSpeedRate(float value)
    {
        turnSpeedRate += value;
        if (turnSpeedRate < 0) turnSpeedRate = 0;
    }
}

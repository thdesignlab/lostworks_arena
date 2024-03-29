﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChargeWeaponController : BulletWeaponController
{
    private bool isCharge = false;
    private float chargeTime = 0;
    private List<Transform> bulletTrans = new List<Transform>();
    private List<ChargeBulletController> bulletCtrls = new List<ChargeBulletController>();

    private float npcChargeTime = 0;
    private float rapidIntervalTime = 0;
    private GameObject GameCtrlObj;

    protected override void Awake()
    {
        base.Awake();

        GameCtrlObj = GameObject.Find("GameController");
    }
    void Update()
    {
        if (photonView.isMine)
        {
            if (isCharge)
            {
                if ((GameCtrlObj == null || !GameController.Instance.isGameReady)
                    && ((Input.GetMouseButton(0) && !base.isNpc) || (chargeTime <= npcChargeTime && base.isNpc))
                )
                {
                    //チャージ中
                    chargeTime += Time.deltaTime;
                    rapidIntervalTime += Time.deltaTime;
                    if (rapidCount > bulletCtrls.Count && rapidIntervalTime >= rapidInterval)
                    {
                        int no = GetNextMuzzleNo(bulletTrans.Count-1);
                        CreateBullet(no);
                    }

                    //弾にチャージ情報を送る
                    int muzzleNo = 0;
                    for (int i = 0; i < bulletCtrls.Count; i++)
                    {
                        if (bulletCtrls[i] == null)
                        {
                            ForceChargeEnd();
                            return;
                        }
                        bulletCtrls[i].Charging(chargeTime);
                        bulletTrans[i].position = base.muzzles[muzzleNo].position;
                        bulletTrans[i].rotation = base.muzzles[muzzleNo].rotation;
                        muzzleNo = GetNextMuzzleNo(i);
                    }
                    return;
                }
                else
                {
                    //発射
                    PlayVoice();
                    isCharge = false;
                    base.EndAction();
                    //StopAudio(0);
                    PlayAudio(1, true);
                    foreach (ChargeBulletController bulletCtrl in bulletCtrls)
                    {
                        bulletCtrl.Fire(chargeTime);
                    }
                }
            }
        }
    }

    private void ForceChargeEnd()
    {
        isCharge = false;
        base.EndAction();
    }

    protected override void Action()
    {
        if (isCharge) return;

        isAction = true;
        bulletTrans = new List<Transform>();
        bulletCtrls = new List<ChargeBulletController>();

        //Bit表示
        BitOn();

        //モーション開始
        StartMotion();

        //弾生成
        CreateBullet(0);

        //チャージ開始
        isCharge = true;

        chargeTime = 0;
        //PlayAudio(0);

        //ステータスUP
        if (playerStatus != null && statusChangeCtrl != null)
        {
            if (statusChangeCtrl.GetEffectTime() <= 0) statusChangeCtrl.AddEffectTime(atkMotionTime);
            statusChangeCtrl.Action(playerStatus);
        }

        if (isNpc)
        {
            float maxChargeTime = bulletCtrls[0].GetMaxChargeTime();
            npcChargeTime = Random.Range(maxChargeTime * 0.5f, maxChargeTime * 1.5f);
        }
    }

    protected void CreateBullet(int muzzleNo)
    {
        GameObject ob = SpawnBullet(muzzles[muzzleNo].position, muzzles[muzzleNo].rotation, 0);
        bulletTrans.Add(ob.transform);
        bulletCtrls.Add(ob.transform.GetComponent<ChargeBulletController>());
        rapidIntervalTime = 0;
    }

    protected override float GetAtkMotionTime()
    {
        ChargeBulletController chargeBulletController = bullet.GetComponent<ChargeBulletController>();
        float time = chargeBulletController.GetMaxChargeTime() + bitMoveTime;
        return (time >= 0) ? time : 0;
    }
}

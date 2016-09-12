using UnityEngine;
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

    void Update()
    {
        if (photonView.isMine)
        {
            if (isCharge)
            {
                if (bulletTrans.Count == 0 || bulletCtrls.Count == 0)
                {
                    isCharge = false;
                    base.EndAction();
                    return;
                }

                if ((GameObject.Find("GameController") == null || !GameController.Instance.isGameReady)
                    && ((Input.GetMouseButton(0) && !base.isNpc) || (chargeTime <= npcChargeTime && base.isNpc))
                )
                {
                    //チャージ中
                    chargeTime += Time.deltaTime;
                    rapidIntervalTime += Time.deltaTime;
                    if (rapidCount > bulletCtrls.Count && rapidIntervalTime >= rapidInterval)
                    {
                        int muzzleNo = GetNextMuzzleNo(bulletTrans.Count-1);
                        CreateBullet(muzzleNo);
                    }

                    //弾にチャージ情報を送る
                    for (int i = 0; i < bulletCtrls.Count; i++)
                    {
                        int muzzleNo = GetNextMuzzleNo(i);
                        bulletCtrls[i].Charging(chargeTime);
                        bulletTrans[i].position = base.muzzles[muzzleNo].position;
                        bulletTrans[i].rotation = base.muzzles[muzzleNo].rotation;
                    }
                    return;
                }
                else
                {
                    //発射
                    PlayVoice();
                    isCharge = false;
                    base.EndAction();
                    base.StopAudio(0);
                    base.PlayAudio(1);
                    foreach (ChargeBulletController bulletCtrl in bulletCtrls)
                    {
                        bulletCtrl.Fire(chargeTime);
                    }
                }
            }
        }
    }

    protected override void Action()
    {
        if (isCharge) return;

        isAction = true;
        bulletTrans = new List<Transform>();
        bulletCtrls = new List<ChargeBulletController>();

        BitOn();

        //モーション開始
        StartMotion();

        //弾生成
        CreateBullet(0);

        //チャージ開始
        isCharge = true;
        chargeTime = 0;
        base.PlayAudio(0);

        if (base.isNpc)
        {
            float maxChargeTime = bulletCtrls[0].GetMaxChargeTime();
            npcChargeTime = Random.Range(maxChargeTime * 0.5f, maxChargeTime * 1.5f);
        }
    }

    protected void CreateBullet(int muzzleNo)
    {
        GameObject ob = base.SpawnBullet(base.muzzles[muzzleNo].position, base.muzzles[muzzleNo].rotation, 0);
        bulletTrans.Add(ob.transform);
        bulletCtrls.Add(ob.transform.GetComponent<ChargeBulletController>());
        rapidIntervalTime = 0;
    }
}

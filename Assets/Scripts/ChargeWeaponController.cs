using UnityEngine;
using System.Collections;

public class ChargeWeaponController : BulletWeaponController
{
    private bool isCharge = false;
    private float chargeTime = 0;
    private Transform bulletTran;
    //private Vector3 chargingVector;
    private ChargeBulletController bulletCtrl;

    private bool isNpc = false;
    private float npcChargeTime = 0;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(CheckParent());
    }

    IEnumerator CheckParent()
    {
        for (;;)
        {
            PlayerStatus status = myTran.root.GetComponent<PlayerStatus>();
            if (status != null)
            {
                isNpc = status.IsNpc();
                break;
            }
            yield return null;
        }
    }

    void Update()
    {
        if (photonView.isMine)
        {
            if (isCharge)
            {
                if ((Input.GetMouseButton(0) && !isNpc) || (chargeTime <= npcChargeTime && isNpc))
                {
                    //チャージ中
                    chargeTime += Time.deltaTime;

                    //弾にチャージ情報を送る
                    bulletCtrl.Charging(chargeTime);

                    //bulletTran.position = myTran.position + myTran.TransformVector(chargingVector);
                    bulletTran.position = base.muzzles[0].position;
                    bulletTran.rotation = base.muzzles[0].rotation;
                    return;
                }
                else
                {
                    //発射
                    isCharge = false;
                    base.EndAction();
                    base.StopAudio(0);
                    base.PlayAudio(1);
                    bulletCtrl.Fire(chargeTime);
                }
            }
        }
    }

    protected override void Action()
    {
        if (isCharge) return;

        //モーション開始
        StartMotion();

        //弾生成
        GameObject ob = base.SpawnBullet(base.muzzles[0].position, base.muzzles[0].rotation, 0);
        bulletTran = ob.transform;
        bulletCtrl = bulletTran.GetComponent<ChargeBulletController>();

        //チャージ開始
        isCharge = true;
        chargeTime = 0;
        base.PlayAudio(0);

        ////チャージ位置
        //chargingVector = myTran.InverseTransformVector(bulletTran.position - myTran.position);

        if (isNpc)
        {
            float maxChargeTime = bulletCtrl.GetMaxChargeTime();
            npcChargeTime = Random.Range(maxChargeTime * 0.5f, maxChargeTime * 1.5f);
        }
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletWeaponController : WeaponController
{
    [SerializeField]
    protected GameObject fireEffect;  //発射エフェクト
    [SerializeField]
    protected GameObject bullet;  //発射物
    [SerializeField]
    protected int rapidCount;     //連射数
    [SerializeField]
    protected float rapidInterval;  //連射間隔
    [SerializeField]
    protected int spreadCount;     //同時発射数
    [SerializeField]
    protected int spreadDiffAngle; //角度差分(Spreadのみ)
    [SerializeField]
    protected float focusDiff = 0;    //ブレ幅

    protected List<Transform> muzzles = new List<Transform>();   //発射口
    protected List<Quaternion> defaultMuzzleQuaternions = new List<Quaternion>();

    protected List<GameObject> shootBullets = new List<GameObject>();

    protected float startMuzzleAngle = 0;
    //protected List<float> startMuzzleAngles = new List<float>();


    protected override void Awake()
    {
        base.Awake();

        //発射口取得
        Transform bitPoint = null;
        foreach (Transform child in myTran)
        {
            if (child.tag == Common.CO.TAG_MUZZLE)
            {
                muzzles.Add(child);
                defaultMuzzleQuaternions.Add(child.localRotation);
            }
            else if (child.tag == Common.CO.TAG_WEAPON_BIT)
            {
                foreach (Transform grandsun in child)
                {
                    if (grandsun.tag == Common.CO.TAG_MUZZLE)
                    {
                        muzzles.Add(grandsun);
                        defaultMuzzleQuaternions.Add(grandsun.localRotation);
                    }
                }
            }
            else if (child.tag == Common.CO.TAG_BIT_POINT)
            {
                bitPoint = child;
            }
        }

        //Bit移動用
        if (bitPoint != null)
        {
            base.bitToPos = bitPoint.localPosition;
        }
        else
        {
            base.bitToPos = muzzles[0].localPosition;
        }
        base.radius = Vector3.Distance(base.bitFromPos, base.bitToPos) / 2;

        if (rapidCount <= 0) rapidCount = 1;
        if (spreadCount <= 0) spreadCount = 1;

        if (spreadCount > 1)
        {
            startMuzzleAngle = spreadDiffAngle * (spreadCount - 1) / -2;
        }
    }

    public override void SetTarget(Transform target = null)
    {
        base.SetTarget(target);

        foreach (Transform child in myTran)
        {
            switch (child.tag)
            {
                case Common.CO.TAG_WEAPON_BIT:
                case Common.CO.TAG_MUZZLE:
                    AimingController aimCtrl = child.GetComponent<AimingController>();
                    if (aimCtrl != null)
                    {
                        aimCtrl.SetTarget(target);
                    }
                    break;
            }

        }
    }

    protected override void Action()
    {
        shootBullets = new List<GameObject>();

        if (base.bitMoveTime > 0)
        {
            StartCoroutine(WaitBitMove());
        }
        else
        {
            ActionProccess();
        }
    }

    IEnumerator WaitBitMove()
    {
        //Bit移動
        if (!base.StartBitMove())
        {
            //Bit移動まち
            for (;;)
            {
                if (base.isBitMoved) break;
                yield return null;
            }
        }
        ActionProccess();
    }

    protected void ActionProccess()
    {
        base.Action();

        if (rapidCount <= 1 && spreadCount <= 1)
        {
            SpawnBullet(muzzles[0].position, muzzles[0].rotation, 0);
            EndAction();
            return;
        }

        if (rapidCount <= 1)
        {
            SpreadFire();
            EndAction();
        }
        else
        {
            StartCoroutine(RapidFire());
        }
    }

    IEnumerator RapidFire()
    {
        int muzzleNo = 0;
        for (int i = 0; i < rapidCount; i++)
        {
            if (spreadCount <= 1)
            {
                SpawnBullet(muzzles[muzzleNo].position, muzzles[muzzleNo].rotation, 0);
            }
            else
            {
                SpreadFire(muzzleNo, i);
            }
            muzzleNo = GetNextMuzzleNo(muzzleNo);
            yield return new WaitForSeconds(rapidInterval);
        }
        EndAction();
    }

    private void SpreadFire(int muzzleNo = 0, int rapidNo = 1)
    {
        RotateMuzzle(muzzleNo, startMuzzleAngle);
        for (int k = 0; k < spreadCount; k++)
        {
            SpawnBullet(muzzles[muzzleNo].position + muzzles[muzzleNo].forward, muzzles[muzzleNo].rotation, 0);
            RotateMuzzle(muzzleNo, spreadDiffAngle);
        }
        ResetMuzzle(muzzleNo);
    }

    private int GetNextMuzzleNo(int nowNo)
    {
        nowNo++;
        if (nowNo >= muzzles.Count) nowNo = 0;
        return nowNo;
    }

    protected void RotateMuzzle(int muzzleNo, float angle, Vector3 axis = default(Vector3))
    {
        if (axis == default(Vector3)) axis = Vector3.up;
        muzzles[muzzleNo].Rotate(axis, angle);
    }

    protected void ResetMuzzle(int muzzleNo)
    {
        muzzles[muzzleNo].localRotation = defaultMuzzleQuaternions[muzzleNo];
    }

    protected GameObject SpawnBullet(Vector3 pos, Quaternion quat, int groupId)
    {
        if (fireEffect != null)
        {
            //発射エフェクト
            GameObject.Instantiate(fireEffect, pos, fireEffect.transform.rotation);
        }
        if (focusDiff > 0)
        {
            //ブレ
            Vector3 axis = Vector3.up * Random.Range(-focusDiff, focusDiff) + Vector3.right * Random.Range(-focusDiff, focusDiff);
            quat *= Quaternion.AngleAxis(Random.Range(-focusDiff, focusDiff), axis);
        }
        //弾生成
        GameObject ob = PhotonNetwork.Instantiate(Common.Func.GetResourceBullet(bullet.name), pos, quat, groupId);
        SetBulletTarget(ob);
        base.PlayAudio();

        shootBullets.Add(ob);
        return ob;
    }

    public override bool IsEnableFire()
    {
        if (!base.IsEnableFire()) return false;
        if (muzzles.Count == 0 || bullet == null) return false;
        return true;
    }

    public override float GetBulletSpeed()
    {
        float v = 0;

        BulletController bulletCtrl = bullet.GetComponent<BulletController>();
        if (bulletCtrl != null)
        {
            v = bulletCtrl.GetSpeed();
        }

        return v;
    }

    protected void SetBulletTarget(GameObject bulletObj)
    {
        BulletController bulletCtrl = bulletObj.GetComponent<BulletController>();
        if (bulletCtrl != null)
        {
            bulletCtrl.SetTarget(targetTran);
            bulletCtrl.SetOwner(playerTran);
        }
    }

    public override string GetDescriptionText()
    {
        string description = "";
        description += "武器のステータス出す?\n";
        description += "Reload: " + base.reloadTime.ToString() + "s\n";
        if (bullet != null)
        {
            BulletController bulletCtrl = bullet.GetComponent<BulletController>();
            description += bulletCtrl.GetBulletDescription();
            if (rapidCount > 0) description += "ShootCount: " + rapidCount.ToString() + "s";
        }
        return description;
    }
}

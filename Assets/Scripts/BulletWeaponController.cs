using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletWeaponController : WeaponController
{

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

    protected AimingController aimingCtrl;

    protected float startMuzzleAngle = 0;
    //protected List<float> startMuzzleAngles = new List<float>();

    const string BULLET_FOLDER = "Bullet/";

    // Use this for initialization
    protected override void Awake()
    {
        base.Awake();

        //発射口取得
        foreach (Transform child in myTran)
        {
            if (child.tag == Common.CO.TAG_MUZZLE)
            {
                muzzles.Add(child);
                defaultMuzzleQuaternions.Add(child.localRotation);
            }
        }
        if (rapidCount <= 0) rapidCount = 1;
        if (spreadCount <= 0) spreadCount = 1;

        if (spreadCount > 1)
        {
            //startMuzzleAngles.Add(spreadDiffAngle * (Mathf.Floor(spreadCount) - 1) / -2);
            //startMuzzleAngles.Add(spreadDiffAngle * (Mathf.Ceil(spreadCount) - 1) / -2);
            startMuzzleAngle = spreadDiffAngle * (spreadCount - 1) / -2;
        }
    }

    protected override void Start()
    {
        base.Start();

        aimingCtrl = GetComponent<AimingController>();
    }

    public override void SetTarget(Transform target = null)
    {
        base.SetTarget(target);

        if (aimingCtrl != null)
        {
            aimingCtrl.SetTarget(target);
        }
    }

    protected override void Action()
    {
        if (rapidCount <= 1 && spreadCount <= 1)
        {
            SpawnBullet(muzzles[0].position, muzzles[0].rotation, 0);
            StartReload();
            return;
        }

        if (rapidCount <= 1)
        {
            SpreadFire();
            StartReload();
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
        StartReload();
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

    private void SpawnBullet(Vector3 pos, Quaternion quat, int groupId)
    {
        if (focusDiff > 0)
        {
            Vector3 axis = Vector3.up * Random.Range(-focusDiff, focusDiff) + Vector3.right * Random.Range(-focusDiff, focusDiff);
            quat *= Quaternion.AngleAxis(Random.Range(-focusDiff, focusDiff), axis);
        }
        GameObject ob = PhotonNetwork.Instantiate(BULLET_FOLDER + bullet.name, pos, quat, groupId);
        SetBulletTarget(ob);
    }

    public override bool IsEnableFire()
    {
        if (!isEnabledFire) return false;
        if (muzzles.Count == 0 || bullet == null) return false;
        return true;
    }

    public override float GetBulletSpeed()
    {
        float v = 0;

        if (Common.Func.IsPhysicsBullet(bullet.tag))
        {
            PhysicsBulletController pb = bullet.GetComponent<PhysicsBulletController>();
            if (pb != null)
            {
                v = pb.GetFirstSpeed();
            }
        }
        else
        {
            EnergyBulletController eb = bullet.GetComponent<EnergyBulletController>();
            if (eb != null)
            {
                v = eb.GetFirstSpeed();
            }
        }

        return v;
    }

    protected void SetBulletTarget(GameObject bulletObj)
    {
        if (base.targetTran == null) return;

        if (Common.Func.IsPhysicsBullet(bullet.tag))
        {
            PhysicsBulletController pb = bulletObj.GetComponent<PhysicsBulletController>();
            if (pb != null)
            {
                pb.SetTarget(base.targetTran);
            }
        }
        else
        {
            EnergyBulletController eb = bulletObj.GetComponent<EnergyBulletController>();
            if (eb != null)
            {
                eb.SetTarget(base.targetTran);
            }
        }
    }
}

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
    [SerializeField]
    protected float recoil = 0;    //反動

    protected List<Transform> muzzles = new List<Transform>();   //発射口
    protected List<Quaternion> defaultMuzzleQuaternions = new List<Quaternion>();

    protected List<GameObject> shootBullets = new List<GameObject>();

    protected float startMuzzleAngle = 0;
    //protected List<float> startMuzzleAngles = new List<float>();

    protected int bulletNo = 0;

    protected override void Awake()
    {
        base.Awake();
        SetMuzzle();
    }

    protected void SetMuzzle()
    {
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

        if (rapidCount <= 0) rapidCount = 1;
        if (spreadCount <= 0) spreadCount = 1;

        if (spreadCount > 1)
        {
            startMuzzleAngle = spreadDiffAngle * (spreadCount - 1) / -2;
        }

        //Bit移動用
        if (bitPoint != null)
        {
            bitToPos = bitPoint.localPosition;
        }
        else
        {
            bitToPos = muzzles[0].localPosition;
        }
        radius = Vector3.Distance(bitFromPos, bitToPos) / 2;
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
        bulletNo = 0;
        shootBullets = new List<GameObject>();

        if (bitMoveTime  > 0)
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
        BitOn();
        StartBitMove(bitFromPos, bitToPos);
        yield return new WaitForSeconds(bitMoveTime);
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

    protected int GetNextMuzzleNo(int nowNo)
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
            PhotonNetwork.Instantiate(Common.Func.GetResourceEffect(fireEffect.name), pos, fireEffect.transform.rotation, 0);
        }
        if (focusDiff > 0)
        {
            //ブレ
            Vector3 axis = Vector3.up * Random.Range(-focusDiff, focusDiff) + Vector3.right * Random.Range(-focusDiff, focusDiff);
            quat *= Quaternion.AngleAxis(Random.Range(-focusDiff, focusDiff), axis);
        }
        //弾生成
        GameObject ob = PhotonNetwork.Instantiate(Common.Func.GetResourceBullet(bullet.name), pos, quat, groupId);
        bulletNo++;
        ob.name = ob.name + "_" + bulletNo.ToString();
        BulletSetting(ob);
        PlayAudio();

        if (recoil > 0 && playerStatus != null)
        {
            //反動
            playerStatus.ActionRecoil(recoil);
        }

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

    protected void BulletSetting(GameObject bulletObj)
    {
        BulletController bulletCtrl = bulletObj.GetComponent<BulletController>();
        if (bulletCtrl != null) bulletCtrl.BulletSetting(playerTran, targetTran, myTran);
    }


    //##### CUSTOM #####

    //RapidCount
    public void CustomRapidCount(int value)
    {
        rapidCount += value;
        if (rapidCount < 1) rapidCount = 1;
    }

    //RapidInterval
    public void CustomRapidInterval(float value)
    {
        rapidInterval += value;
        if (rapidInterval < 0) rapidInterval = 0;
    }

    //SpreadCount
    public void CustomSpreadCount(int value)
    {
        spreadCount += value;
        if (spreadCount < 1) spreadCount = 1;
        SetMuzzle();
    }

    //SpreadDiff
    public void CustomSpreadDiff(int value)
    {
        spreadDiffAngle += value;
        SetMuzzle();
    }

    //ブレ抑制
    public void CustomFocusDiff(float value)
    {
        focusDiff += value;
        if (focusDiff < 0) focusDiff = 0;
    }

    //弾変更
    public void CustomChangeBullet(GameObject obj)
    {
        bullet = obj;
    }
}

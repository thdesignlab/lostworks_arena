using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LaserWeaponController : WeaponController
{
    [SerializeField]
    private float effectiveLength;   //射程距離
    [SerializeField]
    private float effectiveWidth;   //幅
    [SerializeField]
    private float effectiveTime;   //最大幅照射時間
    [SerializeField]
    private float effectiveWidthTime;   //最大幅になるまでの時間
    [SerializeField]
    private float damagePerSecond;   //ダメージ(1sあたり)
    [SerializeField]
    private float runSpeedRate;   //移動速度制限

    private LineRenderer laser;
    private CapsuleCollider laserCollider;
    
    private AimingController aimingCtrl;

    private PlayerStatus playerStatus;
    private PlayerStatus targetStatus;

    // Use this for initialization
    protected override void Awake()
    {
        base.Awake();

        StartCoroutine(SetPlayerStatus());

        //発射口取得
        foreach (Transform child in myTran)
        {
            if (child.tag == Common.CO.TAG_MUZZLE)
            {
                laser = child.gameObject.GetComponent<LineRenderer>();
                laserCollider = child.gameObject.GetComponent<CapsuleCollider>();
                laser.SetPosition(1, new Vector3(0, 0, effectiveLength));
                laserCollider.height = effectiveLength;
                laserCollider.center = new Vector3(0, 0, effectiveLength / 2);
                break;
            }
        }
    }

    IEnumerator SetPlayerStatus()
    {
        base.isEnabledFire = false;
        for (;;)
        {
            playerStatus = base.myTran.root.gameObject.GetComponent<PlayerStatus>();
            if (playerStatus != null) break;
            yield return null;
        }
        base.isEnabledFire = true;
    }

    protected override void Start()
    {
        base.Start();

        aimingCtrl = GetComponent<AimingController>();
    }

    public override void SetTarget(Transform target = null)
    {
        base.SetTarget(target);

        if (base.targetTran == null) return;
        targetStatus = base.targetTran.gameObject.GetComponent<PlayerStatus>();

        if (aimingCtrl != null)
        {
            aimingCtrl.SetTarget(target);
        }
    }

    protected override void Action()
    {
        StartCoroutine(LaserShoot());
    }

    IEnumerator LaserShoot()
    {
        int factor = 1;
        float nowWidth = 0;
        SetLaserWidth(nowWidth);
        laser.enabled = true;
        laserCollider.enabled = true;
        for (;;)
        {
            nowWidth += effectiveWidth * Time.deltaTime / effectiveWidthTime * factor;
            SetLaserWidth(nowWidth);
            if (nowWidth >= effectiveWidth)
            {
                nowWidth = effectiveWidth;
                SetLaserWidth(nowWidth);
                factor = -1;
                playerStatus.AccelerateRunSpeed(runSpeedRate, effectiveTime);
                yield return new WaitForSeconds(effectiveTime);
            }
            if (nowWidth <= 0) break;
            yield return null;
        }
        laser.enabled = false;
        laserCollider.enabled = false;
        base.StartReload();
    }
    private void SetLaserWidth(float width)
    {
        laser.SetWidth(width, width);
        laserCollider.radius = Mathf.Sqrt(width);
    }

    public override bool IsEnableFire()
    {
        if (!isEnabledFire) return false;
        if (laser == null || laserCollider == null) return false;
        if (effectiveTime <= 0 || effectiveWidthTime <= 0) return false;
        return true;
    }

    void OnChildTriggerEnter(Collider other)
    {
        return;
    }

    private float totalDamage = 0;
    void OnChildTriggerStay(Collider other)
    {
        if (other.transform == base.targetTran)
        {
            totalDamage += damagePerSecond * Time.deltaTime;
            if (totalDamage > 5)
            {
                targetStatus.AddDamage((int)totalDamage);
                totalDamage = totalDamage % 1;
            }
        }
    }
}

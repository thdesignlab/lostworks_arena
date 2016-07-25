using UnityEngine;
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
    private float effectiveWidthTime;   //最大幅になるまでの時間
    [SerializeField]
    private float runSpeedRate;   //移動速度制限
    [SerializeField]
    private float turnSpeedRate;   //回転速度制限

    private float laserMazzleMaxScale = 2.5f;

    private Transform muzzle;
    private GameObject laser;
    private Transform laserTran;
    private Transform laserMuzzle;
    private CapsuleCollider laserCollider;
    
    //private AimingController aimingCtrl;

    private PlayerStatus playerStatus;
    //private PlayerStatus targetStatus;

    //private Dictionary<int, int> laserMap = new Dictionary<int, int>();
    private int laserViewId;
    private int muzzleViewId;

    // Use this for initialization
    protected override void Awake()
    {
        base.Awake();

        if (photonView.isMine)
        {
            StartCoroutine(SetPlayerStatus());

            //発射口取得
            foreach (Transform child in myTran)
            {
                if (child.tag == Common.CO.TAG_MUZZLE)
                {
                    muzzle = child;
                    break;
                }
            }

            //レーザー生成
            laser = PhotonNetwork.Instantiate(Common.CO.RESOURCE_BULLET + laserPrefab.name, muzzle.position, muzzle.rotation, 0);
            muzzleViewId = PhotonView.Get(muzzle.gameObject).viewID;
            laserViewId = PhotonView.Get(laser).viewID;

            laserTran = laser.transform;
            laserTran.parent = muzzle;
            laserTran.localPosition = Vector3.zero;
            foreach (Transform child in laserTran)
            {
                if (child.tag == Common.CO.TAG_MUZZLE)
                {
                    laserMuzzle = child;
                    break;
                }
            }

            //レーザー初期設定
            laser.SetActive(false);
        }
        else
        {
            //レーザー初期設定
            photonView.RPC("SetInitRPC", PhotonTargets.Others);
        }
    }

    [PunRPC]
    private void SetInitRPC()
    {
        if (photonView.isMine)
        {
            object[] args = new object[] { muzzleViewId, laserViewId };
            photonView.RPC("InitLaserRPC", PhotonTargets.Others, args);
        }
    }

    [PunRPC]
    private void InitLaserRPC(int parentViewId, int childViewId)
    {
        //武器にレーザー取り付け
        PhotonView muzzleView = PhotonView.Find(parentViewId);
        PhotonView laserView = PhotonView.Find(childViewId);
        if (muzzleView == null || laserView == null) return;

        laser = laserView.gameObject;
        laserTran = laser.transform;
        laserTran.parent = muzzleView.gameObject.transform;
        laserTran.localPosition = Vector3.zero;

        //レーザー非表示
        laser.SetActive(false);
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

    private void SwitchLaser(bool flg)
    {
        photonView.RPC("SwitchLaserRPC", PhotonTargets.All, flg);
    }

    [PunRPC]
    private void SwitchLaserRPC(bool flg)
    {
        laser.SetActive(flg);
    }

    public override void SetTarget(Transform target = null)
    {
        base.SetTarget(target);
        laser.GetComponent<LaserBulletController>().SetTarget(target);
    }

    protected override void Action()
    {
        if (playerStatus == null)
        {
            base.isEnabledFire = false;
            return;
        }

        //移動・回転制限
        float laserShootTime = effectiveTime + effectiveWidthTime * 2;
        playerStatus.AccelerateRunSpeed(runSpeedRate, laserShootTime);
        playerStatus.InterfareTurn(turnSpeedRate, laserShootTime);
        base.StartReload(laserShootTime);

        StartCoroutine(LaserShoot());
    }

    IEnumerator LaserShoot()
    {
        //レーザー幅変更
        int factor = 1;
        float nowWidth = 0;
        SetLaserLength(effectiveLength);
        SetLaserWidth(nowWidth);
        SwitchLaser(true);
        //laserCollider.enabled = true;
        for (;;)
        {
            nowWidth += effectiveWidth * Time.deltaTime / effectiveWidthTime * factor;
            SetLaserWidth(nowWidth);
            if (nowWidth >= effectiveWidth)
            {
                nowWidth = effectiveWidth;
                SetLaserWidth(nowWidth);
                factor = -1;
                yield return new WaitForSeconds(effectiveTime);
            }
            if (nowWidth <= 0) break;
            yield return null;
        }
        SwitchLaser(false);
    }

    private void SetLaserLength(float length)
    {
        //レーザーの長さ設定
        foreach (Transform child in laserTran)
        {
            if (child.tag == "LaserEnd")
            {
                child.localPosition = new Vector3(0, 0, effectiveLength);
                break;
            }
        }

        //コライダーの長さ設定
        laserCollider = laser.GetComponent<CapsuleCollider>();
        if (laserCollider != null)
        {
            laserCollider.height = effectiveLength;
            laserCollider.center = new Vector3(0, 0, effectiveLength / 2);
        }
    }

    private void SetLaserWidth(float width)
    {
        laserTran.localScale = new Vector3(width, width, laserTran.localScale.z);
        laserMuzzle.localScale = Vector3.Lerp(Vector3.one, Vector3.one * laserMazzleMaxScale, width / effectiveWidth);
    }

    public override bool IsEnableFire()
    {
        if (!isEnabledFire) return false;
        if (laser == null) return false;
        if (effectiveTime <= 0 || effectiveWidthTime <= 0) return false;
        return true;
    }
}

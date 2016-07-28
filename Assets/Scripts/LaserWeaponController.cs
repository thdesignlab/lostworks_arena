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
    private Transform laserEndTran;
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
            laser = PhotonNetwork.Instantiate(Common.Func.GetResourceBullet(laserPrefab.name), muzzle.position, muzzle.rotation, 0);
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
                }
                else if (child.tag == "LaserEnd")
                {
                    laserEndTran = child;
                }
            }
            //Debug.Log(laserMuzzle.name+" / "+ laserEndTran.name);

            //レーザー初期設定
            laser.SetActive(false);
        }
        else
        {
            //Debug.Log("RPC:SetInitRPC");
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
            //Debug.Log("SetInitRPC: "+ muzzleViewId.ToString()+" >> "+ laserViewId.ToString());

            photonView.RPC("InitLaserRPC", PhotonTargets.Others, args);
        }
    }

    [PunRPC]
    private void InitLaserRPC(int parentViewId, int childViewId)
    {
        //Debug.Log("InitLaserRPC: " + parentViewId.ToString() + " >> " + childViewId.ToString());
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

        base.Action();

        //移動・回転制限
        float laserShootTime = effectiveTime + effectiveWidthTime * 2;
        playerStatus.AccelerateRunSpeed(runSpeedRate, laserShootTime);
        playerStatus.InterfareTurn(turnSpeedRate, laserShootTime);
        //base.StartReload(laserShootTime);

        StartCoroutine(LaserShoot());
    }

    IEnumerator LaserShoot()
    {
        //レーザー幅変更
        int factor = 1;
        float nowWidth = 0;
        float nowLength = 0;
        SetLaserLength(nowLength);
        SetLaserWidth(nowWidth);
        SwitchLaser(true);
        //laserCollider.enabled = true;
        for (;;)
        {
            //長さ
            nowLength = GetLaserLength();
            SetLaserLength(nowLength);

            //幅
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
        base.EndAction();
    }

    private int hitCnt = 0;
    private float GetLaserLength()
    {
        float length = effectiveLength;

        RaycastHit hit;
        int layerNo = LayerMask.NameToLayer(Common.CO.LAYER_STRUCTURE);
        int layerMask = 1 << layerNo;
        Ray ray = new Ray(laserMuzzle.position, laserMuzzle.forward);
        if (Physics.Raycast(ray, out hit, effectiveLength, layerMask))
        {
            length = Vector3.Distance(laserMuzzle.position, hit.transform.position);
            StructureParentController atructureCtrl = hit.transform.root.GetComponent<StructureParentController>();
            if (atructureCtrl != null)
            {
                hitCnt++;
                if (hitCnt >= 10)
                {
                    atructureCtrl.AddDamage(1);
                    hitCnt = 0;
                }
            }
        }
        return length;
    }

    private void SetLaserLength(float length)
    {
        //レーザーの長さ設定
        laserEndTran.localPosition = new Vector3(0, 0, length);

        //コライダーの長さ設定
        laserCollider = laser.GetComponent<CapsuleCollider>();
        if (laserCollider != null)
        {
            laserCollider.height = length;
            laserCollider.center = new Vector3(0, 0, length / 2);
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

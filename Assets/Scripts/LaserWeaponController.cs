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
    private float effectiveLengthTime;   //最大長になるまでの時間
    [SerializeField]
    private float effectiveWidthTime;   //最大幅になるまでの時間
    [SerializeField]
    private float runSpeedRate;   //移動速度制限
    [SerializeField]
    private float turnSpeedRate;   //回転速度制限
    [SerializeField]
    private float laserMazzleMaxScale;  //発射口スケール

    [SerializeField]
    private bool isShoulder;
    private bool isFire = false;
    private Transform myBitTran;
    private float bitMoveTime = 0.3f;
    private float laserSwitchTime = 0;
    private float lerpRate;
    private Vector3 bitFromPos;
    private Vector3 bitToPos;
    private float radius;

    private Transform muzzle;
    //private GameObject laser;
    //private Transform laserTran;
    //private Transform laserEndTran;
    //private Transform laserMuzzle;
    //private CapsuleCollider laserCollider;
    
    //private AimingController aimingCtrl;

    //private PlayerStatus playerStatus;
    //private PlayerStatus targetStatus;

    //private Dictionary<int, int> laserMap = new Dictionary<int, int>();
    //private int laserViewId;
    //private int muzzleViewId;

    // Use this for initialization
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
            myBitTran = myTran.FindChild("Bit");
            bitFromPos = myBitTran.localPosition;
            bitToPos = muzzle.localPosition;
            radius = Vector3.Distance(bitFromPos, bitToPos);
        }
    }

    private float bitReturnTime = 0;
    void Update()
    {
        if (photonView.isMine)
        {
            if (isShoulder)
            {
                bool isBitMove = false;
                float startAngle = 0;
                if (laserSwitchTime <= bitMoveTime && isFire)
                {
                    //発射口の場所へ移動
                    isBitMove = true;
                    lerpRate = laserSwitchTime / bitMoveTime;
                    if (lerpRate > 1) lerpRate = 1;
                    startAngle = 0;
                    bitReturnTime = bitMoveTime;
                }
                else if (bitReturnTime > 0 && !isFire)
                {
                    //元の場所へ移動
                    bitReturnTime -= Time.deltaTime;
                    isBitMove = true;
                    lerpRate = bitReturnTime / bitMoveTime;
                    if (lerpRate < 0) lerpRate = 0;
                    startAngle = 180;
                }

                if (isBitMove)
                {
                    Vector3 leftSide = Vector3.left * Common.Func.GetSin(lerpRate, 180, startAngle) * radius / 2;
                    //myBitTran.position = myBitTran.TransformDirection(Vector3.Lerp(bitFromPos, bitToPos, lerpRate) + leftSide);
                    myBitTran.localPosition = Vector3.Lerp(bitFromPos, bitToPos, lerpRate) + leftSide;
                }
            }
        }
    }

    protected override void Action()
    {
        if (base.playerStatus == null)
        {
            base.isEnabledFire = false;
            return;
        }

        base.Action();

        //移動・回転制限
        base.playerStatus.AccelerateRunSpeed(runSpeedRate, effectiveTime);
        base.playerStatus.InterfareTurn(turnSpeedRate, effectiveTime);

        //発射
        StartCoroutine(LaserShoot());
    }

    IEnumerator LaserShoot()
    {
        //レーザー生成
        base.PlayAudio();
        GameObject laser = PhotonNetwork.Instantiate(Common.Func.GetResourceBullet(laserPrefab.name), muzzle.position, muzzle.rotation, 0);
        SetBulletTarget(laser);
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

        if (laserEndTran == null)
        {
            base.EndAction();
            yield break;
        }

        //Bit移動用パラメータ
        isFire = true;
        laserSwitchTime = 0;

        float nowWidth = 0;
        float nowLength = 0;
        for (;;)
        {
            //発射位置固定
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
            laserEndTran.localPosition = new Vector3(0, 0, nowLength);

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
            }
            else
            {
                //effectiveTime
                if (laserSwitchTime <= effectiveWidthTime)
                {
                    //太くする
                    nowWidth += effectiveWidth * Time.deltaTime / effectiveWidthTime;
                    if (nowWidth >= effectiveWidth)
                    {
                        nowWidth = effectiveWidth;
                    }
                }
                else if (laserSwitchTime >= effectiveTime - effectiveWidthTime)
                {
                    //細くする
                    nowWidth -= effectiveWidth * Time.deltaTime / effectiveWidthTime;
                    if (nowWidth <= 0)
                    {
                        isFire = false;
                        nowWidth = 0;
                    }
                }
            }
            
            //レーザー幅設定
            laserTran.localScale = new Vector3(nowWidth, nowWidth, laserTran.localScale.z);
            if (laserMuzzle != null && laserMazzleMaxScale > 0)
            {
                laserMuzzle.localScale = Vector3.Lerp(Vector3.one, Vector3.one * laserMazzleMaxScale, nowWidth / effectiveWidth);
            }

            //照射時間チェック
            if (laserSwitchTime >= effectiveTime) isFire = false;

            if (!isFire)
            {
                //照射終了
                laserTran.GetComponent<ObjectController>().DestoryObject();
                break;
            }

            yield return null;
            laserSwitchTime += Time.deltaTime;
        }
        //SwitchLaser(false);
        base.StopAudio();
        base.EndAction();
    }

    private int hitCnt = 0;
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
            StructureController structureCtrl = hit.transform.GetComponent<StructureController>();
            if (structureCtrl != null)
            {
                hitCnt++;
                if (hitCnt >= 10)
                {
                    structureCtrl.AddDamage(Random.Range(0, 20));
                    hitCnt = 0;
                }
            }
        }
        return length;
    }

    public override bool IsEnableFire()
    {
        if (!isEnabledFire) return false;
        if (laserPrefab == null) return false;
        if (effectiveTime <= 0) return false;
        return true;
    }

    private void SetBulletTarget(GameObject bulletObj)
    {
        if (base.targetTran == null) return;

        LaserBulletController lb = bulletObj.GetComponent<LaserBulletController>();
        if (lb != null)
        {
            lb.SetTarget(base.targetTran);
        }
        EnergyBulletController eb = bulletObj.GetComponent<EnergyBulletController>();
        if (eb != null)
        {
            eb.SetTarget(base.targetTran);
        }
    }
}

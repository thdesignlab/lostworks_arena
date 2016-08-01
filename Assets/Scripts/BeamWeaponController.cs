using UnityEngine;
using System.Collections;

public class BeamWeaponController : WeaponController
{
    [SerializeField]
    private GameObject laserPrefab;

    [SerializeField]
    private float effectiveLength;   //射程距離
    [SerializeField]
    private float effectiveWidth;   //幅
    [SerializeField]
    private float effectiveTime;   //最大射程になるまでの時間
    [SerializeField]
    private float effectiveWidthTime; //最大射程になってから消滅するまでの時間
    [SerializeField]
    private float runSpeedRate;   //移動速度制限
    [SerializeField]
    private float turnSpeedRate;   //回転速度制限

    private Transform myBitTran;
    private float bitMoveTime = 0.5f;
    private float laserSwitchTime = 0;
    private float lerpRate;
    private Vector3 bitFromPos;
    private Vector3 bitToPos;
    private float radius;

    private Transform muzzle;
    private GameObject laser;
    private Transform laserTran;
    private Transform laserEndTran;
    private Transform laserMuzzle;
    private CapsuleCollider laserCollider;

    //private AimingController aimingCtrl;

    //private PlayerStatus playerStatus;
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
            //発射口取得
            foreach (Transform child in myTran)
            {
                if (child.tag == Common.CO.TAG_MUZZLE)
                {
                    muzzle = child;
                    break;
                }
            }
        }
    }

    void Update()
    {
        //if (photonView.isMine && !base.isNpc)
        //{
        //    bool isBitMove = false;
        //    float startAngle = 0;
        //    if (laser.GetActive() && laserSwitchTime < bitMoveTime)
        //    {
        //        //発射口の場所へ移動
        //        isBitMove = true;
        //        laserSwitchTime += Time.deltaTime;
        //        lerpRate = laserSwitchTime / bitMoveTime;
        //        if (lerpRate > 1) lerpRate = 1;
        //        startAngle = 0;
        //    }
        //    else if (!laser.GetActive() && laserSwitchTime > 0)
        //    {
        //        //元の場所へ移動
        //        isBitMove = true;
        //        laserSwitchTime -= Time.deltaTime;
        //        lerpRate = laserSwitchTime / bitMoveTime;
        //        if (lerpRate < 0) lerpRate = 0;
        //        startAngle = 180;
        //    }

        //    if (isBitMove)
        //    {
        //        Vector3 leftSide = Vector3.left * Common.Func.GetSin(lerpRate, 180, startAngle) * radius / 2;
        //        myBitTran.localPosition = Vector3.Lerp(bitFromPos, bitToPos, lerpRate) + leftSide;
        //    }
        //}
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
        float laserShootTime = effectiveTime + effectiveWidthTime;
        playerStatus.AccelerateRunSpeed(runSpeedRate, laserShootTime);
        playerStatus.InterfareTurn(turnSpeedRate, laserShootTime);
        //base.StartReload(laserShootTime);

        SpawnBullet(muzzle.position, muzzle.rotation, 0);
    }

    protected GameObject SpawnBullet(Vector3 pos, Quaternion quat, int groupId)
    {
        //弾生成
        GameObject ob = PhotonNetwork.Instantiate(Common.Func.GetResourceBullet(laserPrefab.name), pos, quat, groupId);
        base.PlayAudio();
        return ob;
    }

    public override bool IsEnableFire()
    {
        if (!isEnabledFire) return false;
        if (effectiveTime <= 0) return false;
        return true;
    }
}

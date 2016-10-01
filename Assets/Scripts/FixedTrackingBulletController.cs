using UnityEngine;
using System.Collections;

public class FixedTrackingBulletController : BulletController
{
    [SerializeField]
    protected float fixTime;    //固定するまでの時間
    [SerializeField]
    private Transform rollBody;   //回転する場所
    [SerializeField]
    private float fixedRollSpeed;   //回転速度
    [SerializeField]
    private Vector3 fixedRollVector; //回転軸
    [SerializeField]
    protected float fixedSpeed; //固定解除後のスピード
    [SerializeField]
    protected float fixedTurnSpeed; //固定解除後の旋回速度
    [SerializeField]
    protected float searchTurnSpeed; //索敵時旋回速度
    [SerializeField]
    protected bool isNeedLock = false;    //誘導に要ロック(画面に捕らえている)
    [SerializeField]
    protected bool isStickable = false;    //地面・障害物に当たった際に刺さる
    [SerializeField]
    protected float autoFireTime = 0;    //武器からの命令無しで固定解除
    [SerializeField]
    protected float autoFireDiff = 0;    //固定解除差分

    protected LaserPointerController pointCtrl;    //ロック後ポインター

    protected bool enableSetAngle = true;
    protected float defaultSpeed;
    protected float defaultRollSpeed;
    protected float defaultTurnSpeed;
    protected Quaternion defaultBodyRot;

    protected bool isFix = false;
    protected bool isShoot = false;

    protected override void Awake()
    {
        base.Awake();

        if (rollBody == null)
        {
            rollBody = myTran;
        }
        else
        {
            defaultBodyRot = rollBody.localRotation;
        }
        pointCtrl = myTran.GetComponentInChildren<LaserPointerController>();
    }

    protected override void Update()
    {
        base.Update();

        if (!isFix && !isShoot)
        {
            //固定前
            if (fixTime <= base.activeTime)
            {
                //固定
                base.speed = 0;
                isFix = true;
                if (pointCtrl != null && searchTurnSpeed > 0) pointCtrl.SetOn();

                if (autoFireTime > 0 && autoFireDiff > 0)
                {
                    int no = Common.Func.GetBulletNo(myTran.name);
                    autoFireTime += autoFireDiff * no;
                }
            }
        }

        if (isShoot)
        {
            //固定解除後
            //ロック可能チェック
            enableSetAngle = true;
            if (isNeedLock)
            {
                enableSetAngle = false;
                if (base.targetStatus != null)
                {
                    enableSetAngle = base.targetStatus.IsLocked();
                }
            }

            //向き調整
            bool isSetAngleFinish = false;
            if (enableSetAngle)
            {
                isSetAngleFinish = base.SetAngle(base.targetTran, fixedTurnSpeed);
            }
        }
        else
        {
            //固定解除前
            //回転
            if (fixedRollSpeed > 0)
            {
                if (fixedRollVector == Vector3.zero)
                {
                    float x = Random.Range(0, 1.0f);
                    float y = Random.Range(0, 1.0f);
                    float z = Random.Range(0, 1.0f);
                    fixedRollVector = new Vector3(x, y, z);
                }
                rollBody.Rotate(fixedRollVector, fixedRollSpeed * Time.deltaTime);
            }
            
            if (isFix && searchTurnSpeed > 0)
            {
                if (SetAngle(targetTran, searchTurnSpeed)) Shoot();
            }

            if (autoFireTime > 0 && autoFireTime + fixTime <= activeTime) Shoot();
        }
    }

    public void Shoot()
    {
        if (isShoot) return;

        //ロック可能チェック
        enableSetAngle = true;
        if (isNeedLock)
        {
            enableSetAngle = false;
            if (targetStatus != null)
            {
                enableSetAngle = targetStatus.IsLocked();
            }
        }
        photonView.RPC("ShootRPC", PhotonTargets.All, enableSetAngle);
    }

    [PunRPC]
    public void ShootRPC(bool isSetAngle)
    {
        //向き調整
        if (isSetAngle)
        {
            if (searchTurnSpeed == 0)
            {
                Vector3 diffVector = DifferentialCorrection(targetTran, fixedSpeed);
                myTran.LookAt(targetTran.position + diffVector);
            }
            if (pointCtrl != null) pointCtrl.SetOn();
        }
        else
        {
            //とりあえず下に向ける
            Vector3 floorPos = new Vector3(myTran.position.x, 0, myTran.position.z);
            myTran.LookAt(floorPos);
        }

        if (rollBody != myTran) rollBody.localRotation = defaultBodyRot;

        PlayAudio();
        base.speed = fixedSpeed;
        isShoot = true;
        isFix = false;
    }


    //衝突時処理(共通)
    protected override void OnHit(GameObject otherObj)
    {
        if (isStickable)
        {
            //床に刺さる
            if (otherObj.layer == LayerMask.NameToLayer(Common.CO.LAYER_STRUCTURE)
                || otherObj.layer == LayerMask.NameToLayer(Common.CO.LAYER_FLOOR)
            )
            {
                base.speed = 0;
                fixedTurnSpeed = 0;
                photonView.RPC("StopStructure", PhotonTargets.Others);
            }
        }

        base.OnHit(otherObj);

        base.isHit = false;
    }

    [PunRPC]
    private void StopStructure()
    {
        base.speed = 0;
        fixedTurnSpeed = 0;
        if (pointCtrl != null) pointCtrl.SetOff();
    }
}

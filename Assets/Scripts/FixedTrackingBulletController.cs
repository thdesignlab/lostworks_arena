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
    protected bool isNeedLock = false;    //誘導に要ロック(画面に捕らえている)

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
            if (enableSetAngle)
            {
                base.SetAngle(base.targetTran, fixedTurnSpeed);
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
        }
    }

    public void Shoot()
    {
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
        photonView.RPC("ShootRPC", PhotonTargets.All, enableSetAngle);
    }

    [PunRPC]
    public void ShootRPC(bool isSetAngle)
    {
        //向き調整
        if (isSetAngle)
        {
            Vector3 diffVector = base.DifferentialCorrection(base.targetTran, fixedSpeed);
            myTran.LookAt(base.targetTran.position + diffVector);
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
        //床に刺さる
        if (otherObj.layer == LayerMask.NameToLayer(Common.CO.LAYER_STRUCTURE)
            || otherObj.layer == LayerMask.NameToLayer(Common.CO.LAYER_FLOOR)
        )
        {
            base.speed = 0;
            fixedTurnSpeed = 0;
            photonView.RPC("StopStructure", PhotonTargets.Others);
        }

        base.OnHit(otherObj);

        base.isHit = false;
    }

    [PunRPC]
    private void StopStructure()
    {
        base.speed = 0;
        fixedTurnSpeed = 0;
    }
}

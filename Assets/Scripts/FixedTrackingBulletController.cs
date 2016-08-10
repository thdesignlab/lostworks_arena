using UnityEngine;
using System.Collections;

public class FixedTrackingBulletController : BulletController
{
    [SerializeField]
    protected float fixTime;    //固定するまでの時間
    [SerializeField]
    private float rollSpeed;   //回転速度
    [SerializeField]
    private Vector3 rollVector; //回転軸
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

    protected bool isFix = false;
    protected bool isShoot = false;

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
            if (rollSpeed > 0)
            {
                if (rollVector == Vector3.zero)
                {
                    float x = Random.Range(0, 1.0f);
                    float y = Random.Range(0, 1.0f);
                    float z = Random.Range(0, 1.0f);
                    rollVector = new Vector3(x, y, z);
                }
                myTran.Rotate(rollVector, rollSpeed * Time.deltaTime);
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
            base.myTran.LookAt(base.targetTran.position + diffVector);
        }

        PlayAudio();
        base.speed = fixedSpeed;
        isShoot = true;
        isFix = false;
    }
}

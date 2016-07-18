﻿using UnityEngine;
using System.Collections;

public abstract class BaseMoveController : Photon.MonoBehaviour
{
    protected Transform myTran;
    protected Rigidbody myRigidbody;

    [SerializeField]
    protected bool isCheckGrounded = false;
    [SerializeField]
    protected float glideGravityRate = 1;
    [SerializeField]
    protected bool isLockOn = false;
    [SerializeField]
    protected float preserveSpeedTime = 0;
    protected bool isPreserveSpeed = false;
    protected float leftPreserveSpeedTime = 0;

    //地面接地判定用
    protected Transform groundCheckTran;
    private float groundLimit = 1.0f;
    protected bool isGrounded = false;

    //移動判定用
    protected bool isMoving = false;
    protected Vector3 moveDiffVector = Vector3.zero;
    protected float flameTime = 0;

    //ブースト中判定
    protected bool isBoost = false;

    //ノックバック
    protected float knockBackBaseTime = 0.2f;
    protected bool isKnockBack = false;

    protected virtual void Awake()
    {
        myTran = transform;
        myRigidbody = GetComponent<Rigidbody>();
    }

    protected virtual void Start()
    {
        //地面接地判定
        StartCoroutine(checkGrounded());

        //移動中判定
        StartCoroutine(CheckMooving());
    }

    //地面接地判定
    IEnumerator checkGrounded()
    {
        if (!isCheckGrounded) yield break;

        groundCheckTran = myTran.FindChild(Common.CO.PARTS_GROUNDED);
        if (groundCheckTran == null) groundCheckTran = myTran;

        for (;;)
        {
            if (IsHitGroundedRay())
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }
            //Debug.Log("groundCheckTran: " + groundCheckTran.position.ToString()+ " / groundLimit:" + groundLimit.ToString() + " = " + isGrounded.ToString());
            yield return null;
        }
    }
    protected bool IsHitGroundedRay()
    {
        Ray ray = new Ray(groundCheckTran.position, Vector3.down);
        return Physics.Raycast(ray, groundLimit);
    }

    IEnumerator CheckMooving()
    {
        Vector3 prePos = myTran.position;
        for (;;)
        {
            //if (prePos.x == myTran.position.x && prePos.z == myTran.position.z)
            if (Mathf.Abs(prePos.x - myTran.position.x) <= 0.1f && Mathf.Abs(prePos.z - myTran.position.z) <= 0.1f)
            {
                isMoving = false;
            }
            else
            {
                isMoving = true;
            }
            flameTime = Time.deltaTime;
            moveDiffVector = myTran.position - prePos;
            prePos = myTran.position;
            yield return null;
        }
    }

    //ターゲットの方向へ回転(自動)
    protected void LookAtTarget(Transform targetTran, float rotateSpeed, Vector3 rotateVector = default(Vector3), bool finishFlg = true)
    {
        StartCoroutine(RotationToTraget(targetTran, rotateSpeed, rotateVector, finishFlg));
    }
    IEnumerator RotationToTraget(Transform targetTran, float rotateSpeed, Vector3 rotateVector, bool finishFlg)
    {
        for (;;)
        {
            if (targetTran == null) yield break;
            if (SetAngle(targetTran, rotateSpeed, rotateVector))
            {
                if (finishFlg) yield break;
            }
            yield return null;
        }
    }

    //ターゲットの方向へ回転(1フレーム)
    protected bool SetAngle(Transform targetTran, float rotateSpeed, Vector3 rotateVector = default(Vector3))
    {
        if (targetTran == null) return false;
        if (rotateVector == default(Vector3)) rotateVector = Vector3.one;

        Vector3 targetPos = targetTran.position;
        if (isLockOn)
        {
            //速度、距離による差分修正
            targetPos += DifferentialCorrection(targetTran);
        }

        //対象へのベクトル
        float x = (targetPos.x - myTran.position.x) * rotateVector.x;
        float y = (targetPos.y - myTran.position.y) * rotateVector.y;
        float z = (targetPos.z - myTran.position.z) * rotateVector.z;
        Vector3 targetVector = new Vector3(x, y, z).normalized;
        
        if (targetVector == Vector3.zero)
        {
            return true;
        }

        //対象までの角度
        float angleDiff = Vector3.Angle(myTran.forward, targetVector);
        // 回転角
        float angleAdd = rotateSpeed * Time.deltaTime;
        // ターゲットへ向けるクォータニオン
        Quaternion rotTarget = Quaternion.LookRotation(targetVector);
        if (angleDiff <= angleAdd)
        {
            // ターゲットが回転角以内なら完全にターゲットの方を向く
            myTran.rotation = rotTarget;
            return true;
        }
        else
        {
            // ターゲットが回転角の外なら、指定角度だけターゲットに向ける
            float t = (angleAdd / angleDiff);
            myTran.rotation = Quaternion.Slerp(myTran.rotation, rotTarget, t);
        }

        return false;
    }

    protected void DestoryObject(float delay = 0)
    {
        GetComponent<ObjectController>().DestoryObject(delay);
    }

    private Vector3 DifferentialCorrection(Transform targetTran)
    {
        float distance = Vector3.Distance(targetTran.position, myTran.position);
        float myVelocity = GetVelocity(myTran);
        Vector3 targetVelocityVector = GetVelocityVector(targetTran);
        if (distance == 0 || myVelocity == 0)
        {
            return targetTran.position;
        }

        //ターゲットまでの到達時間
        float arriveTime = distance / myVelocity;
        if (arriveTime >= 1.5f) return Vector3.zero;

        //到達するまでにターゲットが移動するベクトル
        Vector3 targetMoveVector = targetVelocityVector * arriveTime;

        return targetMoveVector;
    }

    public Vector3 GetVelocityVector(Transform tran = null)
    {
        Vector3 velocity = Vector3.zero;
        if (tran == null)
        {
            if (moveDiffVector != Vector3.zero || flameTime > 0)
            {
                velocity = moveDiffVector / flameTime;
            }
        }
        else
        {
            BaseMoveController ctrl = tran.GetComponent<BaseMoveController>();
            if (ctrl != null)
            {
                velocity = ctrl.GetVelocityVector();
            }
        }

        return velocity;
    }

    public float GetVelocity(Transform tran = null)
    {
        float v = 0;
        if (tran == null)
        {
            v = GetVelocityVector().magnitude;
        }
        else
        {
            if (tran.tag == Common.CO.TAG_WEAPON)
            {
                v = tran.GetComponent<WeaponController>().GetBulletSpeed();
            }
            else
            {
                v = GetVelocityVector(tran).magnitude;
            }
        }

        return v;
    }

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public bool IsMoving()
    {
        return isMoving;
    }

    //protected abstract void Move(Vector3 vector, float speed, float limit = 0);

    //移動の入力受付(LocalVector)
    protected void Move(Vector3 vector, float speed, float limit = 0)
    {
        //Debug.Log("Move");
        Vector3 moveDirection = myTran.TransformDirection(vector).normalized;
        MoveWorld(moveDirection, speed, limit);
    }

    protected void MoveWorld(Vector3 worldVector, float speed, float limit = 0, bool isOutForce = true)
    {
        //Debug.Log("MoveWorld");
        Vector3 moveDirection = worldVector.normalized * speed;
        if (limit > 0)
        {
            StartCoroutine(Boost(moveDirection, limit, isOutForce));
        }
        else
        {
            MoveProcess(moveDirection * Time.deltaTime);
        }
    }

    //対象にRigidBodyがあり、IsKinematicがONの場合のみ
    protected void TargetKnockBack(Transform targetTran, bool isReturnForce = false)
    {
        if (isKnockBack) return;
        if (myRigidbody == null) return;
        BaseMoveController targetCtrl = targetTran.gameObject.GetComponent<BaseMoveController>();
        if (targetCtrl == null || targetCtrl.myRigidbody == null || !targetCtrl.myRigidbody.isKinematic) return;

        Vector3 velocity = GetVelocityVector();
        float forceRate = myRigidbody.mass / targetCtrl.myRigidbody.mass;
        float force = Mathf.Pow(velocity.magnitude, 2) / 100 * forceRate;
        float limit = knockBackBaseTime * forceRate;
        if (limit < knockBackBaseTime / 2)
        {
            limit = knockBackBaseTime / 2;
        }
        else if (knockBackBaseTime * 2 < limit)
        {
            limit = knockBackBaseTime * 2;
        }
        //Debug.Log(velocity + " / "+force.ToString()+" / "+limit.ToString());
        targetCtrl.MoveWorld(velocity, force, limit);

        if (isReturnForce)
        {
            //自分にも力を加える
        }
    }

    private float startAngle = 135;
    private float totalAngle = 135;
    IEnumerator Boost(Vector3 v, float limit, bool isOutForce = false)
    {
        if (!isOutForce)
        {
            if (isBoost) yield break;
            isBoost = true;
        }
        else
        {
            if (isKnockBack) yield break;
            isKnockBack = true;
        }

        float limitTime = limit;
        float nowTime = 0;
        float nowAngle = startAngle;
        for (;;)
        {
            //時間
            nowTime = Time.deltaTime;
            if (limit < nowTime) nowTime = limit;

            //角度
            nowAngle -= totalAngle * nowTime / limitTime;
            float radian = Mathf.PI / 180 * nowAngle;
            //Debug.Log("limit: "+limit.ToString()+" / angle: "+ nowAngle.ToString() + " / sin: "+ Mathf.Sin(radian).ToString());

            //移動
            MoveProcess(v * Mathf.Sin(radian) * nowTime);

            //残り時間計算
            limit -= nowTime;
            if (limit <= 0) break;
            yield return null;
        }

        if (!isOutForce)
        {
            isBoost = false;
        }
        else
        {
            isKnockBack = false;
        }
    }

    protected virtual void MoveProcess(Vector3 v)
    {
        //子で実装(abstarctにする場合継承しているClass注意)
        return;
    }        
}

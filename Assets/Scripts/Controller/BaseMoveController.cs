using UnityEngine;
using System.Collections;

public abstract class BaseMoveController : Photon.MonoBehaviour
{
    protected Transform myTran;
    protected Rigidbody myRigidbody;
    protected PhotonTransformView ptv;
    protected WeaponController weaponCtrl;

    [SerializeField]
    protected bool isCheckGrounded = false;
    [SerializeField]
    protected float glideGravityRate = 1;
    [SerializeField]
    protected bool isLockOn = false;

    protected float leftPreserveSpeedTime = 0;

    //地面接地判定用
    protected Transform groundCheckTran;
    private float groundLimit = 0.5f;
    protected bool isGrounded = false;

    //移動判定用
    protected bool isMoving = false;
    protected Vector3 moveDiffVector = Vector3.zero;
    protected float flameTime = 0;

    //ブースト中判定
    protected bool isBoost = false;
    protected bool isSpecialBoost = false;

    //ノックバック
    protected float knockBackBaseTime = 0.3f;
    protected bool isKnockBack = false;

    protected bool isNpc = false;

    protected virtual void Awake()
    {
        myTran = transform;
        myRigidbody = GetComponent<Rigidbody>();
        ptv = GetComponent<PhotonTransformView>();
        weaponCtrl = GetComponent<WeaponController>();
    }

    protected virtual void Start()
    {
        if (photonView.isMine)
        {
            //地面接地判定
            StartCoroutine(checkGrounded());
        }

        //if (myTran.tag == "Player")
        //{
            //移動中判定
            StartCoroutine(CheckMooving());
        //}
    }

    protected virtual void Update()
    {
        return;
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

    protected void DestoryObject(bool isSendRpc = false)
    {
        GetComponent<ObjectController>().DestoryObject(isSendRpc);
    }

    protected Vector3 DifferentialCorrection(Transform targetTran, float mySpeed = -1)
    {
        float distance = Vector3.Distance(targetTran.position, myTran.position);
        float myVelocity = mySpeed;
        if (myVelocity < 0) myVelocity = GetVelocity();
        BaseMoveController targetCtrl = targetTran.gameObject.GetComponent<BaseMoveController>();
        Vector3 targetVelocityVector = Vector3.zero;
        if (targetCtrl != null)
        {
            targetVelocityVector = targetCtrl.GetVelocityVector();
        }
        //if (!isNpc) Debug.Log(targetTran.name + " >> v=" + targetVelocityVector.ToString() + " / distance: " + distance.ToString() + " / myVelocity: " + myVelocity.ToString());

        if (distance == 0 || myVelocity == 0)
        {
            return Vector3.zero;
        }

        //ターゲットまでの到達時間
        float arriveTime = distance / myVelocity;
        if (arriveTime >= 1.5f) return Vector3.zero;
        //if (!isNpc) Debug.Log("arriveTime=" + arriveTime.ToString());

        //到達するまでにターゲットが移動するベクトル
        Vector3 targetMoveVector = targetVelocityVector * arriveTime;
        return targetMoveVector;
    }

    public virtual Vector3 GetVelocityVector()
    {
        //Debug.Log("[Parent]GetVelocityVector");

        Vector3 velocity = Vector3.zero;
        if (moveDiffVector != Vector3.zero && flameTime > 0)
        {
            velocity = moveDiffVector / flameTime;
        }

        return velocity;
    }

    public virtual float GetVelocity()
    {
        //Debug.Log("parent GetVelocity");
        float v = 0;
        if (myTran.tag == Common.CO.TAG_WEAPON)
        {
            if (!weaponCtrl)
            {
                weaponCtrl = GetComponent<WeaponController>();
            }
            else
            {
                v = weaponCtrl.GetBulletSpeed();
            }
        }
        else if (myTran.tag == Common.CO.TAG_WEAPON_BIT)
        {
            if (!weaponCtrl)
            {
                weaponCtrl = myTran.parent.GetComponent<WeaponController>();
            }
            else
            {
                v = weaponCtrl.GetBulletSpeed();
            }
        }
        else
        {
            v = GetVelocityVector().magnitude;
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

    public bool IsBoost()
    {
        return isBoost;
    }

    public bool IsKnockBack()
    {
        return isKnockBack;
    }

    public Vector3 GetMoveDiff()
    {
        return moveDiffVector;
    }

    //protected abstract void Move(Vector3 vector, float speed, float limit = 0);

    public void SpecialBoost(Vector3 vector, float speed, float limit)
    {
        Move(vector, speed, limit);
        StartCoroutine(CheckSpecialBoost(limit));
    }
    IEnumerator CheckSpecialBoost(float limit)
    {
        isSpecialBoost = true;
        yield return new WaitForSeconds(limit);
        isSpecialBoost = false;
    }

    //移動の入力受付(LocalVector)
    protected void Move(Vector3 vector, float speed, float limit = 0, bool isOutForce = false)
    {
        //Debug.Log("Move");
        Vector3 moveDirection = myTran.TransformDirection(vector).normalized;
        MoveWorld(moveDirection, speed, limit, isOutForce, false);
    }

    protected void MoveWorld(Vector3 worldVector, float speed, float limit = 0, bool isOutForce = true, bool isSendRPC = true)
    {
        Vector3 moveDirection = worldVector.normalized * speed;
        if (photonView.isMine || ptv == null)
        {
            leftPreserveSpeedTime = 0;

            if (limit > 0)
            {
                if (isOutForce)
                {
                    //ノックバック
                    StartCoroutine(KnockBack(moveDirection, limit));
                }
                else
                {
                    //ブースト
                    leftBoostTime = limit;
                    boostLimitTime = limit;
                    boostVector = moveDirection;
                    if (!isBoost)
                    {
                        StartCoroutine(Boost());
                    }
                }
            }
            else
            {
                if (isBoost && worldVector == Vector3.zero)
                {
                    //ブースト停止
                    leftBoostTime = 0;
                    return;
                }
                MoveProcess(moveDirection * Time.deltaTime);
            }
        }
        else
        {
            if (isSendRPC)
            {
                object[] args = new object[] { worldVector, speed, limit, isOutForce };
                photonView.RPC("MoveWorldRPC", PhotonTargets.Others, args);
            }
        }
    }
    [PunRPC]
    protected void MoveWorldRPC(Vector3 worldVector, float speed, float limit = 0, bool isOutForce = true)
    {
        MoveWorld(worldVector, speed, limit, isOutForce, false);
    }

    //衝突によるノックバック
    //IsKinematicがONの場合のみ
    private float defaultMass = 100;
    protected void TargetKnockBack(Transform targetTran, float rate = 100.0f)
    {
        if (isKnockBack) return;
        BaseMoveController targetCtrl = targetTran.gameObject.GetComponent<BaseMoveController>();
        if (targetCtrl == null) return;

        //質量取得
        float myMass = defaultMass;
        float targetMass = defaultMass;
        if (myRigidbody != null) myMass = myRigidbody.mass;
        if (targetCtrl.myRigidbody != null) targetMass = targetCtrl.myRigidbody.mass;

        Vector3 velocity = GetVelocityVector();
        float forceRate = myMass / targetMass;
        float force = Mathf.Pow(velocity.magnitude, 2) / targetMass * forceRate * rate / 100;
        float limit = knockBackBaseTime * forceRate;
        if (limit < knockBackBaseTime / 2)
        {
            limit = knockBackBaseTime / 2;
        }
        else if (knockBackBaseTime * 2 < limit)
        {
            limit = knockBackBaseTime * 2;
        }
        //Debug.Log(velocity + " / " + force.ToString() + " / " + limit.ToString());
        targetCtrl.MoveWorld(velocity, force, limit);
    }

    //自分の行動による反動
    public void ActionRecoil(Vector3 forceVector, float speed, float limit = 0)
    {
        if (limit <= 0) limit = knockBackBaseTime;
        Move(forceVector, speed, limit, true);
    }
    
    //ブースト(重複不可)
    private float leftBoostTime = 0;
    private float boostLimitTime = 0;
    private Vector3 boostVector = Vector3.zero;
    private float startAngle = 45;
    private float totalAngle = 120;
    IEnumerator Boost()
    {
        if (boostVector == Vector3.zero) yield break;
        if (leftBoostTime < 0) yield break;
        if (boostLimitTime < 0) yield break;

        isBoost = true;
        for (;;)
        {
            //時間
            float processTime = boostLimitTime - leftBoostTime;

            //速度係数
            float sinVal = Common.Func.GetSin(processTime, totalAngle / boostLimitTime, startAngle);

            //移動
            MoveProcess(boostVector * sinVal * Time.deltaTime);

            //残り時間チェック
            leftBoostTime -= Time.deltaTime;
            if (leftBoostTime <= 0) break;
            yield return null;
        }

        isBoost = false;
    }

    //ノックバック
    IEnumerator KnockBack(Vector3 v, float limit)
    {
        if (v == Vector3.zero) yield break;

        isKnockBack = true;

        float leftTime = limit;
        for (;;)
        {
            //時間
            float processTime = limit - leftTime;

            //速度係数
            float sinVal = Common.Func.GetSin(processTime, totalAngle / limit, startAngle);

            //移動
            MoveProcess(v * sinVal * Time.deltaTime);

            //残り時間チェック
            leftTime -= Time.deltaTime;
            if (leftTime <= 0)
            {
                yield return new WaitForSeconds(0.2f);
                break;
            }
            yield return null;
        }

        isKnockBack = false;
    }

    protected virtual void MoveProcess(Vector3 v)
    {
        return;
    }        

    //protected void CheckNpc()
    //{
    //    PlayerStatus status = myTran.root.gameObject.GetComponent<PlayerStatus>();
    //    if (status != null) isNpc = status.IsNpc();
    //}
}

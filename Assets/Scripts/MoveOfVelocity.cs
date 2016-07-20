using UnityEngine;
using System.Collections;

public class MoveOfVelocity : BaseMoveController
{

    protected override void Awake()
    {
        base.Awake();
        base.myRigidbody.isKinematic = false;
        base.myRigidbody.freezeRotation = true;

        CharacterController charaCtrl = GetComponent<CharacterController>();
        if (charaCtrl != null)
        {
            charaCtrl.enabled = false;
        }
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(AddGravity());
    }

    IEnumerator AddGravity()
    {
        if (base.glideGravityRate == 1) yield break;

        Vector3 groundG = Physics.gravity;
        Vector3 glideG = groundG * base.glideGravityRate;
        for (;;)
        {
            Vector3 g = groundG;
            if (!isGrounded)
            {
                //滞空時
                g = glideG;
            }

            if (Physics.gravity != g) Physics.gravity = g;
            yield return null;
        }
    }

    ////移動の入力受付(LocalVector)
    //protected override void Move(Vector3 vector, float speed, float limit = 0)
    //{
    //    //Debug.Log("Move");
    //    Vector3 moveDirection = myTran.TransformDirection(vector).normalized;
    //    MoveWorld(moveDirection, speed, limit);
    //    //if (limit > 0)
    //    //{
    //    //    StartCoroutine(Boost(moveDirection, limit));
    //    //}
    //    //else
    //    //{
    //    //    MoveProcess(moveDirection * Time.deltaTime);
    //    //}
    //}
    
    ////外部から動かす力が働いた際に呼ぶ(WorldVector)
    //protected void MoveWorld(Vector3 worldVector, float speed, float limit = 0, bool isOutForce = true)
    //{
    //    //Debug.Log("Move");
    //    Vector3 moveDirection = worldVector.normalized * speed;
    //    if (limit > 0)
    //    {
    //        StartCoroutine(Boost(moveDirection, limit, isOutForce));
    //    }
    //    else
    //    {
    //        MoveProcess(moveDirection * Time.deltaTime);
    //    }
    //}

    //IEnumerator Boost(Vector3 v, float limit, bool isOutForce = false)
    //{
    //    if (!isOutForce)
    //    {
    //        if (base.isBoost) yield break;
    //    }

    //    for (;;)
    //    {
    //        MoveProcess(v * Time.deltaTime);
    //        limit -= Time.deltaTime;
    //        if (limit <= 0)
    //        {
    //            break;
    //        }
    //        yield return new WaitForSeconds(0.01f);
    //    }

    //    if (!isOutForce)
    //    {
    //        base.isBoost = false;
    //    }
    //}

    //速度変化は全てここで行う
    protected override void MoveProcess(Vector3 v)
    {
        //base.isPreserveSpeed = false;
        base.myRigidbody.velocity += v;

        //同期処理
        if (photonView.isMine && base.ptv != null)
        {
            base.ptv.SetSynchronizedValues(speed: base.myRigidbody.velocity, turnSpeed: 0);
        }
    }

    protected void SetSpeed(float sp)
    {
        base.myRigidbody.velocity = myTran.forward * sp;
    }

    protected void PreserveSpeed(float time = 0)
    {
        if (time == 0) time = base.preserveSpeedTime;
        if (time == 0) return;
        if (base.isPreserveSpeed)
        {
            base.leftPreserveSpeedTime = time;
        }
        StartCoroutine(PreserveSpeedProccess(time));
    }
    IEnumerator PreserveSpeedProccess(float time)
    {
        base.isPreserveSpeed = true;
        Vector3 v = base.myRigidbody.velocity;
        base.leftPreserveSpeedTime = time;
        for (;;)
        {
            leftPreserveSpeedTime -= Time.deltaTime;
            if (!base.isPreserveSpeed || leftPreserveSpeedTime <= 0)
            {
                base.isPreserveSpeed = false;
                yield break;
            }
            base.myRigidbody.velocity = new Vector3(v.x, base.myRigidbody.velocity.y, v.z);
            yield return null;
        }
    }

    public override Vector3 GetVelocityVector(Transform tran = null)
    {
        Vector3 velocity = Vector3.zero;
        if (tran == null)
        {
            velocity = base.myRigidbody.velocity;
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
}

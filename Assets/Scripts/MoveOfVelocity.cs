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

        if (photonView.isMine)
        {
            StartCoroutine(AddGravity());
        }
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
        //Debug.Log("MoveProcess"+v);
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

    protected void PreserveSpeed(float time = 0, float maxSpeed = 0)
    {
        if (time == 0) return;
        if (base.leftPreserveSpeedTime > 0)
        {
            base.leftPreserveSpeedTime = time;
            return;
        }
        StartCoroutine(PreserveSpeedProccess(time, maxSpeed));
    }

    IEnumerator PreserveSpeedProccess(float time, float maxSpeed = 0)
    {
        Vector3 v = new Vector3(base.myRigidbody.velocity.x, 0, base.myRigidbody.velocity.y);
        if (v.magnitude == 0)
        {
            base.leftPreserveSpeedTime = 0;
            yield break;
        }
        base.leftPreserveSpeedTime = time;

        if (maxSpeed > 0 && v.magnitude > maxSpeed)
        {
            v = v.normalized * maxSpeed;
        }

        for (;;)
        {
            leftPreserveSpeedTime -= Time.deltaTime;
            if (base.leftPreserveSpeedTime <= 0)
            {
                base.leftPreserveSpeedTime = 0;
                yield break;
            }
            base.myRigidbody.velocity = new Vector3(v.x, base.myRigidbody.velocity.y, v.z);
            yield return null;
        }
    }

    public override Vector3 GetVelocityVector()
    {
        Vector3 velocity = Vector3.zero;
        velocity = base.myRigidbody.velocity;
        return velocity;
    }
}

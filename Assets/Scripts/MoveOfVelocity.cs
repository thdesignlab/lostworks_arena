using UnityEngine;
using System.Collections;

public class MoveOfVelocity : BaseMoveController
{
    protected bool UseGravity = false;

    protected override void Awake()
    {
        base.Awake();
        base.myRigidbody.isKinematic = false;
        base.myRigidbody.freezeRotation = true;
        if (base.myRigidbody.useGravity)
        {
            base.myRigidbody.useGravity = false;
            UseGravity = true;
        }

        CharacterController charaCtrl = GetComponent<CharacterController>();
        if (charaCtrl != null)
        {
            charaCtrl.enabled = false;
        }
    }

    protected override void Start()
    {
        base.Start();

        if (photonView.isMine && UseGravity)
        {
            StartCoroutine(AddGravity());
        }
    }

    IEnumerator AddGravity()
    {
        Vector3 groundG = Physics.gravity;
        Vector3 glideG = groundG + (groundG * (base.glideGravityRate - 1));
        //Debug.Log(groundG+" >> "+ glideG);
        for (;;)
        {
            Vector3 g = groundG;
            if (!isGrounded)
            {
                //滞空時
                g = glideG;
            }

            MoveProcess(g * Time.deltaTime);
            yield return null;
        }
    }

    //速度変化は全てここで行う
    protected override void MoveProcess(Vector3 v)
    {
        //Debug.Log("MoveProcess"+v);
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

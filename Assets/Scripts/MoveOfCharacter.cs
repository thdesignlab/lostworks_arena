using UnityEngine;
using System.Collections;

public class MoveOfCharacter : BaseMoveController
{
    protected CharacterController charaCtrl;
    [SerializeField]
    protected bool UseGravity = false;

    private float glideTime = 0;
    private Vector3 moveVector = Vector3.zero;

    private bool isCharacterController = false;

    protected override void Awake()
    {
        base.Awake();
        charaCtrl = GetComponent<CharacterController>();
        if (charaCtrl != null)
        {
            isCharacterController = true;
            charaCtrl.enabled = true;
        }
        if (base.myRigidbody != null)
        {
            myRigidbody.isKinematic = true;
            myRigidbody.useGravity = false;
        }
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(MoveRoutine());
    }

    //移動は全てここで行う
    IEnumerator MoveRoutine()
    {
        for (;;)
        {
            //重力加算
            if (UseGravity) moveVector += GetGravity();

            //移動
            if (isCharacterController)
            {
                //CharacterController.Moveによる移動
                charaCtrl.Move(moveVector);
            }
            else
            {
                //Transform.positionによる移動
                base.myTran.position += moveVector;
            }

            //同期処理
            if (photonView.isMine && base.ptv != null)
            {
                base.ptv.SetSynchronizedValues(speed: moveVector / Time.deltaTime , turnSpeed: 0);
            }

            moveVector = Vector3.zero;
            yield return null;
        }
    }

    private Vector3 GetGravity()
    {
        Vector3 g = Physics.gravity;
        Vector3 glideG = g * base.glideGravityRate;

        if (base.isGrounded)
        {
            //接地時
            glideTime = 0;
            g *= Time.deltaTime;
        }
        else
        {
            //滞空時
            glideTime += Time.deltaTime / 4;
            g = glideG * glideTime;

        }

        return g;
    }

    protected override void MoveProcess(Vector3 v)
    {
        moveVector += v;
    }

    protected void PreserveSpeed(float time = 0)
    {
        if (time == 0) time = base.preserveSpeedTime;
        if (time == 0) return;
        if (base.isPreserveSpeed)
        {
            leftPreserveSpeedTime = time;
            return;
        }
        StartCoroutine(PreserveSpeedProccess(time));
    }

    IEnumerator PreserveSpeedProccess(float time)
    {
        base.isPreserveSpeed = true;
        Vector3 v = new Vector3(moveDiffVector.x, 0, moveDiffVector.z);
        base.leftPreserveSpeedTime = time;
        for (;;)
        {
            leftPreserveSpeedTime -= Time.deltaTime;
            if (!base.isPreserveSpeed || leftPreserveSpeedTime <= 0)
            {
                base.isPreserveSpeed = false;
                yield break;
            }
            MoveProcess(v);
            yield return null;
        }
    }

    public override Vector3 GetVelocityVector(Transform tran = null)
    {
        Vector3 velocity = Vector3.zero;
        if (tran == null)
        {
            if (charaCtrl != null)
            {
                velocity = charaCtrl.velocity;
            }
            else
            {
                velocity = base.GetVelocityVector();
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
}

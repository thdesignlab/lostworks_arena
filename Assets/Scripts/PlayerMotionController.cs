using UnityEngine;
using System.Collections;

public class PlayerMotionController : MonoBehaviour
{
    //[SerializeField]
    private Animator animator;
    [SerializeField]
    private GameObject boostEffect;
    [SerializeField]
    private bool isBodyRotate;

    private float runAnimationVelocity = 0.1f;  //モーションを適用するスピード
    //private float backAnimationVelocity = -0.3f;  //Backモーションに移行するスピード

    private Transform myPlayerTran;
    private Transform myBodyTran;   //向きを変えるBody部分
    //private Rigidbody myRigidbody;
    //private PlayerController playerCtrl;
    private BaseMoveController moveCtrl;
    private MeshRenderer shadow;
    private Transform boostEffectTran;
    private float leftBoostEffectTime = 0;

    void Awake()
    {
        myPlayerTran = transform;

        //myRigidbody = GetComponent<Rigidbody>();
        //playerCtrl = GetComponent<PlayerController>();
        moveCtrl = GetComponent<BaseMoveController>();

        Transform shadowTran = myPlayerTran.FindChild(Common.CO.PARTS_GROUNDED);
        if (shadowTran != null)
        {
            shadow = shadowTran.GetComponent<MeshRenderer>();
        }

        if (boostEffect != null)
        {
            boostEffectTran = boostEffect.transform;
        }
    }

    void Start()
    {
        myBodyTran = myPlayerTran.FindChild(Common.Func.GetBodyStructure());
        animator = myBodyTran.GetComponent<Animator>();
    }

    void Update()
    {
        Vector3 moveDiff = moveCtrl.GetMoveDiff();
        Vector3 localMoveDiff = myPlayerTran.InverseTransformDirection(moveDiff).normalized;

        //ジャンプモーションチェック
        CheckJumpMotion(localMoveDiff.y);

        //移動モーション
        CheckMoveMotion(localMoveDiff.x, localMoveDiff.z);
        
        if (IsAttack())
        {
            //攻撃中は体を正面に向ける
            SetBodyAngle();
        }
        else if (moveCtrl.IsMoving())
        {
            //移動中は移動方向へ
            SetBodyAngle(localMoveDiff.x, localMoveDiff.z);
        }

        //ブーストチェック
        bool boostOn = moveCtrl.IsBoost();
        SwitchBoostEffect(boostOn);
    }

    public void SetBodyAngle(float x = 0, float y = 0)
    {
        if (x == 0 && y == 0)
        {
            //体を正面に向ける
            if (isBodyRotate)
            {
                myBodyTran.rotation = myPlayerTran.rotation;
            }
        }
        else
        {
            //体を移動方向に向ける
            //if (y < backAnimationVelocity)
            //{
            //    //後退時
            //    motionName = Common.CO.MOTION_BACK;
            //    x *= -1;
            //    y *= -1;
            //}
            Vector3 lookPos = myBodyTran.root.forward * y + myBodyTran.root.right * x;
            if (isBodyRotate)
            {
                myBodyTran.LookAt(myBodyTran.position + lookPos);
            }
            boostEffectTran.LookAt(myBodyTran.position + lookPos);
        }
    }

    //### 走る ###

    private void CheckMoveMotion(float x = 0, float z = 0)
    {
        bool isRun = true;
        if (Mathf.Abs(x) < runAnimationVelocity && Mathf.Abs(z) < runAnimationVelocity)
        {
            isRun = false;
            x = 0;
            z = 0;
        }

        animator.SetBool(Common.CO.MOTION_RUN, isRun);
        animator.SetFloat(Common.CO.MOTION_RUN_VERTICAL, z);
        animator.SetFloat(Common.CO.MOTION_RUN_HORIZONTAL, x);
    }

    //### ブースト ###

    private void SwitchBoostEffect(bool flg)
    {
        if (boostEffect == null) return;
        boostEffect.SetActive(flg);
        if (flg)
        {
            animator.speed = 1.5f;
        }
        else
        {
            animator.speed = 1.0f;
        }
    }

    //public void StartBoostEffect(float limit)
    //{
    //    if (boostEffect == null) return;

    //    if (leftBoostEffectTime > 0)
    //    {
    //        leftBoostEffectTime = limit;
    //        return;
    //    }

    //    StartCoroutine(BoostEffect(limit));

    //}
    //IEnumerator BoostEffect(float limit)
    //{
    //    leftBoostEffectTime = limit;

    //    boostEffect.SetActive(true);
    //    animator.speed = 1.5f;
    //    for (;;)
    //    {
    //        leftBoostEffectTime -= Time.deltaTime;
    //        if (leftBoostEffectTime <= 0)
    //        {
    //            break;
    //        }
    //        //boostEffectTran.rotation = myBodyTran.rotation;
    //        yield return null;
    //    }
    //    animator.speed = 1.0f;
    //    boostEffect.SetActive(false);
    //}


    //### ジャンプ ###

    private void SetJumpMotion()
    {
        //Debug.Log("Jump on");
        animator.SetBool(Common.CO.MOTION_JUMP, true);
        animator.SetBool(Common.CO.MOTION_DOWN, false);
        shadow.enabled = false;
    }

    private void ResetJumpMotion()
    {
        //Debug.Log("Jump off");
        animator.SetBool(Common.CO.MOTION_JUMP, false);
        animator.SetBool(Common.CO.MOTION_DOWN, false);
        shadow.enabled = true;
    }

    private bool preIsGrounded;
    private void CheckJumpMotion(float y)
    {
        bool isGrounded = moveCtrl.IsGrounded();
        //Debug.Log(preIsGrounded.ToString()+" / "+ isGrounded.ToString());
        if (preIsGrounded != isGrounded)
        {
            if (isGrounded)
            {
                ResetJumpMotion();
            }
            else
            {
                SetJumpMotion();
            }
        }
        else
        {
            bool isDown = false;
            if (y < runAnimationVelocity * -1)
            {
                //降下中
                isDown = true;
            }
            animator.SetBool(Common.CO.MOTION_DOWN, isDown);
        }
        preIsGrounded = isGrounded;
    }

    //### 攻撃 ###

    private bool IsAttack()
    {
        bool isAttack = false;
        foreach (string motionName in Common.CO.attackMotionArray)
        {
            if (animator.GetBool(motionName))
            {
                isAttack = true;
                break;
            }
        }
        return isAttack;
    }
}

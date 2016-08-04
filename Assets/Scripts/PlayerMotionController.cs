using UnityEngine;
using System.Collections;

public class PlayerMotionController : MonoBehaviour
{
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private GameObject boostEffect;

    //private float runAnimationVelocity = 0.1f;  //モーションを適用するスピード
    //private float backAnimationVelocity = -0.3f;  //Backモーションに移行するスピード
    private string[] runMotionNames = new string[] { Common.CO.MOTION_RUN, Common.CO.MOTION_BACK };

    private Transform myPlayerTran;
    private Transform myBodyTran;   //向きを変えるBody部分
    //private Rigidbody myRigidbody;
    private PlayerController playerCtrl;
    private MeshRenderer shadow;
    private Transform boostEffectTran;
    private float leftBoostEffectTime = 0;

    void Awake()
    {
        myPlayerTran = transform;
        myBodyTran = myPlayerTran.FindChild(Common.CO.PARTS_BODY);
        //myRigidbody = GetComponent<Rigidbody>();
        playerCtrl = GetComponent<PlayerController>();

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
        ////ジャンプモーションチェック
        //StartCoroutine(CheckJumpMotion());
    }

    void Update()
    {
        //ジャンプモーションチェック
        CheckJumpMotion();

        if (IsAttack())
        {
            //攻撃中は体を正面に向ける
            SetBodyAngle();
        }
        else if (playerCtrl.IsMoving())
        {
            //移動中は移動方向へ
            Vector3 diffVector = playerCtrl.GetMoveDiff();
            diffVector = myPlayerTran.InverseTransformDirection(diffVector).normalized;
            SetBodyAngle(diffVector.x, diffVector.z);
        }
    }

    public void SetBodyAngle(float x = 0, float y = 0)
    {
        if (x == 0 && y == 0)
        {
            //体を正面に向ける
            myBodyTran.rotation = myPlayerTran.rotation;
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
            myBodyTran.LookAt(myBodyTran.position + lookPos);
        }
    }

    //### 走る ###

    public void SetRunMotion(float x = 0, float y = 0)
    {
        ////体の方向変換
        //SetBodyAngle(x, y);

        if (x == 0 && y == 0)
        {
            leftBoostEffectTime = 0;
            return;
        }

        string motionName = Common.CO.MOTION_RUN;
        //if (y < backAnimationVelocity)
        //{
        //    //後退時
        //    motionName = Common.CO.MOTION_BACK;
        //}

        //モーション設定
        if (!animator.GetBool(motionName))
        {
            InitRunMotion(motionName);
        }
    }

    private void InitRunMotion(string setMotion = "")
    {
        bool isStartMotion = false;
        foreach (string motionName in runMotionNames)
        {
            if (setMotion == motionName)
            {
                animator.SetBool(motionName, true);
                isStartMotion = true;
            }
            else
            {
                animator.SetBool(motionName, false);
            }
        }

        if (isStartMotion) StartCoroutine(CheckRunEnd());
    }

    private string GetRunningMotion()
    {
        string motion = "";
        foreach (string motionName in runMotionNames)
        {
            if (animator.GetBool(motionName))
            {
                motion = motionName;
                break;
            }
        }
        return motion;
    }

    IEnumerator CheckRunEnd()
    {
        for (;;)
        {
            if (GetRunningMotion() == "") break;
            if (!playerCtrl.IsGrounded() || !playerCtrl.IsMoving())
            {
                InitRunMotion();
                break;
            }
            yield return null;
        }
    }

    //### ブースト ###

    public void StartBoostEffect(float limit)
    {
        if (boostEffect == null) return;

        if (leftBoostEffectTime > 0)
        {
            leftBoostEffectTime = limit;
            return;
        }

        StartCoroutine(BoostEffect(limit));

    }
    IEnumerator BoostEffect(float limit)
    {
        leftBoostEffectTime = limit;

        boostEffect.SetActive(true);
        for (;;)
        {
            leftBoostEffectTime -= Time.deltaTime;
            if (leftBoostEffectTime <= 0)
            {
                break;
            }
            boostEffectTran.rotation = myBodyTran.rotation;
            yield return null;
        }
        boostEffect.SetActive(false);
    }


    //### ジャンプ ###

    private void SetJumpMotion()
    {
        //Debug.Log("Jump on");
        animator.SetBool(Common.CO.MOTION_JUMP, true);
        shadow.enabled = false;
    }

    private void ResetJumpMotion()
    {
        //Debug.Log("Jump off");
        animator.SetBool(Common.CO.MOTION_JUMP, false);
        //animator.SetBool(Common.CO.MOTION_LANDING, true);
        shadow.enabled = true;
    }

    private bool preIsGrounded;
    private void CheckJumpMotion()
    {
        bool isGrounded = playerCtrl.IsGrounded();
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

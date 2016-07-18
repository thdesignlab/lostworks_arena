using UnityEngine;
using System.Collections;

public class PlayerMotionController : MonoBehaviour
{
    [SerializeField]
    private Animator animator;

    //private float runAnimationVelocity = 0.1f;  //モーションを適用するスピード
    //private float backAnimationVelocity = -0.3f;  //Backモーションに移行するスピード
    private string[] runMotionNames = new string[] { Common.CO.MOTION_RUN, Common.CO.MOTION_BACK };

    private Transform myPlayerTran;
    private Transform myBodyTran;   //向きを変えるBody部分
    //private Rigidbody myRigidbody;
    private PlayerController playerCtrl;
    private MeshRenderer shadow;

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
    }

    void Start()
    {
        //ジャンプモーション
        StartCoroutine(CheckJumpMotion());
    }

    //### 走る ###

    public void SetRunMotion(float x = 0, float y = 0)
    {
        //体の方向変換
        SetBodyAngle(x, y);

        if (x == 0 && y == 0) return;

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
    IEnumerator CheckJumpMotion()
    {
        for (;;)
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
            yield return null;
        }
    }

    //### 攻撃 ###

}

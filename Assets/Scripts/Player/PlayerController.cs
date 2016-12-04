using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MoveOfCharacter
{
    [SerializeField]
    private bool isAttackAndTurn;
    [SerializeField]
    private float attackPreserveSpeedTime = 0;    //攻撃時速度維持時間
    [SerializeField]
    private float movePreserveSpeedTime = 0;  //移動終了時速度維持時間
    [SerializeField]
    private GameObject circleArrow;
    private Transform circleArrowTran;
    
    private PlayerMotionController motionCtrl;
    private Animator animator;
    private PlayerStatus status;
    private Transform targetTran;
    private PlayerStatus targetStatus;

    private Dictionary<int, WeaponController> rightHandCtrls = new Dictionary<int, WeaponController>();
    private Dictionary<int, WeaponController> leftHandCtrls = new Dictionary<int, WeaponController>();
    private Dictionary<int, WeaponController> shoulderCtrls = new Dictionary<int, WeaponController>();
    private WeaponController subCtrl;
    private ExtraWeaponController extraCtrl;

    private Button leftHandBtn;
    private Button rightHandBtn;
    private Button shoulderBtn;
    private Button subBtn;
    private Button extraBtn;

    private bool isPC = false;

    private Vector3 setAngleVector = new Vector3(1, 0, 1);

    private bool isActiveSceane = true;

    // Use this for initialization
    protected override void Awake()
    {
        base.Awake();

        if (SceneManager.GetActiveScene().name == Common.CO.SCENE_CUSTOM)
        {
            //カスタム画面
            isActiveSceane = false;
        }

        if (photonView.isMine)
        {
            motionCtrl = GetComponent<PlayerMotionController>();
            status = GetComponent<PlayerStatus>();

            if (isActiveSceane)
            {
                if (circleArrow != null)
                {
                    circleArrowTran = circleArrow.transform;
                }

                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                    case RuntimePlatform.IPhonePlayer:
                        isPC = false;
                        break;
                    default:
                        isPC = true;
                        break;
                }
            }
        }
    }

    protected override void Start()
    {
        if (!isActiveSceane) return;

        base.Start();
        if (photonView.isMine)
        {
            //キャンパスボタン構造
            Transform screenInputBtnTran = Camera.main.transform.FindChild(Common.CO.SCREEN_CANVAS + Common.CO.SCREEN_INPUT_BUTTON);

            leftHandBtn = screenInputBtnTran.FindChild(Common.CO.BUTTON_LEFT_ATTACK).GetComponent<Button>();
            rightHandBtn = screenInputBtnTran.FindChild(Common.CO.BUTTON_RIGHT_ATTACK).GetComponent<Button>();
            shoulderBtn = screenInputBtnTran.FindChild(Common.CO.BUTTON_SHOULDER_ATTACK).GetComponent<Button>();
            subBtn = screenInputBtnTran.FindChild(Common.CO.BUTTON_USE_SUB).GetComponent<Button>();
            extraBtn = screenInputBtnTran.FindChild(Common.CO.BUTTON_EXTRA_ATTACK).GetComponent<Button>();

            SetWeapon();
        }
    }

    protected override void Update()
    {
        if (!isActiveSceane) return;

        base.Update();

        if (photonView.isMine)
        {
            if (targetTran == null || targetStatus == null)
            {
                SearchTarget();
            }
            else
            {
                if (base.isBoost)
                {
                    base.SetAngle(targetTran, status.boostTurnSpeed, setAngleVector);
                }

                if (targetStatus.IsLocked())
                {
                    SetAngle(targetTran, status.turnSpeed, setAngleVector);
                    if (circleArrow != null) circleArrow.SetActive(false);
                }
                else
                {
                    if (circleArrow != null)
                    {
                        circleArrowTran.LookAt(new Vector3(targetTran.position.x, circleArrowTran.position.y, targetTran.position.z));
                        circleArrow.SetActive(true);
                    }
                }
            }

            if (isPC && (UserManager.isAdmin || MyDebug.Instance.isDebugMode))
            {
                float x = Input.GetAxis("Horizontal");
                float y = Input.GetAxis("Vertical");
                bool j = Input.GetButtonDown("Jump");
                if (j)
                {
                    if (x > 0)
                    {
                        x = 1;
                    }
                    else if (x < 0)
                    {
                        x = -1;
                    }
                    if (y > 0)
                    {
                        y = 1;
                    }
                    else if (y < 0)
                    {
                        y = -1;
                    }
                    Jump((int)x, (int)y);
                }
                else if (x != 0 || y != 0)
                {
                    Run(x, y);
                }
            }
        }
    }

    public void SetWeapon()
    {
        //アニメーション
        Transform mainBody = myTran.FindChild(Common.Func.GetBodyStructure());
        animator = mainBody.GetComponent<Animator>();

        WeaponController wepCtrl;
        foreach (int partsNo in Common.CO.partsNameArray.Keys)
        {
            string partsName = Common.Func.GetPartsStructure(Common.CO.partsNameArray[partsNo]);
            Transform parts = myTran.FindChild(partsName);
            if (parts == null) continue;
            //foreach (Transform child in base.myTran)
            //{
            wepCtrl = parts.GetComponentInChildren<WeaponController>();
            if (wepCtrl == null) continue;
            wepCtrl.SetTarget(targetTran);

            switch (parts.name)
            {
                case Common.CO.PARTS_LEFT_HAND:
                    //通常時左武器
                    wepCtrl.SetMotionCtrl(animator, Common.CO.MOTION_LEFT_ATTACK);
                    wepCtrl.SetBtn(leftHandBtn);
                    leftHandCtrls[Common.CO.WEAPON_NORMAL] = wepCtrl;
                    break;

                case Common.CO.PARTS_LEFT_HAND_DASH:
                    //ダッシュ時左武器
                    wepCtrl.SetMotionCtrl(animator, Common.CO.MOTION_LEFT_ATTACK);
                    wepCtrl.SetBtn(leftHandBtn);
                    leftHandCtrls[Common.CO.WEAPON_DASH] = wepCtrl;
                    break;

                case Common.CO.PARTS_RIGHT_HAND:
                    //通常時右武器
                    wepCtrl.SetMotionCtrl(animator, Common.CO.MOTION_RIGHT_ATTACK);
                    wepCtrl.SetBtn(rightHandBtn);
                    rightHandCtrls[Common.CO.WEAPON_NORMAL] = wepCtrl;
                    break;

                case Common.CO.PARTS_RIGHT_HAND_DASH:
                    //ダッシュ時右武器
                    wepCtrl.SetMotionCtrl(animator, Common.CO.MOTION_RIGHT_ATTACK);
                    wepCtrl.SetBtn(rightHandBtn);
                    rightHandCtrls[Common.CO.WEAPON_DASH] = wepCtrl;
                    break;

                case Common.CO.PARTS_SHOULDER:
                    //通常時背中武器
                    wepCtrl.SetMotionCtrl(animator, Common.CO.MOTION_SHOULDER_ATTACK);
                    wepCtrl.SetBtn(shoulderBtn);
                    shoulderCtrls[Common.CO.WEAPON_NORMAL] = wepCtrl;
                    break;

                case Common.CO.PARTS_SHOULDER_DASH:
                    //ダッシュ時背中武器
                    wepCtrl.SetMotionCtrl(animator, Common.CO.MOTION_SHOULDER_ATTACK);
                    wepCtrl.SetBtn(shoulderBtn);
                    shoulderCtrls[Common.CO.WEAPON_DASH] = wepCtrl;
                    break;

                case Common.CO.PARTS_SUB:
                    //サブ武器
                    wepCtrl.SetMotionCtrl(animator, Common.CO.MOTION_USE_SUB);
                    wepCtrl.SetBtn(subBtn);
                    subCtrl = wepCtrl;
                    break;

                case Common.CO.PARTS_EXTRA:
                    //専用武器
                    extraCtrl = parts.GetComponentInChildren<ExtraWeaponController>();
                    if (extraCtrl != null)
                    {
                        //wepCtrl.SetMotionCtrl(animator, Common.CO.MOTION_EXTRA_ATTACK);
                        wepCtrl.SetBtn(extraBtn, true);
                        extraCtrl.SetInit(wepCtrl, animator, status);
                    }
                    break;
            }
        }
    }

    private void SearchTarget()
    {
        if (targetTran != null) return;

        Transform tran = GameController.Instance.GetTarget();

        if (SetTarget(tran)) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.name != gameObject.name)
            {
                SetTarget(player.transform);
                break;
            }
        }
    }
    public bool SetTarget(Transform tran)
    {
        if (tran == null) return false;
        targetTran = tran;
        targetStatus = tran.gameObject.GetComponent<PlayerStatus>();
        GameController.Instance.SetTarget(tran);
        SetWeapon();
        return true;
    }

    public void QuickTarget(Transform target, bool isSetBodyAngle = false)
    {
        //Debug.Log("QuickTarget");
        if (target == null) return;
        base.LookAtTarget(target, status.boostTurnSpeed, setAngleVector);
        if (isSetBodyAngle) motionCtrl.SetBodyAngle();
    }

    public bool CustomSceaneFire(int partsNo)
    {
        if (isActiveSceane) return false;

        WeaponController ctrl = null;
        switch (partsNo)
        {
            case Common.CO.PARTS_LEFT_HAND_NO:
                ctrl = leftHandCtrls[Common.CO.WEAPON_NORMAL];
                break;
            case Common.CO.PARTS_LEFT_HAND_DASH_NO:
                ctrl = leftHandCtrls[Common.CO.WEAPON_DASH];
                break;
            case Common.CO.PARTS_RIGHT_HAND_NO:
                ctrl = rightHandCtrls[Common.CO.WEAPON_NORMAL];
                break;
            case Common.CO.PARTS_RIGHT_HAND_DASH_NO:
                ctrl = rightHandCtrls[Common.CO.WEAPON_DASH];
                break;
            case Common.CO.PARTS_SHOULDER_NO:
                ctrl = shoulderCtrls[Common.CO.WEAPON_NORMAL];
                break;
            case Common.CO.PARTS_SHOULDER_DASH_NO:
                ctrl = shoulderCtrls[Common.CO.WEAPON_DASH];
                break;
            case Common.CO.PARTS_SUB_NO:
                ctrl = subCtrl;
                break;
            case Common.CO.PARTS_EXTRA_NO:
                if (extraCtrl != null)
                {
                    ctrl = extraCtrl.GetWeaponController();
                }
                break;
        }
        if (ctrl == null) return false;

        ctrl.SetEnable(true);
        ctrl.Fire();

        return true;
    }

    //####################
    //###　TouchAction ###
    //####################

    //private float swipeX = 0;
    //private float swipeY = 0;

    private void TouchAction()
    {
        if (isBoost && !isSpecialBoost)
        {
            //ダッシュ中はその場で停止
            Run();
            return;
        }

        //ジャンプ
        Jump();
    }
    private void DoubleTouchAction()
    {
        if (isBoost && !isSpecialBoost)
        {
            //ダッシュ中はダッシュキャンセル
            Run();
        }

        //降下andターゲット
        FallDown();
    }
    private void SwipeAction(float x, float y)
    {
        //Debug.Log("Swipe :" + x.ToString() + " : " + y.ToString());
        float maxDiff = 50;
        x /= maxDiff;
        y /= maxDiff;
        if (x > 1) x = 1;
        if (x < -1) x = -1;
        if (y > 1) y = 1;
        if (y < -1) y = -1;

        Run(x, y);
    }
    private void FlickAction(float x, float y)
    {
        Jump(x, y);
    }

    //###　CharcterAction ###

    private void Run(float x = 0, float y = 0)
    {
        Vector3 moveDirection = new Vector3(x, 0, y).normalized;
        base.Move(moveDirection, status.runSpeed);
        //motionCtrl.SetRunMotion(x, y);
    }

    private void Jump(float x = 0, float y = 0)
    {
        //Debug.Log("Jump :" + x.ToString() + " : " + y.ToString());
        if (!status.CheckSp(status.boostCost)) return;

        bool isSetAngle = false;
        Vector3 move = Vector3.zero;
        float speed = 0;
        float limit = 0;
        if (x == 0 && y == 0)
        {
            //ジャンプ
            move = Vector3.up;
            speed = status.jumpSpeed;
            limit = status.jumpLimit;
            if (!base.isGrounded)
            {
                speed *= status.glideJump;
            }
            //Debug.Log("speed: " + speed.ToString());
            if (speed == 0) return;

            //base.Move(move, speed);
            base.isGrounded = false;

            //旋回
            QuickTarget(targetTran, isSetAngle);
        }
        else
        {
            //ブースト
            move = new Vector3(x, 0, y);
            speed = status.boostSpeed;
            limit = status.boostLimit;
            if (!base.isGrounded)
            {
                speed *= status.glideBoost;
            }
            if (speed == 0) return;
        }

        if (move == Vector3.zero || speed <= 0) return;

        //SP消費
        status.UseSp(status.boostCost);

        //無敵状態
        status.SetInvincible(true);

        base.Move(move, speed, limit);
    }

    private void FallDown()
    {
        //Debug.Log("FallDown");
        if (!base.isGrounded)
        {
            //無敵時間セット
            status.SetInvincible(true);

            //降下
            StartCoroutine(Landing());
        }
        else
        {
            //慣性移動停止
            base.Move(Vector3.zero, 0);
        }

        //旋回
        //Transform target = SearchTarget();
        QuickTarget(targetTran, true);
    }
    IEnumerator Landing()
    {
        float speed = status.jumpSpeed * -1;
        float landingTime = 0;
        for (;;)
        {
            landingTime += Time.deltaTime;
            if (base.IsHitGroundedRay())
            {
                base.isGrounded = true;
                break;
            }
            if (landingTime >= 3.0f)
            {
                //Debug.Log(landingTime);
                break;
            }

            base.Move(Vector3.up, speed);
            yield return null;
        }
    }

    public void FireExtra()
    {
        if (!photonView.isMine) return;
        if (extraCtrl == null) return;
        Run();
        extraCtrl.Fire(targetTran);
    }

    public void FireRightHand()
    {
        Fire(rightHandCtrls);
    }

    public void FireLeftHand()
    {
        Fire(leftHandCtrls);
    }

    public void FireShoulder()
    {
        Fire(shoulderCtrls);
    }
    private void Fire(Dictionary<int, WeaponController> weaponCtrls)
    {
        if (!photonView.isMine) return;
        if (weaponCtrls.Count <= 0) return;
        if (extraCtrl != null && extraCtrl.IsShooting()) return;

        foreach (int wepNo in weaponCtrls.Keys)
        {
            if (!weaponCtrls[wepNo].IsEnableFire()) return;
        }

        WeaponController weapon = null;
        if (isBoost)
        {
            //ブースト中攻撃
            //QuickTarget(targetTran, true);
            weapon = weaponCtrls[Common.CO.WEAPON_DASH];
        }
        else
        {
            //通常攻撃
            weapon = weaponCtrls[Common.CO.WEAPON_NORMAL];
        }

        if (weapon == null) return;

        //攻撃時速度維持
        base.PreserveSpeed(attackPreserveSpeedTime, status.runSpeed);

        //体の向き変更
        motionCtrl.SetBodyAngle();

        //攻撃
        weapon.Fire(targetTran);
    }

    public void UseSub()
    {
        if (!photonView.isMine) return;
        if (subCtrl == null) return;
        subCtrl.Fire(targetTran);
    }


    //#########################
    //##### TourchManager #####
    //#########################
    private bool isTouchStart = false;
    private bool isTouchEnd = false;
    private bool isSwipe = false;
    //private float lastTouchTime = 0;
    private float doubleTouchTime = 0.2f;
    private float touchPosX = 0;
    private float touchPosY = 0;

    void OnEnable()
    {
        if (!isActiveSceane) return;

        if (photonView.isMine)
        {
            TouchManager.Instance.Drag += OnSwipe;
            TouchManager.Instance.TouchStart += OnTouchStart;
            TouchManager.Instance.TouchEnd += OnTouchEnd;
            TouchManager.Instance.FlickStart += OnFlickStart;
            TouchManager.Instance.FlickComplete += OnFlickComplete;
        }
    }

    void OnDisable()
    {
        if (!isActiveSceane) return;

        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.Drag -= OnSwipe;
            TouchManager.Instance.TouchStart -= OnTouchStart;
            TouchManager.Instance.TouchEnd -= OnTouchEnd;
            TouchManager.Instance.FlickStart -= OnFlickStart;
            TouchManager.Instance.FlickComplete -= OnFlickComplete;
        }
    }

    void OnTouchStart(object sender, CustomInputEventArgs e)
    {
        if (Time.timeScale == 0) return;

        //string text = string.Format("OnTouchStart X={0} Y={1}", e.Input.ScreenPosition.x, e.Input.ScreenPosition.y);
        //Debug.Log(e.Input.);

        //Debug.Log("isTouchStart: "+isTouchStart.ToString());
        string onGuiButton = GameController.OnUGuiButton(e.Input.ScreenPosition);
        if (onGuiButton != null)
        {
            //Debug.Log("UI Click:"+ onGuiButton);
            switch (onGuiButton)
            {
                //PlayerController
                case Common.CO.BUTTON_LEFT_ATTACK:
                    FireLeftHand();
                    break;
                case Common.CO.BUTTON_RIGHT_ATTACK:
                    FireRightHand();
                    break;
                case Common.CO.BUTTON_SHOULDER_ATTACK:
                    FireShoulder();
                    break;
                case Common.CO.BUTTON_USE_SUB:
                    UseSub();
                    break;
                case Common.CO.BUTTON_EXTRA_ATTACK:
                    FireExtra();
                    break;
            }
            return;
        }

        //タッチ座標
        touchPosX = e.Input.ScreenPosition.x;
        touchPosY = e.Input.ScreenPosition.y;

        if (isTouchStart)
        {
            isTouchStart = false;
            isTouchEnd = false;
            DoubleTouchAction();
        }
        else
        {
            isTouchStart = true;
            isTouchEnd = false;
            StartCoroutine(WaitInput());
        }
    }

    void OnTouchEnd(object sender, CustomInputEventArgs e)
    {
        //string text = string.Format("OnTouchEnd X={0} Y={1}", e.Input.ScreenPosition.x, e.Input.ScreenPosition.y);
        //Debug.Log(text);
        if (isSwipe)
        {
            //スワイプ終了時現在の速度維持
            base.PreserveSpeed(movePreserveSpeedTime, status.runSpeed);
        }
        isTouchEnd = true;
        isSwipe = false;

        touchPosX = 0;
        touchPosY = 0;
        //swipeX = 0;
        //swipeY = 0;
    }

    IEnumerator WaitInput()
    {
        yield return new WaitForSeconds(doubleTouchTime);
        if (!isTouchStart) yield break;
        isTouchStart = false;
        if (!isTouchEnd) yield break;
        TouchAction();
    }

    void OnSwipe(object sender, CustomInputEventArgs e)
    {
        //string text = string.Format("OnSwipe Pos[{0},{1}] Move[{2},{3}]", new object[] {
        //                e.Input.ScreenPosition.x.ToString ("0"),
        //                e.Input.ScreenPosition.y.ToString ("0"),
        //                e.Input.DeltaPosition.x.ToString ("0"),
        //                e.Input.DeltaPosition.y.ToString ("0")
        //        });
        //Debug.Log(text);
        //circle.MovePosition(e.Input.DeltaPosition);

        if (!isTouchStart && !isSwipe) return;

        float diffX = e.Input.ScreenPosition.x - touchPosX;
        float diffY = e.Input.ScreenPosition.y - touchPosY;
        float diffLimit = 10.0f;
        if (Mathf.Abs(diffX) < diffLimit && Mathf.Abs(diffY) < diffLimit) return;

        isTouchStart = false;
        isSwipe = true;
        SwipeAction(diffX, diffY);
    }

    void OnFlickStart(object sender, FlickEventArgs e)
    {
        //string text = string.Format("OnFlickStart [{0}] Speed[{1}] Accel[{2}] ElapseTime[{3}]", new object[] {
        //                e.Direction.ToString (),
        //                e.Speed.ToString ("0.000"),
        //                e.Acceleration.ToString ("0.000"),
        //                e.ElapsedTime.ToString ("0.000"),
        //        });
        //Debug.Log(text);
    }

    void OnFlickComplete(object sender, FlickEventArgs e)
    {
        //string text = string.Format("OnFlickComplete [{0}] Speed[{1}] Accel[{2}] ElapseTime[{3}]", new object[] {
        //                e.Direction.ToString (),
        //                e.Speed.ToString ("0.000"),
        //                e.Acceleration.ToString ("0.000"),
        //                e.ElapsedTime.ToString ("0.000")
        //        });
        //Debug.Log(text);

        Vector2 flickVector = new Vector2(e.MovedDistance.x, e.MovedDistance.y).normalized;
        if (flickVector == Vector2.zero) return;

        isTouchStart = false;
        FlickAction(flickVector.x, flickVector.y);
    }

}

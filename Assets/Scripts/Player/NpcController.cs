using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NpcController : MoveOfCharacter
{
    private PlayerStatus status;
    private Transform targetTran;
    private Vector3 randomDirection;

    private int targetType = 1;
    private float walkRadius = 200.0f;  //移動半径
    private float stockSpPer = 75;
    private float quickTargetTime = 3;  //対象へクイックターンする時間
    private float atackIntervalTime = 0;
    private float boostIntervalTime = 3;

    //private WeaponController[] weapons;
    private List<WeaponController> weapons = new List<WeaponController>();

    private float preBoostTime = 0;
    private float leftTargetSearch = 0;
    private float quickTurnTime = 0;
    private int preHp = 0;
    private float preHpPer = 0;

    private Vector3 randomMoveTarget = Vector3.zero;

    //private PlayerMotionController motionCtrl;
    private Animator animator;
    private ExtraWeaponController extraCtrl;

    // Use this for initialization
    protected override void Awake()
    {
        base.Awake();
        status = GetComponent<PlayerStatus>();
        preHp = status.GetNowHp();
        preHpPer = 100;
        //motionCtrl = GetComponent<PlayerMotionController>();
    }

    protected override void Start()
    {
        base.Start();

        SetWeapons();
        SearchTarget();
        StartCoroutine(RandomMoveTarget());
        StartCoroutine(Attack());
    }

    protected override void Update()
    {
        base.Update();

        preBoostTime += Time.deltaTime;
        quickTurnTime += Time.deltaTime;

        if (GameController.Instance.isPractice)
        {
            float hpPer = status.GetNowHpPer();
            if (hpPer <= 25 && preHpPer > 25)
            {
                boostIntervalTime = 0.5f;
                status.runSpeed = 40;
                status.defenceRate = 0.5f;
                Transform super = myTran.FindChild("Effect/Super");
                if (super != null) super.gameObject.SetActive(true);
            }
            else if (hpPer <= 50 && preHpPer > 50)
            {
                boostIntervalTime = 1.0f;
                status.runSpeed = 35;
                status.defenceRate = 0.75f;
            }
            else if (hpPer <= 75 && preHpPer > 75)
            {
                boostIntervalTime = 2.0f;
                status.runSpeed = 30;
                status.defenceRate = 1.0f;
            }
            preHpPer = hpPer;
        }

        //ランダム移動
        RandomMove();

        if (targetTran == null)
        {
            SearchTarget();
        }
        else
        {
            if (quickTurnTime >= quickTargetTime)
            {
                QuickTarget(targetTran);
                quickTurnTime = 0;
            }
            else
            {
                base.SetAngle(targetTran, status.turnSpeed, new Vector3(1, 0, 1));
            }
        }

        //回避
        if (boostIntervalTime > 0)
        {
            bool isBoost = false;
            if (preBoostTime >= boostIntervalTime)
            {
                //一定以上のSPがある場合はブースト
                if (status.GetNowSpPer() >= stockSpPer) isBoost = true;
            }
            else if (preBoostTime >= boostIntervalTime / 10)
            {
                //ダメージを受けている場合はブースト
                int nowHp = status.GetNowHp();
                if (preHp != nowHp)
                {
                    preHp = nowHp;
                    isBoost = true;
                }
            }

            if (isBoost)
            {
                AvoidBoost();
            }
        }
    }

    public void SetLevel(int level)
    {
        //レベル決定
        if (level < 0) level = 0;
        int settingMaxLevel = Common.Mission.npcLevelStatusDic.Count;
        int overLevel = 0;
        if (settingMaxLevel < level)
        {
            overLevel = settingMaxLevel - level;
            level = settingMaxLevel;
        }

        //キャラステータス取得
        int[] npcStatusArray = Common.Character.StatusDic[GameController.Instance.npcNo];
        float[] statusLevelRate = Common.Mission.npcLevelStatusDic[level];
        if (overLevel > 0)
        {
            int i = 0;
            foreach (float addRate in Common.Mission.overLevelState)
            {
                statusLevelRate[i] += addRate * overLevel;
                i++;
            }
        }

        //ステータス設定
        status.SetStatus(npcStatusArray, statusLevelRate);

        //攻撃間隔
        int index = Common.Character.STATUS_ATTACK_INTERVAL;
        atackIntervalTime = npcStatusArray[index] * statusLevelRate[index];

        //ブースト間隔
        index = Common.Character.STATUS_BOOST_INTERVAL;
        boostIntervalTime = npcStatusArray[index] * statusLevelRate[index];

        //ターゲット間隔
        index = Common.Character.STATUS_TARGET_INTERVAL;
        quickTargetTime = npcStatusArray[index] * statusLevelRate[index];

        //ターゲットタイプ
        targetType = npcStatusArray[Common.Character.STATUS_TARGET_TYPE];
        walkRadius = npcStatusArray[Common.Character.STATUS_TARGET_DISTANCE];

        //Debug.Log("level:" + level + "(" + overLevel + ")");
        //Debug.Log("atackIntervalTime:" + atackIntervalTime);
        //Debug.Log("boostIntervalTime:" + boostIntervalTime);
        //Debug.Log("quickTargetTime:" + quickTargetTime);
    }

    public void SetWeapons()
    {
        Transform mainBody = myTran.FindChild(Common.Func.GetBodyStructure());
        animator = mainBody.GetComponent<Animator>();

        Transform partsJoint = myTran.FindChild(Common.CO.PARTS_JOINT);
        foreach (Transform parts in partsJoint)
        {
            WeaponController wepCtrl = parts.GetComponentInChildren<WeaponController>();
            if (wepCtrl != null)
            {
                wepCtrl.SetTarget(targetTran);
                switch (parts.name)
                {
                    case Common.CO.PARTS_LEFT_HAND:
                        //通常時左武器
                        wepCtrl.SetMotionCtrl(animator, Common.CO.MOTION_LEFT_ATTACK);
                        break;

                    case Common.CO.PARTS_LEFT_HAND_DASH:
                        //ダッシュ時左武器
                        wepCtrl.SetMotionCtrl(animator, Common.CO.MOTION_LEFT_ATTACK);
                        break;

                    case Common.CO.PARTS_RIGHT_HAND:
                        //通常時右武器
                        wepCtrl.SetMotionCtrl(animator, Common.CO.MOTION_RIGHT_ATTACK);
                        break;

                    case Common.CO.PARTS_RIGHT_HAND_DASH:
                        //ダッシュ時右武器
                        wepCtrl.SetMotionCtrl(animator, Common.CO.MOTION_RIGHT_ATTACK);
                        break;

                    case Common.CO.PARTS_SHOULDER:
                        //通常時背中武器
                        wepCtrl.SetMotionCtrl(animator, Common.CO.MOTION_SHOULDER_ATTACK);
                        break;

                    case Common.CO.PARTS_SHOULDER_DASH:
                        //ダッシュ時背中武器
                        wepCtrl.SetMotionCtrl(animator, Common.CO.MOTION_SHOULDER_ATTACK);
                        break;

                    case Common.CO.PARTS_SUB:
                        //サブ武器
                        wepCtrl.SetMotionCtrl(animator, Common.CO.MOTION_USE_SUB);
                        break;

                    case Common.CO.PARTS_EXTRA:
                        //専用武器
                        extraCtrl = parts.GetComponentInChildren<ExtraWeaponController>();
                        if (extraCtrl != null)
                        {
                            extraCtrl.SetInit(wepCtrl, animator, status);
                        }
                        continue;
                }
                weapons.Add(wepCtrl);
            }
        }
        //weapons = parts.GetComponentsInChildren<WeaponController>();
        //foreach (WeaponController weapon in weapons)
        //{
        //    weapon.SetTarget(targetTran);
        //}
    }

    IEnumerator RandomMoveTarget()
    {
        //GameObject[] spawnPoint = GameObject.FindGameObjectsWithTag("SpawnPoint");
        //int targetNo = 0;
        leftTargetSearch = 0;

        for (;;)
        {
            leftTargetSearch -= Time.deltaTime;
            if (leftTargetSearch > 0)
            {
                //Debug.Log("moving: "+leftTargetSearch.ToString());
                yield return null;
                continue;
            }

            Vector3 pos = myTran.position;
            if (targetType == 1)
            {
                if (targetTran != null) pos = targetTran.position;
            }
            
            randomDirection = Random.insideUnitSphere * walkRadius + pos;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1))
            {
                randomMoveTarget = hit.position;
            }
            else
            {
                if (targetTran != null) randomMoveTarget = targetTran.position;
            }
            leftTargetSearch = 5.0f;
        }
    }

    IEnumerator Attack()
    {
        if (atackIntervalTime == 0) yield break;

        int weaponNo = 0;
        for (;;)
        {
            if (!GameController.Instance.isGameStart)
            {
                yield return null;
                continue;
            }

            float interval = atackIntervalTime;

            if (targetTran == null)
            {
                yield return null;
                continue;
            }

            //専用武器
            if (extraCtrl != null)
            {
                if (extraCtrl.IsShooting())
                {
                    yield return null;
                    continue;
                }

                if (extraCtrl.IsEnabled())
                {
                    extraCtrl.Fire(targetTran);
                    continue;
                }
            }

            //通常武器
            weaponNo = Random.Range(0, weapons.Count);
            for (;;)
            {
                if (weapons[weaponNo].IsEnableFire())
                {
                    QuickTarget(targetTran);
                    yield return new WaitForSeconds(0.1f);
                    quickTurnTime = 0;
                    weapons[weaponNo].Fire(targetTran);
                    break;
                }
                weaponNo = (weaponNo + 1) % weapons.Count;
                yield return null;
            }
            yield return new WaitForSeconds(interval);
        }
    }

    private void SearchTarget()
    {
        if (targetTran != null) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.name != gameObject.name)
            {
                targetTran = player.transform;
                targetTran.gameObject.GetComponent<PlayerStatus>().SetLocked(true);
                break;
            }
        }
    }

    //###　CharcterAction ###

    private void Run(float x, float y)
    {
        Vector3 moveDirection = new Vector3(x, 0, y);
        base.MoveWorld(moveDirection, status.runSpeed);
    }

    private void Jump(int x, int y)
    {
        //Debug.Log("sp:" + status.GetNowSpPer());
        if (!status.CheckSp(status.boostCost))
        {
            return;
        }
        //Debug.Log("Jump: " + x.ToString() + " / " + y.ToString());
        Vector3 move = Vector3.zero;
        float speed = 0;
        float limit = 0;
        if (x == 0 && y == 0)
        {
            //ジャンプ
            move = Vector3.up;
            speed = status.jumpSpeed;
            limit = status.jumpLimit;
            if (!isGrounded)
            {
                speed *= status.glideJump;
            }
            if (speed == 0) return;

            //base.Move(move, speed);
            isGrounded = false;
        }
        else
        {
            //ブースト
            move = new Vector3(x, 0, y);
            speed = status.boostSpeed;
            limit = status.boostLimit;
            if (!isGrounded)
            {
                speed *= status.glideBoost;
            }
            if (speed == 0) return;
        }

        if (move != Vector3.zero && speed > 0)
        {
            //SP消費
            status.UseSp(status.boostCost);

            //無敵時間セット
            status.SetInvincible(true, status.invincibleTime);

            base.Move(move, speed, limit);
        }

        //旋回
        //if (target != null)
        //{
        //StartCoroutine(quickTarget(target));
        QuickTarget(targetTran);
        //}
    }

    private void FallDown()
    {
        //Debug.Log("FallDown");

        //Debug.Log("FallDown");
        //無敵時間セット
        status.SetInvincible(true, status.invincibleTime);

        //降下
        StartCoroutine(Landing());

        //旋回
        //if (target != null)
        //{
        //StartCoroutine(quickTarget(target));
        QuickTarget(targetTran);
        //}
    }
    IEnumerator Landing()
    {
        float speed = status.jumpSpeed * -1;
        for (;;)
        {
            if (isGrounded)
            {
                break;
            }

            base.Move(Vector3.up, speed);
            yield return null;
        }
    }

    private void QuickTarget(Transform target)
    {
        if (target == null) return;
        base.LookAtTarget(target, status.boostTurnSpeed, new Vector3(1, 0, 1));
    }
    
    void OnTriggerStay(Collider other)
    {
        if (preBoostTime < boostIntervalTime || boostIntervalTime == 0) return;

        if (Common.Func.IsDamageAffect(other.gameObject.tag))
        {
            AvoidBoost();
        }
    }

    private void AvoidBoost()
    {
        preBoostTime = 0;

        //回避行動
        //Vector3 bulletVector = myTran.position - other.transform.position;
        int x = Random.Range(-1, 2);
        int y = Random.Range(-1, 2);
        //Debug.Log(x + " >> " + y);
        Jump(x, y);
    }

    private void RandomMove()
    {
        if (status.runSpeed == 0) return;

        Vector3 targetPos = randomMoveTarget;

        float distance = Vector3.Distance(targetPos, myTran.position);
        if (distance < 10)
        {
            leftTargetSearch = 0;
            return;
        }

        Vector3 targetWorldVector = (targetPos - myTran.position).normalized;
        //Vector3 targetVector = myTran.InverseTransformPoint(targetWorldVector);
        //targetVector = new Vector3(targetVector.x, 0, targetVector.z).normalized;
        //Debug.Log(targetWorldVector.ToString() + " >> " + targetVector.ToString());
        Run(targetWorldVector.x, targetWorldVector.z);
    }
}

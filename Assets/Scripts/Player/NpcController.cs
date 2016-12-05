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
    private float atackIntervalTime = 3;
    private float boostIntervalTime = 3;
    private float searchIntervalTime = 3;

    //private WeaponController[] weapons;
    private List<WeaponController> weapons = new List<WeaponController>();

    private float preBoostTime = 0;
    private float leftTargetSearch = 0;
    private float quickTurnTime = 0;
    private int preHp = 0;
    private float preHpPer = 0;

    private int npcLevel = -1;
    private bool isSetWeapon = false;

    private Vector3 randomMoveTarget = Vector3.zero;

    //private PlayerMotionController motionCtrl;
    private Animator animator;
    private ExtraWeaponController extraCtrl;

    //回避
    const int AVOID_JUMP_RATE = 10;

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

        SearchTarget();
        if (GameController.Instance.isPractice)
        {
            EquipWeaponPracticeNpc();
        }
        else
        {
            SetWeapons();
        }
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
            //練習モード
            float hpPer = status.GetNowHpPer();
            if (hpPer <= 25 && preHpPer > 25)
            {
                status.ResetTargetNpcStatus();
                atackIntervalTime = 0.5f;
                boostIntervalTime = 0.5f;
                searchIntervalTime = 2.0f;
                status.runSpeed = 40;
                status.defenceRate = 20;
                Transform super = myTran.FindChild("Effect/Super");
                if (super != null) super.gameObject.SetActive(true);
                SoundManager.Instance.PlayBattleBgm();
            }
            else if (hpPer <= 50 && preHpPer > 50)
            {
                status.ResetTargetNpcStatus();
                atackIntervalTime = 1.0f;
                boostIntervalTime = 1.0f;
                searchIntervalTime = 2.0f;
                status.runSpeed = 35;
                status.defenceRate = 40;
                SoundManager.Instance.PlayBattleBgm();
            }
            else if (hpPer <= 75 && preHpPer > 75)
            {
                status.ResetTargetNpcStatus();
                atackIntervalTime = 2.0f;
                boostIntervalTime = 2.0f;
                searchIntervalTime = 3.0f;
                status.runSpeed = 30;
                status.defenceRate = 60;
                SoundManager.Instance.PlayBattleBgm();
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
        npcLevel = level;
        if (level < 0) level = 0;
        int settingMaxLevel = Common.Mission.npcLevelStatusDic.Count - 1;
        int overLevel = 0;
        if (settingMaxLevel < level)
        {
            overLevel = level - settingMaxLevel;
            level = settingMaxLevel;
        }

        //キャラステータス取得
        int[] npcStatusArray = Common.Character.StatusDic[GameController.Instance.npcNo];
        float[] statusLevelRate = new float[Common.Mission.npcLevelStatusDic[level].Length];
        Common.Mission.npcLevelStatusDic[level].CopyTo(statusLevelRate, 0);
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
        status.SetStatus(Common.Character.StatusDic[GameController.Instance.npcNo], statusLevelRate);

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

        //装備強化
        StartCoroutine(SetWeaponCustom());
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

        isSetWeapon = true;
    }

    IEnumerator SetWeaponCustom()
    {
        for (;;)
        {
            if (isSetWeapon) break;
            yield return null;
        }

        if (npcLevel >= Common.Mission.NPC_CUSTOM_CHANGE_LEVEL)
        {
            foreach (WeaponController weapon in weapons)
            {
                int type = Common.Func.RandomDic(Common.Weapon.customTypeNameDic);
                weapon.SetWeaponCustom(type);
            }
        }
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
            leftTargetSearch = searchIntervalTime;
        }
    }

    IEnumerator Attack()
    {
        int weaponNo = 0;
        for (;;)
        {
            if (atackIntervalTime == 0) yield return new WaitForSeconds(1.0f);
            if (!GameController.Instance.isGameStart && !GameController.Instance.isPractice)
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
                    yield return new WaitForSeconds(0.15f);
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
        if (extraCtrl != null && extraCtrl.IsShooting()) return;
        Vector3 moveDirection = new Vector3(x, 0, y);
        MoveWorld(moveDirection, status.runSpeed);
    }

    private void Jump(float x, float y)
    {
        if (extraCtrl != null && extraCtrl.IsShooting()) return;
        if (!status.CheckSp(status.boostCost))
        {
            return;
        }
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
            if (speed <= 0) return;

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

        if (move != Vector3.zero && speed != 0)
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

    public void QuickTarget(Transform target, float turnSpeedRate = 1.0f)
    {
        if (target == null) return;
        LookAtTarget(target, status.boostTurnSpeed * turnSpeedRate, new Vector3(1, 0, 1));
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
        float x = 0;
        float y = 0;
        if (Random.Range(0, 100) > AVOID_JUMP_RATE)
        {
            x = Random.Range(-1.0f, 1.0f);
            y = Random.Range(-1.0f, 1.0f);
        }
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
        Run(targetWorldVector.x, targetWorldVector.z);
    }


    //#### PracticeNpc ####
    private void EquipWeaponPracticeNpc()
    {
        foreach (int partsNo in Common.CO.partsNameArray.Keys)
        {
            if (partsNo == Common.CO.PARTS_EXTRA_NO) continue;

            //ユーザーの武器をコピー
            int weaponNo = UserManager.userEquipment[Common.CO.partsNameArray[partsNo]];

            //武器取得
            string weaponName = Common.Weapon.GetWeaponName(weaponNo, true);

            //部位取得
            string partsName = Common.Func.GetPartsStructure(Common.CO.partsNameArray[partsNo]);
            Transform parts = myTran.FindChild(partsName);
            if (parts != null)
            {
                //装備
                GameObject weapon = EquipWeapon(parts, weaponName);
                if (weapon != null)
                {
                    //ユーザーのカスタムレベルをコピー
                    int type = UserManager.GetWeaponCustomType(weaponNo);
                    weapon.GetComponent<WeaponController>().SetWeaponCustom(type);
                }
            }
        }
        SetWeapons();

        foreach (WeaponController w in weapons)
        {
            w.SetEnable(true);
        }
    }
    private GameObject EquipWeapon(Transform parts, string weaponName)
    {
        if (parts == null || string.IsNullOrEmpty(weaponName)) return null;

        //すでに装備している場合は破棄
        foreach (Transform child in parts)
        {
            PhotonNetwork.Destroy(child.gameObject);
        }

        GameObject weaponObj = PhotonNetwork.Instantiate(Common.Func.GetResourceWeapon(weaponName), parts.position, parts.rotation, 0);
        weaponObj.name = weaponObj.name.Replace("(Clone)", "");
        weaponObj.transform.SetParent(parts.transform, true);

        return weaponObj;
    }
}

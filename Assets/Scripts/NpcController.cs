using UnityEngine;
using System.Collections;

public class NpcController : MoveOfCharacter
{
    private float[] hpRateArray = new float[] { 1, 1.5f, 2 , 3};
    private float[] invincibleTimeArray = new float[] { 1, 1, 1.5f , 1.5f};
    private float[] atackIntervalArray = new float[] { 0, 3, 1 , 0.5f};
    private float[] boostIntervalArray = new float[] { 0, 3, 1 , 0.5f};
    private float[] searchRangeArray = new float[] { 0, 3, 6 , 10};
    private float[] runSpeedArray = new float[] { 1.0f, 1.0f, 1.5f , 1.5f};
    [SerializeField]
    private SphereCollider searchCollider;

    private PlayerStatus status;
    private Transform targetTran;

    //private WeaponController rightHandCtrl;
    //private WeaponController leftHandCtrl;
    //private WeaponController shoulderCtrl;
    private WeaponController[] weapons;

    private float preBoostTime = 0;
    private float leftTargetSearch = 0;
    private float quickTurnTime = 0;

    private float atackIntervalTime;
    private float boostIntervalTime;
    private float runSpeedRate;
    private float invincibleTimeRate;

    private Transform randomMoveTarget;

    private int preHp = 0;

    // Use this for initialization
    protected override void Awake()
    {
        base.Awake();
        status = GetComponent<PlayerStatus>();
        preHp = status.GetNowHp();
   }

    protected override void Start()
    {
        base.Start();

        SearchTarget();
        StartCoroutine(RandomMoveTarget());
        StartCoroutine(Attack());
    }

    protected override void Update()
    {
        base.Update();

        preBoostTime += Time.deltaTime;
        quickTurnTime += Time.deltaTime;

        //ランダム移動
        RandomMove();

        if (targetTran == null)
        {
            SearchTarget();
        }
        else
        {
            if (quickTurnTime >= 3)
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
        if (preBoostTime >= boostIntervalTime && boostIntervalTime > 0)
        {
            int nowHp = status.GetNowHp();
            if (preHp != nowHp)
            {
                preHp = nowHp;
                AvoidBoost();
            }
        };
    }

    public void SetLevel(int npcLevel)
    {
        if (npcLevel < 0 || atackIntervalArray.Length < npcLevel) npcLevel = 0;

        status.ReplaceMaxHp(hpRateArray[npcLevel]);

        float radius = searchRangeArray[npcLevel];
        searchCollider.radius = radius;
        searchCollider.center = new Vector3(0, radius / 3, radius / 3);

        atackIntervalTime = atackIntervalArray[npcLevel];
        boostIntervalTime = boostIntervalArray[npcLevel];
        runSpeedRate = runSpeedArray[npcLevel];
        invincibleTimeRate = invincibleTimeArray[npcLevel];

        if (npcLevel >= 3)
        {
            foreach (Transform child in myTran)
            {
                foreach (int key in Common.CO.partsNameArray.Keys)
                {
                    if (child.name == Common.CO.partsNameArray[key])
                    {
                        child.gameObject.SetActive(true);
                        break;
                    }
                }
            }
        }
        weapons = myTran.GetComponentsInChildren<WeaponController>();
        foreach (WeaponController weapon in weapons)
        {
            weapon.SetTarget(targetTran);
        }
    }

    IEnumerator RandomMoveTarget()
    {
        GameObject[] spawnPoint = GameObject.FindGameObjectsWithTag("SpawnPoint");
        int targetNo = 0;
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

            targetNo = Random.Range(0, spawnPoint.Length + 2);
            if (targetNo >= spawnPoint.Length)
            {
                randomMoveTarget = targetTran;
            }
            else
            {
                randomMoveTarget = spawnPoint[targetNo].transform;
            }
            leftTargetSearch = 3.0f;
        }
    }

    IEnumerator Attack()
    {
        if (atackIntervalTime == 0) yield break;

        int weaponNo = 0;
        for (;;)
        {
            float interval = atackIntervalTime;

            if (targetTran == null)
            {
                yield return new WaitForSeconds(3.0f);
                continue;
            }
            if (weapons[weaponNo].IsEnableFire())
            {
                weapons[weaponNo].Fire(targetTran);
            }
            else
            {
                interval = 0.2f;
            }
            weaponNo = (weaponNo + 1) % weapons.Length;
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
        base.MoveWorld(moveDirection, status.runSpeed * runSpeedRate);
    }

    private void Jump(int x, int y)
    {
        if (!status.CheckSp(status.boostCost)) return;
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
            status.SetInvincible(true, status.invincibleTime * invincibleTimeRate);

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
        status.SetInvincible(true, status.invincibleTime * invincibleTimeRate);

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

    //public void FireRightHand()
    //{
    //    if (!photonView.isMine) return;
    //    if (rightHandCtrl == null) return;
    //    base.PreserveSpeed();
    //    rightHandCtrl.Fire(targetTran);
    //}

    //public void FireLeftHand()
    //{
    //    if (!photonView.isMine) return;
    //    if (leftHandCtrl == null) return;
    //    base.PreserveSpeed();
    //    leftHandCtrl.Fire(targetTran);
    //}

    //public void FireShoulder()
    //{
    //    if (!photonView.isMine) return;
    //    if (shoulderCtrl == null) return;
    //    base.PreserveSpeed();
    //    shoulderCtrl.Fire(targetTran);
    //}

    private void QuickTarget(Transform target)
    {
        if (target == null) return;
        base.LookAtTarget(target, status.boostTurnSpeed, new Vector3(1, 0, 1));
    }
    
    void OnTriggerStay(Collider other)
    {
        if (preBoostTime < boostIntervalTime || boostIntervalTime == 0) return;

        bool isBullet = false;
        foreach (string bulletTag in Common.CO.bulletTagArray)
        {
            if (other.gameObject.CompareTag(bulletTag))
            {
                isBullet = true;
                break;
            }
        }

        if (!isBullet) return;

        AvoidBoost();
    }

    private void AvoidBoost()
    {
        preBoostTime = 0;

        //回避行動
        //Vector3 bulletVector = myTran.position - other.transform.position;
        if (isGrounded && Random.Range(0, 100) < 15)
        {
            Jump(0, 0);
        }
        else
        {
            int x = Random.Range(0, 2);
            int y = Random.Range(-1, 2);
            Jump(x, y);
            preBoostTime = 0;
        }
    }

    private void RandomMove()
    {
        if (runSpeedRate == 0) return;

        Vector3 targetPos = Vector3.zero;
        if (!base.isGrounded && myTran.position.y < 0.5f)
        {
            //落下防止
            randomMoveTarget = null;
            leftTargetSearch = 5;
            Jump(0, 0);
        }
        else
        {
            if (randomMoveTarget != null)
            {
                targetPos = randomMoveTarget.position;
            }
        }

        float distance = Vector3.Distance(targetPos, myTran.position);
        if (distance < 10)
        {
            randomMoveTarget = null;
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

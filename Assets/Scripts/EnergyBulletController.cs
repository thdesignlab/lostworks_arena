using UnityEngine;
using System.Collections;

public class EnergyBulletController : MoveOfCharacter
{
    [SerializeField]
    protected GameObject hitEffect;
    [SerializeField]
    protected float speed; //速度
    [SerializeField]
    protected int damage; //ダメージ量
    [SerializeField]
    protected float stuckTime; //スタック時間
    [SerializeField]
    private bool isPhysicsBulletBreak;
    [SerializeField]
    private bool isEnergyBulletBreak;

    protected float activeTime = 0;
    [SerializeField]
    protected float safetyTime = 0.05f;
    protected bool isHit = false;

    protected Transform targetTran;
    protected PlayerStatus targetStatus;
    //protected AudioSource audioSource;

    private float totalDamage = 0;
    private int sendMinDamage = 5;

    protected Collider myCollider;

    protected override void Awake()
    {
        base.Awake();

        myCollider = myTran.GetComponentInChildren<Collider>();
        if (myCollider != null) myCollider.enabled = false;
    }

    protected override void Update()
    {
        base.Update();

        //稼働時間
        activeTime += Time.deltaTime;
        if (photonView.isMine)
        {
            if (activeTime >= 10) base.DestoryObject();
        }

        if (activeTime >= safetyTime)
        {
            if (myCollider != null) myCollider.enabled = true;
        }

        base.Move(Vector3.forward, speed);
    }

    //衝突時処理
    protected virtual void OnTriggerEnter(Collider other)
    {
        //Debug.Log("OnCollisionEnter: " + other.gameObject.name);
        GameObject otherObj = other.gameObject;
        if (IsSafety(otherObj)) return;
        isHit = true;

        //ダメージを与える
        AddDamage(otherObj);

        //対象を破壊
        TargetDestory(otherObj);

        base.DestoryObject();
    }

    //ダメージ処理
    protected void AddDamage(GameObject hitObj, int dmg = 0)
    {
        //ダメージ処理は所有者のみ行う
        if (photonView.isMine)
        {
            if (dmg == 0) dmg = damage;

            if (hitObj.CompareTag("Player"))
            {
                //プレイヤー
                PlayerStatus status = targetStatus;
                if (hitObj != targetTran)
                {
                    status = hitObj.GetComponent<PlayerStatus>();
                }
                status.AddDamage(damage);

                //ダメージエフェクト
                if (hitEffect != null)
                {
                    PhotonNetwork.Instantiate(Common.Func.GetResourceEffect(hitEffect.name), myTran.position, hitEffect.transform.rotation, 0);
                }

                //スタック
                if (stuckTime > 0)
                {
                    status.AccelerateRunSpeed(0, stuckTime);
                }
            }
        }
    }

    //継続ダメージ処理
    protected void AddSlipDamage(GameObject hitObj, int dmg = 0)
    {
        //ダメージ処理は所有者のみ行う
        if (photonView.isMine)
        {
            if (dmg == 0) dmg = damage;

            if (hitObj.transform == targetTran)
            {
                totalDamage += dmg * Time.deltaTime;
                if (totalDamage >= sendMinDamage)
                {
                    targetStatus.AddDamage((int)totalDamage);
                    totalDamage = totalDamage % 1;
                }
            }
        }
    }

    //ターゲットを破壊する
    protected bool TargetDestory(GameObject hitObj)
    {
        if (photonView.isMine)
        {
            if ((isEnergyBulletBreak && Common.Func.IsBullet(hitObj.tag))
                || (isPhysicsBulletBreak && Common.Func.IsPhysicsBullet(hitObj.tag)))
            {
                hitObj.GetComponent<ObjectController>().DestoryObject(true);
                return true;
            }
        }

        return false;
    }

    //HIT判定スルーチェック
    protected bool IsSafety(GameObject hitObj, bool checkHit = true)
    {
        //一度衝突しているものは無視
        if (checkHit && isHit) return true;

        //ターゲットの場合はHIT
        if (targetTran != null && targetTran.name == hitObj.name) return false;

        return false;
    }

    //弾の初速取得
    public float GetFirstSpeed()
    {
        return speed;
    }

    //ターゲットを設定する
    public virtual void SetTarget(Transform target)
    {
        if (target == null) return;
        //object[] args = new object[] { target.name };
        photonView.RPC("SetTargetRPC", PhotonTargets.All, PhotonView.Get(target.gameObject).viewID);
    }

    [PunRPC]
    protected virtual void SetTargetRPC(int targetViewId)
    {
        PhotonView targetView = PhotonView.Find(targetViewId);
        if (targetView != null)
        {
            targetTran = targetView.gameObject.transform;
            targetStatus = targetView.gameObject.GetComponent<PlayerStatus>();
        }
    }
}

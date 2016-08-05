using UnityEngine;
using System.Collections;

public class BulletController : MoveOfCharacter
{
    [SerializeField]
    protected GameObject hitEffect; //Hit時エフェクト
    [SerializeField]
    protected float speed;  //発射速度
    [SerializeField]
    protected int damage; //ダメージ量
    [SerializeField]
    protected int damagePerSecond; //継続ダメージ量
    [SerializeField]
    protected float stuckTime; //スタック時間
    [SerializeField]
    protected float knockBackRate; //ノックバック係数
    [SerializeField]
    private bool isPhysicsBulletBreak;  //物理弾破壊FLG
    [SerializeField]
    private bool isEnergyBulletBreak;   //エネルギー弾破壊FLG
    [SerializeField]
    private bool isHitBreak;   //衝突時消滅FLG

    [SerializeField]
    protected float safetyTime = 0.05f;
    protected bool isHit = false;
    protected float activeTime = 0;
    protected float totalDamage = 0;

    protected Transform targetTran;
    protected PlayerStatus targetStatus;
    protected Collider myCollider;

    protected const int MIN_SEND_DAMAGE = 5;


    protected override void Awake()
    {
        base.Awake();

        //判定一時削除
        myCollider = myTran.GetComponentInChildren<Collider>();
        if (myCollider != null) myCollider.enabled = false;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();

        //稼働時間
        activeTime += Time.deltaTime;
        if (photonView.isMine)
        {
            if (activeTime >= 15) base.DestoryObject();

            if (activeTime >= safetyTime)
            {
                //判定復活
                if (myCollider != null) myCollider.enabled = true;
            }
        }

        //推進力
        base.Move(Vector3.forward, speed);
    }

    //衝突時処理(Trigger=true)
    void OnTriggerEnter(Collider other)
    {
        GameObject otherObj = other.gameObject;
        OnHit(otherObj);
    }
    
    //接触時処理
    void OnTriggerStay(Collider other)
    {
        GameObject otherObj = other.gameObject;
        OnStay(otherObj);
    }

    //衝突時処理(共通)
    protected void OnHit(GameObject otherObj)
    {
        if (photonView.isMine)
        {
            //Debug.Log(otherObj.name);
            if (IsSafety(otherObj)) return;

            //ダメージを与える
            AddDamage(otherObj);

            //対象を破壊
            TargetDestory(otherObj);

            if (isHit && isHitBreak) base.DestoryObject();
        }
    }

    //接触時処理(共通)
    protected void OnStay(GameObject otherObj)
    {
        if (photonView.isMine)
        {
            if (IsSafety(otherObj, false)) return;

            //ダメージを与える
            AddSlipDamage(otherObj);
        }
    }

    //ダメージ処理
    protected void AddDamage(GameObject hitObj, int dmg = 0)
    {
        if (hitObj.CompareTag("Player"))
        {
            //ダメージ系処理は所有者のみ行う
            if (photonView.isMine)
            {
                if (dmg == 0) dmg = damage;

                //プレイヤーステータス
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
                    status.InterfareMove(stuckTime);
                }

                //ノックバック
                if (knockBackRate > 0)
                {
                    base.TargetKnockBack(hitObj.transform, knockBackRate);
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
            if (dmg == 0) dmg = damagePerSecond;

            if (hitObj.transform == targetTran)
            {
                totalDamage += dmg * Time.deltaTime;
                if (totalDamage >= MIN_SEND_DAMAGE)
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
        if ((isEnergyBulletBreak && Common.Func.IsBullet(hitObj.tag))
            || (isPhysicsBulletBreak && Common.Func.IsPhysicsBullet(hitObj.tag)))
        {
            hitObj.GetComponent<ObjectController>().DestoryObject(true);
            return true;
        }

        return false;
    }

    //HIT判定スルーチェック
    protected bool IsSafety(GameObject hitObj, bool isHitCheck = true)
    {
        //一度衝突しているものは無視
        if (isHit && isHitCheck) return true;

        //エフェクトはスルー
        //HIT判定はエフェクト側で行う
        if (hitObj.tag == Common.CO.TAG_EFFECT) return false;

        if (isHitCheck) isHit = true;

        return false;
    }

    //弾の速度取得
    public float GetSpeed()
    {
        return speed;
    }

    //ターゲットを設定する
    public void SetTarget(Transform target)
    {
        if (target == null) return;
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

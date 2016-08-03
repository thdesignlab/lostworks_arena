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

    //protected int playerId;
    //protected int ownerId;

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

        //プレイヤーIDと所有者ID取得
        //playerId = PhotonNetwork.player.ID;
        //ownerId = photonView.ownerId;
        //audioSource = GetComponent<AudioSource>();

        myCollider = myTran.GetComponentInChildren<Collider>();
        if (myCollider != null) myCollider.enabled = false;
    }

    protected override void Update()
    {
        base.Update();

        //稼働時間
        activeTime += Time.deltaTime;
        if (activeTime >= 10) base.DestoryObject();

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
        if (IsSafety(other.gameObject)) return;
        isHit = true;

        //ダメージを与える
        AddDamage(other.gameObject);

        //対象を破壊
        if (other.gameObject.tag == Common.CO.TAG_BULLET_PHYSICS)
        {
            TargetDestory(other.gameObject);
            isHit = false;
            return;
        }
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
    protected void TargetDestory(GameObject hitObj)
    {
        hitObj.gameObject.GetComponent<ObjectController>().DestoryObject(true);
    }

    //HIT判定スルーチェック
    protected bool IsSafety(GameObject hitObj, bool checkHit = true)
    {
        //一度衝突しているものは無視
        if (checkHit && isHit) return true;

        //ターゲットの場合はHIT
        if (targetTran != null && targetTran.name == hitObj.name) return false;

        ////自分の撃った弾はSafetyTImeの間無視
        //PhotonView pv = PhotonView.Get(hitObj);
        //if (pv != null)
        //{
        //    if (ownerId == pv.ownerId && activeTime <= safetyTime) return true;
        //}
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

    //protected void PlayAudio()
    //{
    //    if (audioSource != null)
    //    {
    //        audioSource.Play();
    //    }
    //}
}

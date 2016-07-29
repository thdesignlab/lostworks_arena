using UnityEngine;
using System.Collections;

public class PhysicsBulletController : MoveOfVelocity
{
    [SerializeField]
    protected GameObject hitEffect;
    [SerializeField]
    protected float firstSpeed;  //発射速度
    [SerializeField]
    protected int damage; //ダメージ量
    [SerializeField]
    protected float stuckTime; //スタック時間
    [SerializeField]
    protected float knockBackRate; //ノックバック係数
    //[SerializeField]
    //protected float angleSpeed; //回転速度

    //protected int playerId;
    protected int ownerId;

    protected float activeTime = 0;
    protected float safetyTime = 0.2f;
    protected bool isHit = false;

    protected Transform targetTran;
    protected PlayerStatus targetStatus;
    protected AudioSource audioSource;

    protected Collider myCollider;

    protected override void Awake()
    {
        base.Awake();

        //プレイヤーID取得
        //playerId = PhotonNetwork.player.ID;
        ownerId = PhotonView.Get(gameObject).ownerId;
        audioSource = GetComponent<AudioSource>();

        if (photonView.isMine)
        {
            myCollider = myTran.GetComponentInChildren<Collider>();
            if (myCollider != null) myCollider.enabled = false;
        }
    }

    protected override void Start()
    {
        base.Start();

        //初速設定
        SetSpeed(firstSpeed);
    }

    protected override void Update()
    {
        base.Update();

        //稼働時間
        activeTime += Time.deltaTime;
        if (activeTime >= 10) base.DestoryObject();

        //if (angleSpeed > 0)
        //{
        //    myTran.Rotate(Vector3.forward, angleSpeed * Time.deltaTime);
        //}

        if (photonView.isMine && activeTime >= safetyTime)
        {
            if (myCollider != null) myCollider.enabled = true;
        }
    }

    //衝突時処理
    protected virtual void OnCollisionEnter(Collision other)
    {
        //Debug.Log("OnCollisionEnter: "+other.gameObject.name);
        if (IsSafety(other.gameObject)) return;
        isHit = true;

        //ダメージを与える
        AddDamage(other.gameObject);

        //対象を破壊
        if (other.gameObject.tag == Common.CO.TAG_BULLET_MISSILE)
        {
            TargetDestory(other.gameObject);
        }

        base.DestoryObject();
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
                    status.AccelerateRunSpeed(0, stuckTime);
                }

                //ノックバック
                if (knockBackRate > 0)
                {
                    base.TargetKnockBack(hitObj.transform, knockBackRate);
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
    protected bool IsSafety(GameObject hitObj)
    {
        //一度衝突しているものは無視
        if (isHit) return true;
        
        //ターゲットの場合はHIT
        if (targetTran != null && targetTran.name == hitObj.name) return false;

        //自分の撃った弾はSafetyTimeの間無視
        PhotonView pv = PhotonView.Get(hitObj);
        if (pv != null)
        {
            if (ownerId == pv.ownerId && activeTime <= safetyTime) return true;
        }

        return false;
    }

    //弾の初速取得
    public float GetFirstSpeed()
    {
        return firstSpeed;
    }

    //ターゲットを設定する
    public void SetTarget(Transform target)
    {
        if (target == null) return;
        //object[] args = new object[] { PhotonView.Get(target.gameObject).viewID }; 
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

    protected void PlayAudio()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}

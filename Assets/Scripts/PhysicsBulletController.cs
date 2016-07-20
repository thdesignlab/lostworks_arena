using UnityEngine;
using System.Collections;

public class PhysicsBulletController : MoveOfVelocity
{
    [SerializeField]
    protected float firstSpeed;  //発射速度
    [SerializeField]
    protected int damage; //ダメージ量
    [SerializeField]
    protected float stuckTime; //スタック時間

    protected int playerId;
    protected int ownerId;

    protected float activeTime = 0;
    protected float safetyTime = 0.5f;
    protected bool isHit = false;

    protected Transform targetTran;

    protected override void Awake()
    {
        base.Awake();

        //プレイヤーIDと所有者ID取得
        playerId = PhotonNetwork.player.ID;
        ownerId = photonView.ownerId;
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
                PlayerStatus targetStatus = hitObj.GetComponent<PlayerStatus>();
                targetStatus.AddDamage(damage);

                if (stuckTime > 0)
                {
                    targetStatus.AccelerateRunSpeed(0, stuckTime);
                }

                //ノックバック
                base.TargetKnockBack(hitObj.transform);
            }
        }
    }

    //ターゲットを破壊する
    protected void TargetDestory(GameObject hitObj)
    {
        hitObj.gameObject.GetComponent<ObjectController>().DestoryObject();
    }

    //HIT判定スルーチェック
    protected bool IsSafety(GameObject hitObj)
    {
        //一度衝突しているものは無視
        if (isHit) return true;
        
        //ターゲットの場合はHIT
        if (targetTran != null && targetTran.name == hitObj.name) return false;

        //自分の撃った弾はSafetyTImeの間無視
        if (playerId == ownerId && activeTime <= safetyTime) return true;

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
        object[] args = new object[] { target.name }; 
        photonView.RPC("SetTargetRPC", PhotonTargets.All, args);
    }

    [PunRPC]
    protected virtual void SetTargetRPC(string targetName)
    {
        GameObject targetObj = GameObject.Find(targetName);
        if (targetObj != null)
        {
            targetTran = targetObj.transform;
        }
    }
}

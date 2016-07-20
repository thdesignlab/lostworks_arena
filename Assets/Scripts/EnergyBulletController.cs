using UnityEngine;
using System.Collections;

public class EnergyBulletController : MoveOfCharacter
{
    [SerializeField]
    protected float speed; //速度
    [SerializeField]
    protected int damage; //ダメージ量

    protected int playerId;
    protected int ownerId;

    protected float activeTime = 0;
    protected float safetyTime = 0.3f;
    protected bool isHit = false;

    protected Transform targetTran;
    private float totalDamage = 0;
    private int sendMinDamage = 5;

    protected override void Awake()
    {
        base.Awake();

        //プレイヤーIDと所有者ID取得
        playerId = PhotonNetwork.player.ID;
        ownerId = photonView.ownerId;
    }

    protected override void Update()
    {
        base.Update();

        //稼働時間
        activeTime += Time.deltaTime;
        if (activeTime >= 10) base.DestoryObject();

        base.Move(Vector3.forward, speed);
    }

    //衝突時処理
    protected virtual void OnTriggerEnter(Collider other)
    {
        //Debug.Log("OnCollisionEnter: "+other.gameObject.name);
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
                hitObj.GetComponent<PlayerStatus>().AddDamage(damage);
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
                totalDamage += damage * Time.deltaTime;
                if (totalDamage >= sendMinDamage)
                {
                    hitObj.GetComponent<PlayerStatus>().AddDamage((int)damage);
                    totalDamage = damage % 1;
                }
            }
        }
    }

    //ターゲットを破壊する
    protected void TargetDestory(GameObject hitObj)
    {
        hitObj.gameObject.GetComponent<ObjectController>().DestoryObject();
    }

    //HIT判定スルーチェック
    protected bool IsSafety(GameObject hitObj, bool checkHit = true)
    {
        //一度衝突しているものは無視
        if (checkHit && isHit) return true;

        //ターゲットの場合はHIT
        if (targetTran != null && targetTran.name == hitObj.name) return false;

        //自分の撃った弾はSafetyTImeの間無視
        if (playerId == ownerId && activeTime <= safetyTime) return true;
        return false;
    }

    //弾の初速取得
    public float GetFirstSpeed()
    {
        return speed;
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

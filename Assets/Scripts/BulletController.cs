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
    protected bool isPhysicsBulletBreak;  //物理弾破壊FLG
    [SerializeField]
    protected bool isEnergyBulletBreak;   //エネルギー弾破壊FLG
    [SerializeField]
    protected bool isHitBreak;   //衝突時消滅FLG

    [SerializeField]
    protected float safetyTime = 0.05f;
    protected bool isHit = false;
    protected float activeTime = 0;
    protected float totalDamage = 0;

    protected Transform targetTran;
    protected PlayerStatus targetStatus;
    protected Collider myCollider;
    protected Transform ownerTran;
    protected int ownerId = -1;

    protected const int MIN_SEND_DAMAGE = 5;

    protected AudioController audioCtrl;

    protected override void Awake()
    {
        base.Awake();

        //判定一時削除
        myCollider = myTran.GetComponentInChildren<Collider>();
        if (myCollider != null) myCollider.enabled = false;

        audioCtrl = myTran.GetComponent<AudioController>();
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
    protected virtual void OnHit(GameObject otherObj)
    {
        if (photonView.isMine)
        {
            //Debug.Log("OnHit:" + otherObj.name);
            if (IsSafety(otherObj)) return;

            //ダメージを与える
            AddDamage(otherObj, damage);

            //対象を破壊
            TargetDestory(otherObj);

            if (isHit && isHitBreak) base.DestoryObject();
        }
    }

    //接触時処理(共通)
    protected virtual void OnStay(GameObject otherObj)
    {
        if (photonView.isMine)
        {
            if (damagePerSecond <= 0) return;
            //Debug.Log("OnStay:"+otherObj.name);
            if (IsSafety(otherObj, false)) return;

            //ダメージを与える
            AddSlipDamage(otherObj, damagePerSecond);
        }
    }

    //ダメージ処理
    protected void AddDamage(GameObject hitObj, int dmg = 0)
    {
        //ダメージ系処理は所有者のみ行う
        if (photonView.isMine)
        {
            if (hitObj.CompareTag("Player"))
            {
                //プレイヤーステータス
                PlayerStatus status = targetStatus;
                if (hitObj.transform != targetTran)
                {
                    status = hitObj.GetComponent<PlayerStatus>();
                }

                if (damage > 0)
                {
                    //ダメージ
                    status.AddDamage(damage);
                    //Debug.Log(hitObj.name + " >> " + myTran.name + "(" + damage + ")");

                    //ダメージエフェクト
                    if (hitEffect != null)
                    {
                        GameObject effectObj = PhotonNetwork.Instantiate(Common.Func.GetResourceEffect(hitEffect.name), myTran.position, hitEffect.transform.rotation, 0);
                        EffectController effectCtrl = effectObj.GetComponent<EffectController>();
                        if (effectCtrl != null) SetOwner(ownerTran);
                    }
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
            else if (hitObj.CompareTag(Common.CO.TAG_STRUCTURE))
            {
                if (myTran.tag == Common.CO.TAG_BULLET_EXTRA) damage *= Common.CO.EXTRA_BULLET_BREAK_RATE;
                hitObj.GetComponent<StructureController>().AddDamage(damage);
            }
        }
    }

    //継続ダメージ処理
    protected void AddSlipDamage(GameObject hitObj, int dmg = 0)
    {
        //ダメージ処理は所有者のみ行う
        if (photonView.isMine)
        {
            //ダメージ計算
            float fltDmg = dmg * Time.deltaTime;
            int addDmg = (int)Mathf.Floor(fltDmg);
            dmg -= addDmg;
            if (dmg > 0)
            {
                //小数部分は確率
                if (dmg * 100 > Random.Range(0, 100)) addDmg += 1;
            }

            if (addDmg > 0)
            {
                if (hitObj.CompareTag("Player"))
                {
                    //プレイヤーステータス
                    PlayerStatus status = targetStatus;
                    if (hitObj.transform != targetTran)
                    {
                        status = hitObj.GetComponent<PlayerStatus>();
                    }
                    status.AddDamage(addDmg);
                    //Debug.Log(hitObj.name + " >> " + myTran.name + "(slip:" + addDmg + ")");
                }
                else if (hitObj.CompareTag(Common.CO.TAG_STRUCTURE))
                {
                    if (myTran.tag == Common.CO.TAG_BULLET_EXTRA) addDmg *= Common.CO.EXTRA_BULLET_BREAK_RATE;
                    hitObj.GetComponent<StructureController>().AddDamage(addDmg);
                }
            }
        }
    }

    //ターゲットを破壊する
    protected bool TargetDestory(GameObject hitObj)
    {
        PhotonView pv = hitObj.GetPhotonView();
        if (pv != null && pv.isMine) return false;

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
        if (hitObj.tag == Common.CO.TAG_EFFECT) return true;
        
        //ターゲットにあたった場合は有効
        if (hitObj.transform != targetTran && hitObj.tag != Common.CO.TAG_STRUCTURE)
        {
            
            //持ち主に当たった場合無視
            if (hitObj.transform == ownerTran) return true;
            
            PhotonView pv = PhotonView.Get(hitObj);
            if (pv != null && pv.ownerId == ownerId) return true;
        }

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

    //持ち主設定
    public void SetOwner(Transform owner)
    {
        if (owner == null) return;
        ownerTran = owner;
        PhotonView pv = PhotonView.Get(ownerTran.gameObject);
        if (pv != null) ownerId = pv.ownerId;
    }

    public virtual string GetBulletDescription()
    {
        string description = "";
        if (damage > 0) description += "Damage: " + damage.ToString()+"\n";
        if (damagePerSecond > 0) description += "DOT: " + damagePerSecond.ToString() + "/s\n";
        if (stuckTime > 0) description += "Stuck: " + stuckTime.ToString() + "\n";
        if (knockBackRate > 0) description += "KnockBack: " + knockBackRate.ToString() + "\n";
        return description;
    }

    protected void PlayAudio(int no = 0)
    {
        if (audioCtrl == null) return;
        audioCtrl.Play(no);
    }
    protected void StopAudio(int no = 0)
    {
        if (audioCtrl == null) return;
        audioCtrl.Stop(no);
    }

    public Transform GetTarget()
    {
        return targetTran;
    }
    public Transform GetOwner()
    {
        return ownerTran;
    }
}

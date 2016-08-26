using UnityEngine;
using System.Collections;

public class EffectController : Photon.MonoBehaviour
{
    [SerializeField]
    protected GameObject damageEffect;
    [SerializeField]
    protected int damage;
    [SerializeField]
    protected int damagePerSecond;
    [SerializeField]
    protected bool isPhysicsBulletBreak;
    [SerializeField]
    protected bool isEnergyBulletBreak;
    [SerializeField]
    protected float ownDamageRate = 1.0f;

    protected Transform myTran;
    protected Transform ownerTran;
    protected PlayerStatus ownerStatus;

    protected virtual void Awake()
    {
        myTran = transform;
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject otherObj = other.gameObject;
        OnHit(otherObj);
    }
    
    protected void OnHit(GameObject otherObj)
    {
        if (photonView.isMine)
        {
            if (otherObj.tag == "Player")
            {
                int dmg = damage;
                if (ownerTran == otherObj.transform)
                {
                    dmg = (int)(dmg * ownDamageRate);
                }

                if (dmg > 0)
                {
                    //ダメージ
                    PlayerStatus status = otherObj.GetComponent<PlayerStatus>();
                    AddDamageProccess(status, dmg);

                    //エフェクト
                    if (damageEffect != null)
                    {
                        GameObject effectObj = PhotonNetwork.Instantiate(Common.Func.GetResourceEffect(damageEffect.name), otherObj.transform.position, damageEffect.transform.rotation, 0);
                        effectObj.GetComponent<EffectController>().SetOwner(ownerTran);
                    }
                }
            }
            else if (otherObj.CompareTag(Common.CO.TAG_STRUCTURE))
            {
                if (myTran.tag == Common.CO.TAG_BULLET_EXTRA) damage *= Common.CO.EXTRA_BULLET_BREAK_RATE;
                otherObj.GetComponent<StructureController>().AddDamage(damage);
            }

            //対象を破壊
            if ((isEnergyBulletBreak && Common.Func.IsBullet(otherObj.tag))
                || (isPhysicsBulletBreak && Common.Func.IsPhysicsBullet(otherObj.tag)))
            {
                otherObj.GetComponent<ObjectController>().DestoryObject(true);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        GameObject otherObj = other.gameObject;
        OnStay(otherObj);
    }

    protected void OnStay(GameObject otherObj)
    {
        if (photonView.isMine)
        {
            if (otherObj.tag == "Player")
            {
                int dmgPS = damagePerSecond;
                if (ownerTran == otherObj.transform) dmgPS = (int)(dmgPS * ownDamageRate);

                if (dmgPS > 0)
                {
                    //ダメージ
                    float dmg = dmgPS * Time.deltaTime;
                    int addDmg = (int)Mathf.Floor(dmg);
                    dmg -= addDmg;
                    if (dmg > 0)
                    {
                        //小数部分は確率
                        if (dmg * 100 > Random.Range(0, 100)) addDmg += 1;
                    }
                    if (addDmg > 0)
                    {
                        PlayerStatus status = otherObj.GetComponent<PlayerStatus>();
                        AddDamageProccess(status, addDmg, true);
                    }
                }
            }
        }
    }

    protected void AddDamageProccess(PlayerStatus status, int dmg, bool isSlip = false)
    {
        //対象へダメージを与える
        bool isDamage = status.AddDamage(dmg, myTran.name, isSlip);

        //与えたダメージのログを保管
        if (isDamage && ownerStatus != null) ownerStatus.SetBattleLog(PlayerStatus.BATTLE_LOG_ATTACK, dmg, myTran.name, isSlip);
    }

    public void SetOwner(Transform owner)
    {
        ownerTran = owner;
        if (ownerTran != null) ownerStatus = ownerTran.GetComponent<PlayerStatus>();
    }
    public Transform GetOwner()
    {
        return ownerTran;
    }
}

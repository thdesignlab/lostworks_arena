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


    protected virtual void Awake()
    {
        myTran = transform;
    }

    void OnTriggerEnter(Collider other)
    {
        GameObject otherObj = other.gameObject;
        OnHit(otherObj);
    }

    //void OnParticleCollision(GameObject otherObj)
    //{
    //    Debug.Log(otherObj);
    //    OnHit(otherObj);
    //}

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
                    PlayerStatus status = otherObj.GetComponent<PlayerStatus>();

                    //ダメージ
                    status.AddDamage(dmg);

                    //エフェクト
                    if (damageEffect != null)
                    {
                        GameObject effectObj = PhotonNetwork.Instantiate(Common.Func.GetResourceEffect(damageEffect.name), otherObj.transform.position, damageEffect.transform.rotation, 0);
                        effectObj.GetComponent<EffectController>().SetOwner(ownerTran);
                    }
                }
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
                        otherObj.GetComponent<PlayerStatus>().AddDamage(addDmg);
                    }
                }
            }
        }
    }

    public void SetOwner(Transform owner)
    {
        ownerTran = owner;
    }
    public Transform GetOwner()
    {
        return ownerTran;
    }
}

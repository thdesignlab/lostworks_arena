using UnityEngine;
using System.Collections;

public class EffectController : Photon.MonoBehaviour
{
    [SerializeField]
    protected int damage;
    [SerializeField]
    protected int damagePerSecond;
    [SerializeField]
    protected bool isPhysicsBulletBreak;
    [SerializeField]
    protected bool isEnergyBulletBreak;

    protected Transform myTran;

    protected virtual void Awake()
    {
        myTran = transform;
    }

    void OnTriggerEnter(Collider other)
    {
        if (photonView.isMine)
        {
            GameObject otherObj = other.gameObject;
            if (otherObj.tag == "Player")
            {
                if (damage > 0)
                {
                    //ダメージ
                    otherObj.GetComponent<PlayerStatus>().AddDamage(damage);
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
        if (photonView.isMine)
        {
            Transform otherTran = other.transform;
            if (otherTran.tag == "Player")
            {
                if (damagePerSecond > 0)
                {
                    //ダメージ
                    float dmg = damagePerSecond * Time.deltaTime;
                    int addDmg = (int)Mathf.Floor(dmg);
                    dmg -= addDmg;
                    if (dmg > 0)
                    {
                        //小数部分は確率
                        if (dmg * 100 > Random.Range(0, 100)) addDmg += 1;
                    }
                    if (addDmg > 0)
                    {
                        otherTran.GetComponent<PlayerStatus>().AddDamage(addDmg);
                    }
                }
            }
        }
    }
}

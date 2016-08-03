using UnityEngine;
using System.Collections;

public class EffectController : Photon.MonoBehaviour
{
    [SerializeField]
    private int damage;
    [SerializeField]
    private int damagePerSecond;

    void OnTriggerEnter(Collider other)
    {
        if (photonView.isMine)
        {
            Transform otherTran = other.transform;
            if (otherTran.tag == "Player")
            {
                if (damage > 0)
                {
                    //ダメージ
                    otherTran.GetComponent<PlayerStatus>().AddDamage(damage);
                }
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
                    otherTran.GetComponent<PlayerStatus>().AddDamage(addDmg);
                }
            }
        }
    }
}

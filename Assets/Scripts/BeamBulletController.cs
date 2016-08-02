using UnityEngine;
using System.Collections;

public class BeamBulletController : EnergyBulletController
{
    [SerializeField]
    private int damagePerSecond;

    //衝突時処理
    protected override void OnTriggerEnter(Collider other)
    {
        //Debug.Log("OnCollisionEnter: " + other.gameObject.name);
        if (IsSafety(other.gameObject)) return;
        isHit = true;

        //ダメージを与える
        AddDamage(other.gameObject);

        //対象を破壊
        if (Common.Func.IsBullet(other.gameObject.tag))
        {
            TargetDestory(other.gameObject);
            isHit = false;
            return;
        }
    }

    //継続ダメージ
    void OnTriggerStay(Collider other)
    {
        //Debug.Log("OnTriggerStay: " + other.name);
        AddSlipDamage(other.gameObject, damagePerSecond);
    }
}

using UnityEngine;
using System.Collections;

public class LaserBulletController : EnergyBulletController
{
    [SerializeField]
    private int damagePerSecond;

    //衝突時処理
    protected override void OnTriggerEnter(Collider other)
    {
        //Debug.Log("OnCollisionEnter: " + other.gameObject.name);
        GameObject otherObj = other.gameObject;
        if (base.IsSafety(otherObj)) return;
        base.isHit = true;

        //ダメージを与える
        base.AddDamage(otherObj);

        //対象を破壊
        base.TargetDestory(otherObj);
    }

    //継続ダメージ
    void OnTriggerStay(Collider other)
    {
        //Debug.Log("OnTriggerStay: " + other.name);
        AddSlipDamage(other.gameObject, damagePerSecond);
    }
}

using UnityEngine;
using System.Collections;

public class EnergyBombController : EnergyBulletController
{
    [SerializeField]
    private float turnSpeed;  //旋回速度

    void FixedUpdate()
    {
        if (base.isHit)
        {
            //向き調整
            base.SetAngle(targetTran, turnSpeed);
        }
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (IsSafety(other.gameObject)) return;

        if (other.transform == targetTran)
        {
            base.isHit = true;
            base.speed *= 0.75f;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!base.isHit) return;

        if (other.transform == targetTran)
        {
            base.AddSlipDamage(other.gameObject);
        }
    }

}

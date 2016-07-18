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
        base.isHit = true;

        if (other.transform.tag == "Player")
        {
            base.SetTarget(other.transform);
            base.speed *= 0.5f;
        }
        return;
    }

    void OnTriggerStay(Collider other)
    {
        if (!base.isHit) return;

        if (other.gameObject.CompareTag("Player"))
        {
            if (photonView.isMine)
            {
                //ダメージ処理
                base.AddDamage(other.gameObject, damage);
            }
        }
    }

    [PunRPC]
    protected override void SetTargetRPC(string targetName)
    {
        GameObject targetObj = GameObject.Find(targetName);
        if (targetObj != null)
        {
            targetTran = targetObj.transform;
            //targetStatus = targetObj.GetComponent<PlayerStatus>();
        }
    }
}

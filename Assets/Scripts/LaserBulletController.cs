using UnityEngine;
using System.Collections;

public class LaserBulletController : Photon.MonoBehaviour
{
    private Transform myTran;

    [SerializeField]
    private int damagePerSecond; //ダメージ量

    private Transform targetTran;
    private float totalDamage = 0;
    private int sendMinDamage = 5;

    void Awake()
    {
        myTran = transform;
    }

    //HIT時処理
    void OnTriggerStay(Collider other)
    {
        //ダメージを与える
        AddDamage(other.gameObject);

        //対象を破壊
        if (Common.Func.IsBullet(other.gameObject.tag))
        {
            TargetDestory(other.gameObject);
            return;
        }
    }

    //ダメージ処理
    private void AddDamage(GameObject hitObj)
    {
        //ダメージ処理は所有者のみ行う
        if (photonView.isMine)
        {
            if (hitObj.transform == targetTran)
            {
                totalDamage += damagePerSecond * Time.deltaTime;
                if (totalDamage >= sendMinDamage)
                {
                    hitObj.GetComponent<PlayerStatus>().AddDamage((int)totalDamage);
                    totalDamage = totalDamage % 1;
                }
            }
        }
    }

    //ターゲットを破壊する
    private void TargetDestory(GameObject hitObj)
    {
        hitObj.gameObject.GetComponent<ObjectController>().DestoryObject(true);
    }

    //ターゲットを設定する
    public void SetTarget(Transform target)
    {
        if (target == null) return;
        targetTran = target;
    }
}

﻿using UnityEngine;
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
    protected float ownDamageRate = 0.0f;
    [SerializeField]
    protected bool isFloorEffect = false;

    protected Transform myTran;
    protected Transform ownerTran;
    protected PlayerStatus ownerStatus;
    protected string ownerWeapon = "";

    protected StatusChangeController statusChangeCtrl;

    protected virtual void Awake()
    {
        myTran = transform;
        if (isFloorEffect) myTran.position = new Vector3(myTran.position.x, 0, myTran.position.z);
        statusChangeCtrl = myTran.GetComponent<StatusChangeController>();
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
            float dmg = damage;
            if (ownerStatus != null) dmg *= (ownerStatus.attackRate / 100);

            if (otherObj.tag == "Player" || otherObj.tag == "Target")
            {
                if (ownerTran == otherObj.transform) dmg *= ownDamageRate;

                if (dmg > 0)
                {
                    //ダメージ
                    PlayerStatus status = otherObj.GetComponent<PlayerStatus>();
                    AddDamageProccess(status, dmg);

                    //エフェクト
                    if (damageEffect != null)
                    {
                        GameObject effectObj = PhotonNetwork.Instantiate(Common.Func.GetResourceEffect(damageEffect.name), otherObj.transform.position, damageEffect.transform.rotation, 0);
                        effectObj.GetComponent<EffectController>().SetOwner(ownerTran, ownerWeapon);
                    }
                }
            }
            else if (otherObj.CompareTag(Common.CO.TAG_STRUCTURE))
            {
                if (myTran.tag == Common.CO.TAG_BULLET_EXTRA) dmg *= Common.CO.EXTRA_BULLET_BREAK_RATE;
                otherObj.GetComponent<StructureController>().AddDamage((int)dmg);
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
            if (otherObj.tag == "Player" || otherObj.tag == "Target")
            {
                float dmgPS = damagePerSecond;
                if (ownerTran == otherObj.transform) dmgPS *= ownDamageRate;
                if (ownerStatus != null) dmgPS *= (ownerStatus.attackRate / 100);

                if (dmgPS > 0)
                {
                    //ダメージ
                    dmgPS *= Time.deltaTime;
                    PlayerStatus status = otherObj.GetComponent<PlayerStatus>();
                    AddDamageProccess(status, dmgPS, true);
                }
            }
        }
    }

    protected void AddDamageProccess(PlayerStatus status, float dmg, bool isSlip = false)
    {
        //対象へダメージを与える
        bool isDamage = status.AddDamage(dmg, ownerWeapon, isSlip);

        //デバフ
        AddDebuff(status);

        //与えたダメージのログを保管
        if (isDamage && ownerStatus != null)
        {
            ownerStatus.SetBattleLog(PlayerStatus.BATTLE_LOG_ATTACK, (int)dmg, ownerWeapon, isSlip);
        }
    }

    protected void AddDebuff(PlayerStatus status)
    {
        if (statusChangeCtrl == null) return;
        statusChangeCtrl.Action(status);
    }

    public void SetOwner(Transform owner, string weaponName)
    {
        ownerTran = owner;
        ownerWeapon = weaponName;
        if (ownerTran != null) ownerStatus = ownerTran.GetComponent<PlayerStatus>();
    }
    public void GetOwner(out Transform tran, out string name)
    {
        tran = ownerTran;
        name = ownerWeapon;
    }
}

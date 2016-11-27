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
    protected float ownDamageRate = 0.0f;
    [SerializeField]
    protected bool isFloorEffect = false;

    private Transform _myTran;
    protected Transform myTran
    {
        get { return _myTran ? _myTran : _myTran = transform; }
    }
    protected Transform ownerTran;
    protected PlayerStatus ownerStatus;
    protected Transform targetTran;
    protected PlayerStatus targetStatus;
    protected Transform weaponTran;
    private ObjectController _obCtrl;
    protected ObjectController obCtrl
    {
        get { return _obCtrl ? _obCtrl : _obCtrl = GetComponent<ObjectController>(); }
    }

    protected StatusChangeController statusChangeCtrl;

    protected virtual void Awake()
    {
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
            //ダメージ計算
            float dmg = damage;
            if (ownerStatus != null) dmg *= (ownerStatus.attackRate / 100);
            if (ownerTran == otherObj.transform) dmg *= ownDamageRate;

            if (otherObj.tag == "Player" || otherObj.tag == "Target")
            {
                if (dmg > 0 || statusChangeCtrl != null)
                {
                    //PlayerStatus
                    PlayerStatus status = GetHitObjStatus(otherObj);

                    //ダメージ
                    AddDamageProccess(status, dmg);

                    //デバフ
                    AddDebuff(status);
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
                //ダメージ計算
                float dmgPS = damagePerSecond;
                if (ownerTran == otherObj.transform) dmgPS *= ownDamageRate;
                if (ownerStatus != null) dmgPS *= (ownerStatus.attackRate / 100);
                dmgPS *= Time.deltaTime;

                if (dmgPS > 0 || statusChangeCtrl != null)
                {
                    //対象のStatuController取得
                    PlayerStatus status = GetHitObjStatus(otherObj);
                    //ダメージ
                    AddDamageProccess(status, dmgPS, true);

                    if (ownerTran != otherObj.transform)
                    {
                        //デバフ
                        AddDebuff(status);
                    }
                }
            }
        }
    }

    protected PlayerStatus GetHitObjStatus(GameObject hitObj)
    {
        return (hitObj.transform == targetTran) ? targetStatus : hitObj.GetComponent<PlayerStatus>();
    }

    protected void AddDamageProccess(PlayerStatus status, float dmg, bool isSlip = false)
    {
        if (dmg <= 0) return;

        //対象へダメージを与える
        float addDamage = status.AddDamage(dmg, GetWeaponName(), isSlip);

        //HITエフェクト
        if (damageEffect != null && !isSlip)
        {
            GameObject effectObj = PhotonNetwork.Instantiate(Common.Func.GetResourceEffect(damageEffect.name), myTran.position, damageEffect.transform.rotation, 0);
            effectObj.GetComponent<EffectController>().EffectSetting(ownerTran, targetTran, weaponTran);
        }

        //与えたダメージのログを保管
        if (addDamage > 0 && ownerStatus != null)
        {
            ownerStatus.SetBattleLog(PlayerStatus.BATTLE_LOG_ATTACK, addDamage, GetWeaponName(), isSlip);
        }
    }

    protected void AddDebuff(PlayerStatus status)
    {
        if (statusChangeCtrl == null) return;
        statusChangeCtrl.Action(status);
    }

    public void EffectSetting(Transform owner, Transform target, Transform weapon)
    {
        SetOwner(owner);
        SetTarget(target);
        SetWeapon(weapon);
    }

    public void SetOwner(Transform owner)
    {
        ownerTran = owner;
        if (obCtrl != null) obCtrl.SetOwner(ownerTran);
        if (ownerTran != null) ownerStatus = ownerTran.GetComponent<PlayerStatus>();
    }
    public Transform GetOwner()
    {
        return ownerTran;
    }

    public void SetTarget(Transform target)
    {
        targetTran = target;
        if (obCtrl != null) obCtrl.SetTarget(targetTran);
        if (targetTran != null) targetStatus = targetTran.GetComponent<PlayerStatus>();
    }

    public void SetWeapon(Transform weapon)
    {
        weaponTran = weapon;
        if (obCtrl != null) obCtrl.SetWeapon(weaponTran);

        //カスタム
    }

    public string GetWeaponName()
    {
        return (weaponTran != null) ? weaponTran.name : myTran.name;
    }
}

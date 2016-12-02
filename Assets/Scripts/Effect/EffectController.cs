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
    private StatusChangeController _statusChangeCtrl;
    protected StatusChangeController statusChangeCtrl
    {
        get { return _statusChangeCtrl ? _statusChangeCtrl : _statusChangeCtrl = GetComponent<StatusChangeController>(); }
        set { _statusChangeCtrl = value; }
    }
    protected EffectLevelController effectLevelCtrl;


    protected virtual void Awake()
    {
        if (isFloorEffect) myTran.position = new Vector3(myTran.position.x, 0, myTran.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetType().Name == "CharacterController") return;
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
        if (other.GetType().Name == "CharacterController") return;
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

    public void EffectSetting(Transform owner, Transform target, Transform weapon, bool isCustom = true)
    {
        SetOwner(owner);
        SetTarget(target);
        SetWeapon(weapon, isCustom);
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

    public void SetWeapon(Transform weapon, bool isCustom)
    {
        weaponTran = weapon;
        if (obCtrl != null) obCtrl.SetWeapon(weaponTran);

        //カスタム
        if (isCustom && weaponTran != null) SetCustom();
    }

    public string GetWeaponName()
    {
        return (weaponTran != null) ? weaponTran.name : myTran.name;
    }


    //##### CUSTOM #####

    private void SetCustom(bool isSendRPC = true)
    {
        EffectLevelController effectLevelCtrl = weaponTran.GetComponent<EffectLevelController>();
        if (effectLevelCtrl != null)
        {
            effectLevelCtrl.EffectCustom(this);
            if (isSendRPC) photonView.RPC("SetCustomRPC", PhotonTargets.Others);
        }
    }
    [PunRPC]
    private void SetCustomRPC()
    {
        SetCustom(false);
    }

    //Damage
    public void CustomDamage(int value)
    {
        damage += value;
    }

    //DamagePerSecont
    public void CustomDPS(int value)
    {
        damagePerSecond += value;
    }

    //EndScale
    public virtual void CustomEndScale(float value)
    {
        myTran.localScale *= value;
        ParticleSystem particle = myTran.GetComponentInChildren<ParticleSystem>();
        if (particle != null) particle.startSize *= value;
    }

    //ActiveTime
    public void CustomActiveTime(float value)
    {
        if (obCtrl != null) obCtrl.CustomActiveTime(value);
    }

    //ActiveDistance
    public void CustomActiveDistance(float value)
    {
        if (obCtrl != null) obCtrl.CustomActiveDistance(value);
    }

    //SpawnObject
    public void CustomBreakEffect(GameObject obj)
    {
        if (obCtrl != null) obCtrl.CustomSpawnEffect(obj);
    }

    //StatusChangeController追加
    private void AddStatusChangeCtrl()
    {
        if (statusChangeCtrl != null) return;
        statusChangeCtrl = gameObject.AddComponent<StatusChangeController>();
    }

    //デバフ時間
    public void CustomDebuffTime(float value)
    {
        AddStatusChangeCtrl();
        statusChangeCtrl.AddEffectTime(value);
    }

    //デバフ:ATTACK
    public void CustomDebuffAttack(float value)
    {
        AddStatusChangeCtrl();
        statusChangeCtrl.AddStatusChange(StatusChangeController.EFFECT_ATTACK, value);
    }

    //デバフ:SP
    public void CustomDebuffSp(float value)
    {
        AddStatusChangeCtrl();
        statusChangeCtrl.AddStatusChange(StatusChangeController.EFFECT_RECOVER_SP, value);
    }

    //デバフ:AVOID
    public void CustomDebuffAvoid(float value)
    {
        AddStatusChangeCtrl();
        statusChangeCtrl.AddStatusChange(StatusChangeController.EFFECT_AVOID, value);
    }

    //デバフ:SPEED
    public void CustomDebuffSpeed(float value)
    {
        AddStatusChangeCtrl();
        statusChangeCtrl.AddStatusChange(StatusChangeController.EFFECT_SPEED, value);
    }

    //デバフ:DEF
    public void CustomDebuffDefence(float value)
    {
        AddStatusChangeCtrl();
        statusChangeCtrl.AddStatusChange(StatusChangeController.EFFECT_DEFENCE, value);
    }
}

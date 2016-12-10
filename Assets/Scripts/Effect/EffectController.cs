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
    //[SerializeField]
    //protected float ownDamageRate = 0.0f;
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
        if (!Common.Func.IsColliderHitTag(other.tag)) return;
        GameObject otherObj = other.gameObject;
        OnHit(otherObj);
    }
    
    protected void OnHit(GameObject otherObj)
    {
        if (photonView.isMine)
        {
            if (ownerTran == otherObj.transform) return;

            //ダメージ計算
            float dmg = damage;
            if (ownerStatus != null) dmg *= (ownerStatus.attackRate / 100);
            
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
            TargetDestory(otherObj);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.GetType().Name == "CharacterController") return;
        if (!Common.Func.IsColliderHitTag(other.tag)) return;
        GameObject otherObj = other.gameObject;
        OnStay(otherObj);
    }

    protected void OnStay(GameObject otherObj)
    {
        if (photonView.isMine)
        {
            if (ownerTran == otherObj.transform) return;

            if (otherObj.tag == "Player" || otherObj.tag == "Target")
            {
                //ダメージ計算
                float dmgPS = damagePerSecond;
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

    //ターゲットを破壊する
    protected bool TargetDestory(GameObject hitObj)
    {
        if (ownerTran == hitObj.transform) return false;

        //自分のオブジェクトは無視
        PhotonView pv = PhotonView.Get(hitObj);
        if (pv != null && pv.isMine && Common.Func.IsBullet(hitObj.tag))
        {
            BulletController hitBullet = hitObj.GetComponent<BulletController>();
            if (hitBullet != null && hitBullet.GetOwner() == ownerTran) return false;
        }

        if ((isEnergyBulletBreak && Common.Func.IsEnergyBullet(hitObj.tag))
            || (isPhysicsBulletBreak && Common.Func.IsPhysicsBullet(hitObj.tag)))
        {
            hitObj.GetComponent<ObjectController>().DestoryObject(true);
            return true;
        }

        return false;
    }

    public void EffectSetting(Transform owner, Transform target, Transform weapon, bool isCustom = true, bool isSendRPC = true)
    {
        SetOwner(owner);
        SetTarget(target);
        SetWeapon(weapon, isCustom);

        if (isSendRPC)
        {
            int ownerViewId = (owner != null) ? PhotonView.Get(owner.gameObject).viewID : -1;
            int targetViewId = (target != null) ? PhotonView.Get(target.gameObject).viewID : -1;
            int weaponViewId = (weapon != null) ? PhotonView.Get(weapon.gameObject).viewID : -1;
            object[] args = new object[] { ownerViewId, targetViewId, weaponViewId, isCustom };
            photonView.RPC("EffectSettingRPC", PhotonTargets.Others, args);
        }
    }
    [PunRPC]
    protected void EffectSettingRPC(int ownerViewId, int targetViewId, int weaponViewId, bool isCustom)
    {
        SetOwnerRPC(ownerViewId);
        SetTargetRPC(targetViewId);
        SetWeaponRPC(weaponViewId, isCustom);
    }

    public void SetOwner(Transform owner, bool isSendRPC = true)
    {
        ownerTran = owner;
        if (obCtrl != null) obCtrl.SetOwner(ownerTran);
        if (ownerTran != null) ownerStatus = ownerTran.GetComponent<PlayerStatus>();

        if (isSendRPC)
        {
            int viewId = -1;
            if (ownerTran != null) viewId = PhotonView.Get(ownerTran.gameObject).viewID;
            photonView.RPC("SetOwnerRPC", PhotonTargets.Others, viewId);
        }
    }
    [PunRPC]
    protected void SetOwnerRPC(int ownerViewId)
    {
        PhotonView ownerView = PhotonView.Find(ownerViewId);
        Transform owner = (ownerView != null) ? ownerView.transform : null;
        SetOwner(owner, false);
    }

    public Transform GetOwner()
    {
        return ownerTran;
    }

    public void SetTarget(Transform target, bool isSendRPC = true)
    {
        targetTran = target;
        if (obCtrl != null) obCtrl.SetTarget(targetTran);
        if (targetTran != null) targetStatus = targetTran.GetComponent<PlayerStatus>();

        if (isSendRPC)
        {
            int viewId = -1;
            if (targetTran != null) viewId = PhotonView.Get(targetTran.gameObject).viewID;
            photonView.RPC("SetTargetRPC", PhotonTargets.Others, viewId);
        }
    }
    [PunRPC]
    protected void SetTargetRPC(int targetViewId)
    {
        PhotonView targetView = PhotonView.Find(targetViewId);
        Transform target = (targetView != null) ? targetView.transform : null;
        SetTarget(target, false);
    }

    public void SetWeapon(Transform weapon, bool isCustom, bool isSendRPC = true)
    {
        weaponTran = weapon;
        if (obCtrl != null) obCtrl.SetWeapon(weaponTran);

        //カスタム
        if (isCustom && weaponTran != null)
        {
            EffectLevelController effectLevelCtrl = weaponTran.GetComponent<EffectLevelController>();
            if (effectLevelCtrl != null) effectLevelCtrl.EffectCustom(this);
        }

        if (isSendRPC)
        {
            int viewId = -1;
            if (weaponTran != null) viewId = PhotonView.Get(weaponTran.gameObject).viewID;
            object[] args = new object[] { viewId, isCustom };
            photonView.RPC("SetWeaponRPC", PhotonTargets.Others, args);
        }
    }
    [PunRPC]
    public void SetWeaponRPC(int viewId, bool isCustom)
    {
        PhotonView weaponView = PhotonView.Find(viewId);
        Transform weapon = (weaponView != null) ? weaponView.transform : null;
        SetWeapon(weapon, isCustom, false);
    }

    public string GetWeaponName()
    {
        return (weaponTran != null) ? weaponTran.name : myTran.name;
    }


    //##### CUSTOM #####

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

    //HitEffect
    public void CustomDamageEffect(GameObject obj)
    {
        damageEffect = obj;
    }

    //isPhysicsBulletBreak
    public void CustomPhysicsBreak()
    {
        isPhysicsBulletBreak = true;
    }

    //isEnergyBulletBreak
    public void CustomEnergyBreak()
    {
        isEnergyBulletBreak = true;
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

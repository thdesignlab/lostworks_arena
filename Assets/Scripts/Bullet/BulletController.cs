using UnityEngine;
using System.Collections;

public class BulletController : MoveOfCharacter
{
    [SerializeField]
    protected GameObject hitEffect; //Hit時エフェクト
    [SerializeField]
    protected float speed;  //発射速度
    [SerializeField]
    protected int damage; //ダメージ量
    [SerializeField]
    protected int damagePerSecond; //継続ダメージ量
    [SerializeField]
    protected float stuckTime; //スタック時間
    [SerializeField]
    protected float knockBackRate; //ノックバック係数
    [SerializeField]
    protected bool isPhysicsBulletBreak;  //物理弾破壊FLG
    [SerializeField]
    protected bool isEnergyBulletBreak;   //エネルギー弾破壊FLG
    [SerializeField]
    protected bool isHitBreak;   //衝突時消滅FLG

    [SerializeField]
    protected float safetyTime = 0.0f;
    protected bool isHit = false;
    protected float activeTime = 0;
    protected float totalDamage = 0;

    protected Transform targetTran;
    protected PlayerStatus targetStatus;
    protected Transform ownerTran;
    protected PlayerStatus ownerStatus;
    protected Transform weaponTran;
    private Collider _myCollider;
    protected Collider myCollider
    {
        get { return _myCollider ? _myCollider : _myCollider = transform.GetComponentInChildren<Collider>(); }
    }
    private ObjectController _obCtrl;
    protected ObjectController obCtrl
    {
        get { return _obCtrl ? _obCtrl : _obCtrl = GetComponent<ObjectController>(); }
    }
    private ChangeEffectController _effectCtrl;
    protected ChangeEffectController effectCtrl
    {
        get { return _effectCtrl ? _effectCtrl : _effectCtrl = GetComponent<ChangeEffectController>(); }
    }
    private StatusChangeController _statusChangeCtrl;
    protected StatusChangeController statusChangeCtrl
    {
        get { return _statusChangeCtrl ? _statusChangeCtrl : _statusChangeCtrl = GetComponent<StatusChangeController>(); }
        set { _statusChangeCtrl = value; }
    }
    protected BulletLevelController bulletLevelCtrl;

    protected AudioController audioCtrl;

    protected override void Awake()
    {
        base.Awake();

        //判定一時削除
        if (myCollider != null)
        {
            myCollider.enabled = (safetyTime == 0) ? true : false;
        }

        audioCtrl = myTran.GetComponent<AudioController>();
    }

    protected override void Update()
    {
        base.Update();

        //稼働時間
        activeTime += Time.deltaTime;
        if (myCollider != null)
        {
            if (activeTime >= safetyTime && !myCollider.enabled)
            {
                //判定復活
                myCollider.enabled = true;
            }
        }

        //推進力
        Move(Vector3.forward, speed);
    }

    //衝突時処理(Trigger=true)
    void OnTriggerEnter(Collider other)
    {
        if (other.GetType().Name == "CharacterController") return;
        if (!Common.Func.IsColliderHitTag(other.tag)) return;
        GameObject otherObj = other.gameObject;
        OnHit(otherObj);
    }
    
    //接触時処理
    void OnTriggerStay(Collider other)
    {
        if (other.GetType().Name == "CharacterController") return;
        if (!Common.Func.IsColliderHitTag(other.tag)) return;
        GameObject otherObj = other.gameObject;
        OnStay(otherObj);
    }

    //衝突時処理(共通)
    protected virtual void OnHit(GameObject otherObj)
    {
        if (photonView.isMine)
        {
            //HIT有効チェック
            if (IsSafety(otherObj)) return;

            //ダメージを与える
            AddDamage(otherObj);

            //対象を破壊
            TargetDestory(otherObj);

            //反射チェック
            if (IsReflection(otherObj))
            {
                Reflection();
            }

            //破壊チェック
            if (isHit && isHitBreak)
            {
                DestoryObject();
            }
            else
            {
                isHit = false;
            }
        }
    }

    //接触時処理(共通)
    protected virtual void OnStay(GameObject otherObj)
    {
        if (photonView.isMine)
        {
            //HIT有効チェック
            if (IsSafety(otherObj, false)) return;

            //ダメージを与える
            AddSlipDamage(otherObj);
        }
    }

    //ダメージ処理
    protected void AddDamage(GameObject hitObj)
    {
        //ダメージ系処理は所有者のみ行う
        if (photonView.isMine)
        {
            //AttackRate計算
            float dmg = damage;
            if (ownerStatus != null) dmg *= (ownerStatus.attackRate / 100);

            if (hitObj.CompareTag("Player") || hitObj.tag == "Target")
            {
                isHit = true;

                if (dmg > 0 || statusChangeCtrl != null)
                {
                    //プレイヤーステータス
                    PlayerStatus status = GetHitObjStatus(hitObj);

                    if (status.IsReflection())
                    {
                        Reflection();
                        return;
                    }

                    //ダメージ
                    AddDamageProccess(status, dmg);

                    //デバフ
                    AddDebuff(status);

                    //スタック
                    if (stuckTime > 0)
                    {
                        status.AttackInterfareMove(stuckTime);
                    }
                }

                //ノックバック
                if (knockBackRate != 0)
                {
                    TargetKnockBack(hitObj.transform, knockBackRate);
                }
            }
            else if (hitObj.CompareTag(Common.CO.TAG_STRUCTURE))
            {
                isHit = true;

                //ダメージ倍率計算
                if (myTran.tag == Common.CO.TAG_BULLET_EXTRA) dmg *= Common.CO.EXTRA_BULLET_BREAK_RATE;
                StructureController structCtrl = hitObj.GetComponent<StructureController>();
                if (structCtrl != null) structCtrl.AddDamage((int)dmg);
            }
            else if (hitObj.CompareTag(Common.CO.TAG_FLOOR))
            {
                isHit = true;
            }
        }
    }

    //継続ダメージ処理
    protected void AddSlipDamage(GameObject hitObj)
    {
        //ダメージ処理は所有者のみ行う
        if (photonView.isMine)
        {
            //ダメージ計算
            float dmg = damagePerSecond;
            if (ownerStatus != null) dmg *= (ownerStatus.attackRate / 100);
            dmg *= Time.deltaTime;

            if (hitObj.CompareTag("Player") || hitObj.tag == "Target")
            {
                if (dmg > 0 || statusChangeCtrl != null)
                {
                    //プレイヤーステータス
                    PlayerStatus status = GetHitObjStatus(hitObj);

                    //ダメージ
                    AddDamageProccess(status, dmg, true);

                    //デバフ
                    AddDebuff(status);
                }
            }
            else if (hitObj.CompareTag(Common.CO.TAG_STRUCTURE))
            {
                if (myTran.tag == Common.CO.TAG_BULLET_EXTRA) dmg *= Common.CO.EXTRA_BULLET_BREAK_RATE;
                hitObj.GetComponent<StructureController>().AddDamage((int)dmg);
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
        if (hitEffect != null && !isSlip)
        {
            GameObject effectObj = PhotonNetwork.Instantiate(Common.Func.GetResourceEffect(hitEffect.name), myTran.position, hitEffect.transform.rotation, 0);
            EffectController effectCtrl = effectObj.GetComponent<EffectController>();
            if (effectCtrl != null) effectCtrl.EffectSetting(ownerTran, targetTran, weaponTran, false);
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
        if ((isEnergyBulletBreak && Common.Func.IsEnergyBullet(hitObj.tag))
            || (isPhysicsBulletBreak && Common.Func.IsPhysicsBullet(hitObj.tag)))
        {
            isHit = true;
            hitObj.GetComponent<ObjectController>().DestoryObject(true);
            return true;
        }

        return false;
    }

    //HIT判定スルーチェック
    protected bool IsSafety(GameObject hitObj, bool isHitCheck = true)
    {
        //一度衝突しているものは無視
        if (isHit && isHitCheck) return true;

        //ターゲット,障害物は有効
        if (hitObj.transform != targetTran && !hitObj.CompareTag(Common.CO.TAG_STRUCTURE))
        {
            //持ち主に当たった場合無視
            if (hitObj.transform == ownerTran) return true;

            //自分のオブジェクトは無視
            PhotonView pv = PhotonView.Get(hitObj);
            if (pv != null && pv.isMine && Common.Func.IsBullet(hitObj.tag))
            {
                BulletController hitBullet = hitObj.GetComponent<BulletController>();
                if (hitBullet != null && hitBullet.GetOwner() == ownerTran) return true;
            }
        }

        //if (isHitCheck) isHit = true;

        return false;
    }

    //弾の速度取得
    public float GetSpeed()
    {
        return speed;
    }

    //Owner,Target,Weapon情報をセット、同期する
    public void BulletSetting(Transform owner, Transform target, Transform weapon, bool isCustom = true, bool isSendRPC = true)
    {
        SetOwner(owner, false);
        SetTarget(target, false);
        SetWeapon(weapon, isCustom, false);

        if (isSendRPC)
        {
            int ownerViewId = (owner != null) ? PhotonView.Get(owner.gameObject).viewID : -1;
            int targetViewId = (target != null) ? PhotonView.Get(target.gameObject).viewID : -1;
            int weaponViewId = (weapon != null) ? PhotonView.Get(weapon.gameObject).viewID : -1;
            object[] args = new object[] { ownerViewId, targetViewId, weaponViewId, isCustom };
            photonView.RPC("BulletSettingRPC", PhotonTargets.Others, args);
        }
    }
    [PunRPC]
    protected void BulletSettingRPC(int ownerViewId, int targetViewId, int weaponViewId, bool isCustom)
    {
        SetOwnerRPC(ownerViewId);
        SetTargetRPC(targetViewId);
        SetWeaponRPC(weaponViewId, isCustom);
    }

    //ターゲットを設定する
    public void SetTarget(Transform target, bool isSendRPC = true)
    {
        if (targetTran == target) return;
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

    //持ち主設定
    public void SetOwner(Transform owner, bool isSendRPC = true)
    {
        if (ownerTran == owner) return;
        ownerTran = owner;
        if (obCtrl != null) obCtrl.SetOwner(ownerTran);

        if (ownerTran != null)
        {
            ownerStatus = ownerTran.GetComponent<PlayerStatus>();
            //PhotonView pv = PhotonView.Get(ownerTran.gameObject);
            //if (pv != null) ownerId = pv.ownerId;
        }

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


    //武器設定
    public void SetWeapon(Transform weapon, bool isCustom = true, bool isSendRPC = true)
    {
        if (weaponTran == weapon) return;
        weaponTran = weapon;
        if (weaponTran != null)
        {
            WeaponController weaponCtrl = weaponTran.GetComponent<WeaponController>();
            if (weaponCtrl != null) weaponCtrl.PlayAudio();

            if (isCustom)
            {
                //カスタム処理
                bulletLevelCtrl = weaponTran.GetComponent<BulletLevelController>();
                if (bulletLevelCtrl != null)
                {
                    bulletLevelCtrl.BulletCustom(this);
                }
            }
        }

        if (obCtrl != null) obCtrl.SetWeapon(weaponTran, isCustom);

        if (isSendRPC)
        {
            int viewId = -1;
            if (weaponTran != null) viewId = PhotonView.Get(weaponTran.gameObject).viewID;
            object[] args = new object[] { viewId, isCustom };
            photonView.RPC("SetWeaponRPC", PhotonTargets.Others, args);
        }
    }
    [PunRPC]
    protected void SetWeaponRPC(int weaponViewId, bool isCustom)
    {
        PhotonView weaponView = PhotonView.Find(weaponViewId);
        Transform weapon = (weaponView != null) ? weaponView.transform : null;
        SetWeapon(weapon, isCustom, false);
    }

    protected void PlayAudio(int no = 0)
    {
        if (audioCtrl == null) return;
        audioCtrl.Play(no);
    }
    protected void StopAudio(int no = 0)
    {
        if (audioCtrl == null) return;
        audioCtrl.Stop(no);
    }

    public Transform GetTarget()
    {
        return targetTran;
    }
    public Transform GetOwner()
    {
        return ownerTran;
    }

    //反射判定
    protected bool IsReflection(GameObject otherObj)
    {
        if (!otherObj.CompareTag(Common.CO.TAG_STRUCTURE)) return false;
        if (!otherObj.GetComponent<StructureController>().IsReflaction()) return false;

        BulletController bulletCtrl = otherObj.GetComponent<BulletController>();
        if (myTran.tag == Common.CO.TAG_EFFECT || myTran.tag == Common.CO.TAG_BULLET_EXTRA) return false;
        if (bulletCtrl != null && bulletCtrl.GetOwner() == ownerTran) return false;
        return true;
    }

    //反射処理
    protected void Reflection(bool isSendRPC = true)
    {
        //owner,target
        Transform preOwnerTran = ownerTran;
        BulletSetting(null, preOwnerTran, weaponTran, false, false);

        //object reset
        if (obCtrl != null) obCtrl.Reset();

        //方向を変える
        myTran.LookAt(preOwnerTran);
        isHit = false;

        if (isSendRPC)
        {
            photonView.RPC("ReflectionRPC", PhotonTargets.Others);
        }
    }
    [PunRPC]
    protected void ReflectionRPC()
    {
        Reflection(false);
    }

    public string GetWeaponName()
    {
        return (weaponTran != null) ? weaponTran.name : myTran.name;
    }


    //##### CUSTOM #####
    
    //ダメージ
    public void CustomDamage(int value)
    {
        damage += value;
        damagePerSecond += value;
        if (damage < 0) damage = 0;
        if (damagePerSecond < 0) damagePerSecond = 0;
    }

    //旋回速度UP
    public virtual void CustomTurnSpeed(float value)
    {
        return;
    }

    //判定拡大
    public void CustomCollider(float value)
    {
        if (myCollider == null) return;
        switch (myCollider.GetType().Name)
        {
            case "BoxCollider":
                BoxCollider box = (BoxCollider)myCollider;
                box.size *= value;
                break;

            case "SphereCollider":
                SphereCollider sphere = (SphereCollider)myCollider;
                sphere.radius *= value;
                break;

            case "CapsuleCollider":
                CapsuleCollider capsule = (CapsuleCollider)myCollider;
                capsule.radius *= value;
                capsule.height *= value;
                break;
        }
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

    //StuckTime
    public void CustomStuckTime(float value)
    {
        stuckTime += value;
        if (stuckTime < 0) stuckTime = 0;
    }

    //Knockback
    public void CustomKnockBack(float value)
    {
        knockBackRate += 1 + value;
    }

    //HitEffect
    public void CustomHitEffect(GameObject obj)
    {
        hitEffect = obj;
    }

    //BreakEffect
    public void CustomBreakEffect(GameObject obj)
    {
        if (obCtrl != null) obCtrl.CustomSpawnEffect(obj);
    }

    //弾速
    public void CustomSpeed(float value)
    {
        speed += value;
    }

    //弾スケール
    public void CustomScale(float value)
    {
        if (effectCtrl != null)
        {
            effectCtrl.CustomEndScale(value);
        }
        else
        {
            myTran.localScale *= value;
            ParticleSystem particle = myTran.GetComponentInChildren<ParticleSystem>();
            if (particle != null) particle.startSize *= value;
        }
    }

    //ロック開始時間
    public virtual void CustomLockTime(float value)
    {
        return;
    }

    //ロック後スピードrate
    public virtual void CustomLockedSpeedRate(float value)
    {
        return;
    }

    //ロック後ターンRate
    public virtual void CustomLockedTurnRate(float value)
    {
        return;
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

    public void CustomPhysicsBreak(bool flg)
    {
        isPhysicsBulletBreak = flg;
    }

    public void CustomEnergyBreak(bool flg)
    {
        isEnergyBulletBreak = flg;
    }

    public void CustomHitBreak(bool flg)
    {
        isHitBreak = flg;
    }

    public void CustomSafetyTime(float value)
    {
        safetyTime += value;
        if (safetyTime < 0) safetyTime = 0;
    }
}

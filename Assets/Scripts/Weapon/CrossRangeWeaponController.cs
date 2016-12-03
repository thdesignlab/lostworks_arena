using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CrossRangeWeaponController : WeaponController
{
    [SerializeField]
    protected GameObject blade;
    [SerializeField]
    protected float attackTime; //bladeをONにしている時間
    [SerializeField]
    protected float attackWaitTime; //モーション開始後bladeをONにするまでの時間
    [SerializeField]
    protected float boostSpeed;
    [SerializeField]
    protected float boostTime;
    [SerializeField]
    private float boostWaitTime; //モーション開始後boost開始までの時間
    [SerializeField]
    protected int boostCost;
    [SerializeField]
    protected bool isStopInAttack;

    private EffectController _effectCtrl;
    protected EffectController effectCtrl
    {
        get { return _effectCtrl ? _effectCtrl : _effectCtrl = blade.GetComponent<EffectController>(); }
        set { _effectCtrl = value; }
    }
    private Animator weaponAnimator;
    private string animationName = "";

    private int secondAttackRate = 0;

    private const string MOTION_RIGHT_SLASH = "SlashR";
    private const string MOTION_LEFT_SLASH = "SlashL";
    private const string MOTION_CENTER_SLASH = "SlashC";
    private const string MOTION_SECOND_ATTACK = "SecondAttack";


    protected override void Awake()
    {
        base.Awake();
        weaponAnimator = myTran.GetComponent<Animator>();
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(SetOwnerRoutine());
    }

    IEnumerator SetOwnerRoutine()
    {
        for (;;)
        {
            if (playerTran == null)
            {
                yield return null;
                continue;
            }
            if (effectCtrl != null) effectCtrl.EffectSetting(playerTran, targetTran, myTran);
            break;
        }
    }

    protected override void Action()
    {
        base.Action();
        StartCoroutine(BladeOn());
    }

    IEnumerator BladeOn(bool isCheckSecondAttack = true)
    {
        bool isBladeOn = false;
        bool isBladeOff = false;
        bool isBoostOn = false;
        bool isBoostOff = false;
        float attackProcTime = 0;

        //斬戟モーション
        if (weaponAnimator != null) weaponAnimator.SetBool(animationName, true);

        for (;;)
        {
            attackProcTime += Time.deltaTime;

            if (playerStatus != null && boostSpeed >= 0 && boostTime >= 0)
            {
                //ブーストチェック
                if (!isBoostOn)
                {
                    if (attackProcTime >= boostWaitTime)
                    {
                        //ブースト開始
                        playerStatus.ForceBoost(new Vector3(0, 0, 1), boostSpeed, boostTime, boostCost);
                        isBoostOn = true;
                    }
                }
                if (isBoostOn && !isBoostOff)
                {
                    //ブースト終了チェック
                    if (attackProcTime >= boostTime + boostWaitTime) isBoostOff = true;
                }
            }
            else
            {
                isBoostOff = true;
            }

            if (!isBladeOn)
            {
                //準備期間
                if (attackProcTime >= attackWaitTime)
                {
                    //ブレードON
                    SetBlade(true);
                    PlayAudio();
                    isBladeOn = true;

                    if (isStopInAttack)
                    {
                        //停止
                        playerStatus.InterfareMove(attackTime, null, false);
                    }
                }
                yield return null;
                continue;
            }

            if (isBladeOn && !isBladeOff)
            {
                if (attackProcTime >= attackTime + attackWaitTime)
                {
                    //ブレードOFF
                    SetBlade(false);
                    isBladeOff = true;
                }
            }

            if (isBladeOff && isBoostOff) break;

            yield return null;
        }

        
        if (isCheckSecondAttack && Random.Range(0, 100) <= secondAttackRate)
        {
            Coroutine secondAtk = StartCoroutine(SecondAttack());
            yield return secondAtk;
        }
        if (weaponAnimator != null) weaponAnimator.SetBool(animationName, false);

        base.EndAction();
    }
    IEnumerator SecondAttack()
    {
        //MOTION_SECOND_ATTACK
        Coroutine bladeOn = StartCoroutine(BladeOn(false));
        yield return bladeOn;
    }

    private void SetBlade(bool flg, bool isSendRPC = true)
    {
        blade.SetActive(flg);
        if (isSendRPC)
        { 
            photonView.RPC("SetBladeRPC", PhotonTargets.Others, flg);
        }
    }
    [PunRPC]
    private void SetBladeRPC(bool flg)
    {
        SetBlade(flg, false);
    }

    public override bool IsEnableFire()
    {
        if (!base.IsEnableFire()) return false;
        if (blade == null) return false;
        return true;
    }

    public override void SetMotionCtrl(Animator a, string s)
    {
        base.SetMotionCtrl(a, s);

        //近接モーション名設定
        switch (s)
        {
            case Common.CO.MOTION_LEFT_ATTACK:
                animationName = MOTION_LEFT_SLASH;
                break;

            case Common.CO.MOTION_RIGHT_ATTACK:
                animationName = MOTION_RIGHT_SLASH;
                break;

            default:
                animationName = MOTION_CENTER_SLASH;
                break;
        }
    }

    public override void SetTarget(Transform target = null)
    {
        base.SetTarget(target);
        if (effectCtrl != null) effectCtrl.SetTarget(target);
    }


    //##### CUSTOM #####

    //blade変更
    public void CustomChangeBlade(GameObject obj)
    {
        GameObject newBlade = PhotonNetwork.Instantiate(Common.Func.GetResourceEffect(obj.name), Vector3.zero, Quaternion.identity, 0);
        newBlade.transform.SetParent(myTran, false);
        int bladeViewId = PhotonView.Get(newBlade).viewID;
        photonView.RPC("CustomChangeBladeRPC", PhotonTargets.Others, bladeViewId);
        blade = newBlade;
        effectCtrl = blade.GetComponent<EffectController>();
    }
    [PunRPC]
    private void CustomChangeBladeRPC(int bladeViewId)
    {
        PhotonView bladeView = PhotonView.Find(bladeViewId);
        if (bladeView == null) return;
        bladeView.transform.SetParent(myTran, false);
        blade = bladeView.gameObject;
    }

    //攻撃時間
    public void CustomAttackTime(float value)
    {
        attackTime += value;
    }

    //攻撃開始待機時間
    public void CustomAttackWaitTime(float value)
    {
        attackWaitTime += value;
    }

    //ブースト速度
    public void CustomBoostSpeed(float value)
    {
        boostSpeed += value;
    }

    //ブースト時間
    public void CustomBoostTime(float value)
    {
        boostTime += value;
    }

    //ブースト開始待機時間
    public void CustomBoostWaitTime(float value)
    {
        boostWaitTime += value;
    }

    //ブースト消費SP
    public void CustomBoostCost(int value)
    {
        boostCost += value;
    }

    //追撃発生率
    public void CustomSecondAttack(int value)
    {
        secondAttackRate += value;
    }

    //ダメージ
    public void CustomDamage(int value)
    {
        if (effectCtrl != null) effectCtrl.CustomDamage(value);
    }

    public void CustomPhysicsBreak()
    {
        if (effectCtrl != null) effectCtrl.CustomPhysicsBreak();
    }

    public void CustomEnergyBreak()
    {
        if (effectCtrl != null) effectCtrl.CustomEnergyBreak();
    }

    public void CustomEndScale(float value)
    {
        if (effectCtrl != null) effectCtrl.CustomEndScale(value);
    }

    public void CustomChangeHitEffect(GameObject obj)
    {
        if (effectCtrl != null) effectCtrl.CustomDamageEffect(obj);
    }
}
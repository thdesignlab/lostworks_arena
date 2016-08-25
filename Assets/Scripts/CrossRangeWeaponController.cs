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

    private Animator weaponAnimator;
    private string animationName = "";

    private const string MOTION_RIGHT_SLASH = "SlashR";
    private const string MOTION_LEFT_SLASH = "SlashL";

    protected override void Awake()
    {
        base.Awake();
        //bitPos = myBitTran.localPosition;
        weaponAnimator = myTran.GetComponent<Animator>();
        if (blade != null) blade.SetActive(false);
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(SetOwner());
    }

    IEnumerator SetOwner()
    {
        for (;;)
        {
            if (playerTran == null)
            {
                yield return null;
                continue;
            }
            blade.GetComponent<EffectController>().SetOwner(playerTran);
            break;
        }
    }

    protected override void Action()
    {
        base.Action();
        StartCoroutine(BladeOn());
    }

    IEnumerator BladeOn()
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
                    blade.SetActive(true);
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
                    blade.SetActive(false);
                    isBladeOff = true;
                }
            }

            if (isBladeOff && isBoostOff) break;

            yield return null;
        }

        if (weaponAnimator != null) weaponAnimator.SetBool(animationName, false);

        base.EndAction();
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
                animationName = MOTION_RIGHT_SLASH;
                break;
        }
    }
}
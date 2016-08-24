using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class CrossRangeWeaponController : WeaponController
{
    [SerializeField]
    protected GameObject blade;
    [SerializeField]
    protected float attackTime;
    [SerializeField]
    protected float readyTime;
    [SerializeField]
    protected float boostSpeed = -1;
    [SerializeField]
    protected float boostTime = -1;
    [SerializeField]
    protected int boostCost;
    [SerializeField]
    protected bool isStopInAttack;
    [SerializeField]
    private float chargeTime;

    private Animator weaponAnimator;
    private string animationName = "";
    private PlayerController playerCtrl;

    private const string MOTION_RIGHT_SLASH = "SlashR";
    private const string MOTION_LEFT_SLASH = "SlashL";

    protected override void Awake()
    {
        base.Awake();
        //bitPos = myBitTran.localPosition;
        weaponAnimator = myTran.GetComponent<Animator>();
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
            playerCtrl = playerTran.GetComponent<PlayerController>();
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
        float readyProcTime = 0;
        float attackProcTime = 0;

        if (chargeTime > 0) yield return new WaitForSeconds(chargeTime);

        //斬戟モーション
        if (weaponAnimator != null) weaponAnimator.SetBool(animationName, true);

        if (playerCtrl != null && boostSpeed >= 0 && boostTime >= 0)
        {
            //前方ダッシュ
            playerCtrl.WeaponBoost(0, 1, boostSpeed, boostTime, boostCost);
        }

        for (;;)
        {
            if (!isBladeOn)
            {
                //準備期間
                readyProcTime += Time.deltaTime;
                if (readyProcTime >= readyTime)
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

            attackProcTime += Time.deltaTime;

            if (!isBladeOff && attackProcTime >= attackTime)
            {
                //ブレードOFF
                blade.SetActive(false);
                isBladeOff = true;
            }

            if (isBladeOff && attackProcTime >= attackTime + readyTime) break;

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
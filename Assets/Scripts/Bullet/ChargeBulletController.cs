using UnityEngine;
using System.Collections;

public class ChargeBulletController : TrackingBulletController
{
    [SerializeField]
    private GameObject chargeEffect;
    [SerializeField]
    private float maxChargeTime;
    [SerializeField]
    private float maxSpeedRate;
    [SerializeField]
    private float maxDamageRate;
    [SerializeField]
    private float maxSizeRate;
    [SerializeField]
    private float limitTime;

    private float baseSpeed;
    private float baseDamage;
    private Vector3 baseScale;

    private Vector3 chargingVector;
    private float chargeRate = 0;
    private bool isCharge = true;
    private float firedTime = 0;


    protected override void Awake()
    {
        base.Awake();

        if (photonView.isMine)
        {
            baseSpeed = base.speed;
            baseDamage = base.damage;
            baseScale = base.myTran.localScale;

            base.speed = 0;
            if (chargeEffect != null) chargeEffect.SetActive(true);
        }
    }

    protected override void Update()
    {
        if (photonView.isMine)
        {
            base.Update();

            if (!isCharge)
            {
                //発射
                firedTime += Time.deltaTime;

                if (firedTime >= limitTime)
                {
                    base.DestoryObject();
                }
            }
            else
            {
                //チャージ中
                base.safetyTime += Time.deltaTime;
            }
        }
    }

    public void Charging(float chargeTiem)
    {
        chargeRate = chargeTiem / maxChargeTime;
        if (maxChargeTime < base.activeTime)
        {
            chargeRate = 1;
            if (chargeEffect != null) chargeEffect.SetActive(false);
        }

        damage = (int)Mathf.Lerp(baseDamage, baseDamage * maxDamageRate, chargeRate);
        if (myTran == null) return;
        myTran.localScale = Vector3.Lerp(baseScale, baseScale * maxSizeRate, chargeRate);
    }

    public void Fire(float chargeTime)
    {
        Charging(chargeTime);
        isCharge = false;
        speed = (int)Mathf.Lerp(baseSpeed, baseSpeed * maxSpeedRate, chargeRate);
        defaultSpeed = speed;

        if (chargeEffect != null) chargeEffect.SetActive(false);
    }

    public float GetMaxChargeTime()
    {
        return maxChargeTime;
    }


    //##### CUSTOM #####

    public void CustomChargeTime(float value)
    {
        maxChargeTime += value;
    }
    public void CustomSpeedRate(float value)
    {
        maxSpeedRate += value;
    }
    public void CustomDamageRate(float value)
    {
        maxDamageRate += value;
    }
    public void CustomSizeRate(float value)
    {
        maxSizeRate += value;
    }
    public void CustomLimitTime(float value)
    {
        limitTime += value;
    }
}

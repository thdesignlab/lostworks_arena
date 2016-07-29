using UnityEngine;
using System.Collections;

public class ChargeBulletController : EnergyTrackingBulletController
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
                firedTime += Time.deltaTime;
                if (firedTime >= 0.1f)
                {
                    base.myCollider.enabled = true;
                }
                if (firedTime >= limitTime)
                {
                    base.DestoryObject();
                }
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

        base.damage = (int)Mathf.Lerp(baseDamage, baseDamage * maxDamageRate, chargeRate);
        base.myTran.localScale = Vector3.Lerp(baseScale, baseScale * maxSizeRate, chargeRate);
    }
    public void Fire(float chargeTime)
    {
        Charging(chargeTime);
        isCharge = false;
        base.speed = (int)Mathf.Lerp(baseSpeed, baseSpeed * maxSpeedRate, chargeRate);
        if (chargeEffect != null) chargeEffect.SetActive(false);
    }

    public float GetMaxChargeTime()
    {
        return maxChargeTime;
    }
}

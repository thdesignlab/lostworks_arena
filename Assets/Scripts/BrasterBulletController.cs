using UnityEngine;
using System.Collections;

public class BrasterBulletController : EnergyTrackingBulletController
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

    private CapsuleCollider myCollider;
    private Transform myPlayerTran;
    private PlayerStatus myPlayerStatus;

    private float firedTime = 0;

    protected override void Awake()
    {
        base.Awake();
        if (photonView.isMine)
        {
            myCollider = myTran.GetComponent<CapsuleCollider>();
            myCollider.enabled = false;
            myPlayerTran = GameObject.Find("GameController").GetComponent<GameController>().GetMyTran();
            myPlayerStatus = myPlayerTran.gameObject.GetComponent<PlayerStatus>();

            chargingVector = myPlayerTran.InverseTransformVector(myTran.position - myPlayerTran.position);

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
            if (Input.GetMouseButton(0) && isCharge)
            {
                //チャージ中
                myTran.position = myPlayerTran.position + myPlayerTran.TransformVector(chargingVector);
                myTran.rotation = myPlayerTran.rotation;

                chargeRate = base.activeTime / maxChargeTime;
                if (maxChargeTime < base.activeTime)
                {
                    chargeRate = 1;
                    if (chargeEffect != null) chargeEffect.SetActive(false);
                }

                base.damage = (int)Mathf.Lerp(baseDamage, baseDamage * maxDamageRate, chargeRate);
                base.myTran.localScale = Vector3.Lerp(baseScale, baseScale * maxSizeRate, chargeRate);

                return;
            }
            if (isCharge)
            {
                //発射
                isCharge = false;
                base.speed = (int)Mathf.Lerp(baseSpeed, baseSpeed * maxSpeedRate, chargeRate);
                if (chargeEffect != null) chargeEffect.SetActive(false);
            }

            firedTime += Time.deltaTime;
            if (firedTime >= 0.1f)
            {
                myCollider.enabled = true;
            }
            if (firedTime >= limitTime)
            {
                base.DestoryObject();
            }
        }
    }
}

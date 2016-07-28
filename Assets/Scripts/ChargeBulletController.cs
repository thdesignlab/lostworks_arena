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

    private CapsuleCollider myCollider;
    private Transform myPlayerTran;
    private PlayerStatus myPlayerStatus;
    private AudioController audioCtrl;
    private GameController GameCtrl;

    private float firedTime = 0;

    private float npcChargeTime = 0;

    protected override void Awake()
    {
        base.Awake();
        if (photonView.isMine)
        {
            myCollider = myTran.GetComponent<CapsuleCollider>();
            myCollider.enabled = false;
            GameCtrl = GameObject.Find("GameController").GetComponent<GameController>();
            myPlayerTran = GameCtrl.GetMyTran();

            baseSpeed = base.speed;
            baseDamage = base.damage;
            baseScale = base.myTran.localScale;

            base.speed = 0;
            if (chargeEffect != null) chargeEffect.SetActive(true);
        }

        audioCtrl = myTran.GetComponent<AudioController>();
    }

    protected override void Start()
    {
        base.Start();
        if (audioCtrl != null) audioCtrl.Play(0);
    }

    protected override void Update()
    {
        if (photonView.isMine)
        {
            base.Update();
            if ((isCharge && myPlayerTran != null) 
                && ((npcChargeTime <= 0 && Input.GetMouseButton(0)) || (npcChargeTime > 0 && base.activeTime < npcChargeTime)))
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
                if (audioCtrl != null)
                {
                    audioCtrl.Stop(0);
                    audioCtrl.Play(1);
                }
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

    //ターゲットを設定する
    public override void SetTarget(Transform target)
    {
        base.SetTarget(target);

        if (myPlayerTran == target)
        {
            //NPC
            myPlayerTran = GameCtrl.GetNpcTran();
            npcChargeTime = Random.Range(maxChargeTime * 0.5f, maxChargeTime);
        }
        chargingVector = myPlayerTran.InverseTransformVector(myTran.position - myPlayerTran.position);
    }
}

using UnityEngine;
using System.Collections;

public class BoostRecoverSpController : WeaponController
{
    [SerializeField]
    private float effectTime;
    [SerializeField]
    private float effectRate;
    [SerializeField]
    private GameObject effect;

    protected override void Awake()
    {
        base.myTran = transform;
    }

    protected override void Action()
    {
        playerStatus.AccelerateRecoverSp(effectRate, effectTime, effect);
        base.StartReload(effectTime);
    }
}

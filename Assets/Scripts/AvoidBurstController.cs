using UnityEngine;
using System.Collections;

public class AvoidBurstController : WeaponController
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
        if (base.playerStatus == null) return;
        base.playerStatus.AvoidBurst(effectRate, effectTime, effect);
        base.StartReload(effectTime);
    }
}

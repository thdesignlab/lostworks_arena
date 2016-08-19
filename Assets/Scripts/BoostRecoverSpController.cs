﻿using UnityEngine;
using System.Collections;

public class BoostRecoverSpController : WeaponController
{
    [SerializeField]
    private float effectTime;
    [SerializeField]
    private float effectRate;
    [SerializeField]
    private GameObject effect;


    protected override void Action()
    {
        if (base.playerStatus == null) return;
        base.playerStatus.AccelerateRecoverSp(effectRate, effectTime, effect);
        base.StartReload(effectTime);
    }
}

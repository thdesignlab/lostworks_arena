using UnityEngine;
using System.Collections;

public class SbWeaponController : WeaponController
{
    StatusChangeController statusChangeCtrl;

    protected override void Awake()
    {
        base.Awake();

        statusChangeCtrl = GetComponent<StatusChangeController>();
    }

    protected override void Action()
    {
        if (playerStatus == null || statusChangeCtrl == null) return;

        statusChangeCtrl.Action(playerStatus);
        base.StartReload(statusChangeCtrl.GetEffectTime());
    }
}

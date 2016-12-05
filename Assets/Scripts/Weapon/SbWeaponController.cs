using UnityEngine;
using System.Collections;

public class SbWeaponController : WeaponController
{
    protected override void Action()
    {
        if (playerStatus == null || statusChangeCtrl == null) return;

        statusChangeCtrl.Action(playerStatus);
        StartReload(statusChangeCtrl.GetEffectTime());
    }
}

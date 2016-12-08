using UnityEngine;
using System.Collections;

public class InvincibleShieldController : WeaponController
{
    [SerializeField]
    private float effectTime;
    [SerializeField]
    private bool isReflection = false;

    protected override void Action()
    {
        if (effectTime <= 0) return;
        if (playerStatus == null) return;

        playerStatus.SetInvincible(true, effectTime, true, isReflection);

        base.StartReload(effectTime);
    }
}

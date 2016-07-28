using UnityEngine;
using System.Collections;

public class InvincibleShieldController : WeaponController
{
    [SerializeField]
    private float effectTime;

    protected override void Action()
    {
        if (effectTime <= 0) return;

        playerStatus.SetInvincible(true, effectTime, true);

        base.StartReload(effectTime);
    }
}

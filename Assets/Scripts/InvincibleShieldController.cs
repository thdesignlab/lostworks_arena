using UnityEngine;
using System.Collections;

public class InvincibleShieldController : WeaponController
{
    [SerializeField]
    private float effectTime;

    protected override void Action()
    {
        if (effectTime <= 0) return;
        if (base.playerStatus == null) return;

        base.playerStatus.SetInvincible(true, effectTime, true);

        base.StartReload(effectTime);
    }
}

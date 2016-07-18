using UnityEngine;
using System.Collections;

public class InvincibleShieldController : WeaponController
{
    [SerializeField]
    private float effectTime;

    private PlayerStatus playerStatus;

    protected override void Start()
    {
        base.Start();
        StartCoroutine(SetPlayerStatus());
    }

    IEnumerator SetPlayerStatus()
    {
        base.isEnabledFire = false;
        for (;;)
        {
            playerStatus = base.myTran.root.gameObject.GetComponent<PlayerStatus>();
            if (playerStatus != null) break;
            yield return null;
        }
        base.isEnabledFire = true;
    }

    protected override void Action()
    {
        if (effectTime <= 0) return;

        playerStatus.SetInvincible(true, effectTime, true);

        base.StartReload(effectTime);
    }
}

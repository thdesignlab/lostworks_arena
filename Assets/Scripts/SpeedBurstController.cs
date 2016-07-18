using UnityEngine;
using System.Collections;

public class SpeedBurstController : WeaponController
{
    [SerializeField]
    private float effectTime;
    [SerializeField]
    private float effectRate;
    [SerializeField]
    private GameObject effect;

    private PlayerStatus playerStatus;

    protected override void Awake()
    {
        base.myTran = transform;
    }

    protected override void Start()
    {
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
        playerStatus.AccelerateRunSpeed(effectRate, effectTime, effect);
        base.StartReload(effectTime);
    }
}

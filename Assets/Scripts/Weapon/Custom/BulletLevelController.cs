using UnityEngine;
using System.Collections;

public class BulletLevelController : WeaponLevelController
{
    [SerializeField, TooltipAttribute("0:damage,1:speed")]
    protected int powerType = -1;
    [SerializeField]
    protected float powerEffectValueDiff = 20;
    [SerializeField, TooltipAttribute("0:reload,1:turnSpeed")]
    protected int technicType = -1;
    [SerializeField]
    protected float technicEffectValueDiff = 20;
    [SerializeField, TooltipAttribute("0:damage+reload,1:shootCount,")]
    protected int uniqueType = -1;
    [SerializeField]
    protected float uniqueEffectValueDiff = 10;

    private GameObject bullet;

    protected override void SetEffectValue()
    {
        powerEffectValue = powerEffectValueDiff * customLevel;
        technicEffectValue = technicEffectValueDiff * customLevel;
        uniqueEffectValue = uniqueEffectValueDiff * customLevel;
    }

    public void SetBulletLevel(GameObject target)
    {
        bullet = target;
    }

    protected override void CustomPower()
    {
        switch (customType)
        {
            default:
                base.CustomPower();
                break;
        }
    }
    protected override void CustomSpeed()
    {
        switch (customType)
        {
            default:
                base.CustomSpeed();
                break;
        }
    }
    protected override void CustomUnique()
    {
        switch (customType)
        {
            default:
                base.CustomUnique();
                break;
        }
    }
}
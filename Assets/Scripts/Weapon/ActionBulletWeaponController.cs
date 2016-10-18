using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionBulletWeaponController : BulletWeaponController
{
    [SerializeField]
    protected float boostSpeed;
    [SerializeField]
    protected float boostTime;
    [SerializeField]
    protected int boostCost;
    [SerializeField]
    protected bool isStopInAttack;
    [SerializeField]
    protected float attackStartTime;


    //protected void ActionProccess()
    //{
    //    base.Action();

    //    if (rapidCount <= 1 && spreadCount <= 1)
    //    {
    //        SpawnBullet(muzzles[0].position, muzzles[0].rotation, 0);
    //        EndAction();
    //        return;
    //    }

    //    if (rapidCount <= 1)
    //    {
    //        SpreadFire();
    //        EndAction();
    //    }
    //    else
    //    {
    //        StartCoroutine(RapidFire());
    //    }
    //}
}

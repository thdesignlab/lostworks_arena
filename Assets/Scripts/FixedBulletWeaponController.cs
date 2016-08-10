using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FixedBulletWeaponController : BulletWeaponController
{
    [SerializeField]
    private float fixedTime;
    [SerializeField]
    private float fixedDiffTime;

    protected override void EndAction()
    {
        //発射カウントダウン
        StartCoroutine(CountDownShoot());

        base.EndAction();
    }

    IEnumerator CountDownShoot()
    {
        yield return new WaitForSeconds(fixedTime);

        foreach (GameObject bullet in shootBullets)
        {
            FixedTrackingBulletController ctrl = bullet.GetComponent<FixedTrackingBulletController>();
            if (ctrl != null)
            {
                ctrl.Shoot();
                if (fixedDiffTime > 0) yield return new WaitForSeconds(fixedDiffTime);
            }
        }
    }
}

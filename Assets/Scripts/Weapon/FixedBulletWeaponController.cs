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
            if (bullet == null) yield break;
            FixedTrackingBulletController ctrl = bullet.GetComponent<FixedTrackingBulletController>();
            if (ctrl != null)
            {
                ctrl.Shoot();
                if (fixedDiffTime > 0) yield return new WaitForSeconds(fixedDiffTime);
            }
        }
        yield break;
    }


    //##### CUSTOM #####

    public void CustomFixedTime(float value)
    {
        fixedTime += value;
        if (fixedTime < 0) fixedTime = 0;
    }

    public void CustomFixedDiffTime(float value)
    {
        fixedDiffTime += value;
        if (fixedDiffTime < 0) fixedDiffTime = 0;
    }
}

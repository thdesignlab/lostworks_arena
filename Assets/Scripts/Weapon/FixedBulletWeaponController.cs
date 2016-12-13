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

        float procTime = 0;
        bool isPlayAudio = true;
        foreach (GameObject bullet in shootBullets)
        {
            if (bullet == null) yield break;
            FixedTrackingBulletController ctrl = bullet.GetComponent<FixedTrackingBulletController>();
            if (ctrl != null)
            {
                isPlayAudio = (procTime == 0 || procTime - Time.time >= 0.1f);
                ctrl.Shoot(isPlayAudio);
                procTime = Time.time;
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

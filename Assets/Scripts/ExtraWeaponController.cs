using UnityEngine;
using System.Collections;

public class ExtraWeaponController : Photon.MonoBehaviour
{
    [SerializeField]
    private GameObject extraCam;
    [SerializeField]
    private float fireTimeInAnim = 1;

    private GameObject mainCam;
    private WeaponController wepCtrl;
    private Animator charaAnimator;

    public void SetInit(WeaponController wep, Animator anim)
    {
        mainCam = Camera.main.gameObject;
        wepCtrl = wep;
        charaAnimator = anim;
    }

    public void Fire(Transform targetTran)
    {
        if (!isEnabled()) return;

        StartCoroutine(FireProccess(targetTran));
    }

    IEnumerator FireProccess(Transform targetTran)
    {
        mainCam.SetActive(false);
        extraCam.SetActive(true);

        charaAnimator.SetBool(Common.CO.MOTION_EXTRA_ATTACK, true);

        bool isReady = false;
        bool isFire = false;
        for (;;)
        {
            float animTime = GetActionTime();
            if (animTime <= 1) isReady = true;
            if (isReady)
            {
                if (!isFire && animTime >= fireTimeInAnim)
                {
                    isFire = true;
                    wepCtrl.Fire(targetTran);
                }
                if (isFire && animTime >= 1) break;
            }
            yield return null;
        }
        charaAnimator.SetBool(Common.CO.MOTION_EXTRA_ATTACK, false);
        extraCam.SetActive(false);
        mainCam.SetActive(true);
    }

    private float GetActionTime()
    {
        if (charaAnimator == null) return 1;

        int waitHash = Animator.StringToHash(Common.CO.MOTION_EXTRA_ATTACK);
        int nowAnimHash = charaAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        return charaAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    private bool isEnabled()
    {
        if (extraCam == null || wepCtrl == null || charaAnimator == null) return false;
        if (!wepCtrl.IsEnableFire()) return false;

        return true;
    }
}
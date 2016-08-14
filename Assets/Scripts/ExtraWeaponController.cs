using UnityEngine;
using System.Collections;

public class ExtraWeaponController : Photon.MonoBehaviour
{
    [SerializeField]
    private GameObject extraCam;
    [SerializeField]
    private float fireTimeInAnim = 1;

    private Camera mainCam;
    private WeaponController wepCtrl;
    private Animator charaAnimator;
    private PlayerStatus playerStatus;

    public void SetInit(WeaponController wep, Animator anim, PlayerStatus status)
    {
        mainCam = Camera.main.gameObject.GetComponent<Camera>();
        wepCtrl = wep;
        charaAnimator = anim;
        playerStatus = status;
    }

    public void Fire(Transform targetTran)
    {
        if (!isEnabled()) return;

        StartCoroutine(FireProccess(targetTran));
    }

    IEnumerator FireProccess(Transform targetTran)
    {
        playerStatus.SetForceInvincible(true);
        //mainCam.enabled = false;
        extraCam.SetActive(true);

        charaAnimator.SetBool(Common.CO.MOTION_EXTRA_ATTACK, true);

        bool isReady = false;
        bool isFire = false;
        for (;;)
        {
            float animTime = GetActionTime();
            if (animTime < 1) isReady = true;
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
        //mainCam.enabled = true;
        playerStatus.SetForceInvincible(false);
    }

    private float GetActionTime()
    {
        //int targetHash = Animator.StringToHash("Base Layer."+Common.CO.MOTION_EXTRA_ATTACK);
        //AnimatorStateInfo stateInfo = charaAnimator.GetCurrentAnimatorStateInfo(0);
        ////Debug.Log(targetHash + " >> "+ nowAnimHash);
        //if (stateInfo.IsName("Base Layer."+Common.CO.MOTION_EXTRA_ATTACK)) return -1;
        return charaAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    private bool isEnabled()
    {
        if (extraCam == null || wepCtrl == null || charaAnimator == null) return false;
        if (!wepCtrl.IsEnableFire()) return false;

        return true;
    }
}
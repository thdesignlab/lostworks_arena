using UnityEngine;
using System.Collections;

public class ExtraWeaponController : Photon.MonoBehaviour
{
    [SerializeField]
    private GameObject extraCam;
    [SerializeField]
    private GameObject extraEffect;
    [SerializeField]
    private float fireTimeInAnim = 1;

    private WeaponController wepCtrl;
    private Animator charaAnimator;
    private PlayerStatus playerStatus;
    private Transform myParentTran;

    private const string TAG_ANIMATION_WAIT = "Wait";
    private const string TAG_ANIMATION_EXTRA = "Extra";

    public void SetInit(WeaponController wep, Animator anim, PlayerStatus status)
    {
        wepCtrl = wep;
        charaAnimator = anim;
        playerStatus = status;
        myParentTran = playerStatus.transform;
    }

    public void Fire(Transform targetTran)
    {
        if (!isEnabled()) return;

        StartCoroutine(FireProccess(targetTran));
    }

    IEnumerator FireProccess(Transform targetTran)
    {
        //無敵開始
        playerStatus.SetForceInvincible(true);
        //カメラ切り替え
        extraCam.SetActive(true);
        //追加エフェクト
        SwitchExtraEffect(true);
        //攻撃モーション開始
        charaAnimator.SetBool(Common.CO.MOTION_EXTRA_ATTACK, true);

        bool isReady = false;
        bool isFire = false;
        for (;;)
        {
            float animTime = GetActionTime();
            if (!isReady && 0 < animTime && animTime < 1)
            {
                isReady = true;
            }
            if (isReady)
            {
                //攻撃タイミングチェック
                if (!isFire && animTime >= fireTimeInAnim)
                {
                    isFire = true;
                    wepCtrl.Fire(targetTran);
                }

                //カメラアニメーション終了チェック
                if (isFire && animTime >= 1) break;
            }
            if (!isFire && targetTran != null)
            {
                //攻撃開始までターゲットへ向ける
                Vector3 targetPos = new Vector3(targetTran.position.x, myParentTran.position.y, targetTran.position.z);
                myParentTran.LookAt(targetPos);
            }
            yield return null;
        }

        //カメラ戻し
        extraCam.SetActive(false);

        for (;;)
        {
            //攻撃モーション終了チェック
            if (!wepCtrl.IsAction()) break;
            yield return null;
        }
        //攻撃モーション終了
        charaAnimator.SetBool(Common.CO.MOTION_EXTRA_ATTACK, false);
        SwitchExtraEffect(false);

        //無敵解除
        playerStatus.SetForceInvincible(false);
    }

    private void SwitchExtraEffect(bool flg)
    {
        if (extraEffect == null) return;
        extraEffect.SetActive(flg);
        photonView.RPC("SwitchExtraEffectRPC", PhotonTargets.Others, flg);
    }

    [PunRPC]
    private void SwitchExtraEffectRPC(bool flg)
    {
        extraEffect.SetActive(flg);
    }

    private float GetActionTime()
    {
        //int targetHash = Animator.StringToHash("Base Layer."+Common.CO.MOTION_EXTRA_ATTACK);
        AnimatorStateInfo stateInfo = charaAnimator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsTag(TAG_ANIMATION_EXTRA))
        {
            return -1;
        }
        return charaAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    private bool isEnabled()
    {
        if (extraCam == null || wepCtrl == null || charaAnimator == null) return false;
        if (!wepCtrl.IsEnableFire()) return false;
        AnimatorStateInfo stateInfo = charaAnimator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsTag(TAG_ANIMATION_WAIT)) return false;
        return true;
    }
}
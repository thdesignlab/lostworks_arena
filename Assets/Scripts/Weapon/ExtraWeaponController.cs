using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ExtraWeaponController : Photon.MonoBehaviour
{
    [SerializeField]
    private GameObject extraCam;
    [SerializeField]
    private GameObject extraEffect;
    [SerializeField]
    private GameObject extraAddWeapon;
    [SerializeField]
    private float fireTimeInAnim = 1;   //攻撃開始するタイミング(アニメーション経過時間0-1内で指定)
    [SerializeField]
    private int useHpPerCondition = 25;   //Ex武器の場合に指定

    private WeaponController wepCtrl;
    private Animator charaAnimator;
    private PlayerStatus playerStatus;
    private Transform myParentTran;
    private GameObject extraBtn;
    private GameObject menuCanvas;
    private WeaponController addWeaponCtrl;

    private const string TAG_ANIMATION_WAIT = "Wait";
    private const string TAG_ANIMATION_RUN = "Run";
    private const string TAG_ANIMATION_EXTRA = "Extra";

    private int useCount = 0;   //使用回数
    private const int FREE_HP_CONDITION = 10;   //使用回数無制限になるHP割合

    private bool isShooting = false;
    private bool isActiveScene = true;

    void Awake()
    {
        if (SceneManager.GetActiveScene().name == Common.CO.SCENE_CUSTOM)
        {
            isActiveScene = false;
        }
        if (extraCam != null) extraCam.SetActive(false);
        if (extraEffect != null) extraEffect.SetActive(false);
        if (extraAddWeapon != null) addWeaponCtrl = extraAddWeapon.GetComponent<WeaponController>();
    }

    void Start()
    {
        StartCoroutine(CheckBtn());
    }

    public void SetInit(WeaponController wep, Animator anim, PlayerStatus status)
    {
        wepCtrl = wep;
        charaAnimator = anim;
        playerStatus = status;
        myParentTran = playerStatus.transform;
        menuCanvas = GameObject.Find("MenuCanvas");
    }

    public void Fire(Transform targetTran = null)
    {
        if (!IsEnabled()) return;

        useCount++;
        StartCoroutine(FireProccess(targetTran));
    }

    IEnumerator FireProccess(Transform targetTran)
    {
        isShooting = true;

        //無敵開始
        playerStatus.SetForceInvincible(true);

        //攻撃モーション
        float animTime = -1;
        if (charaAnimator != null)
        {
            for (;;)
            {
                //Wait状態になるまで待つ
                if (IsWaitAnimation()) break;
                yield return null;
            }
            charaAnimator.SetBool(Common.CO.MOTION_EXTRA_ATTACK, true);
        }

        //カメラ切り替え
        SwitchExtraCamera(true);

        //攻撃モーション開始
        SwitchExtraEffect(true);

        bool isReady = false;
        bool isFire = false;
        for (;;)
        {
            animTime = GetActionTime();
            if (!isReady && (charaAnimator == null || 0 < animTime && animTime < 1))
            {
                isReady = true;
            }
            if (isReady)
            {
                //攻撃タイミングチェック
                if (!isFire && (charaAnimator == null || animTime >= fireTimeInAnim))
                {
                    isFire = true;
                    wepCtrl.Fire(targetTran);
                    AddWeaponFire();
                }

                //カメラアニメーション終了チェック
                if (isFire && (charaAnimator == null || animTime >= 1)) break;
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
        SwitchExtraCamera(false);

        for (;;)
        {
            //攻撃モーション終了チェック
            if (!wepCtrl.IsAction()) break;
            yield return null;
        }
        
        //攻撃モーション終了
        SwitchExtraEffect(false);
        if (charaAnimator != null) charaAnimator.SetBool(Common.CO.MOTION_EXTRA_ATTACK, false);

        //無敵解除
        playerStatus.SetForceInvincible(false);

        isShooting = false;
    }

    private void AddWeaponFire()
    {
        if (addWeaponCtrl == null) return;
        addWeaponCtrl.SetEnable(true);
        addWeaponCtrl.Fire();
    }

    private void SwitchExtraCamera(bool flg)
    {
        if (extraCam != null && !playerStatus.IsNpc())
        {
            extraCam.SetActive(flg);
            if (menuCanvas != null) menuCanvas.SetActive(!flg);
        }
    }

    private void SwitchExtraEffect(bool flg)
    {
        if (extraEffect == null && charaAnimator == null) return;
        photonView.RPC("SwitchExtraEffectRPC", PhotonTargets.All, flg);
    }
    [PunRPC]
    private void SwitchExtraEffectRPC(bool flg)
    {
        //追加エフェクト
        if (extraEffect != null) extraEffect.SetActive(flg);
    }

    private float GetActionTime()
    {
        if (charaAnimator == null) return -1;
        //int targetHash = Animator.StringToHash("Base Layer."+Common.CO.MOTION_EXTRA_ATTACK);
        AnimatorStateInfo stateInfo = charaAnimator.GetCurrentAnimatorStateInfo(0);
        if (!stateInfo.IsTag(TAG_ANIMATION_EXTRA))
        {
            return -1;
        }
        return charaAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    public bool IsEnabled()
    {
        if (wepCtrl == null) return false;
        
        if (!wepCtrl.IsEnableFire()) return false;
        
        if (charaAnimator != null && !playerStatus.IsNpc())
        {
            AnimatorStateInfo stateInfo = charaAnimator.GetCurrentAnimatorStateInfo(0);
            if (!stateInfo.IsTag(TAG_ANIMATION_WAIT) && !stateInfo.IsTag(TAG_ANIMATION_RUN)) return false;
        }

        if (!isActiveScene || !GameController.Instance.isGameStart) return true;
        if (!IsLeftUsableCount()) return false;
        
        return true;
    }

    public bool IsWaitAnimation()
    {
        AnimatorStateInfo stateInfo = charaAnimator.GetCurrentAnimatorStateInfo(0);
        return stateInfo.IsTag(TAG_ANIMATION_WAIT);
    }

    private bool IsLeftUsableCount()
    {
        //現在のHP割合
        int hpPer = playerStatus.GetNowHpPer();

        //使用無制限チェック
        if (hpPer <= FREE_HP_CONDITION) return true;

        //使用回数チェック
        if (useHpPerCondition <= 0) return true;
        int usableCount = (int)Mathf.Floor((100 - hpPer) / useHpPerCondition);
        
        if (usableCount <= useCount) return false;

        return true;
    }

    IEnumerator CheckBtn()
    {
        for (;;)
        {
            if (wepCtrl != null)
            {
                //ボタン表示切替
                bool isEnabled = false;
                if (isActiveScene && GameController.Instance.isGameReady)
                {
                    //ゲーム開始準備期間に使用回数をリセット
                    useCount = 0;
                    isEnabled = false;
                }
                else
                {
                    if (isActiveScene && !GameController.Instance.isGameStart)
                    {
                        //ゲーム開始前
                        isEnabled = true;
                    }
                    else
                    {
                        //使用回数チェック
                        isEnabled = IsLeftUsableCount();
                    }
                }
                wepCtrl.SwitchBtn(isEnabled);
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    public WeaponController GetWeaponController()
    {
        return wepCtrl;
    }

    public bool IsShooting()
    {
        return isShooting;
    }

    public void ExtraFree()
    {
        useHpPerCondition = 0;
    }
}
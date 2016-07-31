using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class WeaponController : Photon.MonoBehaviour
{
    [SerializeField]
    protected int bitMotionType;   //Bitのモーション(1:Gun, 2:Missile, 3:Laser)
    protected Animator bitAnimator;
    protected string bitMotionParam;

    [SerializeField]
    protected float reloadTime;   //再装填時間
    protected float leftReloadTime = 0;

    protected Transform targetTran;
    protected AudioController audioCtrl;
    protected Animator charaAnimator;
    protected string motionParam = "";

    protected bool isEnabledFire = true;

    protected Transform myTran;

    protected PlayerStatus playerStatus;
    protected bool isNpc = false;

    protected Button myBtn;
    protected Image imgGage;
    protected Color normalGageColor;

    // Use this for initialization
    protected virtual void Awake()
    {
        myTran = transform;
        audioCtrl = myTran.GetComponent<AudioController>();
        bitAnimator = myTran.GetComponentInChildren<Animator>();
    }

    protected virtual void Start()
    {
        if (photonView.isMine)
        {
            StartCoroutine(CheckPlayerStatus());
        }
    }

    IEnumerator CheckPlayerStatus()
    {
        isEnabledFire = false;
        for (;;)
        {
            playerStatus = myTran.root.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                isNpc = playerStatus.IsNpc();
                break;
            }
            yield return null;
        }
        isEnabledFire = true;
    }

    public virtual void SetTarget(Transform target = null)
    {
        targetTran = target;
    }

    public virtual void Fire(Transform target = null)
    {
        if (!IsEnableFire()) return;

        SetEnable(false);

        if (target != null)
        {
            SetTarget(target);
        }

        Action();
    }

    protected virtual void Action()
    {
        //モーション開始
        StartMotion();
    }
    protected virtual void EndAction()
    {
        //モーション終了
        StopMotion();

        //リロード
        StartReload();
    }

    protected virtual void StartReload(float addReloadTime = 0)
    {
        if (leftReloadTime > 0)
        {
            leftReloadTime += reloadTime + addReloadTime;
            return;
        }
        StartCoroutine(Reload(addReloadTime));
    }

    IEnumerator Reload(float addReloadTime = 0)
    {
        leftReloadTime = reloadTime + addReloadTime;
        for (;;)
        {
            yield return null;
            leftReloadTime -= Time.deltaTime;
            if (imgGage != null) imgGage.fillAmount = (reloadTime - leftReloadTime) / reloadTime;
            if (leftReloadTime <= 0) break;
        }
        if (imgGage != null)
        {
            imgGage.fillAmount = 1;
            imgGage.color = normalGageColor;
        }
        SetEnable(true);
    }

    public virtual bool IsEnableFire()
    {
        return isEnabledFire;
    }

    public void SetMotionCtrl(Animator a, string s)
    {
        charaAnimator = a;
        motionParam = s;

        bitMotionParam = Common.Func.GetBitMotionName(bitMotionType, s);
    }

    public void SetBtn(Button btn)
    {
        myBtn = btn;
        Transform imgGageTran = myBtn.transform.FindChild("ImgGage");
        if (imgGageTran != null)
        {
            imgGage = imgGageTran.GetComponent<Image>();
            imgGage.fillAmount = 1;
            normalGageColor = imgGage.color;
        }
    }

    public void SetEnable(bool flg, bool reloadFlg = false)
    {
        isEnabledFire = flg;
        if (myBtn != null)
        {
            //ボタン使用切り替え
            myBtn.interactable = flg;
        }

        if (flg)
        {
            //使用可能
            leftReloadTime = 0;
            if (imgGage != null)
            {
                //クールゲージ切り替え
                imgGage.fillAmount = 1;
                imgGage.color = normalGageColor;
            }
        }
        else
        {
            //使用不可
            if (imgGage != null)
            {
                //クールゲージ切り替え
                imgGage.fillAmount = 0;
                imgGage.color = Common.CO.reloadGageColor;
            }
            if (reloadFlg)
            {
                //リロード
                StartReload();
            }
        }
    }

    public virtual float GetBulletSpeed()
    {
        return 0;
    }

    protected void StartMotion()
    {
        //Debug.Log("StartMotion >>");
        if (charaAnimator != null && motionParam != "")
        {
            charaAnimator.SetBool(motionParam, true);
        }
        if (bitMotionParam != null && bitMotionParam != "")
        {
            bitAnimator.SetBool(bitMotionParam, true);
        }
    }

    protected void StopMotion()
    {
        //Debug.Log(">> StopMotion");
        if (charaAnimator != null && motionParam != "")
        {
            StartCoroutine(WaitAnimatorEnd(charaAnimator, motionParam));
        }
        if (bitMotionParam != null && bitMotionParam != "")
        {
            StartCoroutine(WaitAnimatorEnd(bitAnimator, bitMotionParam));
        }
    }

    IEnumerator WaitAnimatorEnd(Animator anim, string param)
    {
        yield return new WaitForSeconds(0.5f);
        anim.SetBool(param, false);
    }

    protected void PlayAudio(int no = 0)
    {
        if (audioCtrl == null) return;
        audioCtrl.Play(no);
    }
    protected void StopAudio(int no = 0)
    {
        if (audioCtrl == null) return;
        audioCtrl.Stop(no);
    }
}
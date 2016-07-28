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

    protected Button myBtn;

    // Use this for initialization
    protected virtual void Awake()
    {
        myTran = transform;
        audioCtrl = myTran.GetComponent<AudioController>();
        bitAnimator = myTran.GetComponentInChildren<Animator>();
    }

    protected virtual void Start()
    {
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
            if (leftReloadTime <= 0) break;
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
    }

    public void SetEnable(bool flg, bool reloadFlg = false)
    {
        isEnabledFire = flg;
        if (myBtn != null)
        {
            myBtn.interactable = flg;
        }
        if (flg)
        {
            leftReloadTime = 0;
        }
        else
        {
            if (reloadFlg)
            {
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
            //if (charaAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            //{
            //    charaAnimator.SetBool(motionParam, false);
            //}
            //else
            //{
            //    StartCoroutine(WaitAnimatorEnd(charaAnimator, motionParam));
            //}
        }
        if (bitMotionParam != null && bitMotionParam != "")
        {
            StartCoroutine(WaitAnimatorEnd(bitAnimator, bitMotionParam));
            //if (bitAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
            //{

            //    bitAnimator.SetBool(bitMotionParam, false);
            //}
            //else
            //{
            //    StartCoroutine(WaitAnimatorEnd(bitAnimator, bitMotionParam));
            //}
        }
    }

    IEnumerator WaitAnimatorEnd(Animator anim, string param)
    {
        yield return new WaitForSeconds(0.5f);
        anim.SetBool(param, false);
        //for (;;)
        //{
        //    if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1)
        //    {
        //        anim.SetBool(param, false);
        //        break;
        //    }
        //}
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
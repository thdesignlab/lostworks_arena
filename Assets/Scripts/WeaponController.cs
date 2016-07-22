using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class WeaponController : Photon.MonoBehaviour
{
    [SerializeField]
    protected float reloadTime;   //再装填時間
    protected float leftReloadTime = 0;

    protected Transform targetTran;
    protected AudioSource audio;
    protected Animator animator;
    protected string motionParam = "";

    protected bool isEnabledFire = true;

    protected Transform myTran;

    protected Button myBtn;

    // Use this for initialization
    protected virtual void Awake()
    {
        myTran = transform;
        audio = GetComponent<AudioSource>();
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

        //モーション開始
        if (animator != null)
        {
            animator.SetBool(motionParam, true);
        }

        Action();
    }

    protected virtual void Action()
    {
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
        //モーション終了
        if (animator != null)
        {
            animator.SetBool(motionParam, false);
        }

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
        animator = a;
        motionParam = s;
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

    protected void PlayAudio()
    {
        if (audio != null)
        {
            audio.Play();
        }
    }
}
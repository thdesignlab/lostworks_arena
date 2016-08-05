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
    protected float bitMoveTime;   //発射までにかかる時間
    protected bool isBitMoved;
    protected Vector3 bitFromPos = default(Vector3);
    protected Vector3 bitToPos = default(Vector3);
    protected float radius;

    [SerializeField]
    protected float reloadTime;   //再装填時間
    protected float leftReloadTime = 0;

    protected Transform targetTran;
    protected AudioController audioCtrl;
    protected Animator charaAnimator;
    protected string motionParam = "";

    protected bool isEnabledFire = true;

    protected Transform myTran;
    protected Transform myBitTran;

    protected PlayerStatus playerStatus;
    protected bool isNpc = false;

    protected Button myBtn;
    protected Image imgGage;
    protected string normalGageHexColor = "0695EA4E";
    protected Color normalGageColor;
    protected SpriteStudioController spriteStudioCtrl;
    protected Script_SpriteStudio_Root scriptRoot;


    protected virtual void Awake()
    {
        myTran = transform;
        audioCtrl = myTran.GetComponent<AudioController>();
        bitAnimator = myTran.GetComponentInChildren<Animator>();

        //Bit移動用
        myBitTran = myTran.FindChild("Bit");
        if (myBitTran)
        {
            bitFromPos = myBitTran.localPosition;
        }
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
        //Bit位置を戻す
        ReturnBitMove();

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
        if (spriteStudioCtrl)
        {
            spriteStudioCtrl.Play(scriptRoot);
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
        if (btn == null) return;

        myBtn = btn;
        Transform imgGageTran = myBtn.transform.FindChild("ImgGage");
        if (imgGageTran != null)
        {
            imgGage = imgGageTran.GetComponent<Image>();
            imgGage.fillAmount = 1;
            
            Color color = default(Color);
            if (ColorUtility.TryParseHtmlString(normalGageHexColor, out color))
            {
                normalGageColor = color;
            }
            else
            {
                normalGageColor = imgGage.color;
            }
        }
        spriteStudioCtrl = GameObject.Find("SpriteStudioController").GetComponent<SpriteStudioController>();
        if (spriteStudioCtrl != null)
        {
            scriptRoot = spriteStudioCtrl.CreateButtonFlash(myBtn.gameObject);
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
        yield return new WaitForSeconds(0.2f);
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

    protected bool StartBitMove()
    {
        if (bitFromPos == default(Vector3) || bitToPos == default(Vector3)) return true;

        if (photonView.isMine)
        {
            isBitMoved = false;
            if (bitMoveTime > 0)
            {
                StartCoroutine(BitMoveProccess(bitFromPos, bitToPos, 0));
            }
            else
            {
                isBitMoved = true;
            }
        }
        return isBitMoved;
    }

    protected void ReturnBitMove()
    {
        if (bitFromPos == default(Vector3) || bitToPos == default(Vector3)) return;

        if (photonView.isMine)
        {
            if (bitMoveTime > 0)
            {
                StartCoroutine(BitMoveProccess(bitToPos, bitFromPos, 180));
            }
            else
            {
                isBitMoved = false;
            }
        }
    }

    IEnumerator BitMoveProccess(Vector3 fromPos, Vector3 toPos, float startAngle)
    {
        float time = 0;
        for (;;)
        {
            time += Time.deltaTime;
            float lerpRate = time / bitMoveTime;
            if (lerpRate > 1) lerpRate = 1;
            
            Vector3 leftSide = Vector3.left * Common.Func.GetSin(lerpRate, 180, startAngle) * radius / 2;
            myBitTran.localPosition = Vector3.Lerp(fromPos, toPos, lerpRate) + leftSide;

            if (lerpRate >= 1) break;
            yield return null;
        }

        if (isBitMoved)
        {
            isBitMoved = false;
        }
        else
        {
            isBitMoved = true;
        }
    }
}
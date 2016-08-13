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
    [SerializeField]
    protected bool isBitFixed = false;  //Bit固定FLG
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

    protected Transform playerTran;
    protected PlayerStatus playerStatus;
    protected bool isNpc = false;

    protected Button myBtn;
    protected Image imgGage;
    protected SpriteStudioController spriteStudioCtrl;
    protected Script_SpriteStudio_Root scriptRoot;

    //リロードゲージカラー
    private Color RELOAD_GAGE_COLOR = Color.red;
    private Color NORMAL_GAGE_COLOR = new Color(0.000f, 1.000f, 0.834f, 0.634f);
    private Color EXTRA_GAGE_COLOR = new Color(1.000f, 1.000f, 0.000f, 0.634f);
    //public static string normalGageHexColor = "0695EA4E";
    private Color reloadColor;
    private Color normalColor;

    protected virtual void Awake()
    {
        myTran = transform;
        audioCtrl = myTran.GetComponent<AudioController>();
        bitAnimator = myTran.GetComponentInChildren<Animator>();

        //Bit移動用
        foreach (Transform child in myTran)
        {
            if (child.tag == Common.CO.TAG_WEAPON_BIT)
            {
                myBitTran = child;
                bitFromPos = myBitTran.localPosition;
            }
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
            Transform child = myTran;
            for (;;)
            {
                Transform parent = child.parent;
                if (parent == null) break;
                if (parent.tag == "Player")
                {
                    playerTran = parent;
                    playerStatus = parent.GetComponent<PlayerStatus>();
                    break;
                }
                child = parent;
            }

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
            imgGage.color = NORMAL_GAGE_COLOR;
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

    public void SetBtn(Button btn, bool isExtra = false)
    {
        if (btn == null) return;

        reloadColor = RELOAD_GAGE_COLOR;
        normalColor = NORMAL_GAGE_COLOR;
        if (isExtra) normalColor = EXTRA_GAGE_COLOR;

        myBtn = btn;
        Transform imgGageTran = myBtn.transform.FindChild("ImgGage");
        if (imgGageTran != null)
        {
            //Debug.Log("first:" + imgGage.color);
            imgGage = imgGageTran.GetComponent<Image>();
            imgGage.fillAmount = 1;
            imgGage.color = normalColor;
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
                imgGage.color = normalColor;
            }
        }
        else
        {
            //使用不可
            if (imgGage != null)
            {
                //クールゲージ切り替え
                imgGage.fillAmount = 0;
                imgGage.color = reloadColor;
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
        if (bitAnimator != null && bitMotionParam != "")
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
        if (bitAnimator != null && bitMotionParam != "")
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
        //Debug.Log(bitFromPos + " >> " + bitToPos + " : " + bitMoveTime);
        if (bitFromPos == bitToPos) return true;

        if (photonView.isMine)
        {
            isBitMoved = false;
            if (bitMoveTime > 0)
            {
                StartCoroutine(BitMoveProccess(bitFromPos, bitToPos, 0, true));
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
        if (bitFromPos == bitToPos) return;

        if (photonView.isMine)
        {
            if (bitMoveTime > 0)
            {
                if (isBitFixed)
                {
                    //Bit固定解除
                    isBitMoved = false;
                }
                else
                {
                    StartCoroutine(BitMoveProccess(bitToPos, bitFromPos, 180, false));
                }
            }
            else
            {
                isBitMoved = false;
            }
        }
    }

    IEnumerator BitMoveProccess(Vector3 fromPos, Vector3 toPos, float startAngle, bool afterFlg)
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

        isBitMoved = afterFlg;
        if (!afterFlg)
        {
            //帰り
            myBitTran.rotation = myBitTran.root.rotation;
        }
        else
        {
            //行き
            if (isBitFixed)
            {
                //Bit固定
                StartCoroutine(BitFixed());
            }

        }
    }

    IEnumerator BitFixed()
    {
        Vector3 fixedPos = myBitTran.position;
        for (;;)
        {
            if (isBitMoved)
            {
                //固定
                myBitTran.position = fixedPos;
            }
            else
            {
                //固定解除

                Vector3 returnPos = myBitTran.localPosition;
                StartCoroutine(BitMoveProccess(returnPos, bitFromPos, 180, false));
                break;
            }
            yield return null;
        }
    }

    public int GetBitMotion()
    {
        return bitMotionType;
    }

    public virtual string GetDescriptionText()
    {
        return "";
    }

    public float GetActionTime()
    {
        float time = 1;
        if (motionParam == "" || charaAnimator == null) return time;

        int waitHash = Animator.StringToHash(motionParam);
        int nowAnimHash = charaAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        if (waitHash == nowAnimHash)
        {
            time = charaAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
        }
        return time;
    }
}
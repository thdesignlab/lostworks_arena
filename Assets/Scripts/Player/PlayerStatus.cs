﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class PlayerStatus : Photon.MonoBehaviour {

    public float runSpeed;     //移動スピード
    public float jumpSpeed;    //跳躍スピード
    public float jumpLimit;    //跳躍時間
    public float glideJump;    //滑空時ジャンプ係数
    public float boostSpeed;    //ブーストスピード
    public float boostLimit;    //ブースト時間
    public float glideBoost;    //滑空時ブースト係数
    public float turnSpeed;         //通常時旋回スピード
    public float boostTurnSpeed;    //ターゲット時旋回スピード
    public float attackRate = 100;    //攻撃力
    //public float defenceRate = 100;    //防御力

    //HP
    [SerializeField]
    private int maxHp = 1000;   //最大HP
    private int nowHp;      //現在HP
    private float diffValue = 0.05f;
    private bool isDead = false;

    //SP
    [SerializeField]
    private int maxSp = 100;   //最大SP
    private int nowSp;   //現在SP
    [SerializeField]
    private int recoverSp = 10; //SP回復量
    [SerializeField]
    private float bonusRecoverSpRate = 2; //移動していない場合の追加SP回復量
    [SerializeField]
    private float bonusRecoverSpTime = 3; //ボーナスを受け取るのに必要な時間

    public int boostCost = 25;   //ブースト時消費SP

    //private Transform statusCanvas;
    //private Text hpText;

    //無敵判定
    private float leftInvincibleTime = 0;
    public float invincibleTime = 0;
    [SerializeField]
    private GameObject shield;
    private float leftShieldTime = 0;
    private float shieldTime = 0.5f;
    private Material shieldMat1;
    private Color shieldStartColor1;
    private Color shieldLastColor1;
    private Material shieldMat2;
    private Color shieldStartColor2;
    private Color shieldLastColor2;

    //HP/SPゲージ
    private Slider hpBarMine;
    private Slider spBarMine;
    private Slider hpBarEnemy;
    private Slider spBarEnemy;

    private Image hpBarMineImage;
    private Image hpBarEnemyImage;
    //private Color defaultHpColor;
    //private Color hitHpColor = Color.red;
    private int totalDamage = 0;
    [SerializeField]
    private Image hitEffect;
    private float leftHitEffectTime = 0;
    private const float HIT_EFFECT_TIME = 0.5f;
    private Color hitNoiseStart = new Color(1, 1, 1, 1);
    private Color hitNoiseEnd = new Color(1, 1, 1, 0);

    //private CameraController camCtrl;
    private bool isLocked = false;
    private bool isNpc = false;

    //パラメータ変更用
    private float defaultRunSpeed;
    private float defaultJumpSpeed;
    private float defaultBoostSpeed;
    private float defaultTurnSpeed;
    private float defaultBoostTurnSpeed;
    //private float defaultInvincibleTime;
    //private int defaultRecoverSp;
    //private float defaultAttackRate;

    private BaseMoveController moveCtrl;
    private bool isActiveSceane = true;

    //強制無敵状態
    private bool isForceInvincible = false;

    //スタックエフェクト
    [SerializeField]
    private GameObject stuckEffect;

    //勝数マーク
    private const string TAG_WIN_MARK_MINE = "WinMarkMine";
    private const string TAG_WIN_MARK_ENEMY = "WinMarkEnemy";
    private List<GameObject> winCountMineList = new List<GameObject>();
    private List<GameObject> winCountEnemyList = new List<GameObject>();

    //バトルログ
    private bool isDispBattleLog = false;
    const int BATTLE_LOG_COUNT = 10;
    public const int BATTLE_LOG_ATTACK = 0;
    public const int BATTLE_LOG_DAMAGE = 1;
    private int[] battleLogNo = new int[] { 0, 0 };
    private Text[] battleLogArea = new Text[] { null, null };
    private Queue[] logBattleQueue = new Queue[] { null, null };
    private string[] preSlipDmgName = new string[] { "", "" };
    private int[] slipTotalDmg = new int[] { 0, 0 };

    [HideInInspector]
    public VoiceManager voiceManager;

    //ユーザー情報
    [HideInInspector]
    public int userId = -1;
    [HideInInspector]
    public string userName = "";

    void Awake()
    {
        if (shield != null)
        {
            shieldMat1 = shield.transform.FindChild("Sphere001").GetComponent<Renderer>().material;
            shieldStartColor1 = shieldMat1.GetColor("_TintColor");
            shieldLastColor1 = new Color(shieldStartColor1.r, shieldStartColor1.g, shieldStartColor1.b, 0);
            shieldMat2 = shield.transform.FindChild("Sphere002").GetComponent<Renderer>().material;
            shieldStartColor2 = shieldMat2.GetColor("_TintColor");
            shieldLastColor2 = new Color(shieldStartColor2.r, shieldStartColor2.g, shieldStartColor2.b, 0);
        }

        //初期値保管
        defaultRunSpeed = runSpeed;
        defaultJumpSpeed = jumpSpeed;
        defaultBoostSpeed = boostSpeed;
        defaultTurnSpeed = turnSpeed;
        defaultBoostTurnSpeed = boostTurnSpeed;
        //defaultInvincibleTime = invincibleTime;
        //defaultRecoverSp = recoverSp;
        //defaultAttackRate = attackRate;

        if (SceneManager.GetActiveScene().name == Common.CO.SCENE_CUSTOM)
        {
            //カスタム画面
            isActiveSceane = false;
            return;
        }

        isNpc = GetComponent<PlayerSetting>().isNpc;
        moveCtrl = GetComponent<BaseMoveController>();
        
        //ステータス構造
        Transform screenStatusTran = Camera.main.transform.FindChild(Common.CO.SCREEN_CANVAS + Common.CO.SCREEN_STATUS);

        //HPバー
        Transform hpBarMineTran = screenStatusTran.FindChild("HpLine/Mine/HpBar");
        hpBarMine = hpBarMineTran.GetComponent<Slider>();
        hpBarMine.value = 0;
        Transform hpBarEnemyTran = screenStatusTran.FindChild("HpLine/Enemy/HpBar");
        hpBarEnemy = hpBarEnemyTran.GetComponent<Slider>();
        hpBarEnemy.value = 0;

        //HPバーイメージ
        hpBarMineImage = hpBarMineTran.FindChild("Fill Area/Fill").GetComponent<Image>();
        hpBarEnemyImage = hpBarEnemyTran.FindChild("Fill Area/Fill").GetComponent<Image>();
        //defaultHpColor = hpBarMineImage.color;

        //SPバー
        spBarMine = screenStatusTran.FindChild("SpLine/SpBar").GetComponent<Slider>();
        spBarMine.value = 0;
        //spBarEnemy = screenStatusTran.FindChild("SpBarEnemy/SP").GetComponent<Slider>();

        //キャラ付きキャンバス
        //statusCanvas = transform.FindChild("StatusCanvas");
        //hpText = statusCanvas.FindChild("HP").GetComponent<Text>();

        //キャラカメラ
        //camCtrl = Camera.main.gameObject.GetComponent<CameraController>();

        //勝マーク
        foreach (Transform winMark in screenStatusTran.FindChild("HpLine/Mine/WinMark"))
        {
            if (winMark.name.IndexOf("_on") != -1) winCountMineList.Add(winMark.gameObject);
        }
        foreach (Transform winMark in screenStatusTran.FindChild("HpLine/Enemy/WinMark"))
        {
            if (winMark.name.IndexOf("_on") != -1) winCountEnemyList.Add(winMark.gameObject);
        }

        //バトルログエリア
        Transform battleLogTran = screenStatusTran.FindChild("BattleLog");
        if (battleLogTran != null)
        {
            battleLogArea[BATTLE_LOG_ATTACK] = battleLogTran.FindChild("Attack").GetComponent<Text>();
            battleLogArea[BATTLE_LOG_DAMAGE] = battleLogTran.FindChild("Damage").GetComponent<Text>();
        }

        Init();
    }

    void Start()
    {
        //ボイス管理
        Transform charaTran = transform.Find(Common.Func.GetBodyStructure());
        if (charaTran) voiceManager = charaTran.GetComponent<VoiceManager>();

        if (!isActiveSceane) return;

        //ユーザーIDセット
        if (!isNpc)
        {
            userId = int.Parse(UserManager.userInfo[Common.PP.INFO_USER_ID]);
            userName = UserManager.userInfo[Common.PP.INFO_USER_NAME];
            object[] args = new object[] { userId, userName };
            photonView.RPC("SetUserInfoRPC", PhotonTargets.Others, args);
        }

        StartCoroutine(DamageSync());
        StartCoroutine(RecoverSp());
    }

    [PunRPC]
    private void SetUserInfoRPC(int id, string name)
    {
        userId = id;
        userName = name;
    }

    public void Init()
    {
        SetHp(maxHp);
        SetSp(maxSp);

        nowSpeedRate = 1;
        interfareMoveTime = 0;
        runSpeed = defaultRunSpeed;
        jumpSpeed = defaultJumpSpeed;
        boostSpeed = defaultBoostSpeed;
        turnSpeed = defaultTurnSpeed;
        boostTurnSpeed = defaultBoostTurnSpeed;
        //invincibleTime = defaultInvincibleTime;
        //recoverSp = defaultRecoverSp;

        if (photonView.isMine)
        {
            if (isNpc)
            {
                StartCoroutine(SetHpSlider(hpBarEnemy, hpBarEnemyImage));
            }
            else
            {
                StartCoroutine(SetHpSlider(hpBarMine, hpBarMineImage));
            }
        }
        else
        {
            StartCoroutine(SetHpSlider(hpBarEnemy, hpBarEnemyImage));
        }

        logBattleQueue[BATTLE_LOG_ATTACK] = new Queue();
        logBattleQueue[BATTLE_LOG_DAMAGE] = new Queue();
        preSlipDmgName[BATTLE_LOG_ATTACK] = "";
        preSlipDmgName[BATTLE_LOG_DAMAGE] = "";
        slipTotalDmg[BATTLE_LOG_ATTACK] = 0;
        slipTotalDmg[BATTLE_LOG_DAMAGE] = 0;
    }

    //一定間隔ごとにダメージを同期する
    IEnumerator DamageSync()
    {
        for (;;)
        {
            yield return new WaitForSeconds(0.2f);
            if (totalDamage == 0) continue;
            SetHp(nowHp - totalDamage);
            totalDamage = 0;
        }
    }

    private void SetHp(int hp)
    {
        //Debug.Log(transform.name+" : "+hp.ToString());
        photonView.RPC("SetHpRPC", PhotonTargets.All, hp);
    }
    [PunRPC]
    private void SetHpRPC(int hp)
    {
        nowHp = hp;
        //SetHpCanvas();
        if (hp <= 0)
        {
            isDead = true;
            if (photonView.isMine && !isNpc)
            {
                hpBarMine.value = 0;
            }
            else
            {
                hpBarEnemy.value = 0;
            }
        }
        else
        {
            isDead = false;
        }
    }
    private void SetSp(int sp)
    {
        nowSp = sp;
    }

    public bool AddDamage(int damage, string name = "Unknown", bool isSlipDamage = false)
    {
        if (!isActiveSceane) return false;
        if (!GameController.Instance.isGameStart || GameController.Instance.isGameEnd) return false;

        if (isForceInvincible) return false;

        if (leftInvincibleTime > 0)
        {
            if (shield != null)
            {
                photonView.RPC("OpenShieldRPC", PhotonTargets.All, shieldTime);
            }
            return false;
        }

        //ダメージ
        totalDamage += damage;
        if (nowHp - totalDamage <= 0)
        {
            SetHp(0);
        }

        //被ダメボイス
        if (voiceManager != null) voiceManager.Damage();

        //被ダメージログ
        SetBattleLog(BATTLE_LOG_DAMAGE, damage, name, isSlipDamage);
        
        ////カメラ振動
        //if (photonView.isMine && camCtrl != null)
        //{
        //    //camCtrl.Shake();
        //}

        return true;
    }

    [PunRPC]
    private void OpenShieldRPC(float time)
    {
        if (leftShieldTime > 0)
        {
            if (leftShieldTime < time)
            {
                leftShieldTime = time;
            }
        }
        else
        {
            StartCoroutine(OpenShield(time));
        }
    }
    IEnumerator OpenShield(float time)
    {
        //Debug.Log("OpenShield: "+ time.ToString());
        leftShieldTime = time;
        shield.SetActive(true);
        for (;;)
        {
            leftShieldTime -= Time.deltaTime;
            float alpha = leftShieldTime / shieldTime;
            if (alpha > 1) alpha = 1;
            if (alpha < 0) alpha = 0;
            shieldMat1.SetColor("_TintColor", Color.Lerp(shieldLastColor1, shieldStartColor1, alpha));
            shieldMat2.SetColor("_TintColor", Color.Lerp(shieldLastColor2, shieldStartColor2, alpha));
            if (leftShieldTime <= 0) break;
            yield return null;
        }
        shield.SetActive(false);
        //Debug.Log("OpenShield: false");
    }

    //private void SetHpCanvas()
    //{
    //    hpText.text = nowHp.ToString();
    //}

    IEnumerator SetHpSlider(Slider slider, Image image)
    {
        float interval = 0.1f;
        for (;;)
        {
            float targetPer = (float)nowHp / (float)maxHp;
            if (Mathf.Abs(slider.value - targetPer) <= 0.005f)
            {
                yield return new WaitForSeconds(interval);
                continue;
            }

            float diff = diffValue + Time.deltaTime;
            if (slider.value < targetPer)
            {
                if (targetPer < slider.value + diff)
                {
                    diff = targetPer - slider.value;
                }
            }
            else
            {
                diff *= -1;
                if (targetPer > slider.value + diff)
                {
                    diff = targetPer - slider.value;
                }
            }
            slider.value += diff;
            //if (diff < 0) SwitchDamageEffect(image, hitHpColor);
            if (diff < 0) SwitchDamageEffect();
            yield return null;
            //SwitchDamageEffect(image, defaultHpColor);
        }
    }

    private void SwitchDamageEffect(Image img, Color col)
    {
        img.color = col;
    }
    private void SwitchDamageEffect()
    {
        //if (hitEffect == null) return;
        //hitEffect.color = Vector4.one;
        leftHitEffectTime = HIT_EFFECT_TIME;
    }

    public void UseSp(int sp)
    {
        nowSp -= sp;
        if (nowSp < 0) nowSp = 0;
    }
    public bool CheckSp(int sp)
    {
        //Debug.Log(nowSp);
        if (nowSp < sp) return false;
        return true;
    }
    private void SetSpSlider()
    {
        if (!photonView.isMine || isNpc) return;
        spBarMine.value = (float)nowSp / (float)maxSp;
    }

    IEnumerator RecoverSp()
    {
        if (!photonView.isMine) yield break;
        if (spBarMine == null) yield break;

        spBarMine.value = nowSp;

        Vector3 prePos = transform.position;
        float bonusTime = 0;
        float fraction = 0;
        float interval = 0.05f;
        for (;;)
        {
            if (nowSp == maxSp)
            {
                yield return new WaitForSeconds(interval);
                continue;
            }
            bonusTime += interval;

            //回復量計算
            float recover = recoverSp * interval + fraction;
            if (prePos == transform.position)
            {
                if (bonusTime >= bonusRecoverSpTime)
                {
                    recover = recover * bonusRecoverSpRate;
                }
            }
            else
            {
                bonusTime = 0;
            }
            fraction = recover % 1;

            //回復
            nowSp += (int)Mathf.Floor(recover);

            if (nowSp  >= maxSp)
            {
                nowSp = maxSp;
                fraction = 0;
            }

            SetSpSlider();
            prePos = transform.position;
            yield return new WaitForSeconds(interval);
        }
    }

    void Update()
    {
        if (!isActiveSceane) return;
        //statusCanvas.rotation = Camera.main.transform.rotation;

        if (photonView.isMine)
        {
            if (!isNpc && isDispBattleLog)
            {
                //バトルログ
                int logType = BATTLE_LOG_ATTACK;
                foreach (Queue que in logBattleQueue)
                {
                    battleLogArea[logType].text = "";
                    foreach (string text in que)
                    {
                        battleLogArea[logType].text += text + "\n";
                    }
                    logType = BATTLE_LOG_DAMAGE;
                }
            }
            if (!isNpc && (isDead || GameController.Instance.isGameEnd))
            {
                int[] logTypes = new int[] { BATTLE_LOG_ATTACK, BATTLE_LOG_DAMAGE };
                foreach (int logType in logTypes)
                {
                    PushBattleLog(logType, slipTotalDmg[logType], preSlipDmgName[logType]);
                    preSlipDmgName[logType] = "";
                    slipTotalDmg[logType] = 0;
                }
            }

            if (isDead)
            {
                //被ダメボイス
                if (voiceManager != null) voiceManager.Dead();

                //戦闘不能
                //transform.DetachChildren();
                if (!isNpc)
                {
                    Camera.main.transform.parent = null;
                }
                if (hitEffect != null)
                {
                    hitEffect.color = hitNoiseEnd;
                    Destroy(hitEffect.gameObject);
                }
                GetComponent<ObjectController>().DestoryObject();
                return;
            }
            if (transform.position.y < -10)
            {
                //エリアアウト
                AddDamage(11);
            }

            //ヒット時ノイズ
            if (hitEffect != null && (leftHitEffectTime > 0 || hitEffect.color.a > 0))
            {
                leftHitEffectTime -= Time.deltaTime;
                float rate = 1 - leftHitEffectTime / HIT_EFFECT_TIME;
                if (rate > 1) rate = 1;
                hitEffect.color = Color.Lerp(hitNoiseStart, hitNoiseEnd, rate);
            }
        }

        if (leftInvincibleTime > 0)
        {
            leftInvincibleTime -= Time.deltaTime;
        }
    }

    public void SetInvincible(bool flg = true, float time = 0, bool isShieldBisible = false)
    {
        //Debug.Log("SetInvincible :" + flg.ToString()+" / time: "+ time.ToString());

        float setTime = time;
        if (flg)
        {
            if (setTime == 0) setTime = invincibleTime;
            //if (setTime <= 0) return;
            if (leftInvincibleTime >= setTime) return;
        }

        object[] args = new object[] { setTime , isShieldBisible };
        photonView.RPC("SetInvincibleRPC", PhotonTargets.All, args);
    }

    [PunRPC]
    private void SetInvincibleRPC(float time, bool isShieldBisible)
    {
        leftInvincibleTime = time;
        if (time > 0 && isShieldBisible)
        {
            StartCoroutine(OpenShield(time));
        }
    }

    //ロックされているかFLG
    public void SetLocked(bool flg)
    {
        photonView.RPC("SetLockedRPC", PhotonTargets.All, flg);
    }
    [PunRPC]
    private void SetLockedRPC(bool flg)
    {
        isLocked = flg;
    }

    public bool IsLocked()
    {
        return isLocked;
    }
    public bool IsNpc()
    {
        return isNpc;
    }

    public int GetNowHp()
    {
        return nowHp;
    }
    public int GetNowHpPer()
    {
        return (int)(nowHp * 100 / maxHp );
    }
    public int GetNowSpPer()
    {
        return (int)(nowSp * 100 / maxSp);
    }

    //##### パラメータ変更系 #####
    private float nowSpeedRate = 1;
    private float interfareMoveTime = 0;

    //初期ステータス設定(NPC用)
    public void SetStatus(int[] defaultStatus, float[] levelRate)
    {
        //MaxHp
        int index = Common.Character.STATUS_MAX_HP;
        maxHp = (int)(defaultStatus[index] * levelRate[index]);
        SetHp(maxHp);

        //RecoverSp
        index = Common.Character.STATUS_RECOVER_SP;
        recoverSp = (int)(defaultStatus[index] * levelRate[index]);
        //defaultRecoverSp = recoverSp;

        //RunSpeed
        index = Common.Character.STATUS_RUN_SPEED;
        runSpeed = defaultStatus[index] * levelRate[index];
        defaultRunSpeed = runSpeed;

        //BoostSpeed
        index = Common.Character.STATUS_BOOST_SPEED;
        boostSpeed = defaultStatus[index] * levelRate[index];
        defaultBoostSpeed = boostSpeed;

        //TurnSpeed
        index = Common.Character.STATUS_TURN_SPEED;
        turnSpeed = defaultStatus[index] * levelRate[index];
        defaultTurnSpeed = turnSpeed;

        index = Common.Character.STATUS_ATTACK_RATE;
        attackRate = defaultStatus[index] * levelRate[index];
        //defaultAttackRate = attackRate;
    }

    //SP回復量(自分専用)
    public void AccelerateRecoverSp(float rate, float limit, GameObject effect = null)
    {
        StartCoroutine(CheckAccelerateRecoverSp(rate, limit, effect));
    }
    IEnumerator CheckAccelerateRecoverSp(float rate, float limit, GameObject effect = null)
    {
        int changeValue = (int)(recoverSp * rate - recoverSp);
        recoverSp += changeValue;
        SwitchEffect(effect, true);
        yield return new WaitForSeconds(limit);
        SwitchEffect(effect, false);
        recoverSp -= changeValue;
    }

    //攻撃力
    public bool ChangeAttackRate(float rate, float limit, GameObject effect = null, bool isSendRpc = true)
    {
        if (photonView.isMine)
        {
            StartCoroutine(ChangeAttackRateProc(rate, limit, effect));
        }
        else
        {
            if (isSendRpc)
            {
                object[] args = new object[] { rate, limit, effect };
                photonView.RPC("AccelerateRunSpeedRPC", PhotonTargets.Others, args);
            }
        }
        return true;
    }

    [PunRPC]
    public void ChangeAttackRateRPC(float rate, float limit, GameObject effect = null)
    {
        ChangeAttackRate(rate, limit, effect, false);
    }

    IEnumerator ChangeAttackRateProc(float rate, float limit, GameObject effect = null)
    {
        int changeValue = (int)(attackRate * rate - attackRate);
        attackRate += changeValue;
        SwitchEffect(effect, true);
        yield return new WaitForSeconds(limit);
        SwitchEffect(effect, false);
        attackRate -= changeValue;
    }

    //移動速度アップ・ダウン(負の効果優先)
    public bool AccelerateRunSpeed(float rate, float limit, GameObject effect = null, bool isSendRpc = true)
    {
        if (rate == 0)
        {
            InterfareMove(limit, effect, isSendRpc);
            return true;
        }

        if (photonView.isMine)
        {
            if (nowSpeedRate != 1 && nowSpeedRate <= rate)
            {
                //既に別の効果が適用中
                return false;
            }
            StartCoroutine(CheckAccelerateRunSpeed(rate, limit, effect));
        }
        else
        {
            if (isSendRpc)
            {
                object[] args = new object[] { rate, limit, effect };
                photonView.RPC("AccelerateRunSpeedRPC", PhotonTargets.Others, args);
            }
        }
        return true;
    }

    [PunRPC]
    public void AccelerateRunSpeedRPC(float rate, float limit, GameObject effect = null)
    {
        AccelerateRunSpeed(rate, limit, effect, false);
    }

    IEnumerator CheckAccelerateRunSpeed(float rate, float limit, GameObject effect = null)
    {
        nowSpeedRate = rate;
        runSpeed *= rate;
        jumpSpeed *= rate;
        boostSpeed *= rate;
        SwitchEffect(effect, true);

        for (;;)
        {
            yield return null;
            limit -= Time.deltaTime;
            if (limit <= 0) break;
            if (nowSpeedRate != rate) break;
        }

        SwitchEffect(effect, false);
        if (interfareMoveTime <= 0 && nowSpeedRate == rate)
        {
            runSpeed = defaultRunSpeed;
            jumpSpeed = defaultJumpSpeed;
            boostSpeed = defaultBoostSpeed;
            nowSpeedRate = 1;
        }
    }

    //移動制限
    public void InterfareMove(float limit, GameObject effect = null, bool isSendRpc = true)
    {
        //他人からの効果にはエフェクトをつける
        if (isSendRpc && effect == null) effect = stuckEffect;

        if (photonView.isMine)
        {
            if (interfareMoveTime > 0)
            {
                if (interfareMoveTime < limit || limit == 0)
                {
                    //残り時間上書き
                    interfareMoveTime = limit;
                }
                return;
            }
            StartCoroutine(CheckInterfareMove(limit, effect));
        }
        else
        {
            if (isSendRpc)
            {
                int effectViewId = -1;
                PhotonView pv = PhotonView.Get(effect);
                if (pv != null) effectViewId = pv.viewID;
                object[] args = new object[] { limit, effectViewId };
                photonView.RPC("InterfareMoveRPC", PhotonTargets.Others, args);
            }
        }
    }

    [PunRPC]
    public void InterfareMoveRPC(float limit, int effectViewId)
    {
        GameObject effect = null;
        PhotonView pv = PhotonView.Find(effectViewId);
        if (pv != null)
        {
            effect = pv.gameObject;
        }
        else
        {
            if (stuckEffect != null && PhotonView.Get(stuckEffect).viewID == effectViewId)
            {
                effect = stuckEffect;
            }
        }
        InterfareMove(limit, effect, false);
    }

    IEnumerator CheckInterfareMove(float limit, GameObject effect = null)
    {
        interfareMoveTime = limit;
        runSpeed = 0;
        jumpSpeed = 0;
        boostSpeed = 0;
        SwitchEffect(effect, true);
        for (;;)
        {
            yield return null;

            interfareMoveTime -= Time.deltaTime;
            if (interfareMoveTime <= 0)
            {
                interfareMoveTime = 0;
                break;
            }
        }
        SwitchEffect(effect, false);
        runSpeed = defaultRunSpeed * nowSpeedRate;
        jumpSpeed = defaultJumpSpeed * nowSpeedRate;
        boostSpeed = defaultBoostSpeed * nowSpeedRate;
    }

    //回転制限(自分専用)
    public void InterfareTurn(float rate, float limit)
    {
        StartCoroutine(CheckInterfareTurn(rate, limit));
    }

    IEnumerator CheckInterfareTurn(float rate, float limit)
    {
        boostTurnSpeed = turnSpeed * rate;
        if (rate < 1) turnSpeed *= rate;

        yield return new WaitForSeconds(limit);

        boostTurnSpeed = defaultBoostTurnSpeed;
        turnSpeed = defaultTurnSpeed;
    }

    //無敵時間延長(自分専用)
    public void AvoidBurst(float rate, float limit, GameObject effect = null)
    {
        StartCoroutine(CheckAvoidBurst(rate, limit, effect));
    }
    IEnumerator CheckAvoidBurst(float rate, float limit, GameObject effect = null)
    {
        float changeValue = invincibleTime * rate - invincibleTime;
        invincibleTime += changeValue;
        SwitchEffect(effect, true);
        yield return new WaitForSeconds(limit);
        SwitchEffect(effect, false);
        invincibleTime -= changeValue;
    }

    //Ex武器用
    public void SetForceInvincible(bool flg)
    {
        photonView.RPC("SetForceInvincibleRPC", PhotonTargets.All, flg);
    }
    [PunRPC]
    private void SetForceInvincibleRPC(bool flg)
    {
        isForceInvincible = flg;
        float limit = 0;
        if (flg) limit = 10;
        InterfareMove(limit, null, false);
    }

    //エフェクト切り替え
    private void SwitchEffect(GameObject effect, bool flg)
    {
        if (effect == null) return;
        effect.SetActive(flg);
        PhotonView parentPv = PhotonView.Get(effect.transform.parent);
        if (parentPv == null) return;
        object[] arg = new object[] { parentPv.viewID, effect.name, flg };
        photonView.RPC("SwitchEffectRPC", PhotonTargets.All, arg);
    }
    [PunRPC]
    private void SwitchEffectRPC(int parentViewId, string effectName, bool flg)
    {
        PhotonView pv = PhotonView.Find(parentViewId);
        if (pv == null) return;
        Transform effect = pv.transform.FindChild(effectName);
        if (effect == null) return;
        effect.gameObject.SetActive(flg);
    }

    public void ResetWinMark()
    {
        bool markFlg = false;
        if (GameController.Instance.gameMode == GameController.GAME_MODE_VS) markFlg = true;
        foreach (GameObject obj in winCountMineList)
        {
            obj.SetActive(markFlg);
        }
        foreach (GameObject obj in winCountEnemyList)
        {
            obj.SetActive(markFlg);

        }
    }

    public void SetWinMark(int winCount, int loseCount)
    {
        if (GameController.Instance.gameMode == GameController.GAME_MODE_VS)
        {
            ResetWinMark();
            return;
        }

        //自分の勝マーク
        bool markFlg = true;
        int count = 1;
        foreach (GameObject obj in winCountMineList)
        {
            if (winCount < count) markFlg = false;
            obj.SetActive(markFlg);
            count++;
        }

        //敵の勝マーク
        markFlg = true;
        count = 1;
        foreach (GameObject obj in winCountEnemyList)
        {
            if (loseCount < count) markFlg = false;
            obj.SetActive(markFlg);
            count++;
        }
    }

    public bool ForceBoost(Vector3 moveVector, float speed, float time, int consumeSp = 0, bool isInvincible = false)
    {
        if (moveVector == Vector3.zero || speed <= 0 || time <= 0 || moveCtrl == null) return false;
        
        if (consumeSp > 0)
        {
            //SP消費
            UseSp(consumeSp);
        }
        if (isInvincible)
        {
            //無敵状態
            SetInvincible(true);
        }
        //ブースト
        moveCtrl.SpecialBoost(moveVector, speed, time);

        //移動・回転制限
        InterfareMove(time, null, false);
        InterfareTurn(0, time);

        return true;
    }


    //自分の行動による反動
    public void ActionRecoil(float speed, float limit = 0, Vector3 forceVector = default(Vector3))
    {
        if (!isActiveSceane) return;
        if (forceVector == default(Vector3)) forceVector = Vector3.back;
        moveCtrl.ActionRecoil(forceVector, speed, limit);
    }

    
    //##### バトルログ #####

    public void SetBattleLog(int logType, int damage, string name, bool isSlipDamage = false)
    {
        if (!isDispBattleLog) return;
        if (!photonView.isMine || isNpc) return;

        if (isSlipDamage)
        {
            if (preSlipDmgName[logType] == name)
            {
                //Slipダメージはまとめる
                slipTotalDmg[logType] += damage;
            }
            else
            {
                //別のSlipダメージ
                PushBattleLog(logType, slipTotalDmg[logType], preSlipDmgName[logType]);
                preSlipDmgName[logType] = name;
                slipTotalDmg[logType] = damage;
            }
        }
        else
        {
            if (preSlipDmgName[logType] != "")
            {
                //ログへ出力
                PushBattleLog(logType, slipTotalDmg[logType], preSlipDmgName[logType]);
                preSlipDmgName[logType] = "";
                slipTotalDmg[logType] = 0;
            }
            PushBattleLog(logType, damage, name);
        }
    }

    private void PushBattleLog(int logType, int damage, string name, bool console = false)
    {
        if (!isDispBattleLog) return;
        if (!photonView.isMine || isNpc) return;
        if (name == "" || damage <= 0) return;

        //バトルログ
        battleLogNo[logType]++;
        name = name.Replace("(Clone)", "");
        string logName = name;
        if (logName.Length > 10) logName = logName.Substring(0, 10);
        string text = "[" + battleLogNo[logType] + "]" + logName + " > " + damage.ToString();

        if (logBattleQueue[logType].Count >= BATTLE_LOG_COUNT) logBattleQueue[logType].Dequeue();
        logBattleQueue[logType].Enqueue(text);
        if (console) Debug.Log(text);

        //ダメージソース
        GameController.Instance.SetDamageSource(logType, name, damage);
    }

    public bool SwitchBattleLog()
    {
        isDispBattleLog = !isDispBattleLog;
        battleLogArea[BATTLE_LOG_ATTACK].gameObject.SetActive(isDispBattleLog);
        battleLogArea[BATTLE_LOG_DAMAGE].gameObject.SetActive(isDispBattleLog);
        return isDispBattleLog;
    }
}

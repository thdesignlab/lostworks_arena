using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using System.Reflection;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;


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
    public float defenceRate = 100;    //防御力

    //HP
    [SerializeField]
    private int maxHp = 1000;   //最大HP
    private int nowHp;      //現在HP
    private float diffValue = 0.001f;
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

    //反射判定
    private float leftReflectionTime = 0;

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
    private Text nameTextMine;
    private Text nameTextEnemy;

    private Image hpBarMineImage;
    private Image hpBarEnemyImage;
    //private Color defaultHpColor;
    //private Color hitHpColor = Color.red;
    private float totalDamage = 0;
    //[SerializeField]
    private Image hitEffect;
    private float leftHitEffectTime = 0;
    private const float HIT_EFFECT_TIME = 0.7f;
    private Color hitNoiseStart = new Color(1, 1, 1, 1);
    private Color hitNoiseEnd = new Color(1, 1, 1, 0);

    //private CameraController camCtrl;
    private bool isLocked = false;
    private bool isNpc = false;

    //パラメータ変更用
    private float defaultRunSpeed;
    private float defaultJumpSpeed;
    private float defaultBoostSpeed;

    private BaseMoveController moveCtrl;
    private bool isActiveSceane = true;

    //強制無敵状態
    private bool isForceInvincible = false;

    //スタックエフェクト
    [SerializeField]
    private GameObject stuckEffect;
    //デバフエフェクト
    [SerializeField]
    private GameObject debuffEffect;
    private Dictionary<int, float> debuffDic = new Dictionary<int, float>();

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
    private float[] slipTotalDmg = new float[] { 0, 0 };

    [HideInInspector]
    public VoiceManager voiceManager;

    //ユーザー情報
    [HideInInspector]
    public int userId = -1;
    [HideInInspector]
    public string userName = "";
    [HideInInspector]
    public int battleRate = 0;

    [HideInInspector]
    public bool isReadyBattle = false;


    void Start()
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
        boostTurnSpeed = turnSpeed * 10;

        if (SceneManager.GetActiveScene().name == Common.CO.SCENE_CUSTOM)
        {
            //カスタム画面
            isActiveSceane = false;
            return;
        }
        PlayerSetting playerSetting = GetComponent<PlayerSetting> ();
        isNpc = (playerSetting != null) ? playerSetting.isNpc : true;
        moveCtrl = GetComponent<BaseMoveController>();

        //ステータス構造
        Transform screenCanvasTran = Camera.main.transform.FindChild(Common.CO.SCREEN_CANVAS);
        Transform screenStatusTran = screenCanvasTran.FindChild(Common.CO.SCREEN_STATUS);

        //ダメージノイズ
        if (photonView.isMine && !isNpc)
        {
            Transform damageEffect = screenCanvasTran.FindChild("DamageEffect");
            if (damageEffect != null) hitEffect = damageEffect.GetComponent<Image>();
        }

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

        //名前エリア
        nameTextMine = screenStatusTran.FindChild("NameLine/Mine").GetComponent<Text>();
        nameTextEnemy = screenStatusTran.FindChild("NameLine/Enemy").GetComponent<Text>();

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

        //ボイス管理
        Transform charaTran = transform.Find(Common.Func.GetBodyStructure());
        if (charaTran) voiceManager = charaTran.GetComponent<VoiceManager>();

        if (!isActiveSceane) return;

        StartCoroutine(DamageSync());
        StartCoroutine(RecoverSp());

        isReadyBattle = true;
    }

    public void Init()
    {
        if (photonView.isMine)
        {
            SetHp(maxHp);
            SetSp(maxSp);

            if (GameController.Instance.gameMode == GameController.GAME_MODE_VS) SetNmaeText();

            StopAllBuffCoroutine();
            nowSpeedRate = 1;
            interfareMoveTime = 0;
            runSpeed = defaultRunSpeed;
            jumpSpeed = defaultJumpSpeed;
            boostSpeed = defaultBoostSpeed;
            isForceInvincible = false;
            //debuffDic = new Dictionary<int, float>();
            List<int> keys = new List<int>(debuffDic.Keys);
            foreach (int key in keys)
            {
                if (debuffDic.ContainsKey(key)) debuffDic[key] = 0;
            }

            if (isNpc)
            {
                StartCoroutine(SetHpSlider(hpBarEnemy, hpBarEnemyImage));
            }
            else
            {
                StartCoroutine(SetHpSlider(hpBarMine, hpBarMineImage));

                userId = UserManager.GetUserId();
                userName = UserManager.userInfo[Common.PP.INFO_USER_NAME];
                battleRate = ModelManager.battleRecord.battle_rate;
                object[] args = new object[] { userId, userName, battleRate };
                photonView.RPC("SetUserInfoRPC", PhotonTargets.Others, args);
            }
        }
        else
        {
            StartCoroutine(SetHpSlider(hpBarEnemy, hpBarEnemyImage));
        }

        battleLogArea[BATTLE_LOG_ATTACK].text = "";
        battleLogArea[BATTLE_LOG_DAMAGE].text = "";
        logBattleQueue[BATTLE_LOG_ATTACK] = new Queue();
        logBattleQueue[BATTLE_LOG_DAMAGE] = new Queue();
        preSlipDmgName[BATTLE_LOG_ATTACK] = "";
        preSlipDmgName[BATTLE_LOG_DAMAGE] = "";
        slipTotalDmg[BATTLE_LOG_ATTACK] = 0;
        slipTotalDmg[BATTLE_LOG_DAMAGE] = 0;
    }

    [PunRPC]
    private void SetUserInfoRPC(int id, string name, int rate)
    {
        userId = id;
        userName = name;
        battleRate = rate;
    }

    public void SetNmaeText()
    {
        if (!photonView.isMine) return;
        if (string.IsNullOrEmpty(userName)) return;

        //自分の名前セット
        string name = "";
        if (battleRate > 0) name += "[" + battleRate.ToString() + "]";
        name += userName;
        nameTextMine.text = name;
        photonView.RPC("SetNmaeTextRPC", PhotonTargets.Others, name);
    }
    [PunRPC]
    private void SetNmaeTextRPC(string name)
    {
        //敵の名前セット
        if (nameTextEnemy != null) nameTextEnemy.text = name;
    }

    //一定間隔ごとにダメージを同期する
    IEnumerator DamageSync()
    {
        for (;;)
        {
            yield return new WaitForSeconds(0.2f);
            if (totalDamage < 1) continue;
            int d = (int)Mathf.Floor(totalDamage);
            SetHp(nowHp - d);
            totalDamage -= d;
        }
    }

    private void SetHp(int hp)
    {
        photonView.RPC("SetHpRPC", PhotonTargets.All, hp);
    }
    [PunRPC]
    private void SetHpRPC(int hp)
    {
        nowHp = hp;
        if (nowHp > maxHp) maxHp = nowHp;
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
        SetSpSlider();
    }

    public void ForceDamage(float damage)
    {
        if (!isActiveSceane) return;
        if (!photonView.isMine) return;
        if (!GameController.Instance.isPractice)
        { 
            if (!GameController.Instance.isGameStart || GameController.Instance.isGameEnd) return;
        }

        //ダメージ
        totalDamage += damage;
        if (nowHp - totalDamage <= 0)
        {
            SetHp(0);
        }
    }
    public float AddDamage(float damage, string name = "Unknown", bool isSlipDamage = false)
    {
        if (!isActiveSceane) return 0;
        if (!GameController.Instance.isPractice)
        {
            if (!GameController.Instance.isGameStart || GameController.Instance.isGameEnd) return 0;
        }
        if (damage <= 0) return 0;

        //無敵時間判定
        if (leftInvincibleTime > 0 || isForceInvincible)
        {
            if (shield != null)
            {
                OpenShield(shieldTime);
            }
            return 0;
        }

        //防御力考慮
        if (defenceRate > 0 && defenceRate != 100)
        {
            damage = damage * (defenceRate / 100);
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
        SetBattleLog(BATTLE_LOG_DAMAGE, (int)damage, name, isSlipDamage);

        return damage;
    }

    private void OpenShield(float time, bool isSendRPC = true)
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
            StartCoroutine(OpenShieldProc(time));
        }
        if (isSendRPC)
        {
            photonView.RPC("OpenShieldRPC", PhotonTargets.Others, time);
        }
    }
    [PunRPC]
    private void OpenShieldRPC(float time)
    {
        OpenShield(time, false);
    }
    IEnumerator OpenShieldProc(float time)
    {
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
    }

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
        if (nowSp < sp) return false;
        return true;
    }
    private void SetSpSlider()
    {
        if (!photonView.isMine || isNpc) return;
        if (spBarMine != null) spBarMine.value = (float)nowSp / (float)maxSp;
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

        float deltaTime = Time.deltaTime;
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
                if (isDead || GameController.Instance.isGameEnd)
                {
                    int[] logTypes = new int[] { BATTLE_LOG_ATTACK, BATTLE_LOG_DAMAGE };
                    foreach (int Type in logTypes)
                    {
                        PushBattleLog(Type, slipTotalDmg[Type], preSlipDmgName[Type]);
                        preSlipDmgName[Type] = "";
                        slipTotalDmg[Type] = 0;
                    }
                }
            }

            bool isPractice = GameController.Instance.isPractice;
            if (isDead)
            {
                //被ダメボイス
                if (voiceManager != null) voiceManager.Dead();

                if (!isNpc && isPractice)
                {
                    //練習モード
                    transform.position = new Vector3(0, 15, 0);
                    SetHp(maxHp);
                    SetSp(maxSp);
                    return;
                }

                //戦闘不能
                if (!isNpc)
                {
                    Camera.main.transform.parent = null;
                }
                else
                {
                    GameController.Instance.isPractice = false;
                }
                if (hitEffect != null)
                {
                    hitEffect.color = hitNoiseEnd;
                }
                GetComponent<ObjectController>().DestoryObject();
                return;
            }
            if (transform.position.y < -10)
            {
                //エリアアウト
                if (!isNpc && isPractice) transform.position = new Vector3(0, 15, 0);
                ForceDamage(11);
            }

            //ヒット時ノイズ
            if (hitEffect != null && (leftHitEffectTime > 0 || hitEffect.color.a > 0))
            {
                leftHitEffectTime -= deltaTime;
                float rate = 1 - leftHitEffectTime / HIT_EFFECT_TIME;
                if (rate > 1) rate = 1;
                hitEffect.color = Color.Lerp(hitNoiseStart, hitNoiseEnd, rate);
            }
        }

        //デバフリスト
        if (debuffDic.Count > 0)
        {
            bool isDebuff = false;
            List<int> keys = new List<int>(debuffDic.Keys);
            foreach (int key in keys)
            {
                if (debuffDic[key] <= 0) continue;
                debuffDic[key] -= deltaTime;
                if (debuffDic[key] > 0) isDebuff = true;
            }
            if (debuffEffect != null)
            {
                debuffEffect.SetActive(isDebuff);
                photonView.RPC("SwitchDebuffEffectRPC", PhotonTargets.Others, isDebuff);
            }
        }

        //残り無敵時間
        if (leftInvincibleTime > 0) leftInvincibleTime -= deltaTime;
    }
    [PunRPC]
    private void SwitchDebuffEffectRPC(bool flg)
    {
        if (debuffEffect != null) debuffEffect.SetActive(flg);
    }

    public void SetInvincible(bool flg = true, float time = 0, bool isShieldVisible = false, bool isReflection = false)
    {
        float setTime = time;
        if (flg)
        {
            if (setTime == 0) setTime = invincibleTime;
            if (leftInvincibleTime >= setTime) return;
        }

        object[] args = new object[] { setTime , isShieldVisible, isReflection };
        photonView.RPC("SetInvincibleRPC", PhotonTargets.All, args);
    }

    [PunRPC]
    private void SetInvincibleRPC(float time, bool isShieldVisible, bool isReflection = false)
    {
        leftInvincibleTime = time;
        if (time > 0 && isShieldVisible)
        {
            StartCoroutine(OpenShieldProc(time));
        }
        if (isReflection) SetReflectionTime(time, false);
    }

    //反射設定
    public void SetReflectionTime(float time, bool isSendRPC = true)
    {
        StartCoroutine(SetReflectionTimeProc(time));
        if (isSendRPC)
        {
            photonView.RPC("SetInvincibleRPC", PhotonTargets.Others, time);
        }
    }
    [PunRPC]
    public void SetReflectionTimeRPC(float time)
    {
        SetReflectionTime(time, false);
    }
    IEnumerator SetReflectionTimeProc(float time)
    {
        leftReflectionTime = time;
        for (;;)
        {
            leftReflectionTime -= Time.deltaTime;
            if (leftReflectionTime <= 0) break;
            yield return null;
        }
    }
    public bool IsReflection()
    {
        return (leftReflectionTime > 0);
    }

    //ロックされているかFLG
    public void SetLocked(bool flg, bool isSendRPC = true)
    {
        isLocked = flg;
        if (isSendRPC)
        {
            photonView.RPC("SetLockedRPC", PhotonTargets.Others, flg);
        }
    }
    [PunRPC]
    private void SetLockedRPC(bool flg)
    {
        SetLocked(flg, false);
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
    //private Coroutine RunSpeedChangeCoroutine;
    private Dictionary<string, BuffInfo> buffCoroutineList = new Dictionary<string, BuffInfo>();

    //初期ステータス設定
    public void SetStatus(int[] defaultStatus, float[] levelRate)
    {
        if (!photonView.isMine) return;

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
        boostTurnSpeed = turnSpeed * 10;

        //AttackRate
        index = Common.Character.STATUS_ATTACK_RATE;
        attackRate = defaultStatus[index] * levelRate[index];
    }

    //SP回復量
    public void AccelerateRecoverSp(float rate, float limit, GameObject effect = null, bool isSendRPC = true)
    {
        Coroutine cor = StartCoroutine(CheckAccelerateRecoverSp(rate, limit, effect));
        SetBuffCoroutine("AccelerateRecoverSp", cor, effect);
        if (isSendRPC)
        {
            int parentViewId = (effect != null) ? GetParentViewId(effect) : - 1;
            string effectName = (effect != null) ? effect.name : "";
            object[] args = new object[] { rate, limit, parentViewId, effectName };
            photonView.RPC("AccelerateRecoverSpRPC", PhotonTargets.Others, args);
        }
    }
    [PunRPC]
    private void AccelerateRecoverSpRPC(float rate, float limit, int parentViewId, string effectName)
    {
        GameObject effect = GetChildObject(parentViewId, effectName);
        AccelerateRecoverSp(rate, limit, effect, false);
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
    public void ChangeAttackRate(float rate, float limit, GameObject effect = null, bool isSendRPC = true)
    {
        Coroutine cor = StartCoroutine(ChangeAttackRateProc(rate, limit, effect));
        SetBuffCoroutine("ChangeAttackRate", cor, effect);

        if (isSendRPC)
        {
            int parentViewId = (effect != null) ? GetParentViewId(effect) : -1;
            string effectName = (effect != null) ? effect.name : "";
            object[] args = new object[] { rate, limit, parentViewId, effectName };
            photonView.RPC("ChangeAttackRateRPC", PhotonTargets.Others, args);
        }
    }
    [PunRPC]
    private void ChangeAttackRateRPC(float rate, float limit, int parentViewId, string effectName)
    {
        GameObject effect = GetChildObject(parentViewId, effectName);
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

    //防御力
    Coroutine defChange;
    public void ChangeDefRate(float rate, float limit, GameObject effect = null, bool isSendRPC = true)
    {
        Coroutine cor = defChange = StartCoroutine(ChangeDefRateProc(rate, limit, effect));
        SetBuffCoroutine("ChangeDefRate", cor, effect);

        if (isSendRPC)
        {
            int parentViewId = (effect != null) ? GetParentViewId(effect) : -1;
            string effectName = (effect != null) ? effect.name : "";
            object[] args = new object[] { rate, limit, parentViewId, effectName };
            photonView.RPC("ChangeDefRateRPC", PhotonTargets.Others, args);
        }
    }
    [PunRPC]
    private void ChangeDefRateRPC(float rate, float limit, int parentViewId, string effectName)
    {
        GameObject effect = GetChildObject(parentViewId, effectName);
        ChangeDefRate(rate, limit, effect, false);
    }
    IEnumerator ChangeDefRateProc(float rate, float limit, GameObject effect = null)
    {
        if (rate <= 0) yield break;
        rate = 1 / rate;
        int changeValue = (int)(defenceRate * rate - defenceRate);
        defenceRate += changeValue;
        SwitchEffect(effect, true);
        yield return new WaitForSeconds(limit);
        SwitchEffect(effect, false);
        defenceRate -= changeValue;
    }

    //移動速度アップ・ダウン(負の効果優先)
    public bool AccelerateRunSpeed(float rate, float limit, GameObject effect = null, bool isSendRpc = true)
    {
        if (rate == 0)
        {
            InterfareMove(limit, effect, isSendRpc);
            return true;
        }
        if (nowSpeedRate != 1 && nowSpeedRate <= rate)
        {
            //既に別の効果が適用中
            return false;
        }

        Coroutine cor = StartCoroutine(CheckAccelerateRunSpeed(rate, limit, effect));
        SetBuffCoroutine("AccelerateRunSpeed", cor, effect);
        if (isSendRpc)
        {
            int parentViewId = (effect != null) ? GetParentViewId(effect) : -1;
            string effectName = (effect != null) ? effect.name : "";
            object[] args = new object[] { rate, limit, parentViewId, effectName };
            photonView.RPC("AccelerateRunSpeedRPC", PhotonTargets.Others, args);
        }
        return true;
    }
    [PunRPC]
    public void AccelerateRunSpeedRPC(float rate, float limit, int parentViewId, string effectName)
    {
        GameObject effect = GetChildObject(parentViewId, effectName);
        AccelerateRunSpeed(rate, limit, effect, false);
    }
    IEnumerator CheckAccelerateRunSpeed(float rate, float limit, GameObject effect = null)
    {
        nowSpeedRate = rate;
        runSpeed = defaultRunSpeed * rate;
        jumpSpeed = defaultJumpSpeed * rate;
        boostSpeed = defaultBoostSpeed * rate;
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
        }
        nowSpeedRate = 1;
    }

    //移動制限
    public void AttackInterfareMove(float limit, GameObject effect = null)
    {
        if (effect == null) effect = stuckEffect;
        InterfareMove(limit, stuckEffect);
    }
    public void InterfareMove(float limit, GameObject effect = null, bool isSendRpc = true)
    {
        if (interfareMoveTime > 0)
        {
            if (interfareMoveTime < limit || limit == 0)
            {
                //残り時間上書き
                interfareMoveTime = limit;
                if (isSendRpc) photonView.RPC("AddInterfareMoveRPC", PhotonTargets.Others, limit);
            }
            return;
        }

        if (effect == stuckEffect) moveCtrl.StopBoost();

        Coroutine cor = StartCoroutine(CheckInterfareMove(limit, effect));
        SetBuffCoroutine("InterfareMove", cor, effect);

        if (isSendRpc)
        {
            int parentViewId = (effect != null) ? GetParentViewId(effect) : -1;
            string effectName = (effect != null) ? effect.name : "";
            object[] args = new object[] { limit, parentViewId, effectName };
            photonView.RPC("InterfareMoveRPC", PhotonTargets.Others, args);
        }
    }
    [PunRPC]
    public void AddInterfareMoveRPC(float limit)
    {
        interfareMoveTime = limit;
    }
    [PunRPC]
    public void InterfareMoveRPC(float limit, int parentViewId, string effectName)
    {
        GameObject effect = GetChildObject(parentViewId, effectName);
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
        Coroutine cor = StartCoroutine(CheckInterfareTurn(rate, limit));
        SetBuffCoroutine("InterfareTurn", cor, null);

    }
    IEnumerator CheckInterfareTurn(float rate, float limit)
    {
        int turnChangeValue = (int)(turnSpeed * rate - turnSpeed);
        int boostTrunChangeValue = (rate < 1) ? (int)(boostTurnSpeed - turnSpeed - turnChangeValue) * -1 : 0;

        boostTurnSpeed += boostTrunChangeValue;
        turnSpeed += turnChangeValue;
        yield return new WaitForSeconds(limit);
        boostTurnSpeed -= boostTrunChangeValue;
        turnSpeed -= turnChangeValue;
    }

    //無敵時間延長
    public void AvoidBurst(float rate, float limit, GameObject effect = null, bool isSendRPC = true)
    {
        Coroutine cor = StartCoroutine(CheckAvoidBurst(rate, limit, effect));
        SetBuffCoroutine("AvoidBurst", cor, effect);

        if (isSendRPC)
        {
            int parentViewId = (effect != null) ? GetParentViewId(effect) : -1;
            string effectName = (effect != null) ? effect.name : "";
            object[] args = new object[] { rate, limit, parentViewId, effectName };
            photonView.RPC("AvoidBurstRPC", PhotonTargets.Others, args);
        }
    }
    [PunRPC]
    private void AvoidBurstRPC(float rate, float limit, int parentViewId, string effectName)
    {
        GameObject effect = GetChildObject(parentViewId, effectName);
        AvoidBurst(rate, limit, effect, false);
    }
    IEnumerator CheckAvoidBurst(float rate, float limit, GameObject effect = null)
    {
        float changeValue = invincibleTime * rate - invincibleTime;
        invincibleTime += changeValue;
        SwitchEffect(effect, true, true);
        yield return new WaitForSeconds(limit);
        SwitchEffect(effect, false, true);
        invincibleTime -= changeValue;
    }

    //Ex武器用無敵処理
    public void SetForceInvincible(bool flg, bool isSendRPC = true)
    {
        if (flg)
        {
            isForceInvincible = flg;
        }
        else
        {
            StartCoroutine(ForceInvincibleCancel(1.5f));
        }
        
        if (isSendRPC)
        {
            photonView.RPC("SetForceInvincibleRPC", PhotonTargets.Others, flg);
        }
    }
    IEnumerator ForceInvincibleCancel(float limit)
    {
        for (;;)
        {
            limit -= Time.deltaTime;
            if (limit <= 0) break;
            yield return null;
        }
        isForceInvincible = false;
    }
    [PunRPC]
    private void SetForceInvincibleRPC(bool flg)
    {
        SetForceInvincible(flg, false);
    }
    public bool IsForceInvincible()
    {
        return isForceInvincible;
    }

    private int GetParentViewId(GameObject child)
    {
        int viewId = -1;
        if (child != null && child.transform.parent != null)
        {
            PhotonView pv = PhotonView.Get(child.transform.parent);
            if (pv != null) viewId = pv.viewID;
        }
        return viewId;
    }
    private GameObject GetChildObject(int parentViewId, string childName)
    {
        GameObject child = null;
        PhotonView parentPV = PhotonView.Find(parentViewId);
        if (parentPV != null && !string.IsNullOrEmpty(childName))
        {
            Transform parentTran = parentPV.gameObject.transform;
            Transform childTran = parentTran.FindChild(childName);
            if (childTran != null) child = childTran.gameObject;
        }
        return child;
    }

    //エフェクト切り替え
    private void SwitchEffect(GameObject effect, bool flg, bool isSendRPC = false)
    {
        if (effect == null) return;
        if (effect == debuffEffect)
        {
            //デバフカウント
            if (!flg && debuffDic.Count > 0) return;
        }
        effect.SetActive(flg);

        if (isSendRPC)
        {
            int parentViewId = GetParentViewId(effect);
            object[] args = new object[] { parentViewId, effect.name, flg };
            photonView.RPC("SwitchEffectRPC", PhotonTargets.Others, args);
        }
    }
    [PunRPC]
    private void SwitchEffectRPC(int parentViewId, string effectName, bool flg)
    {
        GameObject effect = GetChildObject(parentViewId, effectName);
        SwitchEffect(effect, flg, false);
    }
    public GameObject GetDebuffEffect()
    {
        return debuffEffect;
    }

    //デバフストック
    public bool SetDebuff(int type, float limit, bool isSendRPC = true)
    {
        bool isSet = false;
        if (debuffDic.ContainsKey(type))
        {
            if (debuffDic[type] <= 0)
            {
                debuffDic[type] = limit;
                isSet = true;
            }
        }
        else
        {
            debuffDic.Add(type, limit);
            isSet = true;
        }
        return isSet;
    }

    //バフコルーチンストック
    private void SetBuffCoroutine(string funcName, Coroutine cor, GameObject effect = null)
    {
        BuffInfo buff = new BuffInfo();
        buff.cor = cor;
        buff.effect = effect;

        if (buffCoroutineList.ContainsKey(funcName))
        {
            buffCoroutineList[funcName] = buff;
        }
        else
        {
            buffCoroutineList.Add(funcName, buff);
        }
    }
    //バフコルーチン停止
    private void StopBuffCoroutine(string funcName, bool isStopEffect = true)
    {
        if (!buffCoroutineList.ContainsKey(funcName)) return;
        BuffInfo buff = buffCoroutineList[funcName];
        if (buff == null) return;
        if (buff.cor != null) StopCoroutine(buff.cor);
        if (isStopEffect && buff.effect != null) SwitchEffect(buff.effect, false, false);
        buffCoroutineList.Remove(funcName);
    }
    //バフコルーチン全停止
    private void StopAllBuffCoroutine()
    {
        List<string> keys = new List<string>(buffCoroutineList.Keys);
        foreach (string funcName in keys)
        {
            StopBuffCoroutine(funcName);
        }
    }

    public void ResetTargetNpcStatus()
    {
        if (defChange != null) StopCoroutine(defChange);
        //attackChange;
        //spChange;
        StopAllBuffCoroutine();
        interfareMoveTime = 0;
        nowSpeedRate = 1;
        runSpeed = defaultRunSpeed;
        jumpSpeed = defaultJumpSpeed;
        boostSpeed = defaultBoostSpeed;
    }

    public void SetWinMark(int winCount, int loseCount)
    {
        //自分の勝マーク
        bool markFlg = true;
        int count = 1;
        foreach (GameObject obj in winCountMineList)
        {
            if (winCount < count) markFlg = false;
            if (obj != null) obj.SetActive(markFlg);
            count++;
        }

        //敵の勝マーク
        markFlg = true;
        count = 1;
        foreach (GameObject obj in winCountEnemyList)
        {
            if (loseCount < count) markFlg = false;
            if (obj != null) obj.SetActive(markFlg);
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

    public void SetBattleLog(int logType, float damage, string name, bool isSlipDamage = false)
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

    private void PushBattleLog(int logType, float damage, string name, bool console = false)
    {
        if (!isDispBattleLog) return;
        if (!photonView.isMine || isNpc) return;
        if (name == "" || damage <= 0) return;

        //バトルログ
        battleLogNo[logType]++;
        name = name.Replace("(Clone)", "");
        string logName = name;
        if (logName.Length > 10) logName = logName.Substring(0, 10);
        string text = "[" + battleLogNo[logType] + "]" + logName + " > " + System.Math.Round(damage, 2).ToString();

        if (logBattleQueue[logType].Count >= BATTLE_LOG_COUNT) logBattleQueue[logType].Dequeue();
        logBattleQueue[logType].Enqueue(text);
        if (console) MyDebug.Instance.AdminLog(logType, text);

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

class BuffInfo
{
    public Coroutine cor;
    public GameObject effect;
}

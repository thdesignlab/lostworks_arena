using UnityEngine;
using System.Collections;
using UnityEngine.UI;

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
    private int bonusRecoverSpRate = 2; //移動していない場合の追加SP回復量
    [SerializeField]
    private int bonusRecoverSpTime = 3; //ボーナスを受け取るのに必要な時間

    public int boostCost = 25;   //ブースト時消費SP

    //private Transform statusCanvas;
    //private Text hpText;

    //無敵判定
    private float leftInvincibleTime = 0;
    public float invincibleTime = 0;
    [SerializeField]
    private GameObject shield;
    private float leftShieldTIme = 0;
    private float shieldTime = 0.5f;

    //HP/SPゲージ
    private Slider hpBarMine;
    private Slider spBarMine;
    private Slider hpBarEnemy;
    private Slider spBarEnemy;

    private Image hpBarMineImage;
    private Image hpBarEnemyImage;
    private Color defaultHpColor = Color.green;
    private Color hitHpColor = Color.red;
    private int totalDamage = 0;

    //private CameraController camCtrl;
    private bool isLocked = false;
    private bool isNpc = false;

    //パラメータ変更用
    private float defaultRunSpeed;
    private float defaultJumpSpeed;
    private float defaultBoostSpeed;
    private float defaultTurnSpeed;
    private float defaultBoostTurnSpeed;
    private float defaultInvincibleTime;
    private int defaultRecoverSp;

    private GameController gameCtrl;

    void Awake()
    {
        isNpc = GetComponent<PlayerSetting>().isNpc;

        gameCtrl = GameObject.Find("GameController").GetComponent<GameController>();

        //ステータス構造
        string screenStatus = Common.CO.SCREEN_CANVAS + Common.CO.SCREEN_STATUS;

        //HPSPバー
        hpBarMine = GameObject.Find(screenStatus+"HpBarMine/HP").GetComponent<Slider>();
        spBarMine = GameObject.Find(screenStatus + "SpBarMine/SP").GetComponent<Slider>();
        hpBarEnemy = GameObject.Find(screenStatus + "HpBarEnemy/HP").GetComponent<Slider>();
        //spBarEnemy = GameObject.Find(screenStatus+"SpBarEnemy/SP").GetComponent<Slider>();

        //HPバーイメージ
        hpBarMineImage = GameObject.Find(screenStatus + "HpBarMine/HP/Fill Area/Fill").GetComponent<Image>();
        hpBarEnemyImage = GameObject.Find(screenStatus + "HpBarEnemy/HP/Fill Area/Fill").GetComponent<Image>();

        //キャラ付きキャンバス
        //statusCanvas = transform.FindChild("StatusCanvas");
        //hpText = statusCanvas.FindChild("HP").GetComponent<Text>();

        //キャラカメラ
        //camCtrl = Camera.main.gameObject.GetComponent<CameraController>();

        Init();

        //初期値保管
        defaultRunSpeed = runSpeed;
        defaultJumpSpeed = jumpSpeed;
        defaultBoostSpeed = boostSpeed;
        defaultTurnSpeed = turnSpeed;
        defaultBoostTurnSpeed = boostTurnSpeed;
        defaultInvincibleTime = invincibleTime;
        defaultRecoverSp = recoverSp;
    }

    void Start()
    {
        StartCoroutine(DamageSync());
        StartCoroutine(RecoverSp());
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
    }

    public void Init()
    {
        SetHp(maxHp);
        SetSp(maxSp);
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

    public void AddDamage(int damage)
    {
        if (!gameCtrl.isGameStart) return;

        if (leftInvincibleTime > 0)
        {
            if (shield != null)
            {
                photonView.RPC("OpenShieldRPC", PhotonTargets.All, shieldTime);
            }
            return;
        }
        totalDamage += damage;
        if (nowHp - totalDamage <= 0)
        {
            SetHp(0);
        }

        ////カメラ振動
        //if (photonView.isMine && camCtrl != null)
        //{
        //    //camCtrl.Shake();
        //}
    }

    [PunRPC]
    private void OpenShieldRPC(float time)
    {
        if (leftShieldTIme > 0)
        {
            if (leftShieldTIme < time)
            {
                leftShieldTIme = time;
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
        leftShieldTIme = time;
        shield.SetActive(true);
        for (;;)
        {
            yield return null;
            leftShieldTIme -= Time.deltaTime;
            if (leftShieldTIme <= 0) break;
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
            if (diff < 0) image.color = hitHpColor;
            yield return null;
            image.color = defaultHpColor;
        }
    }

    public void UseSp(int sp)
    {
        nowSp -= sp;
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
        float interval = 0.1f;
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
        //statusCanvas.rotation = Camera.main.transform.rotation;

        if (photonView.isMine)
        {
            if (isDead)
            {
                //戦闘不能
                //transform.DetachChildren();
                if (!isNpc) { 
                    Camera.main.transform.parent = null;
                }
                GetComponent<ObjectController>().DestoryObject();
            }
            if (transform.position.y < -10)
            {
                //エリアアウト
                AddDamage(11);
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

    //##### パラメータ変更系 #####

    //最大HP(NPC用)
    public void ReplaceMaxHp(float rate)
    {
        maxHp = (int)Mathf.Ceil(maxHp * rate);
        SetHp(maxHp);
    }

    //SP回復量(自分専用)
    public void AccelerateRecoverSp(float rate, float limit, GameObject effect = null)
    {
        StartCoroutine(CheckAccelerateRecoverSp(rate, limit, effect));
    }
    IEnumerator CheckAccelerateRecoverSp(float rate, float limit, GameObject effect = null)
    {
        recoverSp = (int)(recoverSp * rate);
        if (effect != null) effect.SetActive(true);
        yield return new WaitForSeconds(limit);
        if (effect != null) effect.SetActive(false);
        recoverSp = defaultRecoverSp;
    }

    //移動速度アップ・ダウン(負の効果優先)
    private float nowSpeedRate = 1;
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
        if (effect != null) effect.SetActive(true);

        for (;;)
        {
            yield return null;
            limit -= Time.deltaTime;
            if (limit <= 0) break;
            if (nowSpeedRate != rate) break;
        }

        if (effect != null) effect.SetActive(false);
        if (interfareMoveTime <= 0 && nowSpeedRate == rate)
        {
            runSpeed = defaultRunSpeed;
            jumpSpeed = defaultJumpSpeed;
            boostSpeed = defaultBoostSpeed;
            nowSpeedRate = 1;
        }
    }

    //移動制限
    private float interfareMoveTime = 0;
    public void InterfareMove(float limit, GameObject effect = null, bool isSendRpc = true)
    {
        if (photonView.isMine)
        {
            if (interfareMoveTime > 0)
            {
                if (interfareMoveTime < limit)
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
                object[] args = new object[] { limit, effect };
                photonView.RPC("InterfareMoveRPC", PhotonTargets.Others, args);
            }
        }
    }

    [PunRPC]
    public void InterfareMoveRPC(float limit, GameObject effect = null)
    {
        InterfareMove(limit, effect, false);
    }

    IEnumerator CheckInterfareMove(float limit, GameObject effect = null)
    {
        interfareMoveTime = limit;
        runSpeed = 0;
        jumpSpeed = 0;
        boostSpeed = 0;
        if (effect != null) effect.SetActive(true);
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
        if (effect != null) effect.SetActive(false);
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
        invincibleTime *= rate;
        if (effect != null) effect.SetActive(true);
        yield return new WaitForSeconds(limit);
        if (effect != null) effect.SetActive(false);
        invincibleTime = defaultInvincibleTime;
    }
}

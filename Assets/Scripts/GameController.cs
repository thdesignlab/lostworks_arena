using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class GameController : SingletonMonoBehaviour<GameController>
{
    [SerializeField]
    private GameObject stageStructure;
    private Text messageText;
    private int readyTime = 3;
    private int needPlayerCount = 2;

    private Text textUp;
    private Text textCenter;
    private Image imageCenter;
    private Text textLine;
    private Color colorLine = Color.white;
    private Color colorWin = Color.red;
    private Color colorLose = Color.blue;
    private Color colorReady = Color.yellow;
    private Color colorWait = new Color(0, 255, 255);
    private float baseAlpha = 0.6f;

    private Transform myTran;
    private PlayerStatus myStatus;
    private Transform targetTran;
    private Transform npcTran;
    [HideInInspector]
    public bool isPause = false;

    private bool isContinue = false;
    private bool isWin = false;
    [HideInInspector]
    public bool isGameReady = false;
    [HideInInspector]
    public bool isGameStart = false;
    [HideInInspector]
    public bool isGameEnd = false;
    private List<PlayerStatus> playerStatuses = new List<PlayerStatus>();
    private PlayerSetting playerSetting;
    private SpriteStudioController spriteStudioCtrl;
    private Script_SpriteStudio_Root scriptRoot;

    const string MESSAGE_WAITING = "Player Waiting...";
    const string MESSAGE_CUSTOMIZE = "Customizing...";
    const string MESSAGE_READY = "Ready";
    const string MESSAGE_START = "Go";
    //const string MESSAGE_WIN = "Win";
    //const string MESSAGE_LOSE = "Lose";
    const string MESSAGE_LEVEL_SELECT = "Mode Selecting...";
    const string MESSAGE_STAGE_READY = "Stage";
    const string MESSAGE_ROUND_READY = "Round";
    const string MESSAGE_MISSION_CLEAR = "Mission Clear!!";
    const string MESSAGE_STAGE_NEXT = "Next";
    //const string MESSAGE_GAME_OVER = "GameOver";

    private Dictionary<string, string[]> spriteTexts = new Dictionary<string, string[]>()
    {
        { MESSAGE_READY, new string[] { "ReadyGo", "ReadyGo_0" } },
        { MESSAGE_START, new string[] { "ReadyGo", "ReadyGo_1" } },
    };

    [HideInInspector]
    public int gameMode = -1;
    public const int GAME_MODE_MISSION = 1;
    public const int GAME_MODE_PLACTICE = 2;
    public const int GAME_MODE_VS = 3;
    [HideInInspector]
    public int stageNo = -1;
    [HideInInspector]
    public int stageLevel = -1;
    [HideInInspector]
    public int npcNo = -1;
    [SerializeField]
    private GameObject levelSelectCanvas;
    [SerializeField]
    private GameObject levelSelectButton;

    private int winCount = 0;
    private int loseCount = 0;

    //バトルログ
    [SerializeField]
    private GameObject resultCanvas;
    private Text resultTextMine;
    private Text resultTextEnemy;
    private Dictionary<string, int> damageSourceMine = new Dictionary<string, int>();
    private Dictionary<string, int> damageSourceEnemy = new Dictionary<string, int>();
    private bool isResultCheck = false;
    
    protected override void Awake()
    {
        isDontDestroyOnLoad = false;
        base.Awake();

        Init();
        CheckMode();
        SpawnMyPlayerEverywhere();
    }

    void Start()
    {
        StartCoroutine(ChceckGame());
    }

    private void Init()
    {
        spriteStudioCtrl = GameObject.Find("SpriteStudioController").GetComponent<SpriteStudioController>();
    }

    private void SetCanvasInfo()
    {
        //キャンバス情報
        Transform screenTran = Camera.main.transform.FindChild(Common.CO.SCREEN_CANVAS);
        textUp = screenTran.FindChild(Common.CO.TEXT_UP).GetComponent<Text>();
        textCenter = screenTran.FindChild(Common.CO.TEXT_CENTER).GetComponent<Text>();
        textLine = screenTran.FindChild(Common.CO.TEXT_LINE).GetComponent<Text>();
        SetTextUp();
        SetTextCenter();
    }

    private void SetTextUp(string text = "", Color color = default(Color), float fadeout = 0)
    {
        SetText(textUp, text, color, fadeout);
    }
    private void SetTextCenter(string text = "", Color color = default(Color), float fadeout = 0)
    {
        SetText(textCenter, text, color, fadeout);
    }
    private void SetTextLine(string text = "", Color color = default(Color), float fadeout = 0)
    {
        SetText(textLine, text, color, fadeout, true);
    }
    private void ClearText(Text textObj, bool isLineText = false)
    {
        SetText(textObj, "", default(Color), 0, isLineText);
    }
    private void FadeOutText(Text textObj, bool isLineText = false)
    {
        SetText(textObj, "", default(Color), 0.5f, isLineText);
    }
    private void SetText(Text textObj, string text, Color color = default(Color), float fadeout = 0, bool isLineText = false)
    {
        //Debug.Log(textObj.name+" >> "+text);
        if (textObj == null) return;
        Image textImage = textObj.transform.GetComponentInChildren<Image>();
        if (text == "")
        {
            //Line削除
            if (isLineText)
            {
                scriptRoot = spriteStudioCtrl.TextLine(textObj.gameObject);
                if (scriptRoot != null)
                {
                    //アニメーション
                    spriteStudioCtrl.Stop(scriptRoot);
                }
            }

            //テキスト削除
            scriptRoot = spriteStudioCtrl.DispMessage(textObj.gameObject, textObj.text);
            if (scriptRoot != null)
            {
                //アニメーション
                spriteStudioCtrl.Stop(scriptRoot);
            }
            else
            {
                GameObject text3d = (GameObject)Resources.Load(Common.Func.GetResourceAnimation3D(textObj.text));
                if (text3d != null)
                {
                    //3DText
                    Reset3DText(textObj.transform);
                }
                else
                {
                    //Text・Image
                    if (fadeout > 0)
                    {
                        ScreenManager.Instance.TextFadeOut(textObj);
                        ScreenManager.Instance.ImageFadeOut(textImage);
                    }
                    else
                    {
                        textObj.enabled = false;
                        textImage.enabled = false;
                    }
                }
            }
            textObj.text = "";
        }
        else
        {
            ClearText(textObj, isLineText);

            //Line表示
            if (isLineText)
            {
                scriptRoot = spriteStudioCtrl.TextLine(textObj.gameObject);
                if (scriptRoot != null)
                {
                    //アニメーション
                    spriteStudioCtrl.Play(scriptRoot);
                }
            }

            //テキスト表示
            textObj.enabled = false;
            textObj.text = text;
            scriptRoot = spriteStudioCtrl.DispMessage(textObj.gameObject, textObj.text);
            if (scriptRoot != null)
            {
                //アニメーション
                spriteStudioCtrl.Play(scriptRoot);
            }
            else
            {
                GameObject text3d = (GameObject)Resources.Load(Common.Func.GetResourceAnimation3D(textObj.text));
                if (text3d != null)
                {
                    //3DText
                    Set3DText(text3d, textObj.transform);
                }
                else
                {
                    Sprite img = GetSpriteText(text);
                    if (img != null)
                    {
                        textImage.sprite = img;
                        textImage.color = Vector4.one;
                        textImage.enabled = true;
                    }
                    else
                    {
                        //Text
                        if (color != default(Color)) color = textObj.color;
                        textObj.color = new Color(color.r, color.g, color.b, baseAlpha);
                        textObj.enabled = true;
                    }
                }
            }

            if (fadeout > 0) StartCoroutine(MessageDelayDelete(textObj, fadeout, isLineText));
        }
    }

    private void Set3DText(GameObject obj, Transform textTran)
    {
        GameObject ob = (GameObject)Instantiate(obj, textTran.position, Camera.main.transform.rotation * obj.transform.rotation);
        ob.transform.parent = textTran;
    }
    private void Reset3DText(Transform textTran)
    {
        //GameObject[] texts = GameObject.FindGameObjectsWithTag("3DText");
        foreach (Transform text in textTran)
        {
            if (text.tag == "3DText") Destroy(text.gameObject);
        }
    }

    private Sprite GetSpriteText(string text)
    {
        Sprite img = null;
        if (spriteTexts.ContainsKey(text))
        {
            string[] texts = spriteTexts[text];
            Sprite[] sprites = Resources.LoadAll<Sprite>(Common.Func.GetResourceSprite(texts[0]));
            img = System.Array.Find<Sprite>(sprites, (sprite) => sprite.name.Equals(texts[1]));
        }
        return img;
    }

    IEnumerator MessageDelayDelete(Text textObj, float fadeout, bool isLineText = false)
    {
        string txt = textObj.text;
        for (;;)
        {
            fadeout -= Time.deltaTime;
            if (fadeout < 0) break;
            if (txt != textObj.text) yield break;
            yield return null;
        }
        FadeOutText(textObj, isLineText);
    }

    public void ResetGame()
    {
        photonView.RPC("ResetGameRPC", PhotonTargets.All);
    }
    [PunRPC]
    private void ResetGameRPC()
    {
        isWin = false;
        isGameReady = false;
        isGameStart = false;
        isGameEnd = false;
        if (PhotonNetwork.player == PhotonNetwork.masterClient)
        {
            StageObjReset();
        }
    }

    private void StageObjReset()
    {
        //ステージオブジェ破壊
        GameObject[] objes = GameObject.FindGameObjectsWithTag(Common.CO.TAG_STRUCTURE);
        foreach (GameObject obj in objes)
        {
            ObjectController objCtrl = obj.GetComponent<ObjectController>();
            if (objCtrl != null)
            {
                objCtrl.DestoryObject();
            }
        }

        //Debug.Log(PhotonNetwork.player +" >> "+ PhotonNetwork.masterClient);
        if (PhotonNetwork.player == PhotonNetwork.masterClient)
        {
            //ステージオブジェ生成
            GameObject spawns = GameObject.Find("StructureSpawns");
            if (!spawns) return;
            foreach (Transform spawnTran in spawns.transform)
            {
                PhotonNetwork.Instantiate(Common.Func.GetResourceStructure(stageStructure.name), spawnTran.position, spawnTran.rotation, 0);
            }
        }
    }

    IEnumerator ChceckGame()
    {
        for (;;)
        {
            if (isGameEnd)
            {
                //Debug.Log("GameEnd");
                yield return new WaitForSeconds(3.0f);
                if (isContinue) continue;

                //結果表示
                bool isPassResult = OpenResult();

                if (gameMode == GAME_MODE_MISSION)
                {
                    //ミッションモード

                    if (isResultCheck || isPassResult)
                    {
                        bool isStageSetting = false;
                        if (isWin)
                        {
                            //勝利
                            if (winCount >= 3)
                            {
                                //ステージクリア
                                //NextStage
                                if (SetNextStage())
                                {
                                    isStageSetting = true;
                                    SetTextLine(MESSAGE_STAGE_NEXT + " " + MESSAGE_STAGE_READY + "...", colorLine);
                                }
                                else
                                {
                                    //全ステージクリア
                                    SetTextCenter(MESSAGE_MISSION_CLEAR, colorWait);
                                    yield return new WaitForSeconds(1.0f);

                                    //ダイアログ
                                    string text = "Next level\n「"+Common.Mission.GetLevelName(stageLevel+1) +"」";
                                    List<string> buttons = new List<string>() { "Next", "Title" };
                                    List<UnityAction> actions = new List<UnityAction>() {
                                        () => OnNextLevel(), () => GoToTitle()
                                    };
                                    DialogController.OpenDialog(text, buttons, actions);
                                }
                            }
                            else
                            {
                                //NextRound
                                isStageSetting = true;
                                SetTextLine(MESSAGE_STAGE_NEXT + " " + MESSAGE_ROUND_READY + "...", colorLine);
                            }
                        }
                        else
                        {
                            //敗北
                            if (loseCount >= 3)
                            {
                                //ゲームオーバー
                                SetTextCenter(spriteStudioCtrl.MESSAGE_GAME_OVER, colorWin);
                                yield return new WaitForSeconds(1.0f);

                                //ダイアログ
                                string text = "広告やで(｀・д・´)";
                                List<string> buttons = new List<string>() { "Continue", "Title" };
                                List<UnityAction> actions = new List<UnityAction>() {
                                        () => ContinueMission(true), () => GoToTitle()
                                    };
                                DialogController.OpenDialog(text, buttons, actions);
                                //自動でタイトルへ遷移
                                float waitTime = 10;
                                for (;;)
                                {
                                    if (isContinue) break;
                                    waitTime -= Time.deltaTime;
                                    if (waitTime < 0)
                                    {
                                        GoToTitle();
                                        yield break;
                                    }
                                    yield return null;
                                }
                            }
                            else
                            {
                                isStageSetting = true;
                                SetTextLine(MESSAGE_STAGE_NEXT + " " + MESSAGE_ROUND_READY + "...", colorLine);
                            }
                        }

                        //ステージセッティング
                        if (isStageSetting)
                        {
                            yield return new WaitForSeconds(3.0f);
                            StageSetting();
                        }
                    }

                }
                else
                {
                    if (isResultCheck || isPassResult)
                    {
                        //レート変化ダイアログ表示
                        string rateText = "Rate : 1050(+50)仮\n\n";
                        int waitTime = 10;
                        List<UnityAction> actions = new List<UnityAction>();
                        List<string> buttons = new List<string>() { "Continue", "Title" };
                        if (isWin)
                        {
                            //続けるorタイトルへ戻る
                            actions = new List<UnityAction>();
                            actions.Add(() => ContinueVs());
                            actions.Add(() => GoToTitle());
                            buttons = new List<string>() { "Continue", "Title" };
                        }
                        else
                        {
                            //タイトルへ戻るだけ
                            waitTime = 5;
                            actions = new List<UnityAction>() { () => GoToTitle() };
                            buttons = new List<string>() { "Title" };
                        }
                        DialogController.OpenDialog(rateText, buttons, actions);
                        Text DialogText = DialogController.GetDialogText();

                        //自動でタイトルへ遷移
                        for (int i = waitTime; i > 0; i--)
                        {
                            if (isContinue) break;
                            string text = rateText + "後" + i + "秒でTitleへもどります";
                            DialogText.text = text;
                            yield return new WaitForSeconds(1.0f);
                        }
                        if (!isContinue)
                        {
                            GoToTitle();
                            yield break;
                        }
                    }
                }
                yield return new WaitForSeconds(1.0f);
                continue;
            }

            if (isGameStart)
            {
                //Debug.Log("Playing");
                //対戦中
                foreach (PlayerStatus playerStatus in playerStatuses)
                {
                    if (playerStatus == null)
                    {
                        //対戦終了
                        GameEnd();
                    }
                }
            }
            else
            {
                //待機中
                if (playerSetting.IsCustomEnd())
                {
                    //装備設定終了
                    GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
                    if (players.Length == needPlayerCount)
                    {
                        //対戦準備
                        playerStatuses = new List<PlayerStatus>();
                        foreach (GameObject player in players)
                        {
                            PlayerSetting setting = player.GetComponent<PlayerSetting>();
                            if (setting == null)
                            {
                                DestroyPlayer(player);
                                continue;
                            }
                            if (!setting.IsCustomEnd())
                            {
                                //SetWaitMessage(MESSAGE_CUSTOMIZE + setting.GetLeftCustomTime().ToString());
                                SetTextUp(MESSAGE_CUSTOMIZE + setting.GetLeftCustomTime().ToString(), colorWait);
                                break;
                            }

                            //Debug.Log(player.name);
                            PlayerStatus ps = player.GetComponent<PlayerStatus>();
                            if (ps == null)
                            {
                                DestroyPlayer(player);
                                continue;
                            }
                            playerStatuses.Add(ps);
                        }

                        //Debug.Log("playerStatuses: " + playerStatuses.Count.ToString());
                        if (playerStatuses.Count == needPlayerCount)
                        {
                            //バトル準備完了
                            GameReady();

                            SetTextUp();
                            SetTextCenter();
                            if (gameMode == GAME_MODE_MISSION)
                            {
                                int round = winCount + loseCount + 1;
                                if (round == 1)
                                {
                                    //ステージ文字
                                    SetTextLine(MESSAGE_STAGE_READY + stageNo.ToString(), colorLine);
                                    yield return new WaitForSeconds(2);
                                }
                                //ラウンド文字
                                SetTextLine(MESSAGE_ROUND_READY + round.ToString(), colorLine, 2.0f);
                                yield return new WaitForSeconds(2);
                            }

                            //カウントダウン
                            SetTextCenter(MESSAGE_READY, colorReady);
                            yield return new WaitForSeconds(2);
                            for (int i = readyTime; i > 0; i--)
                            {
                                SetTextCenter(i.ToString(), colorReady);
                                yield return new WaitForSeconds(1);
                            }
                            SetTextCenter(MESSAGE_START, colorReady, 3);

                            //対戦スタート
                            GameStart();
                        }
                    }
                    else
                    {
                        if (gameMode == GAME_MODE_MISSION)
                        {
                            //ミッションモード
                            if (stageLevel > 0)
                            {
                                //NPC召喚
                                StageSetting();
                            }
                            else
                            {
                                //レベル未選択
                                SetTextUp(MESSAGE_LEVEL_SELECT, colorWait);
                            }
                        }
                        else
                        {
                            //対戦モード
                            if (PhotonNetwork.countOfPlayersInRooms < needPlayerCount)
                            {
                                //SetWaitMessage(MESSAGE_WAITING);
                                SetTextUp(MESSAGE_WAITING, colorWait);
                            }
                        }
                    }
                }
                else
                {
                    //装備設定中
                    //SetWaitMessage(MESSAGE_CUSTOMIZE + playerSetting.GetLeftCustomTime().ToString());
                    SetTextUp(MESSAGE_CUSTOMIZE + playerSetting.GetLeftCustomTime().ToString(), colorWait);
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private bool CheckPlayer()
    {
        if (playerStatuses.Count == needPlayerCount) return false;
        return true;
    }

    //プレイヤー作成
    public void SpawnMyPlayerEverywhere()
    {
        CleanNpc();

        //ベースボディ生成
        PlayerSpawn();
    }

    public void CleanPlayer()
    {
        if (myStatus == null) return;
        myStatus.AddDamage(99999);
    }

    public void CleanNpc()
    {
        photonView.RPC("CleanNpcRPC", PhotonTargets.All);
    }

    [PunRPC]
    private void CleanNpcRPC()
    {
        Transform npc = GetNpcTran();
        if (npc != null)
        {
            npc.GetComponent<PlayerStatus>().AddDamage(99999);
        }
    }

    public void SetMyTran(Transform tran)
    {
        myTran = tran;
        WeaponStore.Instance.SetMyTran();
    }
    public Transform GetMyTran()
    {
        return myTran;
    }

    public void SetNpcTran(Transform tran)
    {
        npcTran = tran;
    }

    public Transform GetNpcTran()
    {
        return npcTran;
    }

    public void SetTarget(Transform target)
    {
        targetTran = target;
    }
    public Transform GetTarget()
    {
        return targetTran;
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void GoToTitle()
    {
        PhotonNetwork.LeaveRoom();
        ScreenManager.Instance.Load(Common.CO.SCENE_TITLE, DialogController.MESSAGE_LOADING);
    }

    private GameObject SpawnProcess(string name, int groupId = 0)
    {
        Transform spawnPoint = GetSpawnPoint();
        Vector3 pos = new Vector3(0, 15, 0);
        Quaternion qua = Quaternion.identity;
        if (spawnPoint != null)
        {
            pos = spawnPoint.position;
            qua = spawnPoint.rotation;
        }
        return PhotonNetwork.Instantiate(name, pos, qua, groupId);
    }

    private Transform GetSpawnPoint()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        if (spawnPoints.Length <= 0) return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject spawnObj = spawnPoints[0];
        if (player != null)
        {
            Transform playerTran = player.transform;
            float preDistance = -1;
            foreach (GameObject spawnPoint in spawnPoints)
            {
                float distance = Vector3.Distance(playerTran.position, spawnPoint.transform.position);
                if (preDistance < 0 || preDistance < distance)
                {
                    spawnObj = spawnPoint;
                    preDistance = distance;
                }
            }
        }
        //int index = PhotonNetwork.countOfPlayersInRooms;
        return spawnObj.transform;
    }

    private void GameReady()
    {
        isGameReady = true;
        foreach (PlayerStatus playerStatus in playerStatuses)
        {
            playerStatus.Init();
        }
        foreach (GameObject weapon in GameObject.FindGameObjectsWithTag(Common.CO.TAG_WEAPON))
        {
            WeaponController weponCtrl = weapon.GetComponent<WeaponController>();
            if (weponCtrl == null) continue;
            weponCtrl.SetEnable(true);
        }

        if (myStatus.voiceManager != null) myStatus.voiceManager.BattleStart();
    }

    private void GameStart()
    {
        Debug.Log("*** GameStart ***");
        foreach (PlayerStatus playerStatus in playerStatuses)
        {
            playerStatus.Init();
        }
        isContinue = false;
        isGameReady = false;
        isGameStart = true;
    }

    private void GameEnd()
    {
        Debug.Log("*** GameEnd ***");
        if (targetTran == null)
        {
            //勝利
            winCount++;
            isWin = true;
            SetTextCenter(spriteStudioCtrl.ANIMATION_TEXT_WIN, colorWin);
            if (myStatus.voiceManager != null) myStatus.voiceManager.Win();
        }
        else
        {
            //敗北
            loseCount++;
            SetTextCenter(spriteStudioCtrl.ANIMATION_TEXT_LOSE, colorLose);
        }
        isGameEnd = true;
        myStatus.SetWinMark(winCount, loseCount);
    }

    public static string OnUGuiButton(Vector3 _scrPos)
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = _scrPos;
        List<RaycastResult> result = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, result);
        Button btn = null;
        foreach (RaycastResult hit in result)
        {
            btn = hit.gameObject.GetComponent<Button>();
            if (btn != null) break;
        }
        if (btn == null) return null;
        return btn.name;
    }
    public static bool IsUGUIHit(Vector3 _scrPos)
    {
        if (OnUGuiButton(_scrPos) == null)
        {
            return false;
        }
        return true;
    }

    private void DestroyPlayer(GameObject player)
    {
        PhotonView photonView = player.GetComponent<PhotonView>();
        if (photonView == null)
        {
            Destroy(player);
        }
        else
        {
            PhotonNetwork.Destroy(player);
        }
    }

    private void CheckMode()
    {
        if (PhotonNetwork.offlineMode)
        {
            //ミッションモード
            gameMode = GAME_MODE_MISSION;

            //解放ミッションチェック
            int maxLevel = UserManager.userOpenMissions[Common.PP.MISSION_LEVEL];

            //レベル設定ダイアログ
            RectTransform content = levelSelectCanvas.transform.FindChild("ScrollView/Viewport/Content").GetComponent<RectTransform>();
            content.sizeDelta = new Vector2(0, maxLevel * 200);
            for (int i = maxLevel; i > 0; i--)
            {
                int setLevel = i;
                GameObject btn = (GameObject)Instantiate(levelSelectButton, Vector3.zero, Quaternion.identity);
                btn.transform.SetParent(content, false);
                btn.transform.GetComponentInChildren<Text>().text = Common.Mission.GetLevelName(setLevel);
                btn.transform.GetComponent<Button>().onClick.AddListener(() => OnSelectLevel(setLevel));
            }
            levelSelectCanvas.SetActive(true);
        }
        else
        {
            //対戦モード
            gameMode = GAME_MODE_VS;
        }

        //BGM再生
        PlayStageBgm();
    }


    //##### MissionMode #####

    public void OnSelectLevel(int level)
    {
        stageLevel = level;
        levelSelectCanvas.SetActive(false);
    }

    //次のレベル挑戦
    public void OnNextLevel()
    {
        stageLevel++;
        stageNo = 1;
        ContinueMission();
    }

    //現在のステージNo取得
    public int GetStageNo()
    {
        return stageNo;
    }

    //次のステージNo設定
    private bool SetNextStage()
    {
        //次のステージOPEN
        UserManager.OpenNextMission(stageLevel, stageNo);

        //次のステージチェック
        if (!Common.Mission.stageNpcNoDic.ContainsKey(stageNo + 1)) return false;
        stageNo++;
        winCount = 0;
        loseCount = 0;
        ResetWinMark();

        return true;
    }

    //勝敗リセット
    private void ResetWinMark()
    {
        winCount = 0;
        loseCount = 0;
        myStatus.ResetWinMark();
    }

    //コンティニュー
    public void ContinueMission(bool isAdPlay = false)
    {
        if (isAdPlay)
        {
            UnityAds.Instance.Play(() => ContinueMission());
        }
        else
        {
            isContinue = true;
            winCount = 0;
            loseCount = 0;
            ResetWinMark();
            StageSetting();
        }
    }

    //
    public void ContinueVs()
    {
        isContinue = true;
    }

    //結果ダイアログ表示
    private bool OpenResult()
    {
        if (!MyDebug.Instance.isDebugMode && !UserManager.isAdmin) return true;
        if (resultCanvas.GetActive()) return false;

        string resultMine = CreateDamageSourceText(PlayerStatus.BATTLE_LOG_ATTACK);
        string resultEnemy = CreateDamageSourceText(PlayerStatus.BATTLE_LOG_DAMAGE);
        if (resultMine == "" && resultEnemy == "") return true;

        resultCanvas.SetActive(true);
        Transform resultTran = resultCanvas.transform.FindChild("DamageSource/Result");
        resultTran.FindChild("Mine").GetComponent<Text>().text = resultMine;
        resultTran.FindChild("Enemy").GetComponent<Text>().text = resultEnemy;
        isResultCheck = false;
        return false;
    }

    //結果確認ボタン押下
    public void OnResultCheck()
    {
        resultCanvas.SetActive(false);
        damageSourceMine = new Dictionary<string, int>();
        damageSourceEnemy = new Dictionary<string, int>();
        isResultCheck = true;
    }

    //ステージのNPCなどの準備
    private void StageSetting()
    {
        if (myTran == null)
        {
            Destroy(Camera.main.gameObject);
            Destroy(targetTran.gameObject);
            Init();
            PlayerSpawn();
            if (targetTran != null)
            {
                targetTran.GetComponent<PlayerStatus>().Init();
            }
        }
        else
        {
            NpcSpawn(stageNo);
        }

        isResultCheck = false;
        resultCanvas.SetActive(false);
        damageSourceMine = new Dictionary<string, int>();
        damageSourceEnemy = new Dictionary<string, int>();
    }

    private void PlayerSpawn()
    {
        //スプライトスタジオキャッシュリセット
        spriteStudioCtrl.ResetSprite();

        //ベースボディ生成
        string charaName = Common.CO.CHARACTER_BASE;
        GameObject player = SpawnProcess(charaName);
        playerSetting = player.GetComponent<PlayerSetting>();
        myStatus = player.GetComponent<PlayerStatus>();
        SetCanvasInfo();
        ResetGame();
        myStatus.SetWinMark(winCount, loseCount);
    }

    //NPC生成
    public void NpcSpawn(int no = -1)
    {
        if (GameObject.FindGameObjectsWithTag("Player").Length >= 2)
        {
            CleanNpc();
        }

        ResetGame();

        //ステージNo設定
        if (no > 0) stageNo = no;
        if (stageNo <= 0) stageNo = 1;

        //BGM
        PlayStageBgm();

        //NpcNo
        npcNo = Common.Mission.stageNpcNoDic[stageNo][Common.Mission.STAGE_NPC_NAME];

        //ステージのNPC取得
        string npcName = "BaseNpc";

        GameObject npc = SpawnProcess(npcName);
        NpcController npcCtrl = npc.GetComponent<NpcController>();
        npcCtrl.SetLevel(stageLevel);
    }

    public void SetDamageSource(int logType, string name, int damage)
    {
        Dictionary<string, int> damageSource;
        if (logType == PlayerStatus.BATTLE_LOG_ATTACK)
        {
            //Debug.Log(myTran.name+" : "+name+" >> atk");
            damageSource = damageSourceMine;
        }
        else
        {
            //Debug.Log(myTran.name + " : " + name + " >> def");
            damageSource = damageSourceEnemy;
        }
        if (!damageSource.ContainsKey(name)) damageSource[name] = 0;
        damageSource[name] += damage;
    }

    private string CreateDamageSourceText(int type)
    {
        Dictionary<string, int> damageSource;
        if (type == PlayerStatus.BATTLE_LOG_ATTACK)
        {
            damageSource = damageSourceMine;
        }
        else
        {
            damageSource = damageSourceEnemy;
        }

        string text = "";
        List<string> nameList = new List<string>();
        List<int> damageList = new List<int>();
        float sumDamage = 0;
        foreach (string name in damageSource.Keys)
        {
            sumDamage += damageSource[name];
            int index = 0;
            foreach (int damage in damageList)
            {
                if (damage <= damageSource[name]) break;
                index++;
            }
            nameList.Insert(index, name);
            damageList.Insert(index, damageSource[name]);
        }

        for (int i = 0; i < nameList.Count; i++)
        {
            text += nameList[i] + " > " + damageList[i] + "(" + (int)(damageList[i] / sumDamage * 100) + "%)\n";
        }
        return text;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            if (UnityAds.Instance.IsPlaying()) return;

            //ホームボタンを押してアプリがバックグランドに移行した時
            Pause();
        }
        else
        {
            //アプリを終了しないでホーム画面からアプリを起動して復帰した時
        }
    }
    public void Pause()
    {
        if (gameMode == GAME_MODE_VS)
        {
            //一時停止禁止
            return;
        }

        isPause = true;
        DialogController.OpenDialog("一時停止中", "再開", () => ResetPause(), false);
        Time.timeScale = 0;
    }
    public void ResetPause()
    {
        isPause = false;
        Time.timeScale = 1;
    }

    private void PlayStageBgm()
    {
        int bgmNo = -1;
        if (gameMode == GAME_MODE_MISSION)
        {
            int index = stageNo;
            if (index < 1) index = 1;
            bgmNo = Common.Mission.stageNpcNoDic[index][Common.Mission.STAGE_NPC_BGM];
        }
        SoundManager.Instance.PlayBattleBgm(bgmNo);
    }
}

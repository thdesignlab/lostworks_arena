using UnityEngine;
using System;
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
    private int readyTime = 2;
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

    const float LIMIT_BATTLE_TIME = 600;
    private float battleTime = 0;
    private bool isFirstBattle = true;
    private bool isContinue = false;
    private bool isWin = false;
    [HideInInspector]
    public bool isGameReady = false;
    [HideInInspector]
    public bool isGameStart = false;
    [HideInInspector]
    public bool isGameEnd = false;
    private bool isVsStart = false;
    [HideInInspector]
    public bool isPractice = false;

    private List<PlayerStatus> playerStatuses = new List<PlayerStatus>();
    private PlayerSetting playerSetting;
    private SpriteStudioController spriteStudioCtrl;
    private Script_SpriteStudio_Root scriptRoot;

    const string MESSAGE_WAITING = "PlayerWaiting...";
    const string MESSAGE_CUSTOMIZE = "Customizing";
    const string MESSAGE_READY = "Ready";
    const string MESSAGE_START = "Go";
    //const string MESSAGE_WIN = "Win";
    //const string MESSAGE_LOSE = "Lose";
    const string MESSAGE_LEVEL_SELECT = "ModeSelecting...";
    const string MESSAGE_STAGE_READY = "Stage";
    const string MESSAGE_ROUND_READY = "Round";
    //const string MESSAGE_MISSION_CLEAR = "MissionClear";
    const string MESSAGE_STAGE_NEXT = "Next";
    //const string MESSAGE_GAME_OVER = "GameOver";

    private Dictionary<string, string[]> spriteTexts = new Dictionary<string, string[]>()
    {
        { MESSAGE_READY, new string[] { "Ready", "Ready" } },
        { MESSAGE_START, new string[] { "Go", "Go" } },
        { MESSAGE_WAITING, new string[] { "PlayerWaiting", "PlayerWaiting" } },
        { MESSAGE_LEVEL_SELECT, new string[] { "ModeSelecting", "ModeSelecting" } },
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

    const int WIN_COUNT_MAX = 2;
    private int winCount = 0;
    private int loseCount = 0;
    private int totalContinueCount = 0;
    private int continueCount = 0;

    //バトルログ
    [SerializeField]
    private GameObject resultCanvas;
    private Text resultTextMine;
    private Text resultTextEnemy;
    private Dictionary<string, int> damageSourceMine = new Dictionary<string, int>();
    private Dictionary<string, int> damageSourceEnemy = new Dictionary<string, int>();
    private bool isResultCheck = false;

    //バトル終了時獲得pt
    const float WIN_POINT_RATE = 1.0f;
    const int LOSE_POINT = 5;

    //ミッションレベル更新pt係数
    const int MISSION_POINT_PER = 100;

    protected override void Awake()
    {
        isDontDestroyOnLoad = false;
        base.Awake();

        //RoomKey取得
        ModelManager.roomData = new RoomData();
        ModelManager.roomData.room_key = PhotonNetwork.room.name;

        Init();
        CheckMode();

        ////フレームレート
        //Application.targetFrameRate = 60;
    }

    void Start()
    {
        SpawnMyPlayerEverywhere();
        StartCoroutine(ChceckGame());
    }

    void Update()
    {
        if (isGameStart)
        {
            battleTime += Time.deltaTime;
            if (gameMode == GAME_MODE_VS && !isPractice && battleTime > LIMIT_BATTLE_TIME)
            {
                if (myStatus != null) myStatus.ForceDamage(5);
            }
        }
    }

    private void Init()
    {
        if (spriteStudioCtrl == null) spriteStudioCtrl = GameObject.Find("SpriteStudioController").GetComponent<SpriteStudioController>();
        //スプライトスタジオキャッシュリセット
        spriteStudioCtrl.ResetSprite();
    }

    private void SetCanvasInfo()
    {
        //キャンバス情報
        Transform screenTran = Camera.main.transform.FindChild(Common.CO.SCREEN_CANVAS);
        if (textUp == null) textUp = screenTran.FindChild(Common.CO.TEXT_UP).GetComponent<Text>();
        if (textCenter == null) textCenter = screenTran.FindChild(Common.CO.TEXT_CENTER).GetComponent<Text>();
        if (textLine == null) textLine = screenTran.FindChild(Common.CO.TEXT_LINE).GetComponent<Text>();
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
                            if (winCount >= WIN_COUNT_MAX)
                            {
                                //ステージクリア
                                //NextStage
                                int preMissionLevel = UserManager.userOpenMissions[Common.PP.MISSION_LEVEL];
                                if (SetNextStage())
                                {
                                    isStageSetting = true;
                                    SetTextLine(MESSAGE_STAGE_NEXT + " " + MESSAGE_STAGE_READY + "...", colorLine);
                                }
                                else
                                {
                                    //全ステージクリア
                                    SetTextCenter(spriteStudioCtrl.MESSAGE_MISSION_CLEAR, colorWait);
                                    yield return new WaitForSeconds(1.0f);

                                    //ステージレベル更新時ポイント付与
                                    int missionPoint = 0; 
                                    if (UserManager.userOpenMissions[Common.PP.MISSION_LEVEL] > preMissionLevel)
                                    {
                                        //point付与
                                        missionPoint = preMissionLevel * MISSION_POINT_PER;
                                        Point.Add pointAdd = new Point.Add();
                                        pointAdd.SetApiErrorIngnore();
                                        pointAdd.Exe(missionPoint, Common.API.POINT_LOG_KIND_MISSION, stageLevel);
                                    } 

                                    //ダイアログ
                                    string text = "Next level\n「"+Common.Mission.GetLevelName(stageLevel+1) +"」";
                                    if (missionPoint > 0) text += "\n"+ missionPoint.ToString()+"pt獲得!!";
                                    List<string> buttons = new List<string>() { "Next", "Title" };
                                    List<UnityAction> actions = new List<UnityAction>() {
                                        () => OnNextLevel(), () => GoToTitle()
                                    };
                                    DialogController.OpenDialog(text, buttons, actions);
                                    for (;;)
                                    {
                                        if (isContinue) break;
                                        yield return null;
                                    }
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
                            if (loseCount >= WIN_COUNT_MAX)
                            {
                                //ゲームオーバー
                                continueCount++;
                                totalContinueCount++;
                                SetTextCenter(spriteStudioCtrl.MESSAGE_GAME_OVER, colorWin);
                                yield return new WaitForSeconds(1.0f);

                                //ダイアログ
                                string text = "コンティニューしますか？\n※動画が再生されます\n※復活時に能力が少しUPします";
                                List<string> buttons = new List<string>() { "Continue", "Titleへ" };
                                List<UnityAction> actions = new List<UnityAction>() {
                                    () => ContinueMission(true), () => GoToTitle()
                                };
                                DialogController.OpenDialog(text, buttons, actions);
                                //自動でタイトルへ遷移
                                float waitTime = 60;
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
                    //対戦モード
                    if (isResultCheck || isPassResult)
                    {
                        if (winCount >= WIN_COUNT_MAX || loseCount >= WIN_COUNT_MAX)
                        {
                            //VS終了
                            //レート変化ダイアログ表示
                            int preRate = ModelManager.battleRecord.battle_rate;
                            int waitTime = 10;
                            int battlePoint = LOSE_POINT;
                            List<UnityAction> actions = new List<UnityAction>();
                            List<string> buttons = new List<string>();
                            if (isWin)
                            {
                                //続ける
                                actions = new List<UnityAction>() { null };
                                buttons = new List<string>() { "OK" };
                            }
                            else
                            {
                                //タイトルへ戻るだけ
                                actions = new List<UnityAction>() { () => GoToTitle() };
                                buttons = new List<string>() { "Titleへ" };
                                PhotonManager.isPlayAd = true;
                            }
                            Action apiCallback = () =>
                            {
                                int diffRate = ModelManager.battleRecord.battle_rate - preRate;
                                if (diffRate > battlePoint) battlePoint = (int)(diffRate * WIN_POINT_RATE);
                                string diffRateSign = diffRate >= 0 ? "+" : "";
                                string dialogText = "Rate : " + ModelManager.battleRecord.battle_rate + "(" + diffRateSign + diffRate.ToString() + ")";
                                if (battlePoint > 0) dialogText += "\n" + battlePoint.ToString() + "pt獲得";
                                DialogController.OpenDialog(dialogText, buttons, actions);
                                if (isWin) ContinueVs();

                                if (battlePoint > 0)
                                {
                                    //point付与
                                    Point.Add pointAdd = new Point.Add();
                                    pointAdd.SetApiErrorIngnore();
                                    pointAdd.Exe(battlePoint, Common.API.POINT_LOG_KIND_BATTLE, ModelManager.battleInfo.battle_id);
                                }
                            };
                            Battle.Finish battleFinish = new Battle.Finish();
                            battleFinish.SetApiFinishCallback(apiCallback);
                            battleFinish.SetRetryCount(3);
                            battleFinish.SetApiErrorIngnore();
                            battleFinish.Exe(isWin);

                            if (isWin)
                            {
                                //勝ちプレイヤー処理
                                PhotonManager.isPlayAd = false;
                                for (;;)
                                {
                                    //負けプレイヤーが退出するのを待つ
                                    if (CheckPlayer()) yield return new WaitForSeconds(0.5f);
                                    break;
                                }
                                ResetVs();
                            }
                            else
                            {
                                //負けPlayer処理
                                //自動でタイトルへ遷移
                                string defaultText = "";
                                for (int i = waitTime; i > 0; i--)
                                {
                                    if (isContinue) break;
                                    Text dialogText = DialogController.GetDialogText();
                                    if (dialogText != null)
                                    {
                                        if (string.IsNullOrEmpty(defaultText)) defaultText = dialogText.text;
                                        dialogText.text = defaultText + "\n" + i + "秒後にTitleへ戻ります";
                                    }
                                    yield return new WaitForSeconds(1.0f);
                                }
                                GoToTitle();
                                yield break;
                            }
                        }
                        else
                        {
                            if (!CheckPlayer())
                            {
                                //相手が離脱した場合
                                winCount = WIN_COUNT_MAX;
                                myStatus.SetWinMark(winCount, loseCount);
                            }
                            else
                            {
                                //Nextバトル
                                yield return new WaitForSeconds(3.0f);
                                VsSetting();
                            }
                        }
                    }
                }
                yield return new WaitForSeconds(1.0f);
                continue;
            }

            if (isGameStart)
            {
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
                                SetTextUp(MESSAGE_CUSTOMIZE + setting.GetLeftCustomTime().ToString(), colorWait);
                                break;
                            }
                            PlayerStatus ps = player.GetComponent<PlayerStatus>();
                            if (ps == null)
                            {
                                DestroyPlayer(player);
                                continue;
                            }
                            if (!ps.isReadyBattle) continue;

                            playerStatuses.Add(ps);
                        }

                        if (playerStatuses.Count == needPlayerCount)
                        {
                            //バトル準備完了
                            GameReady();

                            SetTextUp();
                            SetTextCenter();
                            if (gameMode == GAME_MODE_MISSION || gameMode == GAME_MODE_VS)
                            {
                                int round = winCount + loseCount + 1;
                                if (round == 1)
                                {
                                    //ステージ文字
                                    string stageText = MESSAGE_STAGE_READY + stageNo.ToString();
                                    if (Common.Mission.stageNpcNoDic.Count == stageNo) stageText = "Final "+MESSAGE_STAGE_READY;
                                    SetTextLine(stageText, colorLine);
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
                            SetTextCenter(MESSAGE_START, colorReady, 2);

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
        return (PhotonNetwork.room.playerCount == needPlayerCount);
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
        isPractice = false;
        GameObject targetNpc = GameObject.FindGameObjectWithTag("Target");
        if (targetNpc != null) targetNpc.GetComponent<ObjectController>().DestoryObject(targetNpc);
        Transform npc = GetNpcTran();
        if (npc != null) StartCoroutine(CleanNpcProc(npc));
    }
    IEnumerator CleanNpcProc(Transform npc)
    {
        for (;;)
        {
            if (npc == null) break;
            npc.GetComponent<PlayerStatus>().ForceDamage(99999);
            yield return null;
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

    public void ReStart()
    {
        PhotonManager.isFirstScean = true;
        PhotonNetwork.LeaveRoom();
        ScreenManager.Instance.Load(Common.CO.SCENE_TITLE, DialogController.MESSAGE_LOADING);
    }

    public void GoToTitle()
    {
        SoundManager.Instance.StopBgm();
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
        else if (!PhotonNetwork.isMasterClient)
        {
            spawnObj = spawnPoints[1];
        }
        return spawnObj.transform;
    }

    private void GameReady()
    {
        isPractice = false;
        isGameReady = true;
        myStatus.Init();
        if (gameMode == GAME_MODE_MISSION)
        {
            npcTran.GetComponent<PlayerStatus>().Init();
        }
        else
        {
            PhotonManager.isPlayAd = true;
        }

        foreach (GameObject weapon in GameObject.FindGameObjectsWithTag(Common.CO.TAG_WEAPON))
        {
            WeaponController weponCtrl = weapon.GetComponent<WeaponController>();
            if (weponCtrl == null) continue;
            weponCtrl.SetEnable(true);
        }

        if (myStatus.voiceManager != null) myStatus.voiceManager.BattleStart();

        //BGMランダム
        if (gameMode == GAME_MODE_VS && !isFirstBattle) PlayStageBgm();

        isFirstBattle = false;

        //VS開始,WinMarkリセット
        if (!isVsStart) ResetWinMark();
    }

    private void GameStart()
    {
        Debug.Log("*** GameStart ***");
        int enemyUserid = -1;
        foreach (PlayerStatus playerStatus in playerStatuses)
        {
            if (playerStatus == null) return;
            playerStatus.Init();
            if (playerStatus.userId.ToString() != UserManager.userInfo[Common.PP.INFO_USER_ID]) enemyUserid = playerStatus.userId;
        }
        if (PhotonNetwork.isMasterClient && gameMode == GAME_MODE_VS && !isVsStart)
        {
            //バトル開始ログ作成
            Battle.Start battleStart = new Battle.Start();
            battleStart.SetRetryCount(5);
            battleStart.SetApiFinishCallback(BattleStartCallback);
            battleStart.SetApiErrorIngnore();
            battleStart.Exe(enemyUserid);
        }
        isContinue = false;
        isGameReady = false;
        isGameStart = true;
        battleTime = 0;
        isVsStart = true;
    }
    private void BattleStartCallback()
    {
        photonView.RPC("SetBattleIdRPC", PhotonTargets.Others, ModelManager.battleInfo.battle_id);
    }
    [PunRPC]
    private void SetBattleIdRPC(int battleId)
    {
        ModelManager.battleInfo.battle_id = battleId;
    }

    private void GameEnd()
    {
        Debug.Log("*** GameEnd ***");
        if (myTran != null && targetTran == null)
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
            content.sizeDelta = new Vector2(0, (maxLevel + 1) * 180);
            for (int i = maxLevel + 1; i > 0; i--)
            {
                int setLevel = i;
                GameObject btn = (GameObject)Instantiate(levelSelectButton, Vector3.zero, Quaternion.identity);
                btn.transform.SetParent(content, false);
                btn.transform.GetComponentInChildren<Text>().text = Common.Mission.GetLevelName(setLevel);
                if (i > maxLevel)
                {
                    btn.transform.GetComponent<Button>().interactable = false;
                    btn.transform.GetComponentInChildren<Text>().color = Color.grey;
                }
                else
                {
                    btn.transform.GetComponent<Button>().onClick.AddListener(() => OnSelectLevel(setLevel));
                }
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
        totalContinueCount = 0;
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
        //ステージ記録
        Mission.Update missionUpdate = new Mission.Update();
        missionUpdate.SetApiErrorIngnore();
        missionUpdate.SetRetryCount(5);
        missionUpdate.Exe(stageLevel, stageNo, totalContinueCount);

        //次のステージOPEN
        UserManager.OpenNextMission(stageLevel, stageNo);

        //次のステージチェック
        if (!Common.Mission.stageNpcNoDic.ContainsKey(stageNo + 1)) return false;
        stageNo++;
        //ResetWinMark();
        ResetVs();
        continueCount = 0;
        SetStatus();

        return true;
    }

    //VSリセット
    private void ResetVs()
    {
        isVsStart = false;
    }

    //勝敗リセット
    private void ResetWinMark()
    {
        winCount = 0;
        loseCount = 0;
        myStatus.ResetWinMark();
    }

    //デバッグ用
    public void SetFinalRound()
    {
        winCount = WIN_COUNT_MAX - 1;
        loseCount = WIN_COUNT_MAX - 1;
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
            ResetWinMark();
            StageSetting();
            isContinue = true;
        }
    }

    //
    public void ContinueVs()
    {
        isContinue = true;
        ResetDamageSource();
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
            Destroy(targetTran.gameObject);
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
        ResetDamageSource();
    }

    //VS準備
    private void VsSetting()
    {
        if (myTran == null) PlayerSpawn();
        ResetDamageSource();
    }


    //ダメージソースリセット
    private void ResetDamageSource()
    {
        isResultCheck = false;
        resultCanvas.SetActive(false);
        damageSourceMine = new Dictionary<string, int>();
        damageSourceEnemy = new Dictionary<string, int>();
    }

    private void PlayerSpawn()
    {
        //ベースボディ生成
        string charaName = Common.CO.CHARACTER_BASE;
        GameObject player = SpawnProcess(charaName);
        playerSetting = player.GetComponent<PlayerSetting>();
        myStatus = player.GetComponent<PlayerStatus>();
        SetCanvasInfo();
        ResetGame();
        myStatus.SetWinMark(winCount, loseCount);
        SetStatus();
    }

    private void SetStatus()
    {
        int[] statusArray = Common.Character.StatusDic[UserManager.userSetCharacter];
        float[] statusLevelRate = (float[])Common.Mission.npcLevelStatusDic[0].Clone();
        for (int i = 0; i < statusLevelRate.Length; i++)
        {
            statusLevelRate[i] += Common.Mission.continueBonus[i] * continueCount;
        }
        myStatus.SetStatus(statusArray, statusLevelRate);
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
        npcNo = Common.Mission.GetStageNpcNo(stageLevel, stageNo);

        //ステージのNPC取得
        string npcName = "BaseNpc";

        GameObject npc = SpawnProcess(npcName);
        NpcController npcCtrl = npc.GetComponent<NpcController>();
        npcCtrl.SetLevel(stageLevel);
    }

    public void SpawnTargetNpc()
    {
        if (gameMode == GAME_MODE_MISSION) return;
        if (CheckPlayer()) return;
        if (npcTran != null)
        {
            CleanNpc();
            return;
        }
        isPractice = true;
        GameObject npc = SpawnProcess("TargetNpc");
        targetTran = npc.transform;
        SetNpcTran(targetTran);
        StartCoroutine(StartPractice());
    }
    IEnumerator StartPractice()
    {
        PlayerStatus npcStatus = targetTran.GetComponent<PlayerStatus>();
        for (; ;)
        {
            if (myStatus == null || npcStatus == null) yield break;
            if (myStatus.isReadyBattle && npcStatus.isReadyBattle) break;
            yield return null;
        }
        myStatus.Init();
        npcStatus.Init();
    }

    public void SetDamageSource(int logType, string name, int damage)
    {
        Dictionary<string, int> damageSource;
        if (logType == PlayerStatus.BATTLE_LOG_ATTACK)
        {
            damageSource = damageSourceMine;
        }
        else
        {
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
        if (gameMode == GAME_MODE_VS || !isGameStart || isGameEnd)
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


    //##### Photon Callback #####

    void OnMasterClientSwitched()
    {
        //ルーム名変更
        RoomApi.ChangeMaster roomApiChangeMaster = new RoomApi.ChangeMaster();
        roomApiChangeMaster.SetApiErrorIngnore();
        roomApiChangeMaster.SetRetryCount(5);
        roomApiChangeMaster.Exe();
    }
}

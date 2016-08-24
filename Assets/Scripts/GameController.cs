using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameController : Photon.MonoBehaviour
{
    [SerializeField]
    private GameObject stageStructure;
    private Text messageText;
    private int readyTime = 3;
    private int needPlayerCount = 2;

    private Text textUp;
    private Text textCenter;
    private Color colorWin = Color.red;
    private Color colorLose = Color.blue;
    private Color colorReady = Color.yellow;
    private Color colorWait = new Color(0, 255, 255);
    private float baseAlpha = 0.6f;

    private Transform myTran;
    private Transform targetTran;
    private Transform npcTran;

    private bool isWin = false;
    [HideInInspector]
    public bool isGameReady = false;
    [HideInInspector]
    public bool isGameStart = false;
    [HideInInspector]
    public bool isGameEnd = false;
    private List<PlayerStatus> playerStatuses = new List<PlayerStatus>();
    private PlayerSetting playerSetting;
    private PlayerStatus playerStatus;
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
    const string MESSAGE_GAME_OVER = "GameOver...";


    [HideInInspector]
    public int gameMode = -1;
    public const int GAME_MODE_MISSION = 1;
    public const int GAME_MODE_SELECT = 2;
    public const int GAME_MODE_VS = 3;
    [HideInInspector]
    public int stageNo = -1;
    [HideInInspector]
    public int stageLevel = -1;

    private int winCount = 0;
    private int loseCount = 0;

    [HideInInspector]
    public bool isDebugMode = false;

    void Awake()
    {
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
        isDebugMode = GameObject.Find("Debug").GetComponent<MyDebug>().isDebugMode;
        spriteStudioCtrl = GameObject.Find("SpriteStudioController").GetComponent<SpriteStudioController>();
    }

    private void SetCanvasInfo()
    {
        //キャンバス情報
        Transform screenTran = Camera.main.transform.FindChild(Common.CO.SCREEN_CANVAS);
        textUp = screenTran.FindChild(Common.CO.TEXT_UP).GetComponent<Text>();
        textCenter = screenTran.FindChild(Common.CO.TEXT_CENTER).GetComponent<Text>();
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
    private void SetText(Text textObj, string text, Color color = default(Color), float fadeout = 0)
    {
        //Debug.Log(textObj.name+" >> "+text);
        if (text == "")
        {
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
                    //Text
                    textObj.enabled = false;
                }
            }
            textObj.text = "";
        }
        else
        {
            //テキスト表示
            textObj.enabled = false;
            SetText(textObj, "");
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
                    //Text
                    if (color != default(Color)) textObj.color = new Color(color.r, color.g, color.b, baseAlpha);
                    //if (fadeout > 0) StartCoroutine(MessageFadeOut(textObj, fadeout));
                    textObj.enabled = true;
                }
            }

            if (fadeout > 0) StartCoroutine(MessageDelayDelete(textObj, fadeout));
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

    IEnumerator MessageFadeOut(Text textObj, float fadeout)
    {
        float startAlpha = textObj.color.a;
        float nowAlpha = startAlpha;
        for (;;)
        {
            nowAlpha -= Time.deltaTime / fadeout * startAlpha;
            textObj.color = new Color(textObj.color.r, textObj.color.g, textObj.color.b, nowAlpha);
            if (nowAlpha <= 0) break;
            yield return null;
        }
        textObj.enabled = false;
    }

    IEnumerator MessageDelayDelete(Text textObj, float fadeout)
    {
        yield return new WaitForSeconds(fadeout);
        SetText(textObj, "");
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
                if (gameMode == GAME_MODE_MISSION)
                {
                    //ミッションモード
                    yield return new WaitForSeconds(3.0f);
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
                                SetTextUp(MESSAGE_STAGE_NEXT + MESSAGE_STAGE_READY + "...", colorWait);
                            }
                            else
                            {
                                //全ステージクリア
                                SetTextCenter(MESSAGE_MISSION_CLEAR, colorWait);
                            }
                        }
                        else
                        {
                            //NextRound
                            isStageSetting = true;
                            SetTextUp(MESSAGE_STAGE_NEXT + MESSAGE_ROUND_READY + "...", colorWait);
                        }
                    }
                    else
                    {
                        //敗北
                        if (loseCount >= 3)
                        {
                            //ゲームオーバー
                            SetTextCenter(MESSAGE_GAME_OVER, colorWin);
                        }
                        else
                        {
                            isStageSetting = true;
                            SetTextUp(MESSAGE_STAGE_NEXT + MESSAGE_ROUND_READY + "...", colorWait);
                        }
                    }

                    //ステージセッティング
                    if (isStageSetting)
                    {
                        yield return new WaitForSeconds(5.0f);
                        StageSetting();
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
                            if (gameMode == GAME_MODE_MISSION)
                            {
                                int round = winCount + loseCount + 1;
                                if (round == 1)
                                {
                                    //ステージ文字
                                    SetTextCenter(MESSAGE_STAGE_READY + stageNo.ToString(), colorReady, 2.5f);
                                    yield return new WaitForSeconds(3);
                                }
                                //ラウンド文字
                                SetTextCenter(MESSAGE_ROUND_READY + round.ToString(), colorReady, 2.5f);
                                yield return new WaitForSeconds(3);
                            }

                            //カウントダウン
                            SetTextCenter(MESSAGE_READY, colorReady);
                            yield return new WaitForSeconds(3);
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

    private void CleanNpc()
    {
        photonView.RPC("CleanNpcRPC", PhotonTargets.All);
    }

    [PunRPC]
    private void CleanNpcRPC()
    {
        GameObject npc = GameObject.Find("NPC");
        if (npc != null)
        {
            npc.GetComponent<PlayerStatus>().AddDamage(99999);
            //npc.GetComponent<ObjectController>().DestoryObject();
        }
    }

    public void SetMyTran(Transform tran)
    {
        myTran = tran;
        GameObject.Find("WeaponStore").GetComponent<WeaponStore>().SetMyTran();
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
        //PhotonNetwork.LoadLevel(Common.CO.SCENE_TITLE);
        GameObject.Find("Fade").GetComponent<FadeManager>().Load(Common.CO.SCENE_TITLE, DialogController.MESSAGE_LOADING);
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
    }

    private void GameStart()
    {
        foreach (PlayerStatus playerStatus in playerStatuses)
        {
            playerStatus.Init();
        }
        isGameReady = false;
        isGameStart = true;
    }

    private void GameEnd()
    {
        if (targetTran == null)
        {
            //勝利
            winCount++;
            isWin = true;
            SetTextCenter(spriteStudioCtrl.ANIMATION_TEXT_WIN, colorWin);
        }
        else
        {
            //敗北
            loseCount++;
            SetTextCenter(spriteStudioCtrl.ANIMATION_TEXT_LOSE, colorLose);
        }
        isGameEnd = true;
        playerStatus.SetWinMark(winCount, loseCount);
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

            //レベル設定ダイアログ
            stageLevel = 1;
        }
        else
        {
            //対戦モード
            gameMode = GAME_MODE_VS;
        }
    }


    //##### MissionMode #####

    //現在のステージNo取得
    public int GetStageNo()
    {
        return stageNo;
    }

    //次のステージNo設定
    private bool SetNextStage()
    {
        //次のステージチェック
        if (stageNo >= 3) return false;
        stageNo++;
        winCount = 0;
        loseCount = 0;
        playerStatus.ResetWinMark();

        return true;
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
        }
        NpcSpawn(stageNo);
    }

    private void PlayerSpawn()
    {
        //スプライトスタジオキャッシュリセット
        spriteStudioCtrl.ResetSprite();

        //ベースボディ生成
        string charaName = Common.CO.CHARACTER_BASE;
        GameObject player = SpawnProcess(charaName);
        playerSetting = player.GetComponent<PlayerSetting>();
        playerStatus = player.GetComponent<PlayerStatus>();
        SetCanvasInfo();
        ResetGame();
        playerStatus.SetWinMark(winCount, loseCount);
    }

    //NPC生成
    public void NpcSpawn(int no = -1)
    {
        ResetGame();

        if (no > 0) stageNo = no;
        if (stageNo <= 0) stageNo = 1;

        //ステージのNPC取得
        stageLevel = stageNo;
        string npcName = "BaseNpc";

        GameObject npc = SpawnProcess(npcName);
        NpcController npcCtrl = npc.GetComponent<NpcController>();

        //NPCステータス等設定
        npcCtrl.SetNpcNo(stageNo);
        npcCtrl.SetLevel(stageLevel);
    }
}

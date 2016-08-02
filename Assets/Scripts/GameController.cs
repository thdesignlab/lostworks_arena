using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameController : Photon.MonoBehaviour
{
    [SerializeField]
    private GameObject stageStructure;
    //[SerializeField]
    //private GameObject messageCanvas;
    //private CanvasGroup messageCanvasGroup;
    private Text messageText;
    private int readyTime = 3;
    private int needPlayerCount = 2;

    //[SerializeField]
    //private GameObject waitCanvas;
    //[SerializeField]
    //private GameObject winCanvas;
    //[SerializeField]
    //private GameObject loseCanvas;
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

    [HideInInspector]
    public bool isGameStart = false;
    private bool isGameEnd = false;
    private List<PlayerStatus> playerStatuses = new List<PlayerStatus>();
    private PlayerSetting playerSetting;
    private SpriteStudioController spriteStudioCtrl;
    private Script_SpriteStudio_Root scriptRoot;

    const string MESSAGE_WAITING = "Player waiting...";
    const string MESSAGE_CUSTOMIZE = "Customizing...";
    const string MESSAGE_READY = "Ready";
    const string MESSAGE_START = "Go";
    //const string MESSAGE_WIN = "Win";
    //const string MESSAGE_LOSE = "Lose";

    [HideInInspector]
    public bool isDebugMode = false;

    void Awake()
    {
        UserManager.DispUserInfo();
        isDebugMode = GameObject.Find("Debug").GetComponent<MyDebug>().isDebugMode;
        spriteStudioCtrl = GameObject.Find("SpriteStudioController").GetComponent<SpriteStudioController>();
        SpawnMyPlayerEverywhere();
    }

    void Start()
    {
        //キャンバス情報
        Transform screenTran = Camera.main.transform.FindChild(Common.CO.SCREEN_CANVAS);
        textUp = screenTran.FindChild(Common.CO.TEXT_UP).GetComponent<Text>();
        textCenter = screenTran.FindChild(Common.CO.TEXT_CENTER).GetComponent<Text>();
        SetTextUp();
        SetTextCenter();

        StartCoroutine(ChceckGame());
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
                    Reset3DText();
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
                    Set3DText(text3d, textObj.transform.position);
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

    private void Set3DText(GameObject obj, Vector3 pos)
    {
        Instantiate(obj, pos, Camera.main.transform.rotation * obj.transform.rotation);
    }
    private void Reset3DText()
    {
        GameObject[] texts = GameObject.FindGameObjectsWithTag("3DText");
        foreach (GameObject text in texts)
        {
            Destroy(text);
        }
    }

    IEnumerator MessageFadeOut(Text textObj, float fadeout)
    {
        //int second = 3;
        //messageCanvasGroup.alpha = 1;
        float startAlpha = textObj.color.a;
        float nowAlpha = startAlpha;
        for (;;)
        {
            //    messageCanvasGroup.alpha -= Time.deltaTime / second;
            //    if (messageCanvasGroup.alpha <= 0) break;
            nowAlpha -= Time.deltaTime / fadeout * startAlpha;
            textObj.color = new Color(textObj.color.r, textObj.color.g, textObj.color.b, nowAlpha);
            if (nowAlpha <= 0) break;
            yield return null;
        }
        //messageText.text = "";
        //messageCanvas.SetActive(false);
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
        isGameStart = false;
        isGameEnd = false;
        StageObjReset();
    }

    private void StageObjReset()
    {
        if (PhotonNetwork.player == PhotonNetwork.masterClient)
        {
            //ステージオブジェ破壊
            GameObject[] objes = GameObject.FindGameObjectsWithTag(Common.CO.TAG_STRUCTURE);
            foreach (GameObject obj in objes)
            {
                ObjectController objCtrl = obj.GetComponent<ObjectController>();
                if (objCtrl != null)
                {
                    objCtrl.DestoryObject(true);
                }
            }

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
        float waitTime = 3.0f;
        for (;;)
        {
            if (isGameEnd)
            {
                //Debug.Log("GameEnd");
                yield return new WaitForSeconds(waitTime);
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
                waitTime = 3.0f;
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
                                waitTime = 1;
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
                            //カウントダウン
                            SetTextUp();
                            SetTextCenter(MESSAGE_READY, colorReady, 3);
                            yield return new WaitForSeconds(3);
                            for (int i = readyTime; i > 0; i--)
                            {
                                SetTextCenter(i.ToString(), colorReady, 1);
                                yield return new WaitForSeconds(1);
                            }
                            SetTextCenter(MESSAGE_START, colorReady, 3);

                            //対戦スタート
                            GameStart();
                        }
                    }
                    else
                    {
                        if (PhotonNetwork.countOfPlayersInRooms < needPlayerCount)
                        {
                            //SetWaitMessage(MESSAGE_WAITING);
                            SetTextUp(MESSAGE_WAITING, colorWait);
                            waitTime = 1;
                        }
                    }
                }
                else
                {
                    //装備設定中
                    //SetWaitMessage(MESSAGE_CUSTOMIZE + playerSetting.GetLeftCustomTime().ToString());
                    SetTextUp(MESSAGE_CUSTOMIZE + playerSetting.GetLeftCustomTime().ToString(), colorWait);

                }

                waitTime = 0.5f;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    //public void SetWaitMessage(string message = "")
    //{
    //    if (message == "")
    //    {
    //        waitCanvas.SetActive(false);
    //    }
    //    else
    //    {
    //        waitCanvas.SetActive(true);
    //        waitCanvas.transform.FindChild("Text").GetComponent<Text>().text = message;
    //    }
    //}

    private bool CheckPlayer()
    {
        if (playerStatuses.Count == needPlayerCount) return false;
        return true;
    }

    //プレイヤー作成
    public void SpawnMyPlayerEverywhere()
    {
        CleanNpc();
        ResetGame();
        GameObject player = SpawnProcess("Hero");
        playerSetting = player.GetComponent<PlayerSetting>();
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
        PhotonNetwork.LoadLevel(Common.CO.SCENE_TITLE);
    }

    public void NpcSpawn(int level = 0)
    {
        ResetGame();
        GameObject npc = SpawnProcess("Npc");
        npc.GetComponent<NpcController>().SetLevel(level);
    }

    private GameObject SpawnProcess(string name, int groupId = 0)
    {
        Transform spawnPoint = GetSpawnPoint();
        Vector3 pos = new Vector3(0, 10, 0);
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
        if (spawnPoints == null) return null;

        int index = Random.Range(0, spawnPoints.Length);
        return spawnPoints[index].transform;
    }

    private void GameStart()
    {
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
        isGameStart = true;
    }

    private void GameEnd()
    {
        isGameEnd = true;
        ResultMessage(targetTran == null) ;
    }

    private void ResultMessage(bool isWin)
    {
        if (isWin)
        {
            //winCanvas.SetActive(true);
            SetTextCenter(spriteStudioCtrl.ANIMATION_TEXT_WIN, colorWin);
            //StartCoroutine(ResultMessageDelete(textCenter));
        }
        else
        {
            //loseCanvas.SetActive(true);
            SetTextCenter(spriteStudioCtrl.ANIMATION_TEXT_LOSE, colorLose);
            //StartCoroutine(ResultMessageDelete(textCenter));
        }
    }
    //IEnumerator ResultMessageDelete(GameObject c)
    //{
    //    yield return new WaitForSeconds(10);
    //    c.SetActive(false);
    //}

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
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GameController : Photon.MonoBehaviour
{
    [SerializeField]
    private GameObject messageCanvas;
    private CanvasGroup messageCanvasGroup;
    private Text messageText;
    private int readyTime = 5;
    private int needPlayerCount = 2;

    [SerializeField]
    private GameObject waitCanvas;
    [SerializeField]
    private GameObject winCanvas;
    [SerializeField]
    private GameObject loseCanvas;

    private Transform myTran;
    private Transform targetTran;
    private Transform npcTran;

    [HideInInspector]
    public bool isGameStart = false;
    private bool isGameEnd = false;
    private List<PlayerStatus> playerStatuses = new List<PlayerStatus>();
    private PlayerSetting playerSetting;

    const string MESSAGE_WAITING = "Player waiting...";
    const string MESSAGE_CUSTOMIZE = "Customizing...";

    [HideInInspector]
    public bool isDebugMode = false;

    void Awake()
    {
        isDebugMode = GameObject.Find("Debug").GetComponent<MyDebug>().isDebugMode;
        SpawnMyPlayerEverywhere();
        messageCanvasGroup = messageCanvas.GetComponent<CanvasGroup>();
        messageText = messageCanvas.transform.FindChild("Text").GetComponent<Text>();
    }

    void Start()
    {
        messageCanvasGroup.alpha = 0;
        messageText.text = "";
        StartCoroutine(ChceckGame());
    }

    public void ResetGame()
    {
        isGameStart = false;
        isGameEnd = false;
        photonView.RPC("ResetGameRPC", PhotonTargets.Others);
    }
    [PunRPC]
    private void ResetGameRPC()
    {
        isGameStart = false;
        isGameEnd = false;
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
                                SetWaitMessage(MESSAGE_CUSTOMIZE + setting.GetLeftCustomTime().ToString());
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
                            SetWaitMessage();
                            winCanvas.SetActive(false);
                            loseCanvas.SetActive(false);
                            messageText.text = "Ready";
                            messageCanvasGroup.alpha = 1;
                            messageCanvas.SetActive(true);
                            yield return new WaitForSeconds(1);
                            for (int i = readyTime; i > 0; i--)
                            {
                                messageText.text = i.ToString();
                                yield return new WaitForSeconds(1);
                            }
                            messageText.text = "Start";
                            StartCoroutine(MessageFadeOut());

                            //対戦スタート
                            GameStart();
                        }
                    }
                    else
                    {
                        if (PhotonNetwork.countOfPlayersInRooms < needPlayerCount)
                        {
                            SetWaitMessage(MESSAGE_WAITING);
                            waitTime = 1;
                        }
                    }
                }
                else
                {
                    //装備設定中
                    SetWaitMessage(MESSAGE_CUSTOMIZE + playerSetting.GetLeftCustomTime().ToString());
                }

                waitTime = 0.5f;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    public void SetWaitMessage(string message = "")
    {
        if (message == "")
        {
            waitCanvas.SetActive(false);
        }
        else
        {
            waitCanvas.SetActive(true);
            waitCanvas.transform.FindChild("Text").GetComponent<Text>().text = message;
        }
    }

    private bool CheckPlayer()
    {
        if (playerStatuses.Count == needPlayerCount) return false;
        return true;
    }

    IEnumerator MessageFadeOut()
    {
        int second = 3;
        messageCanvasGroup.alpha = 1;
        for (;;)
        {
            messageCanvasGroup.alpha -= Time.deltaTime / second;
            if (messageCanvasGroup.alpha <= 0) break;
            yield return null;
        }
        messageText.text = "";
        messageCanvas.SetActive(false);
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
        PhotonNetwork.LoadLevel("Title");
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
            winCanvas.SetActive(true);
            StartCoroutine(ResultMessageDelete(winCanvas));
        }
        else
        {
            loseCanvas.SetActive(true);
            StartCoroutine(ResultMessageDelete(loseCanvas));
        }
    }
    IEnumerator ResultMessageDelete(GameObject c)
    {
        yield return new WaitForSeconds(10);
        c.SetActive(false);
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
}

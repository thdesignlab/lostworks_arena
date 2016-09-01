using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class PhotonManager : MonoBehaviour
{
    public static bool isFirstScean = true;

    [SerializeField]
    private GameObject modeSelectArea;
    [SerializeField]
    private GameObject networkArea;
    [SerializeField]
    private GameObject roomListArea;
    //[SerializeField]
    private GameObject messageArea;
    [SerializeField]
    private GameObject configArea;

    [SerializeField]
    private GameObject roomCancelBtn;
    [SerializeField]
    private GameObject roomSelectBtn;
    [SerializeField]
    private GameObject roomFullBtn;

    //ゲームモード
    // 1: Local
    // 2: Network
    // 3: Local + Network
    const int SELECTABLE_MODE_LOCAL = 1;
    const int SELECTABLE_MODE_NETWORK = 2;
    const int SELECTABLE_MODE_BOTH = 3;
    private int selectableMode = SELECTABLE_MODE_BOTH;

    const int MAX_ROOM_PLAYER_COUNT = 2;
    const string ROOM_NAME_PREFIX = "Room";

    private bool isTapToStart = false;
    private bool isConnectFailed = false;
    private bool isDialogOpen = false;
    private bool isNetworkMode = false;

    private float processTime = 0;

    private Text messageAreaText;

    private InputField roomNameIF;
    private Text roomStatusText;

    private ScreenManager screenMgr;
    private string moveScene = "";
    private string loadmessage = "";

    const string MESSAGE_CONNECT_FAILED = "ネットワーク接続に失敗しました\n通信状況をご確認の上\n再度お試しください";
    const string MESSAGE_CREATE_ROOM_FAILED = "Room作成に失敗しました\n既に存在するRoom名です";
    const string MESSAGE_ROOM_LIMIT_FAILED = "Room作成に失敗しました\nこれ以上Roomを作成できません";
    const string MESSAGE_JOIN_ROOM_FAILED = "参加可能なRoomがありません\nRoomを作成するか\n時間を空けてから再度お試しください";

    public void Awake()
    {
        GameObject screenObj = GameObject.Find("ScreenManager");
        screenMgr = screenObj.GetComponent<ScreenManager>();
        DontDestroyOnLoad(screenObj);
        DontDestroyOnLoad(GameObject.Find("Config"));
        DontDestroyOnLoad(GameObject.Find("WeaponStore"));
        DontDestroyOnLoad(GameObject.Find("Debug"));

        //テキストエリア
        //messageAreaText = messageArea.transform.FindChild("Message").GetComponent<Text>();

        //初期化
        if (isFirstScean)
        {
            Init();
            isFirstScean = false;
        }
        else
        {
            ReturnModeSelect();
        }

        //ユーザー情報取得
        UserManager.SetUserInfo();
    }

    void Start()
    {
        GameObject bgmObj = GameObject.Find("BgmManager");
        DontDestroyOnLoad(bgmObj);
        bgmObj.GetComponent<BgmManager>().Play();
    }

    void Update()
    {
        //processTime += Time.deltaTime;

        if (isTapToStart)
        {
            GameObject message = DialogController.OpenMessage(DialogController.MESSAGE_TOP, DialogController.MESSAGE_POSITION_CENTER);
            Image messageImage = DialogController.GetMessageImageObj();
            Text messageText = DialogController.GetMessageTextObj();
            if (Input.GetMouseButtonDown(0))
            {
                TapToStart();
            }
            if (message != null)
            {
                processTime += Time.deltaTime;

                if (processTime > 1.0f)
                {
                    //一定時間ごとに点滅
                    float alpha = Common.Func.GetSin(processTime, 270, 45);
                    messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, alpha);
                    messageImage.color = new Color(messageImage.color.r, messageImage.color.g, messageImage.color.b, alpha);
                }
            }
            else
            {
                processTime = 0;
            }
        }

        if (isNetworkMode)
        {
            if (PhotonNetwork.connected && !isConnectFailed)
            {
                // *** 接続成功 ***

                if (networkArea.GetActive())
                {
                    //接続状況更新
                    roomStatusText.text = "Room :" + PhotonNetwork.countOfRooms + " / Player :" + PhotonNetwork.countOfPlayers;
                    if (roomNameIF.text == ROOM_NAME_PREFIX)
                    {
                        roomNameIF.text = ROOM_NAME_PREFIX + (PhotonNetwork.countOfRooms + 1).ToString();
                    }
                }
                else
                {
                    DialogController.CloseMessage();

                    //部屋選択表示
                    SwitchNetworkArea(true, true);

                    //初期値設定
                    PlayerPrefs.SetString("playerName", PhotonNetwork.playerName);
                    roomNameIF.text = ROOM_NAME_PREFIX;
                    //roomStatusText.text = "Room :" + PhotonNetwork.countOfRooms + " / Player :" + PhotonNetwork.countOfPlayers;
                    //isConnected = true;
                }
            }
            else
            {
                // *** 未接続 ***
                SwitchNetworkArea(false);

                if (PhotonNetwork.connecting)
                {
                    // *** 接続中 ***
                    DialogController.OpenMessage(DialogController.MESSAGE_CONNECT, DialogController.MESSAGE_POSITION_RIGHT);
                }
                else
                {
                    DialogController.CloseMessage();
                }

                if (isConnectFailed && !isDialogOpen)
                {
                    // *** 接続失敗 ***
                    isDialogOpen = true;
                    DialogController.OpenDialog(MESSAGE_CONNECT_FAILED, () => ConnectStart(), () => ExitGame());
                }
            }
        }
    }

    //初期化
    public void Init(bool isReturn = false)
    {
        if (!isReturn)
        {
            SwitchModeSelectArea(false);
        }
        DialogController.CloseMessage();
        SwitchNetworkArea(false);
        isTapToStart = true;
        isConnectFailed = false;
        isNetworkMode = false;
        if (PhotonNetwork.connected)
        {
            PhotonNetwork.Disconnect();
        }
    }

    public void ReturnModeSelect()
    {
        Init(true);
        TapToStart();
    }

    //モードセレクトダイアログ切り替え
    public void SwitchModeSelectArea(bool flg, bool isFade = false)
    {
        if (isFade)
        {
            screenMgr.FadeUI(modeSelectArea, flg);
        }
        else
        {
            modeSelectArea.SetActive(flg);
        }
    }

    //ネットワークダイアログ切り替え
    private void SwitchNetworkArea(bool flg, bool isFade = false)
    {
        if (isFade)
        {
            screenMgr.FadeUI(networkArea.gameObject, flg, false);
        }
        else
        {
            networkArea.SetActive(flg);
        }
        
        if (!flg)
        {
            SwitchRoomListArea(flg, isFade);
        }
    }

    //Room一覧ダイアログ切り替え
    public void OnSwitchRoomListAreaButton(bool flg)
    {
        SwitchRoomListArea(flg, true);
    }
    private void SwitchRoomListArea(bool flg, bool isFade = false)
    {
        if (isFade)
        {
            screenMgr.FadeUI(roomListArea.gameObject, flg, false);
        }
        else
        {
            roomListArea.SetActive(flg);
        }
        
        if (flg)
        {
            SearchRoomList();
        }
    }

    //Room一覧更新
    private void SearchRoomList()
    {
        //中身をリセットする
        foreach (Transform child in roomListArea.transform)
        {
            Destroy(child.gameObject);
        }

        //キャンセルボタン
        GameObject cancelBtn = (GameObject)Instantiate(roomCancelBtn);
        cancelBtn.transform.SetParent(roomListArea.transform, false);
        cancelBtn.GetComponent<Button>().onClick.AddListener(() => SwitchRoomListArea(false, true));

        //Room一覧取得
        RoomInfo[] roomList = PhotonNetwork.GetRoomList();
        if (roomList.Length > 0)
        {
            foreach (RoomInfo room in roomList)
            {
                string roomName = room.name;
                GameObject roomBtn = roomSelectBtn;
                if (room.playerCount >= MAX_ROOM_PLAYER_COUNT)
                {
                    roomBtn = roomFullBtn;
                }
                GameObject joinBtn = (GameObject)Instantiate(roomBtn);
                joinBtn.transform.SetParent(roomListArea.transform, false);
                joinBtn.GetComponent<Button>().onClick.AddListener(() => JoinRoom(roomName));
                joinBtn.transform.FindChild("Text").GetComponent<Text>().text = roomName;
            }
        }
    }


    //タイトル画面から進む
    private void TapToStart()
    {
        isTapToStart = false;
        SwitchModeSelectArea(true, true);

        //現在はBOTHのみ
        switch (selectableMode)
        {
            case SELECTABLE_MODE_LOCAL:
                //ローカルのみ
                DialogController.CloseMessage();
                break;

            case SELECTABLE_MODE_NETWORK:
                //ネットワークのみ
                DialogController.CloseMessage();
                break;

            case SELECTABLE_MODE_BOTH:
                //ローカル+ネットワーク
                DialogController.CloseMessage();
                break;
        }
    }

    // ##### モードセレクト #####

    //ローカルモード選択
    public void LocalModeSelect()
    {
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);
        SwitchModeSelectArea(false, true);

        PhotonNetwork.offlineMode = true;
        moveScene = Common.CO.SCENE_BATTLE;
        PhotonNetwork.CreateRoom(ROOM_NAME_PREFIX);
    }

    //ネットワークモード選択
    public void NetworkModeSelect()
    {
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);
        SwitchModeSelectArea(false, true);

        PhotonNetwork.offlineMode = false;
        PhotonNetwork.automaticallySyncScene = true;
        PhotonNetwork.autoJoinLobby = true;
        moveScene = Common.CO.SCENE_BATTLE;

        // the following line checks if this client was just created (and not yet online). if so, we connect
        if (PhotonNetwork.connectionStateDetailed == PeerState.PeerCreated)
        {
            // Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
            PhotonNetwork.ConnectUsingSettings("0.9");
        }

        // generate a name for this player, if none is assigned yet
        //if (string.IsNullOrEmpty(PhotonNetwork.playerName))
        //{
        PhotonNetwork.playerName = UserManager.userInfo[Common.PP.INFO_USER_NAME];
        //}

        // if you wanted more debug out, turn this on:
        // PhotonNetwork.logLevel = NetworkLogLevel.Full;

        //ネットワークエリア
        roomNameIF = networkArea.transform.FindChild("Room/Name").GetComponent<InputField>();
        roomStatusText = networkArea.transform.FindChild("RoomStatus").GetComponent<Text>();

        isNetworkMode = true;
    }

    //カスタマイズ
    public void CustomSelect()
    {
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);

        PhotonNetwork.offlineMode = true;
        moveScene = Common.CO.SCENE_CUSTOM;
        PhotonNetwork.CreateRoom(ROOM_NAME_PREFIX);
    }

    //コンフィグ
    public void OnConfigButton()
    {
        SwitchModeSelectArea(false, true);
        GameObject.Find("Config").GetComponent<ConfigManager>().OpenConfig();
    }

    // ##### 各操作 #####

    //Photonへ接続開始
    private void ConnectStart()
    {
        isConnectFailed = false;
        isDialogOpen = false;
        DialogController.CloseDialog();
        PhotonNetwork.ConnectUsingSettings("0.9");
    }

    //ゲーム終了
    public void ExitGame()
    {
        Application.Quit();
    }

    //Room作成
    public void CreateRoom()
    {
        DialogController.OpenMessage(DialogController.MESSAGE_CREATE_ROOM, DialogController.MESSAGE_POSITION_RIGHT);
        if (PhotonNetwork.countOfRooms >= 10)
        {
            DialogController.CloseMessage();
            DialogController.OpenDialog(MESSAGE_ROOM_LIMIT_FAILED);
            return;
        }
        PhotonNetwork.CreateRoom(roomNameIF.text, new RoomOptions() { maxPlayers = 2 }, null);
    }

    //入室
    public void JoinRoom(string roomName = "")
    {
        DialogController.OpenMessage(DialogController.MESSAGE_JOIN_ROOM, DialogController.MESSAGE_POSITION_RIGHT);
        if (roomName == "")
        {
            roomName = roomNameIF.text;
        }
        PhotonNetwork.JoinRoom(roomName);
    }

    //ランダム入室
    public void RandomJoinRoom()
    {
        DialogController.OpenMessage(DialogController.MESSAGE_SEARCH_ROOM, DialogController.MESSAGE_POSITION_RIGHT);
        PhotonNetwork.JoinRandomRoom();
    }


    // ##### photon callback #####

    // We have two options here: we either joined(by title, list or random) or created a room.
    public void OnJoinedRoom()
    {
        //Debug.Log("OnJoinedRoom");
    }

    public void OnPhotonCreateRoomFailed()
    {
        DialogController.CloseMessage();
        DialogController.OpenDialog(MESSAGE_CREATE_ROOM_FAILED);
        //ErrorDialog = "Error: Can't create room (room name maybe already used).";
        Debug.Log("OnPhotonCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
    }

    public void OnPhotonJoinRoomFailed(object[] cause)
    {
        DialogController.CloseMessage();
        DialogController.OpenDialog(MESSAGE_JOIN_ROOM_FAILED);
        //ErrorDialog = "Error: Can't join room (full or unknown room name). " + cause[1];
        Debug.Log("OnPhotonJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
    }

    public void OnPhotonRandomJoinFailed()
    {
        DialogController.CloseMessage();
        DialogController.OpenDialog(MESSAGE_JOIN_ROOM_FAILED);
        //ErrorDialog = "Error: Can't join random room (none found).";
        Debug.Log("OnPhotonRandomJoinFailed got called. Happens if no room is available (or all full or invisible or closed). JoinrRandom filter-options can limit available rooms.");
    }

    public void OnCreatedRoom()
    {
        //Debug.Log("OnCreatedRoom");
        //PhotonNetwork.LoadLevel(moveScene);
        screenMgr.Load(moveScene);
    }

    public void OnDisconnectedFromPhoton()
    {
        DialogController.CloseMessage();
        //Debug.Log("Disconnected from Photon.");
    }

    public void OnFailedToConnectToPhoton(object parameters)
    {
        DialogController.CloseMessage();
        isConnectFailed = true;
        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.ServerAddress);
    }

}

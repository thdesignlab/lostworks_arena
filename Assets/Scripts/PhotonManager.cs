using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PhotonManager : MonoBehaviour
{
    [SerializeField]
    private GameObject modeSelectArea;
    [SerializeField]
    private GameObject networkArea;
    [SerializeField]
    private GameObject roomListArea;
    [SerializeField]
    private GameObject messageArea;

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
    private bool isOpenModeSelect = false;
    private bool isConnectFailed = false;
    private bool isDialogOpen = false;
    private bool isNetworkMode = false;
    
    private float processTime = 0;

    private Text messageAreaText;

    private InputField roomNameIF;
    private Text roomStatusText;


    public void Awake()
    {
        //テキストエリア
        messageAreaText = messageArea.transform.FindChild("Message").GetComponent<Text>();
        Init();
    }

    void Update()
    {
        processTime += Time.deltaTime;

        if (isTapToStart)
        {
            SwitchMessageArea("Tap to Start");
            if (Input.GetMouseButtonDown(0))
            {
                TapToStart();
            }
        }
        if (messageArea.GetActive())
        {
            //一定時間ごとに点滅
            float alpha = Common.Func.GetSin(processTime, 270);
            messageAreaText.color = new Color(messageAreaText.color.r, messageAreaText.color.g, messageAreaText.color.b, alpha);
        }
        else
        {
            processTime = 0;
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
                    SwitchMessageArea();

                    //部屋選択表示
                    SwitchNetworkArea(true);

                    //初期値設定
                    PlayerPrefs.SetString("playerName", PhotonNetwork.playerName);
                    roomNameIF.text = ROOM_NAME_PREFIX;
                    roomStatusText.text = "Room :" + PhotonNetwork.countOfRooms + " / Player :" + PhotonNetwork.countOfPlayers;
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
                    SwitchMessageArea("Connecting");
                }
                else
                {
                    SwitchMessageArea();
                }

                if (isConnectFailed && !isDialogOpen)
                {
                    // *** 接続失敗 ***
                    isDialogOpen = true;
                    DialogController.OpenDialog("ネットワーク接続に失敗しました\n通信状況をご確認の上\n再度お試しください", () => ConnectStart(), () => ExitGame());
                }
            }
        }
    }

    //初期化
    public void Init()
    {
        SwitchModeSelectArea(false);
        SwitchMessageArea();
        SwitchNetworkArea(false);
        isTapToStart = true;
        isOpenModeSelect = false;
        isConnectFailed = false;
        isNetworkMode = false;
    }

    //モードセレクトダイアログ切り替え
    private void SwitchModeSelectArea(bool flg)
    {
        modeSelectArea.SetActive(flg);
    }

    //メッセージ切り替え
    private void SwitchMessageArea(string text = "")
    {
        //Debug.Log("SwitchMessageArea: " + text);
        messageAreaText.text = text;

        if (text == "")
        {
            messageArea.SetActive(false);
        }
        else
        {
            messageArea.SetActive(true);
        }
    }

    //ネットワークダイアログ切り替え
    private void SwitchNetworkArea(bool flg)
    {
        networkArea.SetActive(flg);
        if (!flg)
        {
            SwitchRoomListArea(flg);
        }
    }

    //Room一覧ダイアログ切り替え
    public void SwitchRoomListArea(bool flg)
    {
        roomListArea.SetActive(flg);
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
        cancelBtn.GetComponent<Button>().onClick.AddListener(() => SwitchRoomListArea(false));

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

        switch (selectableMode)
        {
            case SELECTABLE_MODE_LOCAL:
                //ローカルのみ
                SwitchModeSelectArea(false);
                LocalModeSelect();
                break;

            case SELECTABLE_MODE_NETWORK:
                //ネットワークのみ
                SwitchModeSelectArea(false);
                NetworkModeSelect();
                break;

            case SELECTABLE_MODE_BOTH:
                //ローカル+ネットワーク
                SwitchModeSelectArea(true);
                SwitchMessageArea();
                break;
        }
    }

    //ローカルモード選択
    public void LocalModeSelect()
    {
        SwitchMessageArea("Now Loading");
        SwitchModeSelectArea(false);

        PhotonNetwork.offlineMode = true;
        PhotonNetwork.CreateRoom(ROOM_NAME_PREFIX);
    }

    //ネットワークモード選択
    public void NetworkModeSelect()
    {
        SwitchMessageArea("Now Loading");
        SwitchModeSelectArea(false);

        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.offlineMode = false;
        PhotonNetwork.automaticallySyncScene = true;
        PhotonNetwork.autoJoinLobby = true;

        // the following line checks if this client was just created (and not yet online). if so, we connect
        if (PhotonNetwork.connectionStateDetailed == PeerState.PeerCreated)
        {
            // Connect to the photon master-server. We use the settings saved in PhotonServerSettings (a .asset file in this project)
            PhotonNetwork.ConnectUsingSettings("0.9");
        }

        // generate a name for this player, if none is assigned yet
        if (string.IsNullOrEmpty(PhotonNetwork.playerName))
        {
            PhotonNetwork.playerName = "Guest" + Random.Range(1, 9999);
        }

        // if you wanted more debug out, turn this on:
        // PhotonNetwork.logLevel = NetworkLogLevel.Full;

        //ネットワークエリア
        roomNameIF = networkArea.transform.FindChild("Room/Name").GetComponent<InputField>();
        roomStatusText = networkArea.transform.FindChild("RoomStatus").GetComponent<Text>();

        isNetworkMode = true;
    }

    //Photonへ接続開始
    private void ConnectStart()
    {
        isConnectFailed = false;
        isDialogOpen = false;
        DialogController.CloseDialog();
        PhotonNetwork.ConnectUsingSettings("0.9");
    }

    //ゲーム終了
    private void ExitGame()
    {
        Application.Quit();
    }

    //Room作成
    public void CreateRoom()
    {
        SwitchMessageArea("Create Room");
        PhotonNetwork.CreateRoom(roomNameIF.text, new RoomOptions() { maxPlayers = 2 }, null);
    }

    //入室
    public void JoinRoom(string roomName = "")
    {
        SwitchMessageArea("Join Room");
        if (roomName == "")
        {
            roomName = roomNameIF.text;
        }
        PhotonNetwork.JoinRoom(roomName);
    }

    //ランダム入室
    public void RandomJoinRoom()
    {
        SwitchMessageArea("Search Room");
        PhotonNetwork.JoinRandomRoom();
    }


    // ##### photon callback #####

    // We have two options here: we either joined(by title, list or random) or created a room.
    public void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
    }

    public void OnPhotonCreateRoomFailed()
    {
        SwitchMessageArea();
        DialogController.OpenDialog("Room作成に失敗しました\n既に存在するRoom名です");
        //ErrorDialog = "Error: Can't create room (room name maybe already used).";
        Debug.Log("OnPhotonCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
    }

    public void OnPhotonJoinRoomFailed(object[] cause)
    {
        SwitchMessageArea();
        DialogController.OpenDialog("参加可能なRoomがありません\nRoomを作成するか\n時間を空けて再度お試しください");
        //ErrorDialog = "Error: Can't join room (full or unknown room name). " + cause[1];
        Debug.Log("OnPhotonJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
    }

    public void OnPhotonRandomJoinFailed()
    {
        SwitchMessageArea();
        DialogController.OpenDialog("参加可能なRoomがありません\nRoomを作成するか\n時間を空けて再度お試しください");
        //ErrorDialog = "Error: Can't join random room (none found).";
        Debug.Log("OnPhotonRandomJoinFailed got called. Happens if no room is available (or all full or invisible or closed). JoinrRandom filter-options can limit available rooms.");
    }

    public void OnCreatedRoom()
    {
        //Debug.Log("OnCreatedRoom");
        PhotonNetwork.LoadLevel(Common.CO.SCEANE_BATTLE);
    }

    public void OnDisconnectedFromPhoton()
    {
        SwitchMessageArea();
        isConnectFailed = true;
        Debug.Log("Disconnected from Photon.");
    }

    public void OnFailedToConnectToPhoton(object parameters)
    {
        SwitchMessageArea();
        isConnectFailed = true;
        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.ServerAddress);
    }
}

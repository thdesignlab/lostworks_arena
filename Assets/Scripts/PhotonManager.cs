using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PhotonManager : MonoBehaviour {

    [SerializeField]
    private GUISkin Skin;
    //public Vector2 WidthAndHeight = new Vector2(400, 400);
    [SerializeField]
    private GameObject networkCanvas;

    //[SerializeField]
    //private int maxPlayerCount = 2;

    private string roomName = "room";

    //private Vector2 scrollPos = Vector2.zero;

    private bool connectFailed = false;

    //private bool isConnected = false;
    private Text messageText;
    private InputField roomNameIF;
    private Text roomStatusText;


    // シーン名定義
    public static readonly string SceneNameMenu = "Title";
    public static readonly string SceneNameGame = "Battle";

    private string errorDialog;
    private double timeToClearDialog;

    public string ErrorDialog
    {
        get { return errorDialog; }
        private set
        {
            errorDialog = value;
            if (!string.IsNullOrEmpty(value))
            {
                timeToClearDialog = Time.time + 4.0f;
            }
        }
    }

    public void Awake()
    {
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.automaticallySyncScene = true;

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

        StartCoroutine(WaitConnect());
    }

    IEnumerator WaitConnect()
    {
        for (;;)
        {
            if (PhotonNetwork.connected)
            {
                //部屋選択表示
                networkCanvas.SetActive(true);

                //フィールド取得
                messageText = networkCanvas.transform.FindChild("Title/Message").GetComponent<Text>();
                roomNameIF = networkCanvas.transform.FindChild("Title/Room/Name").GetComponent<InputField>();
                roomStatusText = networkCanvas.transform.FindChild("Title/RoomStatus").GetComponent<Text>();

                //初期値設定
                PlayerPrefs.SetString("playerName", PhotonNetwork.playerName);
                roomNameIF.text = roomName + (PhotonNetwork.countOfRooms + 1).ToString();
                roomStatusText.text = "Room :" + PhotonNetwork.countOfRooms + " / Player :" + PhotonNetwork.countOfPlayers;
                //isConnected = true;
                StartCoroutine(UpdateStatus());
                break;
            }
            yield return null;
        }
    }

    IEnumerator UpdateStatus()
    {
        for (;;)
        {
            //接続状況
            roomStatusText.text = "Room :" + PhotonNetwork.countOfRooms + " / Player :" + PhotonNetwork.countOfPlayers;

            yield return new WaitForSeconds(3.0f);
        }
    }

    //Room作成
    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(roomNameIF.text, new RoomOptions() { maxPlayers = 2 }, null);
    }

    //入室
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(roomNameIF.text);
    }

    //ランダム入室
    public void RandomJoinRoom()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnGUI()
    {
        if (Skin != null)
        {
            GUI.skin = Skin;
        }

        if (!PhotonNetwork.connected)
        {
            if (PhotonNetwork.connecting)
            {
                GUILayout.Label("Connecting to: " + PhotonNetwork.ServerAddress);
            }
            else
            {
                GUILayout.Label("Not connected. Check console output. Detailed connection state: " + PhotonNetwork.connectionStateDetailed + " Server: " + PhotonNetwork.ServerAddress);
            }

            if (connectFailed)
            {
                GUILayout.Label("Connection failed. Check setup and use Setup Wizard to fix configuration.");
                GUILayout.Label(string.Format("Server: {0}", new object[] { PhotonNetwork.ServerAddress }));
                GUILayout.Label("AppId: " + PhotonNetwork.PhotonServerSettings.AppID.Substring(0, 8) + "****"); // only show/log first 8 characters. never log the full AppId.

                if (GUILayout.Button("Try Again", GUILayout.Width(100)))
                {
                    connectFailed = false;
                    PhotonNetwork.ConnectUsingSettings("0.9");
                }
            }

            return;
        }

        //if (Screen.width < WidthAndHeight.x)
        //{
        //    WidthAndHeight.x = Screen.width * 0.8f;
        //}
        //if (Screen.height < WidthAndHeight.y)
        //{
        //    WidthAndHeight.y = Screen.height * 0.6f;
        //}

        //Rect content = new Rect((Screen.width - WidthAndHeight.x) / 2, (Screen.height - WidthAndHeight.y) / 2, WidthAndHeight.x, WidthAndHeight.y);
        //GUI.Box(content, "Join or Create Room");
        //GUILayout.BeginArea(content);

        //GUILayout.Space(40);

        ////int nameWidth = 150;
        ////int roomWidth = 150;
        ////int buttonWidth = 100;

        ////■ Player name
        //GUILayout.BeginHorizontal();
        //GUILayout.Label("Player name:");
        //PhotonNetwork.playerName = GUILayout.TextField(PhotonNetwork.playerName);
        ////GUILayout.Space(158);
        //if (GUI.changed)
        //{
        //    // Save name
        //    PlayerPrefs.SetString("playerName", PhotonNetwork.playerName);
        //}
        //GUILayout.EndHorizontal();


        //GUILayout.Space(10);

        ////■ Join room by title
        //GUILayout.BeginHorizontal();
        //GUILayout.Label("Room:");
        //roomName = GUILayout.TextField(roomName+(PhotonNetwork.countOfRooms+1).ToString());
        //GUILayout.EndHorizontal();

        //GUILayout.Space(10);

        //GUILayout.BeginHorizontal();
        //if (GUILayout.Button("新規作成"))
        //{
        //    PhotonNetwork.CreateRoom(roomName, new RoomOptions() { maxPlayers = 2 }, null);
        //}
        //GUILayout.EndHorizontal();

        //GUILayout.Space(10);

        ////■ Create a room (fails if exist!)
        //GUILayout.BeginHorizontal();
        ////GUILayout.FlexibleSpace();
        ////roomName = GUILayout.TextField(roomName);
        //if (GUILayout.Button("参加"))
        //{
        //    PhotonNetwork.JoinRoom(roomName);
        //}
        //GUILayout.EndHorizontal();


        //if (!string.IsNullOrEmpty(ErrorDialog))
        //{
        //    GUILayout.Label(ErrorDialog);

        //    if (timeToClearDialog < Time.time)
        //    {
        //        timeToClearDialog = 0;
        //        ErrorDialog = "";
        //    }
        //}
        if (!string.IsNullOrEmpty(ErrorDialog))
        {
            messageText.text = ErrorDialog;

            if (timeToClearDialog < Time.time)
            {
                timeToClearDialog = 0;
                ErrorDialog = "";
            }
        }


        //GUILayout.Space(10);

        ////■ Join random room
        //GUILayout.BeginHorizontal();
        ////GUILayout.Label(PhotonNetwork.countOfPlayers + " users are online in " + PhotonNetwork.countOfRooms + " rooms.");
        //GUILayout.Label("Room :"+PhotonNetwork.countOfRooms+" / Player :" + PhotonNetwork.countOfPlayers);
        //GUILayout.EndHorizontal();

        //GUILayout.Space(10);
        ////GUILayout.FlexibleSpace();

        //GUILayout.BeginHorizontal();
        //if (GUILayout.Button("ランダム参加"))
        //{
        //    PhotonNetwork.JoinRandomRoom();
        //}
        //GUILayout.EndHorizontal();

        ////GUILayout.Space(10);
        ////if (PhotonNetwork.GetRoomList().Length == 0)
        ////{
        ////    GUILayout.Label("Currently no games are available.");
        ////    GUILayout.Label("Rooms will be listed here, when they become available.");
        ////}
        ////else
        ////{
        ////    GUILayout.Label(PhotonNetwork.GetRoomList().Length + " rooms available:");

        ////    // Room listing: simply call GetRoomList: no need to fetch/poll whatever!
        ////    scrollPos = GUILayout.BeginScrollView(scrollPos);
        ////    foreach (RoomInfo roomInfo in PhotonNetwork.GetRoomList())
        ////    {
        ////        GUILayout.BeginHorizontal();
        ////        GUILayout.Label(roomInfo.name + " " + roomInfo.playerCount + "/" + roomInfo.maxPlayers);
        ////        if (GUILayout.Button("Join", GUILayout.Width(150)))
        ////        {
        ////            PhotonNetwork.JoinRoom(roomInfo.name);
        ////        }

        ////        GUILayout.EndHorizontal();
        ////    }

        ////    GUILayout.EndScrollView();
        ////}

        //GUILayout.EndArea();
    }

    // We have two options here: we either joined(by title, list or random) or created a room.
    public void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom");
    }

    public void OnPhotonCreateRoomFailed()
    {
        ErrorDialog = "Error: Can't create room (room name maybe already used).";
        Debug.Log("OnPhotonCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
    }

    public void OnPhotonJoinRoomFailed(object[] cause)
    {
        ErrorDialog = "Error: Can't join room (full or unknown room name). " + cause[1];
        Debug.Log("OnPhotonJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
    }

    public void OnPhotonRandomJoinFailed()
    {
        ErrorDialog = "Error: Can't join random room (none found).";
        Debug.Log("OnPhotonRandomJoinFailed got called. Happens if no room is available (or all full or invisible or closed). JoinrRandom filter-options can limit available rooms.");
    }

    public void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
        PhotonNetwork.LoadLevel(SceneNameGame);
    }

    public void OnDisconnectedFromPhoton()
    {
        Debug.Log("Disconnected from Photon.");
    }

    public void OnFailedToConnectToPhoton(object parameters)
    {
        connectFailed = true;
        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters + " ServerAddress: " + PhotonNetwork.ServerAddress);
    }
}

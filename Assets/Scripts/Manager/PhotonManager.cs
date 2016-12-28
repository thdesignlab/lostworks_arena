using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text.RegularExpressions;


public class PhotonManager : MonoBehaviour
{
    public static bool isFirstScean = true;
    public static bool isPlayAd = false;
    public static bool isReadyGame = false;

    [SerializeField]
    private Transform titleCanvas;
    [SerializeField]
    private GameObject modeSelectArea;
    [SerializeField]
    private GameObject networkArea;
    [SerializeField]
    private GameObject roomListArea;
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
    //const int SELECTABLE_MODE_LOCAL = 1;
    //const int SELECTABLE_MODE_NETWORK = 2;
    //const int SELECTABLE_MODE_BOTH = 3;
    //private int selectableMode = SELECTABLE_MODE_BOTH;

    private int maxRoomCount = 10;
    const int MAX_ROOM_PLAYER_COUNT = 2;
    const string ROOM_NAME_PREFIX = "Room";

    //private bool isTapToStart = false;
    private bool isConnectFailed = false;
    private bool isDialogOpen = false;
    private bool isNetworkMode = false;
    private bool preConnectedAndReady = false;

    private float processTime = 0;

    private Quaternion topCameraQuat = Quaternion.AngleAxis(0, Vector3.up);
    private Quaternion otherCameraQuat = Quaternion.AngleAxis(45, Vector3.up);
    private float camRotateTime = 0.5f;

    const float ROOM_LIST_RELOAD_TIME = 5;
    //private float roomListReloadTime = 0;

    //private InputField roomNameIF;
    private Text roomStatusText;

    private string moveScene = "";
    //private string loadmessage = "";

    public const string MESSAGE_CONNECT_FAILED = "ネットワーク接続に失敗しました\n通信状況をご確認の上\n再度お試しください";
    const string MESSAGE_CREATE_ROOM_FAILED = "Room作成に失敗しました\n既に存在するRoom名です";
    const string MESSAGE_ROOM_LIMIT_FAILED = "Room作成に失敗しました\nこれ以上Roomを作成できません";
    const string MESSAGE_JOIN_ROOM_FAILED = "参加可能なRoomがありません\nRoomを作成するか\n時間を空けてから再度お試しください";

    public void Awake()
    {
        //ステータスバー
        Common.Func.SetStatusbar();

        GameObject MenuFade = titleCanvas.FindChild("FadeLeft").gameObject;
        GameObject titleLogo = titleCanvas.FindChild("TitleLogo").gameObject;

        //初期化
        if (isFirstScean)
        {
            isFirstScean = false;
            isReadyGame = false;
            MenuFade.SetActive(false);
            titleLogo.SetActive(true);
            Init();

            //ユーザー情報取得
            UserManager.SetPlayerPrefs();
        }
        else
        {
            isReadyGame = true;
            MenuFade.SetActive(true);
            titleLogo.SetActive(false);
            ReturnModeSelect();
        }

        //フレームレート
        Application.targetFrameRate = 30;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (!pauseStatus)
        {
            //ステータスバー
            Common.Func.SetStatusbar();
        }
    }

    IEnumerator Start()
    {
        if (isReadyGame) yield break;

        //スプラッシュ終了待ち
        for (;;)
        {
            yield return null;
            if (!Application.isShowingSplashScreen) break;
        }

        //TapToStart点灯
        GameObject message = null;
        Image messageImage = null;
        Text messageText = null;
        for (;;)
        {
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
                message = DialogController.OpenMessage(DialogController.MESSAGE_TOP, DialogController.MESSAGE_POSITION_CENTER);
                messageImage = DialogController.GetMessageImageObj();
                messageText = DialogController.GetMessageTextObj();
            }

            //タップ判定
            if (Input.GetMouseButtonDown(0)) break;
            yield return null;
        }
        messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 1);
        messageImage.color = new Color(messageImage.color.r, messageImage.color.g, messageImage.color.b, 1);

        //初期設定読み込み
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);
        InitApi();
        for (;;)
        {
            if (isReadyGame)
            {
                TapToStart();
                break;
            }
            yield return null;
        }
    }

    void Update()
    {
        if (!isReadyGame) return;

        if (isNetworkMode)
        {
            if (PhotonNetwork.connected && !isConnectFailed)
            {
                // *** 接続成功 ***
                if (!PhotonNetwork.connectedAndReady)
                {
                    //準備中
                    DialogController.OpenMessage(DialogController.MESSAGE_CONNECT, DialogController.MESSAGE_POSITION_RIGHT);
                    return;
                }
                if (!preConnectedAndReady)
                {
                    //準備完了
                    preConnectedAndReady = true;
                    DialogController.CloseMessage();
                }

                if (networkArea.GetActive())
                {
                    //接続状況更新
                    roomStatusText.text = "Room :" + PhotonNetwork.countOfRooms + " / Player :" + PhotonNetwork.countOfPlayers;
                }
                else
                {
                    //部屋選択表示
                    SwitchNetworkArea(true, true);

                    //初期値設定
                    PlayerPrefs.SetString("playerName", PhotonNetwork.playerName);
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
            Camera.main.transform.localRotation = otherCameraQuat;
        }
        SwitchNetworkArea(false);
        isConnectFailed = false;
        isNetworkMode = false;
        if (PhotonNetwork.connected) PhotonNetwork.Disconnect();
    }

    public void ReturnModeSelect()
    {
        if (isPlayAd)
        {
            //広告表示
            UnityAds.Instance.Play();
            isPlayAd = false;
        }

        UnityAction collback = () => { 
            Init(true);
            SwitchModeSelectArea(true, true);
            DialogController.CloseMessage();
        };
        CameraRotate(true, collback);
    }

    //モードセレクトダイアログ切り替え
    public void SwitchModeSelectArea(bool flg, bool isFade = false)
    {
        if (isFade)
        {
            ScreenManager.Instance.FadeUI(modeSelectArea, flg);
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
            ScreenManager.Instance.FadeUI(networkArea.gameObject, flg, false);
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
    const float DISP_ROOM_LIST_LIMIT = 60;
    Coroutine checkRoomList;
    public void OnSwitchRoomListAreaButton(bool flg)
    {
        SwitchRoomListArea(flg, true);
    }
    private void SwitchRoomListArea(bool flg, bool isFade = false)
    {
        if (isFade)
        {
            ScreenManager.Instance.FadeUI(roomListArea.gameObject, flg, false);
        }
        else
        {
            roomListArea.SetActive(flg);
        }
        
        if (flg)
        {
            SearchRoomList();
            checkRoomList = StartCoroutine(CheckRoomListLimit());
        }
        else
        {
            if (checkRoomList != null) StopCoroutine(checkRoomList);
        }
    }
    IEnumerator CheckRoomListLimit()
    {
        float roomListTime = 0;
        for (;;)
        {
            roomListTime += Time.deltaTime;
            if (roomListTime >= DISP_ROOM_LIST_LIMIT) break;
            yield return null;
        }
        SwitchRoomListArea(false);
    }

    //Room一覧更新
    private void SearchRoomList()
    {
        Action searchRoomListCallback = () =>
        {
            int roomCnt = 0;
            RectTransform roomListContent = roomListArea.transform.FindChild("Viewport/Content").GetComponent<RectTransform>();

            //中身をリセットする
            foreach (Transform child in roomListContent)
            {
                Destroy(child.gameObject);
            }

            //キャンセルボタン
            GameObject cancelBtn = (GameObject)Instantiate(roomCancelBtn);
            cancelBtn.transform.SetParent(roomListContent, false);
            cancelBtn.GetComponent<Button>().onClick.AddListener(() => SwitchRoomListArea(false, true));
            roomCnt++;

            //Room一覧取得
            RoomInfo[] roomList = PhotonNetwork.GetRoomList();
            if (roomList.Length > 0)
            {
                foreach (RoomInfo room in roomList)
                {
                    //string roomName = Regex.Replace(room.name, "_[0-9]*$", "");
                    string roomName = GetRoomName(room.name);
                    GameObject roomBtn = roomSelectBtn;
                    if (room.playerCount >= MAX_ROOM_PLAYER_COUNT)
                    {
                        roomBtn = roomFullBtn;
                    }
                    GameObject joinBtn = (GameObject)Instantiate(roomBtn);
                    joinBtn.transform.SetParent(roomListContent, false);
                    joinBtn.GetComponent<Button>().onClick.AddListener(() => JoinRoom(room));
                    joinBtn.transform.FindChild("Text").GetComponent<Text>().text = roomName;
                    roomCnt++;
                }
            }
            roomListContent.sizeDelta = new Vector2(0, 200 * roomCnt + 50);
        };

        //Roomデータ取得
        RoomApi.Get roomApiGet = new RoomApi.Get();
        roomApiGet.SetApiFinishCallback(searchRoomListCallback);
        roomApiGet.Exe();
    }
    private string GetRoomName(string roomKey)
    {
        string roomName = ROOM_NAME_PREFIX;
        foreach (RoomData roomData in ModelManager.roomDataList)
        {
            if (roomData.room_key != roomKey) continue;
            roomName = roomData.room_name;
            break;
        }
        return roomName;
    }

    //タイトル画面から進む
    private void TapToStart()
    {
        ScreenManager.Instance.Load(Common.CO.SCENE_TITLE, DialogController.MESSAGE_LOADING);

        ////現在はBOTHのみ
        //switch (selectableMode)
        //{
        //    case SELECTABLE_MODE_LOCAL:
        //        //ローカルのみ
        //        DialogController.CloseMessage();
        //        break;

        //    case SELECTABLE_MODE_NETWORK:
        //        //ネットワークのみ
        //        DialogController.CloseMessage();
        //        break;

        //    case SELECTABLE_MODE_BOTH:
        //        //ローカル+ネットワーク
        //        DialogController.CloseMessage();
        //        break;
        //}
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
        DialogController.OpenMessage(DialogController.MESSAGE_CONNECT, DialogController.MESSAGE_POSITION_RIGHT);

        Action callback = () =>
        {
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
            //roomNameIF = networkArea.transform.FindChild("Network/Room/Name").GetComponent<InputField>();
            roomStatusText = networkArea.transform.FindChild("Network/RoomStatus").GetComponent<Text>();

            UnityAction uniAction = () =>
            {
                preConnectedAndReady = false;
                isNetworkMode = true;
            };
            CameraRotate(false, uniAction);
        };

        //ユーザー情報取得
        Action getUserDataCallback = () => GetBattleResult(callback);
        GetUserData(getUserDataCallback);
    }

    //カスタマイズ
    public void CustomSelect()
    {
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);

        PhotonNetwork.offlineMode = true;
        moveScene = Common.CO.SCENE_CUSTOM;
        PhotonNetwork.CreateRoom(ROOM_NAME_PREFIX);
    }

    //Store
    public void OnStoreButton()
    {
        DialogController.OpenMessage(DialogController.MESSAGE_CONNECT, DialogController.MESSAGE_POSITION_RIGHT);

        Action callback = () =>
        {
            ScreenManager.Instance.Load(Common.CO.SCENE_STORE, DialogController.MESSAGE_LOADING);
        };

        //ポイント情報取得
        GetUserPoint(callback);
    }

    //コンフィグ
    public void OnConfigButton()
    {
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);

        SwitchModeSelectArea(false, true);
        UnityAction callback = () => ConfigManager.Instance.OpenConfig();
        CameraRotate(false, callback);
    }

    //ランキング
    public void OnRankingButton()
    {
        ScreenManager.Instance.Load(Common.CO.SCENE_RANKING, DialogController.MESSAGE_LOADING);
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
        if (!PhotonNetwork.connectedAndReady) return;

        DialogController.OpenMessage(DialogController.MESSAGE_CREATE_ROOM, DialogController.MESSAGE_POSITION_RIGHT);

        Action createRoomCallback = () =>
        {
            if (PhotonNetwork.countOfRooms >= maxRoomCount)
            {
                DialogController.OpenDialog(MESSAGE_ROOM_LIMIT_FAILED);
                return;
            }

            string roomKey = ModelManager.roomData.room_key;
            PhotonNetwork.CreateRoom(roomKey, new RoomOptions() { maxPlayers = 2, PlayerTtl = 1000 }, null);
        };

        RoomApi.Create roomApiCreate = new RoomApi.Create();
        roomApiCreate.SetApiFinishCallback(createRoomCallback);
        roomApiCreate.Exe();
    }

    //入室
    private void JoinRoom(RoomInfo roomInfo = null)
    {
        if (!PhotonNetwork.connectedAndReady) return;

        string roomName = "";
        if (roomInfo != null) roomName = roomInfo.name;
        JoinRoom(roomName);
    }
    private void JoinRoom(string roomName = "")
    {
        if (!PhotonNetwork.connectedAndReady) return;
        if (roomName == "")
        {
            RandomJoinRoom();
            return;
        }

        DialogController.OpenMessage(DialogController.MESSAGE_JOIN_ROOM, DialogController.MESSAGE_POSITION_RIGHT);

        Action callback = () =>
        {
            PhotonNetwork.JoinRoom(roomName);
        };

        RoomApi.Clear roomApiClear = new RoomApi.Clear();
        roomApiClear.SetApiFinishCallback(callback);
        roomApiClear.Exe();
    }

    //ランダム入室
    public void RandomJoinRoom()
    {
        DialogController.OpenMessage(DialogController.MESSAGE_SEARCH_ROOM, DialogController.MESSAGE_POSITION_RIGHT);
        Action callback = () =>
        {
            PhotonNetwork.JoinRandomRoom();
        };

        RoomApi.Clear roomApiClear = new RoomApi.Clear();
        roomApiClear.SetApiFinishCallback(callback);
        roomApiClear.Exe();
    }


    //カメラ切り替え
    private void CameraRotate(bool isTop = true, UnityAction callback = null)
    {
        Quaternion quat = topCameraQuat;
        if (!isTop) quat = otherCameraQuat;
        //Camera.main.transform.localRotation = quat;
        StartCoroutine(CameraRotateProc(quat, callback));
    }
    IEnumerator CameraRotateProc(Quaternion toQuat, UnityAction callback)
    {
        float time = 0;
        Quaternion fromQuat = Camera.main.transform.localRotation;
        for (;;)
        {
            time += Time.deltaTime;
            float rate = time / camRotateTime;
            Camera.main.transform.localRotation = Quaternion.Lerp(fromQuat, toQuat, rate);
            if (rate > 1) break;
            yield return null;
        }

        if (callback != null) callback.Invoke();
    }

    // ##### photon callback #####

    // We have two options here: we either joined(by title, list or random) or created a room.
    public void OnJoinedRoom()
    {
        //switch (moveScene)
        //{
        //    case Common.CO.SCENE_BATTLE:
        //        if (isNetworkMode) isPlayAd = true;
        //        break;
        //}
    }

    public void OnPhotonCreateRoomFailed()
    {
        DialogController.OpenDialog(MESSAGE_CREATE_ROOM_FAILED);
        //ErrorDialog = "Error: Can't create room (room name maybe already used).";
        Debug.Log("OnPhotonCreateRoomFailed got called. This can happen if the room exists (even if not visible). Try another room name.");
    }

    public void OnPhotonJoinRoomFailed(object[] cause)
    {
        DialogController.OpenDialog(MESSAGE_JOIN_ROOM_FAILED);
        //ErrorDialog = "Error: Can't join room (full or unknown room name). " + cause[1];
        Debug.Log("OnPhotonJoinRoomFailed got called. This can happen if the room is not existing or full or closed.");
    }

    public void OnPhotonRandomJoinFailed()
    {
        DialogController.OpenDialog(MESSAGE_JOIN_ROOM_FAILED);
        //ErrorDialog = "Error: Can't join random room (none found).";
        Debug.Log("OnPhotonRandomJoinFailed got called. Happens if no room is available (or all full or invisible or closed). JoinrRandom filter-options can limit available rooms.");
    }

    public void OnCreatedRoom()
    {
        //Debug.Log("OnCreatedRoom");
        ScreenManager.Instance.Load(moveScene);
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

    public void OnReceivedRoomListUpdate()
    {
        //ルームリスト更新
        if (roomListArea.GetActive()) SearchRoomList();
    }

    //##### 登録情報取得 #####

    //bool isFinishInitApi = false;
    private void InitApi()
    {
        Action gameConfigCallback = () => GetUserData(FinishInitApi, true);
        GetGameConfig(gameConfigCallback);
    }
    private void FinishInitApi()
    {
        isReadyGame = true;
    }

    //ユーザー情報取得
    private void GetUserData(Action callback = null, bool isIgnoreError = false)
    {
        //ユーザー情報
        User.Get userGet = new User.Get();
        if (isIgnoreError)
        {
            userGet.SetNextAction(callback);
            userGet.SetApiErrorIngnore();
        }
        else
        {
            userGet.SetApiFinishCallback(callback);
        }
        userGet.Exe();
    }

    //戦績取得
    private void GetBattleResult(Action callback = null, bool isIgnoreError = false)
    {
        Battle.Record battleRecord = new Battle.Record();
        if (isIgnoreError)
        {
            battleRecord.SetNextAction(callback);
            battleRecord.SetApiErrorIngnore();
        }
        else
        {
            battleRecord.SetApiFinishCallback(callback);
        }
        battleRecord.CheckVersion(false);
        battleRecord.Exe();
    }

    //ポイント情報取得
    private void GetUserPoint(Action callback = null, bool isIgnoreError = false)
    {
        //所持ポイント取得
        Point.Get pointGet = new Point.Get();
        if (isIgnoreError)
        {
            pointGet.SetNextAction(callback);
            pointGet.SetApiErrorIngnore();
        }
        else
        {
            pointGet.SetApiFinishCallback(callback);
        }
        pointGet.Exe();
    }

    //ゲーム設定読み込み
    private void GetGameConfig(Action callback = null)
    {
        if (ModelManager.gameConfigList != null)
        {
            if (callback != null) callback.Invoke();
            return;
        }
        Action finishCallback = () => {
            GameConfigCallback();
            if (callback != null) callback.Invoke();
        };

        Game.Config gameConfig = new Game.Config();
        gameConfig.SetApiFinishCallback(finishCallback);
        gameConfig.SetApiFinishErrorCallback(FinishInitApi);
        gameConfig.SetConnectErrorCallback(FinishInitApi);
        gameConfig.SetApiErrorIngnore();
        gameConfig.SetRetryCount(1);
        //gameConfig.CheckVersion(true);
        gameConfig.Exe();
    }
    private void GameConfigCallback()
    {
        maxRoomCount = GameConfigManager.getMaxRoomCount(maxRoomCount);
    }
}

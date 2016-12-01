using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class MenuController : Photon.MonoBehaviour
{
    [SerializeField]
    private GameObject debugButton;
    [SerializeField]
    private GameObject debugMenu;
    [SerializeField]
    private GameObject npcMenu;
    [SerializeField]
    private float switchTime = 0.3f;
    [SerializeField]
    private bool isRight = false;

    private Transform _myTran;
    private Transform myTran
    {
        get { return _myTran ? _myTran : _myTran = transform; }
    }
    private GameObject pauseButton;
    private GameObject npcButton;
    //private Camera mainCam;
    //private GameObject camObj;

    private bool isMenuOpen = false;
    private bool enableMenuAction = true;
    private bool isEnabledDebug = false;

    const string MENU_BUTTONS = "MenuButtons";
    const string BUTTON_PAUSE = "PauseButton";
    const string BUTTON_CPU_BATTLE = "NpcButton";

    private Vector3 defaultMenuPos;

    void Awake()
    {
        defaultMenuPos = myTran.localPosition;
    }

    void Start()
    { 
        Transform menuObj = myTran.FindChild(MENU_BUTTONS);
        //一時停止ボタン
        Transform pauseTran = menuObj.FindChild(BUTTON_PAUSE);
        if (pauseTran != null) pauseButton = menuObj.FindChild(BUTTON_PAUSE).gameObject;
        //CPU生成ボタン
        Transform npcTran = menuObj.FindChild(BUTTON_CPU_BATTLE);
        if (npcTran != null) npcButton = npcTran.gameObject;

        bool isEnabledPause = false;
        bool isEnabledCpu = false;

        switch (GameController.Instance.gameMode)
        {
            case GameController.GAME_MODE_MISSION:
                isEnabledPause = true;
                isEnabledCpu = false;
                break;

            case GameController.GAME_MODE_VS:
                isEnabledPause = false;
                isEnabledCpu = true;
                break;
        }

        //一時停止ボタンON/OFF
        if (pauseButton != null) pauseButton.SetActive(isEnabledPause);

        //CPU生成ボタンON/OFF
        if (npcButton != null) npcButton.SetActive(isEnabledCpu);

        //デバッグボタンON/OFF
        if (MyDebug.Instance.isDebugMode || UserManager.isAdmin) isEnabledDebug = true;
        if (debugButton != null) debugButton.SetActive(isEnabledDebug);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isMenuOpen && enableMenuAction)
            {
                //メニューが開いている場合画面タップで閉じる
                StartCoroutine(MenuSlide());
            }
        }
    }

    void OnEnable()
    {
        if (defaultMenuPos != null) myTran.localPosition = defaultMenuPos;
        enableMenuAction = true;
    }

    public void OnMenuToggleButton()
    {
        if (enableMenuAction)
        {
            StartCoroutine(MenuSlide());
        }
    }
    IEnumerator MenuSlide()
    {
        enableMenuAction = false;
        float slideLength = debugButton.GetComponent<RectTransform>().rect.width;
        if (!isRight) slideLength *= -1;

        float totalSlide = 0;
        Vector3 slideVector = Vector3.left;
        if (isMenuOpen)
        {
            yield return new WaitForSeconds(0.5f);
            slideVector *= -1;
            isMenuOpen = false;
        }
        else
        {
            isMenuOpen = true;
        }

        for (;;)
        {
            float slide = slideLength * Time.deltaTime / switchTime;
            if (Mathf.Abs(totalSlide + slide) > Mathf.Abs(slideLength))
            {
                slide = slideLength - totalSlide;
            }
            myTran.localPosition += slideVector * slide;

            totalSlide += slide;
            if (Mathf.Abs(totalSlide) >= Mathf.Abs(slideLength)) break;
            yield return null;
        }

        enableMenuAction = true;
    }

    //##### 通常メニュー #####

    //アプリ終了
    public void OnExitButton()
    {
        DialogController.OpenDialog("アプリを終了します", () => GameController.Instance.Exit(), true);
    }

    //タイトルへ戻る
    public void OnTitleButton()
    {
        UnityAction callback = () =>
        {
            GameController.Instance.GoToTitle();
        };

        DialogController.OpenDialog("タイトルに戻ります", callback, true);
    }

    //一時停止
    public void OnPauseButton()
    {
        GameController.Instance.Pause();
    }

    //NPC生成
    public void OnCreateNpcButton()
    {
        GameController.Instance.SpawnTargetNpc();
    }


    //##### デバッグメニュー(管理者のみ実行可能) #####

    //デバッグメニュー表示切り替え
    public void OnDebugMenuButton(bool flg)
    {
        if (!isEnabledDebug) return;

        if (debugMenu != null)
        {
            debugMenu.SetActive(flg);
        }
    }

    //デバッグ機能共通
    private bool CommonDebug()
    {
        OnDebugMenuButton(false);
        return isEnabledDebug;
    }

    //装備カスタム
    public void OnCustomButton()
    {
        if (!CommonDebug()) return;
        if (GameController.Instance.GetMyTran() == null) return;
        WeaponStore.Instance.CustomMenuOpen();
    }

    //バトルログ表示
    public void OnBattleLogButton()
    {
        if (!CommonDebug()) return;
        if (GameController.Instance.GetMyTran() == null) return;
        PlayerStatus status = GameController.Instance.GetMyTran().GetComponent<PlayerStatus>();
        if (status != null) status.SwitchBattleLog();
    }

    //コンフィグ
    public void OnConfigButton()
    {
        if (!CommonDebug()) return;
        ConfigManager.Instance.OpenConfig();
    }

    //強制勝利
    public void OnForceWin()
    {
        if (!CommonDebug()) return;
        GameController.Instance.SetFinalRound();
        GameController.Instance.CleanNpc();
    }

    //強制敗北
    public void OnForceLose()
    {
        if (!CommonDebug()) return;
        GameController.Instance.SetFinalRound();
        GameController.Instance.CleanPlayer();
    }

    //端末情報リセット
    public void ResetUserInfo()
    {
        if (!CommonDebug()) return;
        DialogController.OpenDialog("端末内のユーザー情報を削除します", () => ResetUserInfoExe(), true);
    }
    public void ResetUserInfoExe()
    {
        if (!CommonDebug()) return;
        UserManager.DeleteUser();
        GameController.Instance.ReStart();
    }
    //武器使用フリー
    public void WeaponFree()
    {
        if (!CommonDebug()) return;
        foreach (int partsNo in Common.CO.partsNameArray.Keys)
        {
            Transform playerTran = GameController.Instance.GetMyTran();
            string partsName = Common.Func.GetPartsStructure(Common.CO.partsNameArray[partsNo]);
            Transform parts = playerTran.FindChild(partsName);
            if (parts == null) continue;
            WeaponController wepCtrl = parts.GetComponentInChildren<WeaponController>();
            if (wepCtrl == null) continue;
            wepCtrl.ReloadFree();
            ExtraWeaponController extraCtrl = parts.GetComponentInChildren<ExtraWeaponController>();
            if (extraCtrl != null) extraCtrl.ExtraFree();
        }
    }

    ////UI隠し
    //public void OnHideUI()
    //{
    //    if (!CommonDebug()) return;
    //    mainCam = Camera.main;
    //    Transform mainCamTran = Camera.main.transform;
    //    GameObject cam = new GameObject("Camera");
    //    cam.AddComponent<Camera>();
    //    cam.AddComponent<FlareLayer>();
    //    cam.AddComponent<GUILayer>();
    //    cam.GetComponent<Camera>().depth = 100;

    //    Debug.Log(cam);
    //    camObj = (GameObject)Instantiate(cam, mainCamTran.position, mainCamTran.rotation);
    //    Debug.Log(camObj);
    //    camObj.transform.SetParent(mainCamTran.parent, false);
    //}

}

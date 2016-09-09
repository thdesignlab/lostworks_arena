using UnityEngine;
using System.Collections;

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

    private Transform myTran;
    private GameObject pauseButton;
    private GameObject npcButton;

    private bool isMenuOpen = false;
    private bool enableMenuAction = true;
    private bool isEnabledDebug = false;

    const string MENU_BUTTONS = "MenuButtons";
    const string BUTTON_PAUSE = "PauseButton";
    const string BUTTON_CPU_BATTLE = "NpcButton";

    void Awake()
    {
        myTran = transform;
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
            case GameController.GAME_MODE_PLACTICE:
                isEnabledPause = true;
                isEnabledCpu = true;
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
        Debug.Log(MyDebug.Instance.isDebugMode +" || "+ UserManager.isAdmin);
        Debug.Log("isEnabledDebug:" + isEnabledDebug);
        Debug.Log("debugButton :" + debugButton);
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
        DialogController.OpenDialog("タイトルに戻ります", () => GameController.Instance.GoToTitle(), true);
    }

    //一時停止
    public void OnPauseButton()
    {
        GameController.Instance.Pause();
    }

    //NPC選択表示切替
    public void OnNpcSelectButton(bool flg)
    {
        npcMenu.SetActive(flg);
    }

    //NPC生成
    public void OnNpcCreateButton(int charaNo = 0)
    {
        if (GameController.Instance.gameMode == GameController.GAME_MODE_MISSION) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (PhotonNetwork.countOfPlayersInRooms > 1 || players.Length > 1)
        {
            return;
        }
        GameController.Instance.NpcSpawn(charaNo);
        OnNpcSelectButton(false);
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

    ////復活
    //public void OnRespawnButton()
    //{
    //    if (!CommonDebug()) return;
    //    if (GameController.Instance.GetMyTran() != null) return;

    //    Destroy(Camera.main.gameObject);
    //    GameController.Instance.SpawnMyPlayerEverywhere();
    //}

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
        GameController.Instance.CleanNpc();
    }

    //強制敗北
    public void OnForceLose()
    {
        if (!CommonDebug()) return;
        GameController.Instance.CleanPlayer();
    }
}

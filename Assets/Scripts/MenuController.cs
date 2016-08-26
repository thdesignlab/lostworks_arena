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
    private GameController gameCtrl;
    private GameObject pauseButton;
    private GameObject npcButton;

    private bool isMenuOpen = false;
    private bool enableMenuAction = true;

    const string MENU_BUTTONS = "MenuButtons";
    const string BUTTON_PAUSE = "PauseButton";
    const string BUTTON_CPU_BATTLE = "NpcButton";

    void Awake()
    {
        myTran = transform;
        Transform menuObj = myTran.FindChild(MENU_BUTTONS);
        //一時停止ボタン
        Transform pauseTran = menuObj.FindChild(BUTTON_PAUSE);
        if (pauseTran != null) pauseButton = pauseTran.gameObject;
        //CPU生成ボタン
        Transform npcTran = menuObj.FindChild(BUTTON_CPU_BATTLE);
        if (npcTran != null) npcButton = npcTran.gameObject;

        gameCtrl = GameObject.Find("GameController").GetComponent<GameController>();

        bool isDebug = false;
        bool isEnabledCpu = false;
        if (gameCtrl != null)
        {
            //デバッグモードチェック
            isDebug = gameCtrl.isDebugMode;

            switch (gameCtrl.gameMode)
            {
                case GameController.GAME_MODE_PLACTICE:
                    //CPU生成ボタン表示
                    isEnabledCpu = true;
                    break;

                case GameController.GAME_MODE_VS:
                    //一時停止禁止
                    pauseButton.SetActive(false);
                    //CPU生成ボタン表示
                    isEnabledCpu = true;
                    break;
            }
        }

        //CPU生成ボタンON/OFF
        npcButton.SetActive(isEnabledCpu);

        //デバッグボタンON/OFF
        debugButton.SetActive(isDebug);
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
        DialogController.OpenDialog("アプリを終了します", () => gameCtrl.Exit(), true);
    }

    //タイトルへ戻る
    public void OnTitleButton()
    {
        DialogController.OpenDialog("タイトルに戻ります", () => gameCtrl.GoToTitle(), true);
    }

    //一時停止
    public void OnPauseButton()
    {
        if (gameCtrl == null || gameCtrl.gameMode == GameController.GAME_MODE_VS)
        {
            //一時停止禁止
            return;
        }

        gameCtrl.isPause = true;
        DialogController.OpenDialog("一時停止中", "再開", () => ResetPause(), false);
        Time.timeScale = 0;
    }
    public void ResetPause()
    {
        gameCtrl.isPause = false;
        Time.timeScale = 1;
    }

    //NPC選択表示切替
    public void OnNpcSelectButton(bool flg)
    {
        npcMenu.SetActive(flg);
    }

    //NPC生成
    public void OnNpcCreateButton(int charaNo = 0)
    {
        if (gameCtrl.gameMode == GameController.GAME_MODE_MISSION) return;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (PhotonNetwork.countOfPlayersInRooms > 1 || players.Length > 1)
        {
            return;
        }
        gameCtrl.NpcSpawn(charaNo);
        OnNpcSelectButton(false);
    }

    //コンフィグ
    public void OnConfigButton()
    {
        GameObject.Find("Config").GetComponent<ConfigManager>().OpenConfig();
    }

    //##### デバッグメニュー #####

    //デバッグメニュー表示切り替え
    public void OnDebugMenuButton(bool flg)
    {
        if (debugMenu != null)
        {
            debugMenu.SetActive(flg);
        }
    }

    //復活
    public void OnRespawnButton()
    {
        OnDebugMenuButton(false);
        if (!gameCtrl.isDebugMode) return;
        if (gameCtrl.GetMyTran() != null) return;

        Destroy(Camera.main.gameObject);
        gameCtrl.SpawnMyPlayerEverywhere();
    }

    //装備カスタム
    public void OnCustomButton()
    {
        OnDebugMenuButton(false);
        if (!gameCtrl.isDebugMode) return;
        GameObject.Find("WeaponStore").GetComponent<WeaponStore>().CustomMenuOpen();
    }

    //バトルログ表示
    public void OnBattleLogButton()
    {
        OnDebugMenuButton(false);
        if (!gameCtrl.isDebugMode) return;
        PlayerStatus status = myTran.root.GetComponent<PlayerStatus>();
        if (status != null) status.SwitchBattleLog();
    }
}

using UnityEngine;
using System.Collections;

public class MenuController : Photon.MonoBehaviour
{
    [SerializeField]
    private GameObject pauseButton;
    [SerializeField]
    private GameObject debugButton;
    [SerializeField]
    private GameObject npcMenu;
    [SerializeField]
    private GameObject debugMenu;
    [SerializeField]
    private float switchTime = 0.3f;
    [SerializeField]
    private bool isRight = false;

    private Transform myTran;
    private GameController gameCtrl;

    private bool isMenuOpen = false;
    private bool enableMenuAction = true;

    void Awake()
    {
        myTran = transform;

        gameCtrl = GameObject.Find("GameController").GetComponent<GameController>();
        if (gameCtrl == null || gameCtrl.gameMode == GameController.GAME_MODE_VS)
        {
            //一時停止禁止
            pauseButton.SetActive(false);
        }
        //デバッグボタンON/OFF
        bool isDebug = false;
        if (gameCtrl != null) isDebug = gameCtrl.isDebugMode;
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
}

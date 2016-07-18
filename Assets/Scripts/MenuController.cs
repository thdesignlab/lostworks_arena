using UnityEngine;
using System.Collections;

public class MenuController : Photon.MonoBehaviour
{
    [SerializeField]
    private GameObject debugButton;
    [SerializeField]
    private GameObject debugMenu;
    [SerializeField]
    private float switchTime = 1.0f;
    [SerializeField]
    private float slideLength = 400;

    private Transform myTran;
    private GameController gameCtrl;

    private bool isMenuOpen = false;
    private bool enableMenuAction = true;

    void Awake()
    {
        myTran = transform;
        if (slideLength == 0)
        {
            myTran.gameObject.SetActive(false); 
        }

        gameCtrl = GameObject.Find("GameController").GetComponent<GameController>();
    }

    void Start()
    {
        //デバッグボタンON/OFF
        debugButton.SetActive(gameCtrl.isDebugMode);
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

    public void OnTitleButton()
    {
        GameObject.Find("GameController").GetComponent<GameController>().GoToTitle();
    }

    public void OnCustomButton()
    {
        GameObject.Find("WeaponStore").GetComponent<WeaponStore>().CustomMenuOpen();
    }


    //##### デバッグメニュー #####


    public void OnDebugMenuButton(bool flg)
    {
        if (debugMenu != null)
        {
            debugMenu.SetActive(flg);
        }
    }

    public void OnRespawnButton()
    {
        if (!gameCtrl.isDebugMode) return;
        //PhotonNetwork.LoadLevel("Battle");
        if (gameCtrl.GetMyTran() != null) return;

        Destroy(Camera.main.gameObject);
        gameCtrl.SpawnMyPlayerEverywhere();
        debugMenu.SetActive(false);
    }

    public void OnNpcCreateButton(int level = 0)
    {
        if (!gameCtrl.isDebugMode) return;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        if (PhotonNetwork.countOfPlayersInRooms > 1 || players.Length > 1)
        {
            return;
        }
        gameCtrl.NpcSpawn(level);
        debugMenu.SetActive(false);
    }
}

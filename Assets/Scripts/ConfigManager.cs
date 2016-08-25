using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ConfigManager : MonoBehaviour
{
    [SerializeField]
    private GameObject configCanvas;

    private ScreenManager screenMgr;

    void Awake()
    {
        screenMgr = GameObject.Find("ScreenManager").GetComponent<ScreenManager>();
        configCanvas.SetActive(false);
    }

    public void OpenConfig()
    {
        //ダイアログ表示
        //configCanvas.SetActive(true);
        screenMgr.FadeUI(configCanvas.gameObject, true);

        ////シーンごとの処理
        //switch (SceneManager.GetActiveScene().name)
        //{
        //    case Common.CO.SCENE_TITLE:
        //        GameObject.Find("PhotonManager").GetComponent<PhotonManager>().SwitchModeSelectArea(false);
        //        break;
        //}
    }

    public void CloseConfig()
    {
        //設定保存

        //ダイアログ非表示
        //configCanvas.SetActive(false);
        screenMgr.FadeUI(configCanvas.gameObject, false);

        //シーンごとの処理
        switch (SceneManager.GetActiveScene().name)
        {
            case Common.CO.SCENE_TITLE:
                GameObject.Find("PhotonManager").GetComponent<PhotonManager>().ReturnModeSelect();
                break;
        }
    }
}

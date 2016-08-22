using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ConfigManager : MonoBehaviour
{
    [SerializeField]
    private GameObject configCanvas;

    private FadeManager fadeCtrl;

    void Awake()
    {
        fadeCtrl = GameObject.Find("Fade").GetComponent<FadeManager>();
        configCanvas.SetActive(false);
    }

    public void OpenConfig()
    {
        //ダイアログ表示
        //configCanvas.SetActive(true);
        fadeCtrl.FadeUI(configCanvas.gameObject, true);

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
        fadeCtrl.FadeUI(configCanvas.gameObject, false);

        //シーンごとの処理
        switch (SceneManager.GetActiveScene().name)
        {
            case Common.CO.SCENE_TITLE:
                GameObject.Find("PhotonManager").GetComponent<PhotonManager>().ReturnModeSelect();
                break;
        }
    }
}

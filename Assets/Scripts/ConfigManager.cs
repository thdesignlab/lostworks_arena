using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ConfigManager : MonoBehaviour
{
    private Transform myTran;
    [SerializeField]
    private GameObject configCanvas;

    void Awake()
    {
        myTran = transform;
        configCanvas.SetActive(false);
    }

    public void OpenConfig()
    {
        Debug.Log("OpenConfig");
        //ダイアログ表示
        configCanvas.SetActive(true);
        Debug.Log(configCanvas.GetActive());

        //シーンごとの処理
        switch (SceneManager.GetActiveScene().name)
        {
            case Common.CO.SCENE_TITLE:
                GameObject.Find("PhotonManager").GetComponent<PhotonManager>().SwitchModeSelectArea(false);
                break;
        }
    }

    public void CloseConfig()
    {
        //設定保存

        //ダイアログ非表示
        configCanvas.SetActive(false);

        //シーンごとの処理
        switch (SceneManager.GetActiveScene().name)
        {
            case Common.CO.SCENE_TITLE:
                GameObject.Find("PhotonManager").GetComponent<PhotonManager>().ReturnModeSelect();
                break;
        }
    }
}

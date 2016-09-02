using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ConfigManager : SingletonMonoBehaviour<ConfigManager>
{
    [SerializeField]
    private GameObject configCanvas;

    protected override void Awake()
    {
        base.Awake();

        configCanvas.SetActive(false);
    }

    public void OpenConfig()
    {
        //ダイアログ表示
        ScreenManager.Instance.FadeUI(configCanvas.gameObject, true);
    }

    public void CloseConfig()
    {
        //設定保存

        //ダイアログ非表示
        ScreenManager.Instance.FadeUI(configCanvas.gameObject, false);

        //シーンごとの処理
        switch (SceneManager.GetActiveScene().name)
        {
            case Common.CO.SCENE_TITLE:
                GameObject.Find("PhotonManager").GetComponent<PhotonManager>().ReturnModeSelect();
                break;
        }
    }

    //音量変更
    private void ChangeVolume(int kind, float value)
    {
        Debug.Log(kind+" >> "+value);
    }
    public void ChangeVolumeBgm(float value)
    {
        ChangeVolume(Common.PP.CONFIG_BGM_VALUE, value);
    }
    public void ChangeVolumeSe(float value)
    {
        ChangeVolume(Common.PP.CONFIG_SE_VALUE, value);
    }
    public void ChangeVolumeVoice(float value)
    {
        ChangeVolume(Common.PP.CONFIG_VOICE_VALUE, value);
    }

    //Mute
    public void ChangeMute(int kind, bool flg)
    {
        Debug.Log(kind+" >> "+flg);
    }
    public void ChangeMuteBgm(bool flg)
    {
        ChangeMute(Common.PP.CONFIG_BGM_MUTE, flg);
    }
    public void ChangeMuteSe(bool flg)
    {
        ChangeMute(Common.PP.CONFIG_SE_MUTE, flg);
    }
    public void ChangeMuteVoice(bool flg)
    {
        ChangeMute(Common.PP.CONFIG_VOICE_MUTE, flg);
    }
}

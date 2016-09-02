using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class ConfigManager : SingletonMonoBehaviour<ConfigManager>
{
    [SerializeField]
    private GameObject configCanvas;
    [SerializeField]
    private AudioMixer mixer;

    [SerializeField]
    private AudioSource seAudio;
    [SerializeField]
    private AudioSource voiceAudio;

    private Slider bgmSlider;
    private Slider seSlider;
    private Slider voiceSlider;

    private Toggle bgmToggle;
    private Toggle seToggle;
    private Toggle voiceToggle;

    public const int SLIDER_COUNT = 10;
    const int MIN_DECIBEL = -80;
    const int MAX_DECIBEL = 0;

    private Dictionary<int, string> volumeNameDic = new Dictionary<int, string>()
    {
        { Common.PP.CONFIG_BGM_VALUE, "BgmVolume"},
        { Common.PP.CONFIG_SE_VALUE, "SeVolume"},
        { Common.PP.CONFIG_VOICE_VALUE, "VoiceVolume"},
    };

    protected override void Awake()
    {
        base.Awake();

        configCanvas.SetActive(false);

        //スライダー
        bgmSlider = configCanvas.transform.FindChild("List/Bgm/Slider").GetComponent<Slider>();
        seSlider = configCanvas.transform.FindChild("List/Se/Slider").GetComponent<Slider>();
        voiceSlider = configCanvas.transform.FindChild("List/Voice/Slider").GetComponent<Slider>();
        //トグル
        bgmToggle = configCanvas.transform.FindChild("List/Bgm/Toggle").GetComponent<Toggle>();
        seToggle = configCanvas.transform.FindChild("List/Se/Toggle").GetComponent<Toggle>();
        voiceToggle = configCanvas.transform.FindChild("List/Voice/Toggle").GetComponent<Toggle>();
    }

    void Start()
    {
        foreach (int kind in UserManager.userConfig.Keys)
        {
            if (volumeNameDic.ContainsKey(kind))
            {
                //ミキサー設定
                ChangeVolume(kind, UserManager.userConfig[kind], false);

                //UI設定
                Slider tmpSlider = null;
                Toggle tmpToggle = null;
                int muteKey = -1;
                switch (kind)
                {
                    case Common.PP.CONFIG_BGM_VALUE:
                        tmpSlider = bgmSlider;
                        tmpToggle = bgmToggle;
                        muteKey = Common.PP.CONFIG_BGM_MUTE;
                        break;

                    case Common.PP.CONFIG_SE_VALUE:
                        tmpSlider = seSlider;
                        tmpToggle = seToggle;
                        muteKey = Common.PP.CONFIG_SE_MUTE;
                        break;

                    case Common.PP.CONFIG_VOICE_VALUE:
                        tmpSlider = voiceSlider;
                        tmpToggle = voiceToggle;
                        muteKey = Common.PP.CONFIG_VOICE_MUTE;
                        break;
                }
                tmpSlider.value = UserManager.userConfig[kind];
                tmpToggle.isOn = false;
                if (UserManager.userConfig[muteKey] == 1)
                {
                    tmpSlider.value = 0;
                    tmpToggle.isOn = true;
                }
            }
        }
    }

    public void OpenConfig()
    {
        //ダイアログ表示
        ScreenManager.Instance.FadeUI(configCanvas.gameObject, true);
    }

    public void CloseConfig()
    {
        //設定保存
        UserManager.SaveConfig();

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
    private void ChangeVolume(int kind, float value, bool isSave = true)
    {
        if (volumeNameDic.ContainsKey(kind))
        {
            float volumeDB = 20 * Mathf.Log10(value / SLIDER_COUNT);
            mixer.SetFloat(volumeNameDic[kind], Mathf.Clamp(volumeDB, MIN_DECIBEL, MAX_DECIBEL));

            if (isSave) UserManager.userConfig[kind] = (int)value;
        }
    }
    public void ChangeVolumeBgm(float value)
    {
        ChangeVolume(Common.PP.CONFIG_BGM_VALUE, value);
    }
    public void ChangeVolumeSe(float value)
    {
        if (seAudio != null) seAudio.Play();
        ChangeVolume(Common.PP.CONFIG_SE_VALUE, value);
    }
    public void ChangeVolumeVoice(float value)
    {
        if (voiceAudio != null) voiceAudio.Play();
        ChangeVolume(Common.PP.CONFIG_VOICE_VALUE, value);
    }

    //Mute
    public void ChangeMute(int muteKind, bool flg, int volumeKind)
    {
        int mute = 1;
        int value = 0;
        if (!flg)
        {
            mute = 1;
            value = UserManager.userConfig[volumeKind];
        }
        UserManager.userConfig[muteKind] = mute;
        ChangeVolume(volumeKind, value, false);
    }
    public void ChangeMuteBgm(bool flg)
    {
        ChangeMute(Common.PP.CONFIG_BGM_MUTE, flg, Common.PP.CONFIG_BGM_VALUE);
    }
    public void ChangeMuteSe(bool flg)
    {
        ChangeMute(Common.PP.CONFIG_SE_MUTE, flg, Common.PP.CONFIG_SE_VALUE);
    }
    public void ChangeMuteVoice(bool flg)
    {
        ChangeMute(Common.PP.CONFIG_VOICE_MUTE, flg, Common.PP.CONFIG_VOICE_VALUE);
    }
}

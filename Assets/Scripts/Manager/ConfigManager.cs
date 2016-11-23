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
    //[SerializeField]
    //private GameObject helpCanvas;
    [SerializeField]
    private AudioMixer mixer;

    [SerializeField]
    private AudioSource seAudio;
    [SerializeField]
    private AudioSource voiceAudio;

    private InputField playerNameText;

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
        //helpCanvas.SetActive(false);

        Transform configCanvasTran = configCanvas.transform;

        //プレイヤー名TEXT
        playerNameText = configCanvasTran.FindChild("List/Name/InputField").GetComponent<InputField>();

        //スライダー
        bgmSlider = configCanvasTran.FindChild("List/Bgm/Slider").GetComponent<Slider>();
        seSlider = configCanvasTran.FindChild("List/Se/Slider").GetComponent<Slider>();
        voiceSlider = configCanvasTran.FindChild("List/Voice/Slider").GetComponent<Slider>();
        //トグル
        bgmToggle = configCanvasTran.FindChild("List/Bgm/Toggle").GetComponent<Toggle>();
        seToggle = configCanvasTran.FindChild("List/Se/Toggle").GetComponent<Toggle>();
        voiceToggle = configCanvasTran.FindChild("List/Voice/Toggle").GetComponent<Toggle>();
    }

    void Start()
    {
        Setting();
    }

    private void Setting()
    {
        playerNameText.text = UserManager.userInfo[Common.PP.INFO_USER_NAME];

        Dictionary<int, int> configDic = new Dictionary<int, int>(UserManager.userConfig);
        foreach (int kind in configDic.Keys)
        {
            //Debug.Log(kind+" >> "+configDic[kind]);
            if (volumeNameDic.ContainsKey(kind))
            {
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
                int tmpMute = UserManager.userConfig[muteKey];
                tmpSlider.value = UserManager.userConfig[kind];
                tmpToggle.isOn = (tmpMute == 1) ? true : false;
            }
        }
    }

    public void OpenConfig()
    {
        //ダイアログ表示
        ScreenManager.Instance.FadeUI(configCanvas.gameObject, true);
        Setting();

        DialogController.CloseMessage();
    }

    public void CloseConfig()
    {
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);

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

            case Common.CO.SCENE_BATTLE:
                DialogController.CloseMessage();
                break;
        }
    }

    //音量変更
    private void ChangeVolume(int kind, float value, bool isSave = false)
    {
        SetMixer(kind, value);
        if (isSave) UserManager.userConfig[kind] = (int)value;
    }
    public void OnChangeVolumeBgm(float value)
    {
        ChangeVolume(Common.PP.CONFIG_BGM_VALUE, value, true);
        bgmToggle.isOn = false;
    }
    public void OnChangeVolumeSe(float value)
    {
        if (configCanvas.gameObject.GetActive() && seAudio != null) seAudio.Play();
        ChangeVolume(Common.PP.CONFIG_SE_VALUE, value, true);
        seToggle.isOn = false;
    }
    public void OnChangeVolumeVoice(float value)
    {
        if (configCanvas.gameObject.GetActive() && voiceAudio != null) voiceAudio.Play();
        ChangeVolume(Common.PP.CONFIG_VOICE_VALUE, value, true);
        voiceToggle.isOn = false;
    }

    //Mute
    public void ChangeMute(int muteKind, bool flg, int volumeKind)
    {
        int mute = 1;
        int value = 0;
        if (!flg)
        {
            mute = 0;
            value = UserManager.userConfig[volumeKind];
        }
        UserManager.userConfig[muteKind] = mute;
        ChangeVolume(volumeKind, value);
    }
    public void OnChangeMuteBgm(bool flg)
    {
        ChangeMute(Common.PP.CONFIG_BGM_MUTE, flg, Common.PP.CONFIG_BGM_VALUE);
    }
    public void OnChangeMuteSe(bool flg)
    {
        ChangeMute(Common.PP.CONFIG_SE_MUTE, flg, Common.PP.CONFIG_SE_VALUE);
    }
    public void OnChangeMuteVoice(bool flg)
    {
        ChangeMute(Common.PP.CONFIG_VOICE_MUTE, flg, Common.PP.CONFIG_VOICE_VALUE);
    }

    private void SetMixer(int kind, float value)
    {
        if (volumeNameDic.ContainsKey(kind))
        {
            float volumeDB = 20 * Mathf.Log10(value / SLIDER_COUNT);
            mixer.SetFloat(volumeNameDic[kind], Mathf.Clamp(volumeDB, MIN_DECIBEL, MAX_DECIBEL));
        }
    }

    public void CheckName(string input)
    {
        //名前チェック
        //保存
        UserManager.SetUserName(input);
    }

    //ヘルプ表示
    public void OpenHelp()
    {
        WebViewManager.Instance.Open(Common.CO.HP_TUTORIAL_URL);
        //ScreenManager.Instance.FadeUI(helpCanvas, true);        
    }

    ////ヘルプ非表示
    //public void CloseHelp()
    //{
    //    ScreenManager.Instance.FadeUI(helpCanvas, false);
    //}
}

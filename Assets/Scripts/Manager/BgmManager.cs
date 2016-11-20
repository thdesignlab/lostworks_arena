using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BgmManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip audioClip;
    private AudioSource audioSource;

    //BGM再生設定
    [SerializeField]
    private float playStartTime = 0;

    //BGMループ設定
    [SerializeField]
    private float loopStartTime = 0;   //ループ再生の開始時間
    [SerializeField]
    private float loopEndTime = 0;     //ループ位置に戻る時間

    void Awake()
    {
        audioSource = transform.parent.GetComponent<AudioSource>();
    }

    void Update()
    {
        // 再生中のBGMの再生時間を監視する
        if (audioSource != null && audioSource.clip == audioClip && audioSource.isPlaying)
        {
            //Debug.Log(transform.name+" >> "+audioSource.time);
            if (loopEndTime > 0)
            {
                if (audioSource.time >= loopEndTime)
                {
                    //Debug.Log("### Loop ###");
                    audioSource.time = loopStartTime;
                }
            }
        }
    }

    public void Play()
    {
        if (audioSource == null) return;
        audioSource.clip = audioClip;
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            audioSource.time = playStartTime;
        }
    }

    public void Stop()
    {
        if (audioSource == null) return;
        audioSource.Stop();
    }

    public string GetAudioClipName()
    {
        return audioClip.name;
    }
}

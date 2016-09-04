using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BgmManager : MonoBehaviour
{
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
        audioSource = transform.GetComponent<AudioSource>();
    }

    void Update()
    {
        // 再生中のBGMの再生時間を監視する
        if (audioSource != null && audioSource.isPlaying)
        {
            if (loopEndTime > 0)
            {
                if (audioSource.time >= loopEndTime)
                {
                    audioSource.time = loopStartTime;
                }
            }
        }
    }

    public void Play()
    {
        if (audioSource == null) return;
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
}

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class BgmManager : Photon.MonoBehaviour
{
    private Transform myTran;
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip bgmTitle;
    [SerializeField]
    private AudioClip bgmCustom;
    [SerializeField]
    private AudioClip bgmBattle;

    void Awake()
    {
        myTran = transform;
        audioSource = myTran.GetComponent<AudioSource>();
    }

    public void Play(string sceneName = "")
    {
        if (audioSource == null) return;

        if (sceneName == "") sceneName = SceneManager.GetActiveScene().name;

        AudioClip audioClip = null;
        switch (sceneName)
        {
            case Common.CO.SCENE_TITLE:
                audioClip = bgmTitle;
                break;

            case Common.CO.SCENE_CUSTOM:
                audioClip = bgmCustom;
                break;

            case Common.CO.SCENE_BATTLE:
                audioClip = bgmBattle;
                break;
        }
        Debug.Log(audioClip);
        if (audioClip != null)
        {
            if (audioClip != audioSource.clip)
            {
                audioSource.clip = audioClip;
                audioSource.Play();
            }
        }
        else
        {
            audioSource.clip = null;
            audioSource.Stop();
        }
    }
}

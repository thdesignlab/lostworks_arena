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

    private void PlayBgm()
    {
        AudioClip audioClip = null;
        switch (SceneManager.GetActiveScene().name)
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
            audioSource.Stop();
        }
    }
}

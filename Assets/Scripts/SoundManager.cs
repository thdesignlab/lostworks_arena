using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    private Transform myTran;
    private BgmManager nowBgmMgr;


    protected override void Awake()
    {
        base.Awake();
        myTran = transform;
    }

    void Start()
    {
        nowBgmMgr = GetBgmManager();
        if (nowBgmMgr != null) nowBgmMgr.Play();
    }

    public void PlayBgm(string sceneName = "")
    {
        BgmManager bgmMgr = GetBgmManager(sceneName);
        if (bgmMgr != null)
        {
            if (nowBgmMgr != bgmMgr)
            {
                //Debug.Log("BGM start:"+ audioSource.clip +" >> "+ audioClip);
                nowBgmMgr.Stop();
                nowBgmMgr = bgmMgr;
            }
            nowBgmMgr.Play();
        }
        else
        {
            nowBgmMgr.Stop();
            nowBgmMgr = null;
        }
    }

    public void StopBgm(string sceneName = "", bool isSameBgmStop = false)
    {
        if (!isSameBgmStop)
        {
            BgmManager bgmMgr = GetBgmManager(sceneName);
            if (bgmMgr != null)
            {
                if (nowBgmMgr == bgmMgr)
                {
                    //同じBGMの場合は止めない
                    return;
                }
            }
        }
        nowBgmMgr.Stop();
    }

    private BgmManager GetBgmManager(string sceneName = "")
    {
        if (sceneName == "") sceneName = SceneManager.GetActiveScene().name;

        BgmManager sceneBgm = null;
        switch (sceneName)
        {
            case Common.CO.SCENE_TITLE:
            case Common.CO.SCENE_CUSTOM:
                sceneBgm = myTran.FindChild("BgmCustom").GetComponent<BgmManager>();
                break;

            case Common.CO.SCENE_BATTLE:
                sceneBgm = myTran.FindChild("BgmBattle").GetComponent<BgmManager>();
                break;
        }
        return sceneBgm;
    }
}

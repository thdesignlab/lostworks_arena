using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SoundManager : SingletonMonoBehaviour<SoundManager>
{
    [SerializeField]
    private List<BgmManager> titleBgmList;
    [SerializeField]
    private List<BgmManager> customBgmList;
    [SerializeField]
    private List<BgmManager> battleBgmList;

    private BgmManager nowBgmMgr;


    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        nowBgmMgr = GetBgmManager();
        if (nowBgmMgr != null) nowBgmMgr.Play();
    }

    private void Play(BgmManager bgmMgr)
    {
        if (bgmMgr != null)
        {
            if (nowBgmMgr != bgmMgr)
            {
                //Debug.Log("BGM start:"+ audioSource.clip +" >> "+ audioClip);
                if (nowBgmMgr != null) nowBgmMgr.Stop();
                nowBgmMgr = bgmMgr;
            }
            nowBgmMgr.Play();
        }
        //else
        //{
        //    if (nowBgmMgr != null) nowBgmMgr.Stop();
        //    nowBgmMgr = null;
        //}
    }

    public void PlayBgm(string sceneName = "")
    {
        BgmManager bgmMgr = GetBgmManager(sceneName);
        Play(bgmMgr);
    }

    public void PlayBattleBgm(int no = -1)
    {
        if (no < 0 || battleBgmList.Count <= no)
        {
            //ランダム
            no = Random.Range(0, battleBgmList.Count);
        }
        BgmManager bgmMgr = battleBgmList[no];
        Play(bgmMgr);
    }

    public void PlayStoreBgm()
    {
        BgmManager bgmMgr = customBgmList[0];
        Play(bgmMgr);
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
        int index = 0;
        switch (sceneName)
        {
            case Common.CO.SCENE_TITLE:
                sceneBgm = titleBgmList[index];
                break;

            case Common.CO.SCENE_CUSTOM:
            case Common.CO.SCENE_STORE:
            case Common.CO.SCENE_RANKING:
                sceneBgm = customBgmList[index];
                break;

            case Common.CO.SCENE_BATTLE:
                //GameControllerで制御
                break;
        }
        return sceneBgm;
    }
}

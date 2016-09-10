using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Advertisements;

/// <summary>
/// 動画広告を管理するクラス
/// </summary>
public class UnityAds : SingletonMonoBehaviour<UnityAds>
{

    //Unityの広告ID
    [SerializeField]
    private string _unityAdsiosID, _unityAdsAndroidID;

    private bool isAdPlaying = false;

    //=================================================================================
    //初期化
    //=================================================================================

    protected override void Awake()
    {
        base.Awake();

        //プラットフォームが対応しているか判定し、初期化
        if (Advertisement.isSupported)
        {
            //Advertisement.allowPrecache = true;

#if UNITY_IOS
      Advertisement.Initialize (_unityAdsiosID, MyDebug.Instance.isDebugMode);
#elif UNITY_ANDROID
            Advertisement.Initialize(_unityAdsAndroidID, MyDebug.Instance.isDebugMode);
#endif
        }
        else
        {
            Debug.Log("プラットフォームがUnityAdsに対応していません");
        }
    }

    //=================================================================================
    //判定、取得
    //=================================================================================

    /// <summary>
    /// 動画を再生する事ができるか
    /// </summary>
    public bool CanPlay()
    {
        //プラットフォームが対応しているかつ準備が完了している時だけtrueを返す
        return Advertisement.isSupported && Advertisement.IsReady();
    }

    //=================================================================================
    //動画再生
    //=================================================================================

    /// <summary>
    /// 動画再生
    /// </summary>
    public void Play(Action completeAction = null, string zoneId = null, Action OnFinished = null, Action OnFailed = null, Action OnSkipped = null)
    {
        //コールバック用メソッド作成、Result の値は Finished、Failed、Skipped
        Action<ShowResult> callBack = (result) => {
            Debug.Log(result);
            if (result == ShowResult.Finished && OnFinished != null)
            {
                OnFinished();
            }
            else if (result == ShowResult.Failed && OnFailed != null)
            {
                OnFailed();
            }
            else if (result == ShowResult.Skipped && OnSkipped != null)
            {
                OnSkipped();
            }
            if (completeAction != null)
            {
                completeAction();
            }
            isAdPlaying = false;
        };

        //Options
        ShowOptions options = new ShowOptions();
        options.resultCallback = callBack;


        //動画再生
        isAdPlaying = true;
        Advertisement.Show(zoneId, options);
    }

    public bool IsPlaying()
    {
        return isAdPlaying;
    }

    //IEnumerator Start()
    //{
    //    Debug.Log("start");
    //    for (;;)
    //    {
    //        if (CanPlay())
    //        {
    //            Debug.Log("play");
    //            Play();
    //            yield break;
    //        }
    //        yield return null;
    //    }
    //}
}
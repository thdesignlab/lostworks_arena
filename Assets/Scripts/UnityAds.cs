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
    public void Play(Action OnFinished = null, Action OnFailed = null, Action OnSkipped = null)
    {

        //コールバック用メソッド作成、Result の値は Finished、Failed、Skipped
        Action<ShowResult> callBack = (result) => {

            if (result == ShowResult.Finished && OnFinished != null)
            {
                Debug.Log("OnFinished");
                OnFinished();
            }
            else if (result == ShowResult.Failed && OnFailed != null)
            {
                Debug.Log("OnFailed");
                OnFailed();
            }
            else if (result == ShowResult.Skipped && OnSkipped != null)
            {
                Debug.Log("OnSkipped");
                OnSkipped();
            }
        };

        //動画再生
        Advertisement.Show(null, new ShowOptions
        {
            //trueだとUnityが止まり、音もミュートになる
            //pause = true,
            //広告が表示された後のコールバック設定
            resultCallback = callBack
        });

    }

    IEnumerator Start()
    {
        for (;;)
        {
            if (CanPlay())
            {
                Play();
                yield break;
            }
            yield return null;
        }
    }
}
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class VersionManager : Singleton<VersionManager>
{
    //Version
    private const string IOS_VERSION = "1.0.6";
    private const string ANDROID_VERSION = "1.0.6";

    //バージョン差異チェック
    public bool IsVersionError(string version)
    {
        string appVersion = "";
#if UNITY_IOS
        appVersion = IOS_VERSION;
#elif UNITY_ANDROID
        appVersion = ANDROID_VERSION;
#endif
        if (string.IsNullOrEmpty(appVersion)) return false;
        if (appVersion == version) return false;

        string[] appVersions = appVersion.Split('.');
        string[] versions = version.Split('.');
        if (appVersions.Length != versions.Length) return true;

        bool isError = false;
        for (int i = 0; i < appVersions.Length; i++)
        {
            int app = int.Parse(appVersions[i]);
            int api = int.Parse(versions[i]);
            if (app < api)
            {
                isError = true;
                break;
            }
            else if (app > api)
            {
                isError = false;
                break;
            }
        }
        return isError;
    }

    //バージョン差異警告
    public void WarningVersionDiff()
    {
        PhotonManager.isReadyGame = false;
        string text = "アプリの更新があります\n最新版以外では一部機能が制限されます";
        List<string> buttonTextList = new List<string>()
        {
            "Store",            
            "Cancel",
        };
        List<UnityAction> buttonActionList = new List<UnityAction>()
        {
            ForStore,
            UpdateCancel,
        };
        DialogController.OpenDialog(text, buttonTextList, buttonActionList);
    }

    //バージョン強制アップデート
    public void ForceUpdate()
    {
        PhotonManager.isReadyGame = false;
        string text = "アプリの更新があります\n更新してください";
        List<string> buttonTextList = new List<string>()
        {
            "Store",
            "Cancel",
        };
        List<UnityAction> buttonActionList = new List<UnityAction>()
        {
            ForStore,
            GoToTitle,
        };
        DialogController.OpenDialog(text, buttonTextList, buttonActionList);
    }

    //ストアへ
    private void ForStore()
    {
        string url = Common.Func.GetStoreUrl();
        Application.OpenURL(url);
        PhotonManager.isReadyGame = true;
    }

    //キャンセル
    private void UpdateCancel()
    {
        PhotonManager.isReadyGame = true;
    }

    //タイトルへ
    private void GoToTitle()
    {
        ScreenManager.Instance.Load(Common.CO.SCENE_TITLE, DialogController.MESSAGE_LOADING);
    }
}

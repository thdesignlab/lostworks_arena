using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using LitJson;

public class ApiManager : SingletonMonoBehaviour<ApiManager>
{
    const string BASE_URL = "http://lostworks.th-designlab.com/";

    //APIリクエスト
    public void Post(string uri, string paramJson = "", Action<string> callback = null, Action errorCallback = null)
    {
        StartCoroutine(PostRoutine(uri, paramJson, callback, errorCallback));
    }

    IEnumerator PostRoutine(string uri, string paramJson = "", Action<string> apiCallback = null, Action errorCallback = null)
    {
        //ヘッダー
        Dictionary<string, string> header = new Dictionary<string, string>();
        header.Add("Content-Type", "application/json; charset=UTF-8");
        header.Add("Token", UserManager.apiToken);
        header.Add("UserAgent", SystemInfo.operatingSystem);
        header.Add("Lostworks", "api");
        //Debug.Log(SystemInfo.deviceType);
        //Debug.Log(SystemInfo.operatingSystem);
        //Debug.Log(SystemInfo.);

        //パラメータ
        byte[] postBytes = null;
        if (paramJson != "") postBytes = Encoding.Default.GetBytes(paramJson);

        //送信
        WWW www = new WWW(BASE_URL + uri, postBytes, header);
        yield return www;

        if (www.error == null)
        {
            //JSON取得
            string json = Encoding.UTF8.GetString(www.bytes);

            //コールバック
            apiCallback.Invoke(json);
        }
        else
        {
            Debug.Log(www.error);
            if (errorCallback != null) errorCallback.Invoke();
        }
    }

}

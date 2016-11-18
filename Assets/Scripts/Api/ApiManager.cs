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
    public void Post(string uri, string paramJson = "", Action<string> callback = null, Action errorCallback = null, int retry = 1)
    {
        StartCoroutine(PostRoutine(uri, paramJson, callback, errorCallback, retry));
    }

    IEnumerator PostRoutine(string uri, string paramJson = "", Action<string> apiCallback = null, Action errorCallback = null, int retry = 1)
    {
        if (!string.IsNullOrEmpty(paramJson)) MyDebug.Instance.AdminLog("[Post]"+uri, paramJson);

        //ヘッダー
        Dictionary<string, string> header = new Dictionary<string, string>();
        header.Add("Content-Type", "application/json; charset=UTF-8");
        header.Add("Token", UserManager.apiToken);
        header.Add("UserAgent", SystemInfo.operatingSystem);
        header.Add("Lostworks", "api");

        //パラメータ
        byte[] postBytes = null;
        if (paramJson != "") postBytes = Encoding.UTF8.GetBytes(paramJson);

        int exeCount = 1;
        for (;;)
        {
            //送信
            WWW www = new WWW(BASE_URL + uri, postBytes, header);
            yield return www;

            if (www.error == null)
            {
                //JSON取得
                string json = Encoding.UTF8.GetString(www.bytes);
                MyDebug.Instance.AdminLog("[Success]" + uri, json);
 
                //コールバック
                apiCallback.Invoke(json);
                yield break;
            }
            else
            {
                MyDebug.Instance.AdminLog("[ConnectError]" + uri, www.error);
                if (exeCount >= retry)
                {
                    if (errorCallback != null) errorCallback.Invoke();
                    yield break;
                }
            }
            exeCount++;
            yield return new WaitForSeconds(0.2f);
        }
    }
}

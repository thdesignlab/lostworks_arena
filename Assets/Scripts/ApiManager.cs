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
    public void Post(string uri, string paramJson = "", Action<string> callback = null)
    {
        StartCoroutine(PostRoutine(uri, paramJson, callback));
    }

    IEnumerator PostRoutine(string uri, string paramJson = "", Action<string> callback = null)
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
            callback.Invoke(json);
        }
        else
        {
            Debug.Log(www.error);
            DialogController.OpenDialog("接続エラー", () => SceneManager.LoadScene(Common.CO.SCENE_TITLE));
        }
    }
}

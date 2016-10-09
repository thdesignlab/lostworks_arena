using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

public class ApiManager : SingletonMonoBehaviour<ApiManager>
{
    const string BASE_URL = "http://lostworks.th-designlab.com/";

    public void Post(string uri, string paramJson = "", Action<WWW> callback = null)
    {
        StartCoroutine(PostRoutine(uri, paramJson, callback));
    }

    IEnumerator PostRoutine(string uri, string paramJson = "", Action<WWW> callback = null)
    {
        //ヘッダー
        Dictionary<string, string> header = new Dictionary<string, string>();
        header.Add("Content-Type", "application/json; charset=UTF-8");
        header.Add("Token", "abcde");
        header.Add("UA", "123456789");
        header.Add("Lostworks", "api");

        //パラメータ
        byte[] postBytes = null;
        if (paramJson != "") postBytes = Encoding.Default.GetBytes(paramJson);
        
        //送信
        WWW www = new WWW(BASE_URL + uri, postBytes, header);
        yield return www;

        //コールバック
        if (callback != null)
        {
            callback(www);
        }
        else
        {
            DefaultCallback(www);
        }
    }

    private void DefaultCallback(WWW www)
    {
        if (www.error == null)
        {
            Debug.Log(www.text);
        }
        else
        {
            Debug.Log(www.error);
        }
    }

    public string ToJson<T>(T obj)
    {
        string json = JsonUtility.ToJson(obj);
        return json;
    }

    public T FromJson<T>(string json)
    {
        T obj = JsonUtility.FromJson<T>(json);
        return obj;
    }
}

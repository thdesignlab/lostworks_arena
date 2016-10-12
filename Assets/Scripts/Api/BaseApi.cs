using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Text;
using LitJson;

public class BaseApi
{
    protected bool isNeedToken = true;

    protected void Exe(string uri, Action<string> callback = null)
    {
        Exe(uri, "", callback);
    }
    protected void Exe(string uri, string paramJson = "", Action<string> callback = null)
    {
        if (isNeedToken)
        {
            Action action = () => ApiManager.Instance.Post(uri, paramJson, callback);
            if (string.IsNullOrEmpty(UserManager.userInfo[Common.PP.INFO_UUID])
               || string.IsNullOrEmpty(UserManager.userInfo[Common.PP.INFO_PASSWORD]))
            {
                //ユーザー新規作成
                User.Create userCreate = new User.Create();
                userCreate.Exe(action);
                return;
            }
            else if (string.IsNullOrEmpty(UserManager.apiToken))
            {
                //ログイン
                Auth.Login authLogin = new Auth.Login();
                authLogin.Exe(action);
                return;
            }
        }

        //API実行
        ApiManager.Instance.Post(uri, paramJson, callback);
    }

    protected T GetData<T>(string json, bool isErrorThrough = false)
    {
        Debug.Log(this.ToString()+" >> "+json);
        ResponseData<T> res = JsonMapper.ToObject<ResponseData<T>>(json);

        //エラーチェック
        if (!string.IsNullOrEmpty(res.error_code))
        {
            Debug.Log("[ERR]" + res.error_code);
            if (!isErrorThrough) GoToTitle();
        }

        //管理者FLG
        UserManager.isAdmin = res.admin;

        return res.data;
    }

    protected void GoToTitle()
    {
        //タイトルへ
        DialogController.OpenDialog("接続エラー", () => SceneManager.LoadScene(Common.CO.SCENE_TITLE));
    }
}



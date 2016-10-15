using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Text;
using LitJson;

public abstract class BaseApi
{
    protected abstract string uri { get; }                      //APIuri
    protected virtual bool isNeedToken { get { return true; } } //要TokenFLG

    protected bool isIgnoreError = false;           //API接続エラー無視FLG
    protected Action apiErrorCallback = null;       //API接続エラー時処理
    protected Action apiFinishCallback = null;      //API完了後処理

    //API完了時コールバック設定
    public void SetApiFinishCallback(Action action)
    {
        apiFinishCallback = action;
    }
    protected void ApiFinishCallback()
    {
        if (apiFinishCallback != null) apiFinishCallback.Invoke();
    }


    //接続エラー時コールバック設定
    public void SetApiErrorCallback(Action errorAction)
    {
        apiErrorCallback = errorAction;
    }

    //接続エラー無視FLG設定
    public void SetApiErrorIngnore()
    {
        isIgnoreError = true;
    }

    //POST
    protected void Post(Action<string> callback = null)
    {
        Post("", callback);
    }
    protected void Post(string paramJson = "", Action<string> callback = null)
    {
        //APIエラー時処理
        if (!isIgnoreError && apiErrorCallback == null) apiErrorCallback = () => GoToTitle();

        Action action = () => ApiManager.Instance.Post(uri, paramJson, callback, apiErrorCallback);
        if (isNeedToken)
        {
            if (string.IsNullOrEmpty(UserManager.userInfo[Common.PP.INFO_UUID])
               || string.IsNullOrEmpty(UserManager.userInfo[Common.PP.INFO_PASSWORD]))
            {
                //ユーザー新規作成
                User.Create userCreate = new User.Create();
                userCreate.SetApiFinishCallback(action);
                userCreate.Exe();
                return;
            }
            else if (string.IsNullOrEmpty(UserManager.apiToken))
            {
                //ログイン
                Auth.Login authLogin = new Auth.Login();
                authLogin.SetApiFinishCallback(action);
                authLogin.Exe();
                return;
            }
        }

        //送信
        action.Invoke();
    }

    //レスポンスからdata取得
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

    public void GoToTitle()
    {
        //タイトルへ
        DialogController.OpenDialog("接続エラー", GoToTtileExe);
    }
    public void GoToTtileExe()
    {
        PhotonManager.isFirstScean = true;
        ScreenManager.Instance.Load(Common.CO.SCENE_TITLE);
    }
}



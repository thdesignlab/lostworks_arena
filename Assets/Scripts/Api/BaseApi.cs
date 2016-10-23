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

    protected bool isIgnoreError = false;                   //APIエラー無視FLG
    protected Action apiFinishCallback = null;              //API完了後処理
    protected Action<string> apiFinishErrorCallback = null; //API結果エラー後処理(errorCode)
    protected Action apiConnectErrorCallback = null;        //API接続エラー時処理
    protected int retry = 1;

    //API完了時コールバック
    public void SetApiFinishCallback(Action action)
    {
        apiFinishCallback = action;
    }

    //API完了時エラーコールバック
    public void SetApiFinishErrorCallback(Action<string> errorAction)
    {
        apiFinishErrorCallback = errorAction;
    }

    //接続エラー時コールバック設定
    public void SetConnectErrorCallback(Action errorAction)
    {
        apiConnectErrorCallback = errorAction;
    }

    //接続エラー無視FLG設定
    public void SetApiErrorIngnore()
    {
        isIgnoreError = true;
    }

    //リトライ回数
    public void SetRetryCount(int count)
    {
        retry = count;
    }

    //POST
    //protected void Post<T>(Action callback = null)
    //{
    //    Post<T>("", callback);
    //}
    protected void Post<T>(string paramJson = "")
    {
        Action action = () =>
        {
            ApiManager.Instance.Post(uri, paramJson, (json) => Finish<T>(json), ConnectError, retry);
        };

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

    //APIレスポンスコールバック
    protected void Finish<T>(string json)
    {
        string errorCode = "";
        string errorMessage = "";
        try
        {
            ResponseData<T> responseData = GetResponseData<T>(json);
            errorCode = responseData.error_code;
            if (string.IsNullOrEmpty(errorCode))
            {
                //正常
                FinishCallback(json);
                if (apiFinishCallback != null) apiFinishCallback.Invoke();
                return;
            }
        }
        catch (Exception e)
        {
            errorMessage = e.Message;
        }

        //エラー処理
        Debug.Log("[ERR]" + errorCode + " >> " + errorMessage);
        if (apiFinishErrorCallback != null)
        {
            apiFinishErrorCallback.Invoke(errorCode);
        }
        else if (!isIgnoreError)
        {
            GoToTitle();
        }
    }
    protected virtual void FinishCallback(string json)
    {
        return;
    }
    protected virtual void FinishErrorCallback(string errorCode)
    {
        if (apiFinishErrorCallback != null)
        {
            apiFinishErrorCallback.Invoke(errorCode);
        }
        else if (!isIgnoreError)
        {
            GoToTitle();
        }
    }

    //接続エラー
    protected void ConnectError()
    {
        if (apiConnectErrorCallback != null) apiConnectErrorCallback.Invoke();
    }

    //JSONよりResponseData取得
    protected ResponseData<T> GetResponseData<T>(string json)
    {
        ResponseData<T> res = JsonMapper.ToObject<ResponseData<T>>(json);
        UserManager.isAdmin = res.admin;
        return res;
    }

    //JSONよりdata取得
    protected T GetData<T>(string json)
    {
        ResponseData<T> res = JsonMapper.ToObject<ResponseData<T>>(json);
        return res.data;
    }

    //タイトルへ強制遷移
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



using UnityEngine;
using System;
using System.Collections;

namespace User
{
    //### ユーザー情報取得 ###
    public class Get : BaseApi
    {
        public void Exe()
        {
            //API設定
            string uri = "user/get";

            //実行
            base.Exe(uri, GetCollback);
        }
        public void GetCollback(string json)
        {
            GetResponse data = GetData<GetResponse>(json);

            //名前に変更がある場合更新
        }
    }
    [Serializable]
    public class GetResponse
    {
        public string user_name;
        public string terminal_id;
        public string user_agent;
        public string session_id;
        public int status;
    }

    //### ユーザー作成 ###
    public class Create : BaseApi
    {
        public void Exe(Action callback = null)
        {
            //API設定
            isNeedToken = false;
            string uri = "user/create";

            //パラメータ設定
            CreateRequest data = new CreateRequest();
            data.user_name = UserManager.userInfo[Common.PP.INFO_USER_NAME];
            data.terminal_id = SystemInfo.deviceUniqueIdentifier;
            string paramJson = JsonUtility.ToJson(data);

            //コールバック
            Action<string> baseCollback = (json) => CreateCollback(json, callback);

            //実行
            base.Exe(uri, paramJson, baseCollback);
        }
        public void CreateCollback(string json, Action callback = null)
        {
            CreateResponse data = GetData<CreateResponse>(json);
            UserManager.SetApiInfo(data.uuid, data.password);

            //ログイン
            Auth.Login authLogin = new Auth.Login();
            authLogin.Exe(callback);
        }
    }
    [Serializable]
    public class CreateRequest
    {
        public string user_name;
        public string terminal_id;
    }
    [Serializable]
    public class CreateResponse
    {
        public string uuid;
        public string password;
    }
}

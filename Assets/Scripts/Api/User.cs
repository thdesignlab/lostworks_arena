using UnityEngine;
using System;
using System.Collections;

namespace User
{
    //### ユーザー情報取得 ###
    public class Get : BaseApi
    {
        protected override string uri { get { return "user/get"; } }

        public void Exe()
        {
            //実行
            Post(GetCollback);
        }
        public void GetCollback(string json)
        {
            GetResponse data = GetData<GetResponse>(json);

            //名前に変更がある場合更新

            apiFinishCallback.Invoke();
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
        protected override string uri { get { return "user/create"; } }
        protected override bool isNeedToken { get { return false; } }

        public void Exe()
        {
            //パラメータ設定
            CreateRequest data = new CreateRequest();
            data.user_name = UserManager.userInfo[Common.PP.INFO_USER_NAME];
            data.terminal_id = SystemInfo.deviceUniqueIdentifier;
            string paramJson = JsonUtility.ToJson(data);

            //実行
            Post(paramJson, CreateCollback);
        }
        public void CreateCollback(string json)
        {
            CreateResponse data = GetData<CreateResponse>(json);
            UserManager.SetApiInfo(data.uuid, data.password);

            //ログイン
            Auth.Login authLogin = new Auth.Login();
            authLogin.SetApiFinishCallback(apiFinishCallback);
            authLogin.Exe();
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

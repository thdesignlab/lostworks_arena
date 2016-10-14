using UnityEngine;
using System;
using System.Collections;

namespace Auth
{
    public class Login : BaseApi
    {
        protected override string uri { get { return "auth/login"; } }
        protected override bool isNeedToken { get { return false; } }

        public void Exe()
        {
            //パラメータ設定
            LoginRequest data = new LoginRequest();
            data.uuid = UserManager.userInfo[Common.PP.INFO_UUID];
            data.password = UserManager.userInfo[Common.PP.INFO_PASSWORD];
            string paramJson = JsonUtility.ToJson(data);

            ////コールバック
            //Action<string> baseCollback = (json) => LoginCollback(json, callback);

            //実行
            Post(paramJson, LoginCollback);
        }

        public void LoginCollback(string json)
        {
            LoginResponse data = GetData<LoginResponse>(json);
            if (string.IsNullOrEmpty(data.token)) GoToTitle();
            UserManager.apiToken = data.token;
            apiFinishCallback.Invoke();
        }
    }

    [Serializable]
    public class LoginRequest
    {
        public string uuid;
        public string password;
    }

    [Serializable]
    public class LoginResponse
    {
        public string token;
    }
}

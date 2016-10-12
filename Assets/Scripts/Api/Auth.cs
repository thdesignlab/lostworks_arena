using UnityEngine;
using System;
using System.Collections;

namespace Auth
{
    public class Login : BaseApi
    {
        public void Exe(Action callback = null)
        {
            //API設定
            isNeedToken = false;
            string uri = "auth/login";

            //パラメータ設定
            LoginRequest data = new LoginRequest();
            data.uuid = UserManager.userInfo[Common.PP.INFO_UUID];
            data.password = UserManager.userInfo[Common.PP.INFO_PASSWORD];
            string paramJson = JsonUtility.ToJson(data);

            //コールバック
            Action<string> baseCollback = (json) => LoginCollback(json, callback);

            //実行
            base.Exe(uri, paramJson, baseCollback);
        }

        public void LoginCollback(string json, Action callback = null)
        {
            LoginResponse data = GetData<LoginResponse>(json);
            if (string.IsNullOrEmpty(data.token)) GoToTitle();
            UserManager.apiToken = data.token;
            callback.Invoke();
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

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
            Post<GetResponse>();
        }
        protected override void FinishCallback(string json)
        {
            GetResponse data = GetData<GetResponse>(json);

            //名前とかチェック
            UserManager.userInfo[Common.PP.INFO_USER_ID] = data.user_id.ToString();
            if (UserManager.userInfo[Common.PP.INFO_USER_NAME] != data.user_name
                || UserManager.userSetCharacter != data.character_id)
            {
                Update userUpdate = new Update();
                userUpdate.SetApiErrorIngnore();
                userUpdate.Exe();
            }
        }
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
            //data.terminal_id = SystemInfo.deviceUniqueIdentifier;
            string paramJson = JsonUtility.ToJson(data);

            //実行
            Post<CreateResponse>(paramJson);
        }
        protected override void FinishCallback(string json)
        {
            CreateResponse data = GetData<CreateResponse>(json);
            UserManager.SetApiInfo(data.uuid, data.password);

            //ログイン
            Auth.Login authLogin = new Auth.Login();
            authLogin.SetApiFinishCallback(apiFinishCallback);
            authLogin.Exe();
        }
    }

    //### ユーザー情報更新 ###
    public class Update : BaseApi
    {
        protected override string uri { get { return "user/update"; } }

        public void Exe()
        {
            //パラメータ設定
            UpdateRequest data = new UpdateRequest();
            data.user_name = UserManager.userInfo[Common.PP.INFO_USER_NAME];
            data.character_id = UserManager.userSetCharacter;
            string paramJson = JsonUtility.ToJson(data);

            //実行
            Post<GetResponse>(paramJson);
        }
        protected override void FinishCallback(string json)
        {
            GetResponse data = GetData<GetResponse>(json);

            //名前とかチェック
            UserManager.userInfo[Common.PP.INFO_USER_ID] = data.user_id.ToString();
            UserManager.userInfo[Common.PP.INFO_USER_NAME] = data.user_name;
            UserManager.userSetCharacter = data.character_id;
        }
    }


    [Serializable]
    public class GetResponse
    {
        public int user_id;
        public string user_name;
        public int character_id;
        public int status;
    }
    [Serializable]
    public class UpdateRequest
    {
        public string user_name;
        public int character_id;
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

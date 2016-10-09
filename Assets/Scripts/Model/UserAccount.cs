using UnityEngine;
using System;
using System.Collections;

namespace UserAccount
{
    public class Model : BaseModel
    {
        public void Get()
        {
            string uri = "user/get";
            Data dt = new Data();
            dt.user_id = 1;
            Debug.Log(dt);
            string paramJson = ApiManager.Instance.ToJson<Data>(dt);
            ApiManager.Instance.Post(uri, paramJson, GetCollback);
        }

        public void GetCollback(WWW www)
        {
            Debug.Log("UserAccount.GetCollback");
            if (www.error == null)
            {
                Debug.Log(www.text);
                Debug.Log(ApiManager.Instance.FromJson<UserAccount.Data>(www.text));
            }
            else
            {
                Debug.Log(www.error);
            }
        }
    }

    [Serializable]
    public class Data
    {
        public int user_id { get; set; }
        public string user_name { get; set; }
        public string terminal_id { get; set; }
        public string user_agent { get; set; }
        public string session_id { get; set; }
        public int status { get; set; }
    }
}

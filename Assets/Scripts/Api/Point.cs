using UnityEngine;
using System;
using System.Collections;

namespace Point
{
    //### ポイント取得 ###
    public class Get : BaseApi
    {
        protected override string uri { get { return "point/get"; } }

        public void Exe()
        {
            //実行
            Post(GetCollback);
        }
        public void GetCollback(string json)
        {
            GetResponse data = GetData<GetResponse>(json);
            UserManager.userPoint = data.point;
            apiFinishCallback.Invoke();
        }
    }
    [Serializable]
    public class GetResponse
    {
        public int point;
    }
}

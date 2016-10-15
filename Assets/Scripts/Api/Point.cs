using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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
            ApiFinishCallback();
        }
    }
    [Serializable]
    public class GetResponse
    {
        public int point;
    }

    //### ポイント加算 ###
    public class Add : BaseApi
    {
        protected override string uri { get { return "point/add"; } }

        public void Exe(int addPoint)
        {
            GetResponse data = new GetResponse();
            data.point = addPoint;
            string paramJson = JsonUtility.ToJson(data);

            //実行
            Post(paramJson, AddCollback);
        }
        public void AddCollback(string json)
        {
            GetResponse data = GetData<GetResponse>(json);
            UserManager.userPoint = data.point;
            ApiFinishCallback();
        }
    }

    //### ポイントテーブル取得 ###
    public class Table : BaseApi
    {
        protected override string uri { get { return "point/table"; } }

        public void Exe()
        {
            //実行
            Post(TableCollback);
        }
        public void TableCollback(string json)
        {
            MasterManager.mstPointList = GetData<List<MasterPoint>>(json);
            ApiFinishCallback();
        }
    }
}

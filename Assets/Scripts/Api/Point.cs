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
            Post<GetResponse>();
        }
        protected override void FinishCallback(string json)
        {
            GetResponse data = GetData<GetResponse>(json);
            UserManager.userPoint = data.point;
        }
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
            Post<GetResponse>(paramJson);
        }
        protected override void FinishCallback(string json)
        {
            GetResponse data = GetData<GetResponse>(json);
            UserManager.userPoint = data.point;
        }
    }

    //### ポイント消費 ###
    public class Use : BaseApi
    {
        protected override string uri { get { return "point/used"; } }

        public void Exe(int usePoint)
        {
            GetResponse data = new GetResponse();
            data.point = usePoint;
            string paramJson = JsonUtility.ToJson(data);

            //実行
            Post<GetResponse>(paramJson);
        }
        protected override void FinishCallback(string json)
        {
            GetResponse data = GetData<GetResponse>(json);
            UserManager.userPoint = data.point;
        }
    }

    //### ポイントテーブル取得 ###
    public class Table : BaseApi
    {
        protected override string uri { get { return "point/table"; } }

        public void Exe()
        {
            //実行
            Post<List<MasterPoint>>();
        }
        protected override void FinishCallback(string json)
        {
            List<MasterPoint> data = GetData<List<MasterPoint>>(json);
            ModelManager.mstPointList = data;
            foreach (MasterPoint row in ModelManager.mstPointList)
            {
                Debug.Log(row);
            }
        }
    }

    public class GetResponse
    {
        public int point;
    }
}

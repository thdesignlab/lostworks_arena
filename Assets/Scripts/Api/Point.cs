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
            UserManager.isGachaFree = data.gacha_free;
        }
    }

    //### ポイント加算 ###
    public class Add : BaseApi
    {
        protected override string uri { get { return "point/add"; } }

        public void Exe(int addPoint, int kind = 0, int no = 0)
        {
            GetRequest data = new GetRequest();
            data.point = addPoint;
            if (kind > 0 && no > 0)
            {
                data.kind = kind;
                data.no = no;
            }
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

        public void Exe(int usePoint, int kind = 0, int no = 0)
        {
            GetRequest data = new GetRequest();
            data.point = usePoint;
            data.kind = kind;
            data.no = no;
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
        protected override bool isNeedToken { get { return false; } }

        public void Exe()
        {
            //実行
            Post<List<MasterPoint>>();
        }
        protected override void FinishCallback(string json)
        {
            List<MasterPoint> data = GetData<List<MasterPoint>>(json);
            ModelManager.mstPointList = data;
        }
    }

    public class GetRequest
    {
        public int point;
        public int kind;
        public int no;
    }
    public class GetResponse
    {
        public int point;
        public bool gacha_free;
    }
}

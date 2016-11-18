using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace RoomApi
{
    //### ルームキー作成 ###
    public class Create : BaseApi
    {
        protected override string uri { get { return "room/create"; } }

        public void Exe()
        {
            //実行
            Post<RoomData>();
        }
        protected override void FinishCallback(string json)
        {
            ModelManager.roomData = GetData<RoomData>(json);
        }
    }

    //### ルーム名変更 ###
    public class ChangeMaster : BaseApi
    {
        protected override string uri { get { return "room/change_master"; } }

        public void Exe()
        {
            string paramJson = JsonUtility.ToJson(ModelManager.roomData);

            //実行
            Post<object[]>(paramJson);
        }
    }

    //### ルームクリア ###
    public class Clear : BaseApi
    {
        protected override string uri { get { return "room/clear"; } }

        public void Exe()
        {
            //実行
            Post<object[]>();
        }
        protected override void FinishCallback(string json)
        {
            ModelManager.roomData = new RoomData();
        }
    }

    //### ルーム一覧 ###
    public class Get : BaseApi
    {
        protected override string uri { get { return "room/get"; } }

        public void Exe()
        {
            //実行
            Post<List<RoomData>>();
        }
        protected override void FinishCallback(string json)
        {
            ModelManager.roomDataList = GetData<List<RoomData>>(json);
        }
    }
}

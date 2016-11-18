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
    public class Change : BaseApi
    {
        protected override string uri { get { return "room/change_master"; } }

        public void Exe(int enemyUserId)
        {
        }
    }

    //### ルームクリア ###
    public class Clear : BaseApi
    {
        protected override string uri { get { return "room/clear"; } }

        public void Exe(bool isWin)
        {
            //FinishRequest data = new FinishRequest();
            //data.battle_id = ModelManager.battleInfo.battle_id;
            //data.result = isWin ? BATTLE_RESULT_WIN : BATTLE_RESULT_LOSE;
            //string paramJson = JsonUtility.ToJson(data);
        }
    }

    //### ルーム一覧 ###
    public class Get : BaseApi
    {
        protected override string uri { get { return "room/get"; } }

        public void Exe()
        {
        }
    }
}

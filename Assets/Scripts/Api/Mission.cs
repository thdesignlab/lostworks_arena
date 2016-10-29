using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Mission
{
    //### ステージ記録取得 ###
    public class Record : BaseApi
    {
        protected override string uri { get { return "mission/record"; } }

        public void Exe()
        {
            //実行
            Post<MissionRecord>();
        }
        protected override void FinishCallback(string json)
        {
            ModelManager.missionRecord = GetData<MissionRecord>(json);
        }
    }

    //### ステージ記録更新 ###
    public class Update : BaseApi
    {
        protected override string uri { get { return "mission/update"; } }

        public void Exe(int stageLevel, int stageNo, int continueCount)
        {
            MissionRecord data = new MissionRecord();
            data.level = stageLevel;
            data.stage = stageNo;
            data.continue_count = continueCount;
            string paramJson = JsonUtility.ToJson(data);

            //実行
            Post<MissionRecord>(paramJson);
        }
        protected override void FinishCallback(string json)
        {
            ModelManager.missionRecord = GetData<MissionRecord>(json);
        }
    }

    //### ミッションランキング ###
    public class Ranking : BaseApi
    {
        protected override string uri { get { return "mission/ranking"; } }

        public void Exe()
        {
            //実行
            Post<MissionRanking>();
        }
        protected override void FinishCallback(string json)
        {
            ModelManager.missionRanking = GetData<MissionRanking>(json);
        }
    }
}

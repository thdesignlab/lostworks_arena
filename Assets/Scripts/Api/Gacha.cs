using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Gacha
{
    //### ガチャ実行 ###
    public class Play : BaseApi
    {
        protected override string uri { get { return "gacha/play"; } }

        public void Exe(int addPoint)
        {
            PlayRequest data = new PlayRequest();
            data.point = addPoint;
            if (ModelManager.tipsInfo != null)
            {
                data.tips_id = ModelManager.tipsInfo.tips_id;
                data.no = ModelManager.tipsInfo.no;
            }
            string paramJson = JsonUtility.ToJson(data);

            //実行
            Post<PlayResponse>(paramJson);
        }
        protected override void FinishCallback(string json)
        {
            PlayResponse data = GetData<PlayResponse>(json);
            UserManager.userPoint = data.point;
            ModelManager.tipsInfo = data.tips;
        }
    }

    public class PlayRequest
    {
        public int point;
        public int tips_id;
        public int no;
    }
    public class PlayResponse
    {
        public int point;
        public TipsInfo tips;
    }
}

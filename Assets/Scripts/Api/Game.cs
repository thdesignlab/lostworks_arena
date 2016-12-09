using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Game
{
    //### ゲーム設定取得 ###
    public class Config : BaseApi
    {
        protected override string uri { get { return "game/config"; } }

        public void Exe()
        {
            //実行
            Post<List<GameConfig>>();
        }
        protected override void FinishCallback(string json)
        {
            List<GameConfig> dataList = GetData<List<GameConfig>>(json);
            ModelManager.gameConfigList = new Dictionary<string, string>();
            foreach (GameConfig data in dataList)
            {
                ModelManager.gameConfigList.Add(data.key, data.value);
            }
        }
    }
}

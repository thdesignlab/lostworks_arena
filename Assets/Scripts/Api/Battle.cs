using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Battle
{
    //### 戦績取得 ###
    public class Record : BaseApi
    {
        protected override string uri { get { return "battle/record"; } }

        public void Exe()
        {
            //実行
            Post<BattleRecord>();
        }
        protected override void FinishCallback(string json)
        {
            MasterManager.battleRecord = GetData<BattleRecord>(json);
        }
    }
}

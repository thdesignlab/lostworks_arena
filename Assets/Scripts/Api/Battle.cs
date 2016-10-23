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
            ModelManager.battleRecord = GetData<BattleRecord>(json);
        }
    }

    //### バトル開始 ###
    public class Start : BaseApi
    {
        protected override string uri { get { return "battle/start"; } }

        public void Exe(int enemyUserId)
        {
            StartRequest data = new StartRequest();
            data.enemy_user_id = enemyUserId;
            string paramJson = JsonUtility.ToJson(data);

            //実行
            Post<BattleInfo>(paramJson);
        }
        protected override void FinishCallback(string json)
        {
            ModelManager.battleInfo = GetData<BattleInfo>(json);
        }
    }

    //### バトル終了 ###
    public class Finish : BaseApi
    {
        protected override string uri { get { return "battle/finish"; } }

        const int BATTLE_RESULT_WIN = 1;
        const int BATTLE_RESULT_LOSE = 2;

        public void Exe(bool isWin)
        {
            FinishRequest data = new FinishRequest();
            data.battle_id = ModelManager.battleInfo.battle_id;
            data.result = isWin ? BATTLE_RESULT_WIN : BATTLE_RESULT_LOSE;
            string paramJson = JsonUtility.ToJson(data);

            //実行
            Post<BattleRecord>(paramJson);
        }
        protected override void FinishCallback(string json)
        {
            ModelManager.battleRecord = GetData<BattleRecord>(json);
            ModelManager.battleInfo.battle_id = 0;
        }
    }


    [Serializable]
    public class StartRequest
    {
        public int enemy_user_id;
    }
    [Serializable]
    public class FinishRequest
    {
        public int battle_id;
        public int result;
    }

}

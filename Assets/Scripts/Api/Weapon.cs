using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Weapon
{
    //### 武器購入 ###
    public class Buy : BaseApi
    {
        protected override string uri { get { return "weapon/buy"; } }

        public void Exe(int point, int weaponNo)
        {
            BuyResponse data = new BuyResponse();
            data.point = point;
            data.weapon_no = weaponNo;
            string paramJson = JsonUtility.ToJson(data);

            //実行
            Post<PointRequest>(paramJson);
        }
        protected override void FinishCallback(string json)
        {
            PointRequest data = GetData<PointRequest>(json);
            UserManager.userPoint = data.point;
        }
    }
    public class BuyResponse
    {
        public int point;
        public int weapon_no;
    }
    public class PointRequest
    {
        public int point;
    }
}

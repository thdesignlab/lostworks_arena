using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.Text;

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

    //### 武器情報取得   ###
    public class Get : BaseApi
    {
        protected override string uri { get { return "weapon/get"; } }
        protected override bool isNeedToken { get { return false; } }

        public void Exe()
        {
            //実行
            Post<List<MasterWeapon>>();
        }
        protected override void FinishCallback(string json)
        {
            ModelManager.mstWeaponList = GetData<List<MasterWeapon>>(json);
            if (ModelManager.mstWeaponList.Count == 0) return;

            foreach (MasterWeapon mstWeapon in ModelManager.mstWeaponList)
            {
                Common.Weapon.SetWeaponInfo(mstWeapon.weapon_no, mstWeapon.name, mstWeapon.description);
            }
        }
    }
}

﻿using UnityEngine;
using System.Collections.Generic;

namespace Common
{
    public static class CO
    {
        //シーン名
        public const string SCEANE_TITLE = "Title";
        public const string SCEANE_SETTING = "Setting";
        public const string SCEANE_BATTLE = "Battle";

        //NPCの名前
        public const string NPC_NAME = "NPC";

        //リソースフォルダ
        public const string RESOURCE_WEAPON = "Weapon/";
        public const string RESOURCE_BULLET = "Bullet/";
        public const string RESOURCE_EFFECT = "Effect/";

        //スクリーンUI
        public const string SCREEN_CANVAS = "ScreenCanvas/";
        public const string BUTTON_LEFT_ATTACK = "FireLeft";
        public const string BUTTON_RIGHT_ATTACK = "FireRight";
        public const string BUTTON_SHOULDER_ATTACK = "FireShoulder";
        public const string BUTTON_USE_SUB = "UseSub";
        public const string BUTTON_AUTO_LOCK = "AutoLock";

        //移動モーション
        public const string MOTION_RUN = "Run";
        public const string MOTION_DASH = "Dash";
        public const string MOTION_BACK = "Back";
        public const string MOTION_JUMP = "Jump";
        public const string MOTION_LANDING = "Landing";

        //攻撃モーション
        public const string MOTION_LEFT_ATTACK = "LeftAttack";
        public const string MOTION_RIGHT_ATTACK = "RightAttack";
        public const string MOTION_SHOULDER_ATTACK = "ShoulderAttack";
        public const string MOTION_CROSS_RANGE_ATTACK = "CrossRangeAttack";
        public const string MOTION_USE_SUB = "UseSub";

        public static string[] attackMotionArray = new string[] 
        {
            MOTION_LEFT_ATTACK,
            MOTION_RIGHT_ATTACK,
            MOTION_SHOULDER_ATTACK,
            MOTION_CROSS_RANGE_ATTACK,
            MOTION_USE_SUB
        };

        //パーツ名称
        public const string PARTS_BODY = "Body";
        public const string PARTS_GROUNDED = "Grounded";
        public const string PARTS_LEFT_HAND = "LeftHand";
        public const string PARTS_RIGHT_HAND = "RightHand";
        public const string PARTS_SHOULDER = "Shoulder";
        public const string PARTS_SUB = "Sub";

        //全部位名
        public static string[] partsNameArray = new string[]
        {
            PARTS_LEFT_HAND,
            PARTS_RIGHT_HAND,
            PARTS_SHOULDER,
            PARTS_SUB
        };

        //武器タグ
        public const string TAG_WEAPON = "Weapon";

        //弾発射口タグ
        public const string TAG_MUZZLE = "Muzzle";

        //弾の種類(射出系)
        public const string TAG_BULLET_PHYSICS = "Bullet";
        public const string TAG_BULLET_MISSILE = "Missile";
        public const string TAG_BULLET_ENERGY = "EnergyBullet";

        //弾の種類(放出系)
        public const string TAG_BULLET_LASER = "Laser";

        //弾タグ全て
        public static string[] bulletTagArray = new string[]
        {
            TAG_BULLET_PHYSICS,
            TAG_BULLET_MISSILE,
            TAG_BULLET_ENERGY,
        };

        //物理系の弾タグ
        public static string[] physicsBulletArray = new string[]
        {
            TAG_BULLET_PHYSICS,
            TAG_BULLET_MISSILE
        };

        //ダメージ判定のあるタグ
        public static string[] DamageAffectTagArray = new string[]
        {
            TAG_BULLET_PHYSICS,
            TAG_BULLET_MISSILE,
            TAG_BULLET_ENERGY,
            TAG_BULLET_LASER
        };
    }

    public static class Weapon
    {
        public static Dictionary<string, string[]> weaponLineUp = new Dictionary<string, string[]>()
        {
            { "MachineGun",  new string[]{ "マシンガン", "連射するやつ"}},
        };

    }


    public static class Func
    {
        //リソース取得
        public static string GetResourceBullet(string name)
        {
            return Common.CO.RESOURCE_BULLET + name;
        }
        public static string GetResourceEffect(string name)
        {
            return Common.CO.RESOURCE_EFFECT + name;
        }
        public static string GetResourceWeapon(string name)
        {
            return Common.CO.RESOURCE_WEAPON + name;
        }

        //配列チェック
        private static bool InArrayString(string[] tags, string tagName)
        {
            bool flg = false;
            foreach (string tag in tags)
            {
                if (tagName == tag)
                {
                    flg = true;
                    break;
                }
            }
            return flg;
        }

        //物理弾判定
        public static bool IsPhysicsBullet(string tag)
        {
            return InArrayString(Common.CO.physicsBulletArray, tag);
        }

        //弾判定
        public static bool IsBullet(string tag)
        {
            return InArrayString(Common.CO.physicsBulletArray, tag);
        }

        //ダメージ判定
        public static bool IsDamageAffect(string tag)
        {
            return InArrayString(Common.CO.DamageAffectTagArray, tag);
        }

        //三角関数
        public static float GetSin(float time, float anglePerSec = 360,  float startAngle = 0)
        {
            float angle = (startAngle + anglePerSec * time) % 360;
            float radian = Mathf.PI / 180 * angle;
            return Mathf.Sin(radian) * time;
        }
    }
}

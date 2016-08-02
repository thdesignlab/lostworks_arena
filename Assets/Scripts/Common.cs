using UnityEngine;
using System.Collections.Generic;

namespace Common
{
    //### 定数 ###
    public static class CO
    {
        //シーン名
        public const string SCENE_TITLE = "Title";
        public const string SCENE_BATTLE = "Battle";
        public const string SCENE_CUSTOM = "Custom";

        //NPCの名前
        public const string NPC_NAME = "NPC";

        //リソースフォルダ
        public const string RESOURCE_WEAPON = "Weapon/";
        public const string RESOURCE_BULLET = "Bullet/";
        public const string RESOURCE_EFFECT = "Effect/";
        public const string RESOURCE_STRUCTURE = "Structure/";
        public const string RESOURCE_ANIMATION = "Animation/";

        //スクリーンUI
        public const string SCREEN_CANVAS = "ScreenCanvas/";
        public const string SCREEN_STATUS = "Status/";
        public const string SCREEN_INPUT_BUTTON = "InputButton/";
        public const string BUTTON_LEFT_ATTACK = "FireLeft";
        public const string BUTTON_RIGHT_ATTACK = "FireRight";
        public const string BUTTON_SHOULDER_ATTACK = "FireShoulder";
        public const string BUTTON_USE_SUB = "UseSub";
        public const string BUTTON_AUTO_LOCK = "AutoLock";
        //ターゲットマーク
        public const string TARGET_MARK = "TargetMark";
        //メッセージ
        public const string TEXT_UP = "TextUp";
        public const string TEXT_CENTER = "TextCenter";


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

        //BITモーション
        public const string BIT_MOTION_LEFT_OPEN = "Bit_left_gun_open";
        public const string BIT_MOTION_RIGHT_OPEN = "Bit_right_gun_open";
        public const string BIT_MOTION_MISSILE_OPEN = "Bit_missile_open";
        public const string BIT_MOTION_LASER_OPEN = "Bit_center_weapon_open";
        public const int BIT_MOTION_TYPE_GUN = 1;
        public const int BIT_MOTION_TYPE_MISSILE = 2;
        public const int BIT_MOTION_TYPE_LASER = 3;

        //パーツ名称
        public const string PARTS_BODY = "Body";
        public const string PARTS_GROUNDED = "Grounded";
        public const string PARTS_LEFT_HAND = "LeftHand";
        public const string PARTS_LEFT_HAND_DASH = "LeftHandDash";
        public const string PARTS_RIGHT_HAND = "RightHand";
        public const string PARTS_RIGHT_HAND_DASH = "RightHandDash";
        public const string PARTS_SHOULDER = "Shoulder";
        public const string PARTS_SHOULDER_DASH = "ShoulderDash";
        public const string PARTS_SUB = "Sub";

        public const int WEAPON_NORMAL = 0;
        public const int WEAPON_DASH = 1;

        //全部位名
        public static string[] partsNameArray = new string[]
        {
            PARTS_LEFT_HAND,
            PARTS_LEFT_HAND_DASH,
            PARTS_RIGHT_HAND,
            PARTS_RIGHT_HAND_DASH,
            PARTS_SHOULDER,
            PARTS_SHOULDER_DASH,
            PARTS_SUB
        };

        //リロードゲージカラー
        public static Color reloadGageColor = Color.red;

        //武器タグ
        public const string TAG_WEAPON = "Weapon";
        public const string TAG_WEAPON_BIT = "WeaponBit";

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

        //オブジェ
        public static string TAG_STRUCTURE = "Structure";

        //レイヤー
        public static string LAYER_STRUCTURE = "Structure";
    }

    //### 武器詳細 ###
    public static class Weapon
    {
        public static Dictionary<string, string[]> weaponLineUp = new Dictionary<string, string[]>()
        {
            { "MachineGun",  new string[]{ "マシンガン", "連射するやつ"}},
        };

    }

    //### 端末保持情報 ###
    public static class PP
    {
        public const string USER_INFO = "UserInfo";
        public const string USER_RESULT = "UserResult";
        public const string USER_EQUIP = "UserEquip";
        public const string OPEN_CHARACTERS = "OpenCharacters";
        public const string OPEN_WEAPONS = "OpenWeapons";

        public const string INFO_USER_ID = "UserId";
        public const string INFO_USER_NAME = "UserName";

        public const string RESULT_BATTLE_COUNT = "BattleCount";
        public const string RESULT_WIN_COUNT = "WinCount";
        public const string RESULT_LOSE_COUNT = "LoseCount";
        public const string RESULT_BATTLE_RATE = "BattleRate";

        public const string EQUIP_CHARACTER_NO = "CharacterNo";
        public const string EQUIP_LEFT_HAND = "LeftHandWeapon";
        public const string EQUIP_RIGHT_HAND = "RightHandWeapon";
        public const string EQUIP_SHOULDER = "ShoulderWeapon";
        public const string EQUIP_SUB = "SubWeapon";
    }

    //### 共通関数 ###
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
        public static string GetResourceStructure(string name)
        {
            return Common.CO.RESOURCE_STRUCTURE + name;
        }
        public static string GetResourceAnimation(string name)
        {
            return Common.CO.RESOURCE_ANIMATION + name;
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

        //ダメージオブジェクト判定
        public static bool IsDamageAffect(string tag)
        {
            return InArrayString(Common.CO.DamageAffectTagArray, tag);
        }

        //三角関数
        public static float GetSin(float time, float anglePerSec = 360, float startAngle = 0)
        {
            float angle = (startAngle + anglePerSec * time) % 360;
            //Debug.Log("angle:"+angle.ToString());
            float radian = Mathf.PI / 180 * angle;
            return Mathf.Sin(radian) * time;
        }

        //BitMotion取得
        public static string GetBitMotionName(int motionType, string charaMotionName)
        {
            string motionName = "";

            switch (motionType)
            {
                case Common.CO.BIT_MOTION_TYPE_GUN:
                    if (charaMotionName == Common.CO.MOTION_LEFT_ATTACK)
                    {
                        motionName = Common.CO.BIT_MOTION_LEFT_OPEN;
                    }
                    else if (charaMotionName == Common.CO.MOTION_RIGHT_ATTACK)
                    {
                        motionName = Common.CO.BIT_MOTION_RIGHT_OPEN;
                    }
                    else
                    {
                        motionName = Common.CO.BIT_MOTION_MISSILE_OPEN;
                    }
                    break;

                case Common.CO.BIT_MOTION_TYPE_MISSILE:
                    motionName = Common.CO.BIT_MOTION_MISSILE_OPEN;
                    break;

                case Common.CO.BIT_MOTION_TYPE_LASER:
                    motionName = Common.CO.BIT_MOTION_LASER_OPEN;
                    break;
            }
            return motionName;
        }

        //パーツ構造取得
        public static string GetPartsStructure(string partsName)
        {
            //return Common.CO.PARTS_BODY + "/" + partsName;
            return partsName;
        }
        public static string GetPartsStructure(int partsNo)
        {
            return GetPartsStructure(Common.CO.partsNameArray[partsNo]);
        }
    }
}

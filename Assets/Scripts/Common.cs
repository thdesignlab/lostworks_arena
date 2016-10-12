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
        public const string RESOURCE_ANIMATION_2D = "Animation2D/";
        public const string RESOURCE_ANIMATION_3D = "Animation3D/";
        public const string RESOURCE_CHARACTER = "Character/";
        public const string RESOURCE_IMAGE = "Image/";

        //スクリーンUI
        public const string SCREEN_CANVAS = "ScreenCanvas/";
        public const string SCREEN_STATUS = "Status/";
        public const string SCREEN_INPUT_BUTTON = "InputButton/";
        public const string BUTTON_LEFT_ATTACK = "FireLeft";
        public const string BUTTON_RIGHT_ATTACK = "FireRight";
        public const string BUTTON_SHOULDER_ATTACK = "FireShoulder";
        public const string BUTTON_USE_SUB = "UseSub";
        public const string BUTTON_AUTO_LOCK = "AutoLock";
        public const string BUTTON_EXTRA_ATTACK = "FireExtra";

        //ターゲットマーク
        public const string TARGET_MARK = "TargetMark";
        //メッセージ
        public const string TEXT_UP = "TextUp";
        public const string TEXT_CENTER = "TextCenter";
        public const string TEXT_LINE = "TextLine";


        //移動モーション
        public const string MOTION_RUN_VERTICAL = "VerticalMove";
        public const string MOTION_RUN_HORIZONTAL = "HorizontalMove";
        public const string MOTION_RUN = "Run";

        public const string MOTION_DASH = "Dash";
        public const string MOTION_JUMP = "Jump";
        public const string MOTION_DOWN = "Down";

        //攻撃モーション
        public const string MOTION_LEFT_ATTACK = "LeftAttack";
        public const string MOTION_RIGHT_ATTACK = "RightAttack";
        public const string MOTION_SHOULDER_ATTACK = "ShoulderAttack";
        public const string MOTION_CROSS_RANGE_ATTACK = "CrossRangeAttack";
        public const string MOTION_EXTRA_ATTACK = "ExtraAttack";
        public const string MOTION_USE_SUB = "UseSub";
        public static readonly string[] attackMotionArray = new string[]
        {
            MOTION_LEFT_ATTACK,
            MOTION_RIGHT_ATTACK,
            MOTION_SHOULDER_ATTACK,
            MOTION_USE_SUB,
            MOTION_EXTRA_ATTACK,
        };

        //BITモーション
        public const string BIT_MOTION_LEFT_OPEN = "Bit_left_gun_open";
        public const string BIT_MOTION_RIGHT_OPEN = "Bit_right_gun_open";
        public const string BIT_MOTION_MISSILE_OPEN = "Bit_missile_open";
        public const string BIT_MOTION_LASER_OPEN = "Bit_center_weapon_open";
        public const int BIT_MOTION_TYPE_GUN = 1;
        public const int BIT_MOTION_TYPE_MISSILE = 2;
        public const int BIT_MOTION_TYPE_LASER = 3;

        //キャラベース
        public const string CHARACTER_BASE = "BaseHero";
        public const string PARTS_BODY = "Body";
        public const string PARTS_BOX = "Box";
        public const string PARTS_MAIN_BODY = "MainBody";
        public const string PARTS_GROUNDED = "Grounded";

        //パーツ分類No
        public const int PARTS_KIND_HAND_NO = 1;
        public const int PARTS_KIND_HAND_DASH_NO = 2;
        public const int PARTS_KIND_SHOULDER_NO = 3;
        public const int PARTS_KIND_SHOULDER_DASH_NO = 4;
        public const int PARTS_KIND_SUB_NO = 5;
        public const int PARTS_KIND_EXTRA_NO = 6;

        //パーツ分類(タグ)
        public const string PARTS_KIND_HAND = "Hand";
        public const string PARTS_KIND_HAND_DASH = "HandDash";
        public const string PARTS_KIND_SHOULDER = "Shoulder";
        public const string PARTS_KIND_SHOULDER_DASH = "ShoulderDash";
        public const string PARTS_KIND_SUB = "Sub";
        public const string PARTS_KIND_EXTRA = "Extra";

        //パーツ名称(名前)
        public const string PARTS_JOINT = "Parts";
        public const string PARTS_LEFT_HAND = "LeftHand";
        public const string PARTS_LEFT_HAND_DASH = "LeftHandDash";
        public const string PARTS_RIGHT_HAND = "RightHand";
        public const string PARTS_RIGHT_HAND_DASH = "RightHandDash";
        public const string PARTS_SHOULDER = "Shoulder";
        public const string PARTS_SHOULDER_DASH = "ShoulderDash";
        public const string PARTS_SUB = "Sub";
        public const string PARTS_EXTRA = "Extra";

        public const int WEAPON_NORMAL = 0;
        public const int WEAPON_DASH = 1;

        //装備可能パーツNo
        public const int PARTS_LEFT_HAND_NO = 0;
        public const int PARTS_LEFT_HAND_DASH_NO = 1;
        public const int PARTS_RIGHT_HAND_NO = 2;
        public const int PARTS_RIGHT_HAND_DASH_NO = 3;
        public const int PARTS_SHOULDER_NO = 4;
        public const int PARTS_SHOULDER_DASH_NO = 5;
        public const int PARTS_SUB_NO = 6;
        public const int PARTS_EXTRA_NO = 7;

        //装備可能部位名
        public static Dictionary<int, string> partsNameArray = new Dictionary<int, string>()
        {
            { PARTS_LEFT_HAND_NO, PARTS_LEFT_HAND },
            { PARTS_LEFT_HAND_DASH_NO, PARTS_LEFT_HAND_DASH },
            { PARTS_RIGHT_HAND_NO, PARTS_RIGHT_HAND },
            { PARTS_RIGHT_HAND_DASH_NO , PARTS_RIGHT_HAND_DASH },
            { PARTS_SHOULDER_NO, PARTS_SHOULDER },
            { PARTS_SHOULDER_DASH_NO, PARTS_SHOULDER_DASH },
            { PARTS_SUB_NO, PARTS_SUB },
            { PARTS_EXTRA_NO, PARTS_EXTRA },
        };

        //武器タグ
        public const string TAG_WEAPON = "Weapon";
        public const string TAG_WEAPON_BIT = "WeaponBit";
        public const string TAG_BIT_POINT = "BitPoint";

        //弾発射口タグ
        public const string TAG_MUZZLE = "Muzzle";

        //弾の種類(射出系)
        public const string TAG_BULLET_PHYSICS = "Bullet";
        public const string TAG_BULLET_MISSILE = "Missile";
        public const string TAG_BULLET_ENERGY = "EnergyBullet";

        //弾の種類(放出系)
        public const string TAG_BULLET_LASER = "Laser";

        //弾の種類(特殊系)
        public const string TAG_BULLET_EXTRA = "ExtraBullet";
        public const int EXTRA_BULLET_BREAK_RATE = 10;

        //エフェクト
        public const string TAG_EFFECT = "Effect";

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
            TAG_BULLET_MISSILE,
        };

        //ダメージ判定のあるタグ
        public static string[] DamageAffectTagArray = new string[]
        {
            TAG_BULLET_PHYSICS,
            TAG_BULLET_MISSILE,
            TAG_BULLET_ENERGY,
            TAG_BULLET_LASER,
            TAG_BULLET_EXTRA,
            TAG_EFFECT,
        };

        //オブジェ
        public static string TAG_STRUCTURE = "Structure";

        //レイヤー
        public static string LAYER_FLOOR = "Floor";
        public static string LAYER_STRUCTURE = "Structure";
    }

    //### 端末保持情報 ###
    public static class PP
    {
        //保存情報
        public const string USER_INFO = "UserInfo";
        public const string USER_RESULT = "UserResult";
        public const string USER_EQUIP = "UserEquipment";
        public const string USER_CONFIG = "UserConfig";
        public const string USER_CHARACTER = "UserCharacter";
        public const string OPEN_CHARACTERS = "OpenCharacters";
        public const string OPEN_WEAPONS = "OpenWeapons";
        public const string OPEN_MISSIONS = "OpenMissions";

        //ユーザー情報項目
        public const int INFO_USER_ID = 0;
        public const int INFO_USER_NAME = 1;
        public const int INFO_UUID = 3;
        public const int INFO_PASSWORD = 4;

        //バトル結果項目
        public const int RESULT_BATTLE_COUNT = 0;
        public const int RESULT_WIN_COUNT = 1;
        public const int RESULT_LOSE_COUNT = 2;
        public const int RESULT_BATTLE_RATE = 3;

        //コンフィグ情報項目
        public const int CONFIG_BGM_VALUE = 0;
        public const int CONFIG_BGM_MUTE = 1;
        public const int CONFIG_SE_VALUE = 2;
        public const int CONFIG_SE_MUTE = 3;
        public const int CONFIG_VOICE_VALUE = 4;
        public const int CONFIG_VOICE_MUTE = 5;

        //ミッション項目
        public const int MISSION_LEVEL = 0;
        public const int MISSION_STAGE = 1;
    }

    //### 共通関数 ###
    public static class Func
    {
        //リソース取得
        public static string GetResourceBullet(string name)
        {
            return CO.RESOURCE_BULLET + name;
        }
        public static string GetResourceEffect(string name)
        {
            return CO.RESOURCE_EFFECT + name;
        }
        public static string GetResourceWeapon(string name)
        {
            return CO.RESOURCE_WEAPON + name;
        }
        public static string GetResourceStructure(string name)
        {
            return CO.RESOURCE_STRUCTURE + name;
        }
        public static string GetResourceAnimation(string name)
        {
            return CO.RESOURCE_ANIMATION_2D + name;
        }
        public static string GetResourceAnimation3D(string name)
        {
            return CO.RESOURCE_ANIMATION_3D + name;
        }
        public static string GetResourceCharacter(string name)
        {
            return CO.RESOURCE_CHARACTER + name;
        }
        public static string GetResourceSprite(string name)
        {
            return CO.RESOURCE_IMAGE + name;
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
            return InArrayString(CO.physicsBulletArray, tag);
        }

        //弾判定
        public static bool IsBullet(string tag)
        {
            return InArrayString(CO.bulletTagArray, tag);
        }

        //ダメージオブジェクト判定
        public static bool IsDamageAffect(string tag)
        {
            return InArrayString(CO.DamageAffectTagArray, tag);
        }

        //三角関数
        public static float GetSin(float time, float anglePerSec = 360, float startAngle = 0)
        {
            float angle = (startAngle + anglePerSec * time) % 360;
            float radian = Mathf.PI / 180 * angle;
            //Debug.Log("angle:" + angle + " / sin:"+ Mathf.Sin(radian));
            return Mathf.Sin(radian);
        }

        //BitMotion取得
        public static string GetBitMotionName(int motionType, string charaMotionName)
        {
            string motionName = "";

            switch (motionType)
            {
                case CO.BIT_MOTION_TYPE_GUN:
                    if (charaMotionName == CO.MOTION_LEFT_ATTACK)
                    {
                        motionName = CO.BIT_MOTION_LEFT_OPEN;
                    }
                    else if (charaMotionName == CO.MOTION_RIGHT_ATTACK)
                    {
                        motionName = CO.BIT_MOTION_RIGHT_OPEN;
                    }
                    else
                    {
                        motionName = CO.BIT_MOTION_MISSILE_OPEN;
                    }
                    break;

                case CO.BIT_MOTION_TYPE_MISSILE:
                    motionName = CO.BIT_MOTION_MISSILE_OPEN;
                    break;

                case CO.BIT_MOTION_TYPE_LASER:
                    motionName = CO.BIT_MOTION_LASER_OPEN;
                    break;
            }
            return motionName;
        }

        //パーツ構造取得
        public static string GetPartsStructure(string partsName)
        {
            return CO.PARTS_JOINT + "/" + partsName;
        }
        public static string GetPartsStructure(int partsNo)
        {
            return GetPartsStructure(CO.partsNameArray[partsNo]);
        }

        //パーツNo取得
        public static int GetPartsNo(string partsName)
        {
            int partsNo = -1;
            foreach (int key in CO.partsNameArray.Keys)
            {
                if (CO.partsNameArray[partsNo] == partsName)
                {
                    partsNo = key;
                    break;
                }
            }
            return partsNo;
        }

        //アニメーションボディ構造取得
        public static string GetBodyStructure()
        {
            return CO.PARTS_BODY + "/" + CO.PARTS_MAIN_BODY; 
        }

        //BulletNo取得
        public static int GetBulletNo(string bulletName)
        {
            int no = 0;
            int index = bulletName.IndexOf("_");
            if (index >= 0)
            {
                string noStr = bulletName.Substring(index + 1);
                try
                {
                    no = int.Parse(noStr);
                }
                catch (UnityException e)
                {
                    Debug.Log(e);
                }
            }
            //Debug.Log(bulletName+" >> " +no);
            return no;
        }
    }

    //### キャラクター詳細 ###
    public static class Character
    {
        //獲得タイプ
        public const string OBTAIN_TYPE_NONE = "NONE";
        public const string OBTAIN_TYPE_INIT = "INIT";
        public const string OBTAIN_TYPE_STORE = "STORE";

        //武器リストNo
        public const int DETAIL_PREFAB_NAME_NO = 0;     //プレハブ名
        public const int DETAIL_NAME_NO = 1;            //キャラ名
        public const int DETAIL_COLOR_NO = 2;           //カラー
        public const int DETAIL_OBTAIN_TYPE_NO = 3;     //取得タイプ
        public const int DETAIL_EXTRA_WEAPONS_NO = 4;     //必殺武器No

        //キャラクターリスト
        public static Dictionary<int, string[]> characterLineUp = new Dictionary<int, string[]>()
        {
            {100, new string[]{ "Hero1", "Luri", "0", OBTAIN_TYPE_INIT, "10000"}},
            {101, new string[]{ "Hero1", "Luri", "1", OBTAIN_TYPE_INIT, "10000"}},
            {200, new string[]{ "Hero2", "Dango", "0", OBTAIN_TYPE_INIT, "10001"}},
            {201, new string[]{ "Hero2", "Dango", "1", OBTAIN_TYPE_INIT, "10001"}},
            {300, new string[]{ "Hero3", "Nekomimi", "0", OBTAIN_TYPE_INIT, "10005"}},
            {301, new string[]{ "Hero3", "Nekomimi", "1", OBTAIN_TYPE_INIT, "10005"}},
            {400, new string[]{ "Hero4", "Meisai", "0", OBTAIN_TYPE_INIT, "10006"}},
            {401, new string[]{ "Hero4", "Meisai", "1", OBTAIN_TYPE_INIT, "10006"}},
            {500, new string[]{ "Hero5", "Unknown", "0", OBTAIN_TYPE_INIT, "10007"}},
            {10000, new string[]{ "Npc1", "Capsule1", "0", OBTAIN_TYPE_NONE, "10002,10003,10004"}},
        };

        //ステータス
        public const int STATUS_MAX_HP = 0;
        public const int STATUS_RECOVER_SP = 1;
        public const int STATUS_RUN_SPEED = 2;
        public const int STATUS_BOOST_SPEED = 3;
        public const int STATUS_TURN_SPEED = 4;
        public const int STATUS_ATTACK_RATE = 5;
        public const int STATUS_ATTACK_INTERVAL = 6;
        public const int STATUS_BOOST_INTERVAL = 7;
        public const int STATUS_TARGET_INTERVAL = 8;
        public const int STATUS_TARGET_TYPE = 9;
        public const int STATUS_TARGET_DISTANCE = 10;
        public static Dictionary<int, int[]> StatusDic = new Dictionary<int, int[]>()
        {
                            //hp, sp, run, boost, turn, atk%, atkI, boostI, tagI
            { 100, new int[]{ 1000, 45, 35, 75, 20, 100, 2, 2, 3, 1 , 20} },
            { 101, new int[]{ 1000, 45, 35, 75, 20, 100, 2, 2, 3, 1 , 20} },
            { 200, new int[]{ 1000, 45, 30, 70, 20, 110, 2, 2, 3, 1 , 250} },
            { 201, new int[]{ 1000, 45, 30, 70, 20, 110, 2, 2, 3, 1 , 250} },
            { 300, new int[]{ 1000, 55, 30, 70, 20, 100, 2, 2, 3, 1 , 150} },
            { 301, new int[]{ 1000, 55, 30, 70, 20, 100, 2, 2, 3, 1 , 150} },
            { 400, new int[]{ 1200, 50, 30, 70, 20, 100, 2, 2, 3, 1 , 50} },
            { 401, new int[]{ 1200, 50, 30, 70, 20, 100, 2, 2, 3, 1 , 50} },
            { 500, new int[]{ 1200, 35, 35, 75, 25, 110, 2, 2, 3, 1 , 150} },
            { 10000, new int[]{ 800, 35, 25, 50, 20, 80, 3, 3, 4, 0 , 100} },
        };

        public static string[] GetCharacterInfo(int characterNo)
        {
            string[] charaInfo = new string[] { };
            if (characterLineUp.ContainsKey(characterNo))
            {
                charaInfo = characterLineUp[characterNo];
            }
            return charaInfo; 
        }
    }

    //### 武器詳細 ###
    public static class Weapon
    {
        //獲得タイプ
        public const string OBTAIN_TYPE_NONE = "NONE";
        public const string OBTAIN_TYPE_INIT = "INIT";

        //武器リストNo
        public const int DETAIL_PREFAB_NAME_NO = 0;     //プレハブ名
        public const int DETAIL_NAME_NO = 1;            //武器名
        public const int DETAIL_DESCRIPTION_NO = 2;     //説明
        public const int DETAIL_OBTAIN_TYPE_NO = 3;     //取得タイプ

        //ハンド武器リスト
        public static Dictionary<int, string[]> handWeaponLineUp = new Dictionary<int, string[]>()
        {
            { 1000, new string[]{ "Rifle", "", "", OBTAIN_TYPE_INIT}},
            { 1001, new string[]{ "BrasterLauncher", "", "", OBTAIN_TYPE_INIT}},
            { 1002, new string[]{ "BeamCannon", "", "", OBTAIN_TYPE_INIT}},
            { 1003, new string[]{ "PlasmaGun", "", "", OBTAIN_TYPE_INIT}},
            { 1004, new string[]{ "BlazePillar", "", "", OBTAIN_TYPE_INIT}},
            { 1005, new string[]{ "CERifle", "", "", OBTAIN_TYPE_INIT}},
            { 1006, new string[]{ "Stinger", "", "", OBTAIN_TYPE_INIT}},
            { 1007, new string[]{ "TridentPillar", "", "", OBTAIN_TYPE_INIT}},
        };
        //ハンド武器(ダッシュ)リスト
        public static Dictionary<int, string[]> handDashWeaponLineUp = new Dictionary<int, string[]>()
        {
            { 2000, new string[]{ "MachineGun", "", "", OBTAIN_TYPE_INIT}},
            { 2001, new string[]{ "GatlingGun", "", "", OBTAIN_TYPE_INIT}},
            { 2002, new string[]{ "PulseGun", "", "", OBTAIN_TYPE_INIT}},
            { 2003, new string[]{ "ThrowingDagger", "", "", OBTAIN_TYPE_INIT}},
            { 2004, new string[]{ "PenetrateDagger", "", "", OBTAIN_TYPE_INIT}},
            { 2005, new string[]{ "LaserBlade", "", "", OBTAIN_TYPE_INIT}},
            { 2006, new string[]{ "BulletBomb", "", "", OBTAIN_TYPE_INIT}},
        };
        //背中武器リスト
        public static Dictionary<int, string[]> shoulderWeaponLineUp = new Dictionary<int, string[]>()
        {
            { 3000, new string[]{ "CECannon", "", "", OBTAIN_TYPE_INIT }},
            { 3001, new string[]{ "HugeLaser", "", "", OBTAIN_TYPE_INIT}},
            { 3002, new string[]{ "SatelliteMissile", "", "", OBTAIN_TYPE_INIT}},
            { 3003, new string[]{ "Cyclone", "", "", OBTAIN_TYPE_INIT}},
            { 3004, new string[]{ "EnergyShield", "", "", OBTAIN_TYPE_INIT}},
            { 3005, new string[]{ "ChargeArrow", "", "", OBTAIN_TYPE_INIT}},
            { 3006, new string[]{ "SearchRay", "", "", OBTAIN_TYPE_INIT}},
        };
        //背中武器(ダッシュ)リスト
        public static Dictionary<int, string[]> shoulderDashWeaponLineUp = new Dictionary<int, string[]>()
        {
            { 4000, new string[]{ "MissileLauncher", "", "", OBTAIN_TYPE_INIT}},
            { 4001, new string[]{ "ClusterLaser", "", "", OBTAIN_TYPE_INIT}},
            { 4002, new string[]{ "GatlingCannon", "", "", OBTAIN_TYPE_INIT}},
            { 4003, new string[]{ "Shotgun", "", "", OBTAIN_TYPE_INIT}},
            { 4004, new string[]{ "RoundMissile", "", "", OBTAIN_TYPE_INIT}},
            { 4005, new string[]{ "AssaultCharge", "", "", OBTAIN_TYPE_INIT}},
        };
        //サブ武器リスト
        public static Dictionary<int, string[]> subWeaponLineUp = new Dictionary<int, string[]>()
        {
            { 5001, new string[]{ "AvoidBurst", "", "", OBTAIN_TYPE_INIT}},
            { 5000, new string[]{ "InvincibleShield", "", "", OBTAIN_TYPE_NONE}},
            { 5002, new string[]{ "BoostRecoverSp", "", "", OBTAIN_TYPE_INIT}},
            { 5003, new string[]{ "SpeedBurst", "", "", OBTAIN_TYPE_INIT}},
            { 5004, new string[]{ "AttackBurst", "", "", OBTAIN_TYPE_INIT}},
        };
        //スペシャル武器リスト
        public static Dictionary<int, string[]> extraWeaponLineUp = new Dictionary<int, string[]>()
        {
            { 10000, new string[]{ "ExtraArmor", "ExArmor", "爆発するよ！", OBTAIN_TYPE_INIT}},
            { 10001, new string[]{ "ExtraBeam", "ExBeam", "撃つよ！", OBTAIN_TYPE_INIT}},
            { 10002, new string[]{ "ExtraBurning", "ExBurning", "突撃するよ！", OBTAIN_TYPE_INIT}},
            { 10003, new string[]{ "ExtraRifle", "ExRifle", "ふっとばすよ！", OBTAIN_TYPE_INIT}},
            { 10004, new string[]{ "ExtraShadowSewing", "ExShadowDagger", "拘束するよ！", OBTAIN_TYPE_INIT}},
            { 10005, new string[]{ "ExtraClaw", "ExClaw", "にゃー！", OBTAIN_TYPE_INIT}},
            { 10006, new string[]{ "ExtraHolyRay", "ExHolyRay", "光の裁きを！", OBTAIN_TYPE_INIT}},
            { 10007, new string[]{ "ExtraScythe", "ExScythe", "斬るよー！", OBTAIN_TYPE_INIT}},
        };

        //部位ごとの武器リスト取得
        public static Dictionary<int, string[]> GetWeaponList(int partsNo)
        {
            Dictionary<int, string[]> weaponList = new Dictionary<int, string[]>();
            switch (partsNo)
            {
                case CO.PARTS_LEFT_HAND_NO:
                case CO.PARTS_RIGHT_HAND_NO:
                    weaponList = handWeaponLineUp;
                    break;

                case CO.PARTS_LEFT_HAND_DASH_NO:
                case CO.PARTS_RIGHT_HAND_DASH_NO:
                    weaponList = handDashWeaponLineUp;
                    break;

                case CO.PARTS_SHOULDER_NO:
                    weaponList = shoulderWeaponLineUp;
                    break;

                case CO.PARTS_SHOULDER_DASH_NO:
                    weaponList = shoulderDashWeaponLineUp;
                    break;

                case CO.PARTS_SUB_NO:
                    weaponList = subWeaponLineUp;
                    break;

                case CO.PARTS_EXTRA_NO:
                    //特別武器
                    weaponList = extraWeaponLineUp;
                    break;
            }

            return weaponList;
        }

        //武器情報を取得する
        public static string[] GetWeaponInfo(int weaponNo)
        {
            string[] weaponInfo = new string[] { };
            if (handWeaponLineUp.ContainsKey(weaponNo))
            {
                weaponInfo = handWeaponLineUp[weaponNo];
            }
            else if (handDashWeaponLineUp.ContainsKey(weaponNo))
            {
                weaponInfo = handDashWeaponLineUp[weaponNo];
            }
            else if (shoulderWeaponLineUp.ContainsKey(weaponNo))
            {
                weaponInfo = shoulderWeaponLineUp[weaponNo];
            }
            else if (shoulderDashWeaponLineUp.ContainsKey(weaponNo))
            {
                weaponInfo = shoulderDashWeaponLineUp[weaponNo];
            }
            else if (subWeaponLineUp.ContainsKey(weaponNo))
            {
                weaponInfo = subWeaponLineUp[weaponNo];
            }
            else if (extraWeaponLineUp.ContainsKey(weaponNo))
            {
                weaponInfo = extraWeaponLineUp[weaponNo];
            }
            return weaponInfo;
        }

        //特殊武器装備可能チェック
        public static bool IsEnabledEquipExtraWeapon(int charaNo, int weaponNo)
        {
            bool isEnabled = false;
            int[] weaponNoArray = GetExtraWeaponNoArray(charaNo);
            foreach (int no in weaponNoArray)
            {
                if (no == weaponNo)
                {
                    isEnabled = true;
                    break;
                }
            }
            return isEnabled;
        }

        //特殊武器チェック
        public static bool isExtraWeapon(int weaponNo)
        {
            if (extraWeaponLineUp.ContainsKey(weaponNo))
            {
                return true;
            }
            return false;
        }

        //装備可能な特殊武器Noを取得する
        public static int[] GetExtraWeaponNoArray(int characterNo)
        {
            List<int> weaponNoList = new List<int>();
            string[] charaInfo = Character.GetCharacterInfo(characterNo);
            if (charaInfo.Length > 0)
            {
                string weaponsNoStr = charaInfo[Character.DETAIL_EXTRA_WEAPONS_NO];
                string[] weaponsNo = weaponsNoStr.Split(',');
                foreach (string weaponNo in weaponsNo)
                {
                    weaponNoList.Add(int.Parse(weaponNo));
                }
            }
            return weaponNoList.ToArray();
        }

        //特殊武器Noを取得する
        public static int GetExtraWeaponNo(int characterNo, int index = -1)
        {
            int weaponNo = -1;
            int[] weaponNoArray = GetExtraWeaponNoArray(characterNo);
            if (index < 0)
            {
                index = Random.Range(0, weaponNoArray.Length);
            }
            if (0 < weaponNoArray.Length && index < weaponNoArray.Length)
            {
                weaponNo = weaponNoArray[index];
            }
            return weaponNo;
        }

        //武器名を取得する
        public static string GetWeaponName(int weaponNo, bool isPrefab = false)
        {
            string weaponName = "";
            string[] weaponInfo = GetWeaponInfo(weaponNo);
            if (weaponInfo.Length > 0)
            {
                weaponName = GetWeaponName(weaponInfo, isPrefab);
            }
            return weaponName;
        }
        private static string GetWeaponName(string[] weaponInfo, bool isPrefab = false)
        {
            string weaponName = "";
            if (!isPrefab)
            {
                weaponName = weaponInfo[DETAIL_NAME_NO];
            }
            if (weaponName == "")
            {
                weaponName = weaponInfo[DETAIL_PREFAB_NAME_NO];
            }
            return weaponName;
        }
    }

    //### ミッション ###
    public static class Mission
    {
        //ステージ：NPC
        public const int STAGE_NPC_NAME = 0;
        public const int STAGE_NPC_BGM = 1;
        public static Dictionary<int, int[]> stageNpcNoDic = new Dictionary<int, int[]>()
        {
            { 1, new int[] { 10000, 0 } },
            { 2, new int[] { 100, 1 } },
            { 3, new int[] { 200, 1 } },
            { 4, new int[] { 300, 2 } },
            { 5, new int[] { 400, 3 } },
            { 6, new int[] { 500, 4 } },
        };

        //レベル
        public static Dictionary<int, string> levelNameDic = new Dictionary<int, string>
        {
            { 1, "Easy" },
            { 2, "Normal" },
            { 3, "Hard" },
            { 4, "Heavy" },
            { 5, "Another" },
            { 6, "Expert" },
            { 7, "Challenge" },
            { 8, "Nightmare" },
            { 9, "Inferno" },
            { 10, "Infinite" },
        };
        public static string GetLevelName(int level)
        {
            string name = "";
            if (level > 0)
            {
                if (levelNameDic.ContainsKey(level))
                {
                    name = levelNameDic[level];
                }
                else
                {
                    name = levelNameDic[levelNameDic.Count];
                    int over = levelNameDic.Count - level + 1;
                    name += over.ToString();
                }
            }
            return name;
        }

        //レベルによるステータス変化
        public static Dictionary<int, float[]> npcLevelStatusDic = new Dictionary<int, float[]>()
        {
                            //hp, sp, run, boost, turn, atk%, atkI, boostI, tagI
            { 1, new float[] { 0.7f, 1.0f, 0.8f, 0.7f, 0.7f, 0.6f, 1.5f, 1.5f, 1.5f } },
            { 2, new float[] { 0.8f, 1.0f, 0.9f, 0.8f, 1.0f, 0.8f, 1.2f, 1.2f, 1.0f } },
            { 3, new float[] { 1.0f, 1.2f, 1.0f, 1.0f, 1.2f, 1.0f, 1.0f, 1.0f, 0.7f } },
            { 4, new float[] { 1.1f, 1.3f, 1.1f, 1.1f, 1.5f, 1.0f, 0.8f, 0.7f, 0.5f } },
            { 5, new float[] { 1.2f, 1.5f, 1.2f, 1.2f, 2.0f, 1.0f, 0.7f, 0.5f, 0.3f } },
        };
        //設定レベル以上の場合の追加Rate
        public static float[] overLevelState = new float[] { 0.05f, 0.1f, 0.01f, 0.02f, 0.02f, 0.03f, 0, 0, 0};

        //NPC武器
        public static Dictionary<int, int[]> npcWeaponDic = new Dictionary<int, int[]>()
        {
            { -1, new int[]{ 0, 0, 0, 0, 0, 0, 0} },
            { 100, new int[]{ 2005, 1004, 2005, 2002, 3004, 4003, 5003} },
            { 101, new int[]{ 2005, 1004, 2005, 2002, 3004, 4003, 5003} },
            { 200, new int[]{ 1002, 4002, 2004, 1001, 3002, 4000, 5001} },
            { 201, new int[]{ 1002, 4002, 2004, 1001, 3002, 4000, 5001} },
            { 300, new int[]{ 0, 0, 0, 0, 0, 0, 0} },
            { 400, new int[]{ 0, 0, 0, 0, 0, 0, 0} },
            { 500, new int[]{ 0, 0, 0, 0, 0, 0, 0} },
            { 10000, new int[]{ 0, 0, 2000, 2001, 0, 0, 5002} },
            { 10001, new int[]{ 1000, 1002, 0, 0, 0, 0, 5002} },
            { 10002, new int[]{ 2002, 2002, 2003, 2003, 3001, 4001, 5003} },
        };
    }
}
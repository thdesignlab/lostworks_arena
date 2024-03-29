﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Common
{
    //### 定数 ###
    public static class CO
    {
        //アプリID
        public const string APP_NAME_IOS = "";
        public const string APP_NAME_ANDROID = "com.ThDesignLab";

        //HomePage
        public const string WEBVIEW_KEY = "?webview";
        public static string HP_URL = "http://lostworks.th-designlab.com/";
        public static string HP_TUTORIAL_URL = HP_URL + "arena/howtoplay";

        //シーン名
        public const string SCENE_TITLE = "Title";
        public const string SCENE_BATTLE = "Battle";
        public const string SCENE_CUSTOM = "Custom";
        public const string SCENE_STORE = "Store";
        public const string SCENE_RANKING = "Ranking";

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
        //エネルギー系の弾タグ
        public static string[] energyBulletArray = new string[]
        {
            TAG_BULLET_ENERGY,
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
        public static string TAG_FLOOR = "Floor";
        public static string TAG_STRUCTURE = "Structure";

        //レイヤー
        public static string LAYER_FLOOR = "Floor";
        public static string LAYER_STRUCTURE = "Structure";

        //衝突判定するタグ
        public static string[] ColliderHitTagArray = new string[]
        {
            "Player",
            "Target",
            TAG_BULLET_PHYSICS,
            TAG_BULLET_MISSILE,
            TAG_BULLET_ENERGY,
            TAG_FLOOR,
            TAG_STRUCTURE,
        };
    }

    //### API用定数 ###
    public static class API
    {
        //ポイントKIND
        public const int POINT_LOG_KIND_GACHA = 1;
        public const int POINT_LOG_KIND_MISSION = 2;
        public const int POINT_LOG_KIND_BATTLE = 3;
        public const int POINT_LOG_KIND_WEAPON = 101;
        public const int POINT_LOG_KIND_CUSTOM = 102;
        public const int POINT_LOG_KIND_CHARACTER = 103;
        public const int POINT_LOG_KIND_MUSIC = 104;
    }

    //### 端末保持情報 ###
    public static class PP
    {
        //保存情報
        public const string USER_INFO = "UserInfo";
        public const string USER_EQUIP = "UserEquipment";
        public const string USER_CONFIG = "UserConfig";
        public const string USER_CHARACTER = "UserCharacter";
        public const string OPEN_CHARACTERS = "OpenCharacters";
        public const string OPEN_WEAPONS = "OpenWeapons";
        public const string OPEN_MISSIONS = "OpenMissions";
        public const string OPEN_MUSICS = "OpenMusics";
        public const string CUSTOM_WEAPONS = "CustomWeapons";

        //ユーザー情報項目
        public const int INFO_USER_ID = 0;
        public const int INFO_USER_NAME = 1;
        public const int INFO_UUID = 3;
        public const int INFO_PASSWORD = 4;

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
        //public const int MISSION_CONTINUE = 2;
    }

    //### 共通関数 ###
    public static class Func
    {
        //platform確認
        public static bool IsAndroid()
        {
            if (Application.platform == RuntimePlatform.Android) return true;
            return false;
        }
        public static bool IsIos()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer) return true;
            return false;
        }
        public static bool IsPc()
        {
            if (IsAndroid() || IsIos()) return false;
            return true;
        }

        //storeUrl取得
        public static string GetStoreUrl()
        {
            string url = "";
#if UNITY_IOS
            if (!string.IsNullOrEmpty(CO.APP_NAME_IOS))
            {
                url= string.Format("itms-apps://itunes.apple.com/app/id{0}?mt=8", CO.APP_NAME_IOS);
            }
#elif UNITY_ANDROID
            if (!string.IsNullOrEmpty(CO.APP_NAME_ANDROID))
            {
                if (IsPc())
                {
                    url = "https://play.google.com/store/apps/details?id=" + CO.APP_NAME_ANDROID;
                }
                else
                {
                    url = "market://details?id=" + CO.APP_NAME_ANDROID;
                }
            }
#endif
            return url;
        }

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

        //カスタムIcon
        public static Sprite GetCustomIcon(int customType)
        {
            string name = "";
            switch (customType)
            {
                case Weapon.CUSTOM_TYPE_POWER:
                    name = "Power";
                    break;

                case Weapon.CUSTOM_TYPE_TECHNIC:
                    name = "Technic";
                    break;

                case Weapon.CUSTOM_TYPE_UNIQUE:
                    name = "Unique";
                    break;
            }
            Sprite icon = Resources.Load<Sprite>(CO.RESOURCE_IMAGE + "CustomIcon/" + name);
            return icon;
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

        //衝突判定タグ
        public static bool IsColliderHitTag(string tag)
        {
            return InArrayString(CO.ColliderHitTagArray, tag);
        }

        //物理弾判定
        public static bool IsPhysicsBullet(string tag)
        {
            return InArrayString(CO.physicsBulletArray, tag);
        }
        //エネルギー弾判定
        public static bool IsEnergyBullet(string tag)
        {
            return InArrayString(CO.energyBulletArray, tag);
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
                catch
                {
                }
            }
            return no;
        }

        //抽選
        public static T Draw<T>(Dictionary<T, int> targets)
        {
            T drawObj = default(T);
            int sumRate = 0;
            List<T> targetValues = new List<T>();
            foreach (T obj in targets.Keys)
            {
                sumRate += targets[obj];
                targetValues.Add(obj);
            }
            if (sumRate == 0) return drawObj;

            int drawNum = Random.Range(1, sumRate + 1);
            sumRate = 0;
            for (int i = 0; i < targets.Count; i++)
            {
                int key = Random.Range(0, targetValues.Count);
                sumRate += targets[targetValues[key]];
                if (sumRate >= drawNum)
                {
                    drawObj = targetValues[key];
                    break;
                }
                targetValues.RemoveAt(key);
            }
            return drawObj;
        }

        public static TKey RandomDic<TKey, TValue>(Dictionary<TKey, TValue> dic)
        {
            return dic.ElementAt(Random.Range(0, dic.Count)).Key;
        }

        //ステータスバー設定
        public static void SetStatusbar()
        {
            Screen.fullScreen = true;

            //Androidステータスバー
            ApplicationChrome.statusBarState = ApplicationChrome.States.TranslucentOverContent;
            //ApplicationChrome.statusBarState = ApplicationChrome.States.Visible;
            ApplicationChrome.navigationBarState = ApplicationChrome.States.Hidden;
        }
    }

    //### キャラクター詳細 ###
    public static class Character
    {
        //獲得タイプ
        public const string OBTAIN_TYPE_NONE = "NONE";
        public const string OBTAIN_TYPE_INIT = "INIT";
        public const string OBTAIN_TYPE_STORE = "STORE";
        public const string OBTAIN_TYPE_MISSION = "MISSION";

        //武器リストNo
        public const int DETAIL_PREFAB_NAME_NO = 0;     //プレハブ名
        public const int DETAIL_NAME_NO = 1;            //キャラ名
        public const int DETAIL_COLOR_NO = 2;           //カラー
        public const int DETAIL_OBTAIN_TYPE_NO = 3;     //取得タイプ
        public const int DETAIL_EXTRA_WEAPONS_NO = 4;     //必殺武器No

        //キャラクターリスト
        public static Dictionary<int, string[]> characterLineUp = new Dictionary<int, string[]>()
        {
            {100, new string[]{ "Hero1", "Proto", "0", OBTAIN_TYPE_INIT, "10000"}},
            {101, new string[]{ "Hero1", "Proto", "1", OBTAIN_TYPE_MISSION, "10000"}},
            {200, new string[]{ "Hero2", "Aria", "0", OBTAIN_TYPE_INIT, "10001"}},
            {201, new string[]{ "Hero2", "Aria", "1", OBTAIN_TYPE_MISSION, "10001"}},
            {300, new string[]{ "Hero3", "Mike", "0", OBTAIN_TYPE_INIT, "10005"}},
            {301, new string[]{ "Hero3", "Mike", "1", OBTAIN_TYPE_MISSION, "10005"}},
            {400, new string[]{ "Hero4", "Makina", "0", OBTAIN_TYPE_INIT, "10006"}},
            {401, new string[]{ "Hero4", "Makina", "1", OBTAIN_TYPE_MISSION, "10006"}},
            {500, new string[]{ "Hero5", "Unknown", "0", OBTAIN_TYPE_INIT, "10007"}},
            {501, new string[]{ "Hero5", "Unknown", "1", OBTAIN_TYPE_MISSION, "10007"}},
            {600, new string[]{ "Hero6", "Vaio", "0", OBTAIN_TYPE_INIT, "10010"}},
            {601, new string[]{ "Hero6", "Vaio", "1", OBTAIN_TYPE_MISSION, "10010"}},
            {10000, new string[]{ "Npc1", "Bit", "0", OBTAIN_TYPE_NONE, "10002"}},
            {10001, new string[]{ "Npc2", "BitBrack", "0", OBTAIN_TYPE_NONE, "10003"}},
            {10002, new string[]{ "Npc3", "BitYellow", "0", OBTAIN_TYPE_NONE, "10004"}},
            {10003, new string[]{ "Npc4", "BitRed", "0", OBTAIN_TYPE_NONE, "10009"}},
            {10004, new string[]{ "Npc5", "BitBlue", "0", OBTAIN_TYPE_NONE, "10008"}},
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
        public const int STATUS_TARGET_DISTANCE_MIN = 10;
        public const int STATUS_TARGET_DISTANCE_MAX = 11;
        public static Dictionary<int, int[]> StatusDic = new Dictionary<int, int[]>()
        {                   //hp, sp, run, boost, turn, atk%, atkI, boostI, tagI
            { 100, new int[]{ 900, 45, 36, 60, 31, 100, 3, 2, 2, 1, 0, 10 } },
            { 200, new int[]{ 1000, 40, 30, 65, 22, 110, 3, 2, 2, 1, 150, 250 } },
            { 300, new int[]{ 1000, 50, 30, 70, 28, 95, 3, 2, 2, 1, 20, 60 } },
            { 400, new int[]{ 1000, 40, 33, 60, 25, 105, 3, 2, 2, 1, 100, 200 } },
            { 500, new int[]{ 1100, 30, 33, 65, 28, 105, 2, 2, 2, 1, 50, 150 } },
            { 600, new int[]{ 1200, 45, 25, 50, 25, 100, 3, 2, 2, 0, 0, 250 } },

            { 101, new int[]{ 950, 35, 36, 55, 34, 105, 3, 2, 2, 1, 0, 10 } },
            { 201, new int[]{ 1050, 30, 30, 60, 25, 115, 3, 2, 2, 1, 150, 250 } },
            { 301, new int[]{ 1050, 40, 30, 65, 31, 100, 3, 2, 2, 1, 20, 60 } },
            { 401, new int[]{ 1050, 30, 33, 55, 28, 110, 3, 2, 2, 1, 100, 200 } },
            { 501, new int[]{ 1200, 20, 33, 60, 31, 110, 2, 2, 2, 1, 50, 150 } },
            { 601, new int[]{ 1300, 35, 25, 40, 28, 105, 3, 2, 2, 0, 0, 250 } },

            { 10000, new int[]{ 800, 35, 25, 50, 20, 90, 4, 3, 3, 0, 0, 150 } },
            { 10001, new int[]{ 800, 35, 45, 50, 20, 90, 4, 3, 3, 1, 0, 100 } },
            { 10002, new int[]{ 800, 35, 25, 100, 20, 90, 4, 3, 3, 1, 50, 200 } },
            { 10003, new int[]{ 800, 35, 25, 50, 50, 90, 3, 3, 3, 1, 100, 250 } },
            { 10004, new int[]{ 800, 35, 25, 50, 20, 130, 3, 3, 3, 1, 10, 180 } },
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
        public const string OBTAIN_TYPE_STORE = "STORE";

        //強化タイプ
        public const int CUSTOM_TYPE_POWER = 1;
        public const int CUSTOM_TYPE_TECHNIC = 2;
        public const int CUSTOM_TYPE_UNIQUE = 3;
        public static Dictionary<int, string> customTypeNameDic = new Dictionary<int, string>()
        {
            { CUSTOM_TYPE_POWER, "Power" },
            { CUSTOM_TYPE_TECHNIC, "Technic" },
            { CUSTOM_TYPE_UNIQUE, "Unique" },
        };
        public const int MAX_CUSTOM_LEVEL = 1;

        //武器リストNo
        public const int DETAIL_PREFAB_NAME_NO = 0;     //プレハブ名
        public const int DETAIL_NAME_NO = 1;            //武器名
        public const int DETAIL_DESCRIPTION_NO = 2;     //説明
        public const int DETAIL_OBTAIN_TYPE_NO = 3;     //取得タイプ
        public const int DETAIL_RUBY_NO = 4;     //武器名読み

        //ハンド武器リスト
        public static Dictionary<int, string[]> handWeaponLineUp = new Dictionary<int, string[]>()
        {
            { 1004, new string[]{ "BlazePillar", "", "", OBTAIN_TYPE_INIT, ""}},
            { 1000, new string[]{ "Rifle", "", "", OBTAIN_TYPE_INIT, ""}},
            { 1001, new string[]{ "BlasterLauncher", "", "", OBTAIN_TYPE_INIT, ""}},
            { 1002, new string[]{ "BeamCannon", "", "", OBTAIN_TYPE_INIT, ""}},
            { 1003, new string[]{ "PlasmaGun", "", "", OBTAIN_TYPE_INIT, ""}},
            { 1005, new string[]{ "CERifle", "", "", OBTAIN_TYPE_STORE, ""}},
            { 1006, new string[]{ "Stinger", "", "", OBTAIN_TYPE_INIT, ""}},
            { 1007, new string[]{ "TridentPillar", "", "", OBTAIN_TYPE_STORE, ""}},
            { 1008, new string[]{ "FlameRadiation", "", "", OBTAIN_TYPE_STORE, ""}},
            { 1009, new string[]{ "HeartGun", "", "", OBTAIN_TYPE_STORE, ""}},
            { 1010, new string[]{ "Human", "", "", OBTAIN_TYPE_STORE, ""}},
            { 1011, new string[]{ "Whirlwind", "", "", OBTAIN_TYPE_STORE, ""}},
        };
        //ハンド武器(ダッシュ)リスト
        public static Dictionary<int, string[]> handDashWeaponLineUp = new Dictionary<int, string[]>()
        {
            { 2004, new string[]{ "PenetrateDagger", "", "", OBTAIN_TYPE_INIT, ""}},
            { 2000, new string[]{ "MachineGun", "", "", OBTAIN_TYPE_INIT, ""}},
            { 2001, new string[]{ "GatlingGun", "", "", OBTAIN_TYPE_INIT, ""}},
            { 2002, new string[]{ "PulseGun", "", "", OBTAIN_TYPE_INIT, ""}},
            { 2003, new string[]{ "ThrowingDagger", "", "", OBTAIN_TYPE_INIT, ""}},
            { 2005, new string[]{ "LaserBlade", "", "", OBTAIN_TYPE_STORE, ""}},
            { 2006, new string[]{ "BulletBomb", "", "", OBTAIN_TYPE_INIT, ""}},
            { 2007, new string[]{ "Grudge", "", "", OBTAIN_TYPE_STORE, ""}},
            { 2008, new string[]{ "MachinegunBit", "", "", OBTAIN_TYPE_STORE, ""}},
            { 2009, new string[]{ "GatlingClaw", "", "", OBTAIN_TYPE_STORE, ""}},
            { 2010, new string[]{ "PlasmaBlade", "", "", OBTAIN_TYPE_STORE, ""}},
            { 2011, new string[]{ "HandMissile", "", "", OBTAIN_TYPE_STORE, ""}},
        };
        //背中武器リスト
        public static Dictionary<int, string[]> shoulderWeaponLineUp = new Dictionary<int, string[]>()
        {
            { 3008, new string[]{ "HighBeamCannon", "", "", OBTAIN_TYPE_INIT, "" }},
            { 3000, new string[]{ "Bioparachute", "", "", OBTAIN_TYPE_INIT, ""}},
            { 3001, new string[]{ "HugeLaser", "", "", OBTAIN_TYPE_STORE, ""}},
            { 3002, new string[]{ "SatelliteMissile", "", "", OBTAIN_TYPE_INIT, ""}},
            { 3003, new string[]{ "Cyclone", "", "", OBTAIN_TYPE_STORE, ""}},
            { 3004, new string[]{ "EnergyShield", "", "", OBTAIN_TYPE_INIT, ""}},
            { 3005, new string[]{ "ChargeArrow", "", "", OBTAIN_TYPE_STORE, ""}},
            { 3006, new string[]{ "SearchRay", "", "", OBTAIN_TYPE_STORE, ""}},
            { 3007, new string[]{ "EnergyLauncher", "", "", OBTAIN_TYPE_INIT, "" }},
            { 3009, new string[]{ "ChemicalCannon", "", "", OBTAIN_TYPE_STORE, "" }},
            { 3010, new string[]{ "ReflectionWall", "", "", OBTAIN_TYPE_STORE, ""}},
        };
        //背中武器(ダッシュ)リスト
        public static Dictionary<int, string[]> shoulderDashWeaponLineUp = new Dictionary<int, string[]>()
        {
            { 4000, new string[]{ "MissileLauncher", "", "", OBTAIN_TYPE_INIT, ""}},
            { 4001, new string[]{ "ClusterLaser", "", "", OBTAIN_TYPE_INIT, ""}},
            { 4002, new string[]{ "GatlingBit", "", "", OBTAIN_TYPE_INIT, ""}},
            { 4003, new string[]{ "Shotgun", "", "", OBTAIN_TYPE_INIT, ""}},
            { 4004, new string[]{ "RoundMissile", "", "", OBTAIN_TYPE_STORE, ""}},
            { 4005, new string[]{ "AssaultCharge", "", "", OBTAIN_TYPE_STORE, ""}},
            { 4006, new string[]{ "ClusterMissile", "", "", OBTAIN_TYPE_STORE, ""}},
            { 4007, new string[]{ "BlasterBomer", "", "", OBTAIN_TYPE_STORE, ""}},
            { 4008, new string[]{ "HeatCannon", "", "", OBTAIN_TYPE_INIT, ""}},
            { 4009, new string[]{ "SorwdGeyser", "", "", OBTAIN_TYPE_STORE, ""}},
            { 4010, new string[]{ "AMRifle", "", "", OBTAIN_TYPE_STORE, ""}},
        };
        //サブ武器リスト
        public static Dictionary<int, string[]> subWeaponLineUp = new Dictionary<int, string[]>()
        {
            { 5003, new string[]{ "SpeedBurst", "", "", OBTAIN_TYPE_INIT, ""}},
            { 5001, new string[]{ "AvoidBurst", "", "", OBTAIN_TYPE_INIT, ""}},
            { 5000, new string[]{ "InvincibleShield", "", "", OBTAIN_TYPE_NONE, ""}},
            { 5002, new string[]{ "BoostRecoverSp", "", "", OBTAIN_TYPE_INIT, ""}},
            { 5004, new string[]{ "AttackBurst", "", "", OBTAIN_TYPE_STORE, ""}},
            { 5005, new string[]{ "DefenceBurst", "", "", OBTAIN_TYPE_STORE, ""}},
            { 5006, new string[]{ "CounterShield", "", "", OBTAIN_TYPE_NONE, ""}},
        };
        //スペシャル武器リスト
        public static Dictionary<int, string[]> extraWeaponLineUp = new Dictionary<int, string[]>()
        {
            { 10000, new string[]{ "ExtraArmor", "ExArmor", "", OBTAIN_TYPE_INIT, ""}},
            { 10001, new string[]{ "ExtraBeam", "ExBeam", "", OBTAIN_TYPE_INIT, ""}},
            { 10002, new string[]{ "ExtraBurning", "ExBurning", "", OBTAIN_TYPE_INIT, ""}},
            { 10003, new string[]{ "ExtraRifle", "ExRifle", "", OBTAIN_TYPE_INIT, ""}},
            { 10004, new string[]{ "ExtraShadowSewing", "ExShadowDagger", "", OBTAIN_TYPE_INIT, ""}},
            { 10005, new string[]{ "ExtraClaw", "ExClaw", "", OBTAIN_TYPE_INIT, ""}},
            { 10006, new string[]{ "ExtraHolyRay", "ExHolyRay", "", OBTAIN_TYPE_INIT, ""}},
            { 10007, new string[]{ "ExtraScythe", "ExScythe", "", OBTAIN_TYPE_INIT, ""}},
            { 10008, new string[]{ "ExtraMissileLauncher", "ExMissileLauncher", "", OBTAIN_TYPE_INIT, ""}},
            { 10009, new string[]{ "ExtraGatling", "ExGatling", "", OBTAIN_TYPE_INIT, ""}},
            { 10010, new string[]{ "ExtraMedoroa", "ExMedoroa", "", OBTAIN_TYPE_INIT, ""}},
            { 10011, new string[]{ "ExtraCrystalCage", "ExCrystalCage", "", OBTAIN_TYPE_INIT, ""}},
            { 10012, new string[]{ "ExtraGhost", "ExGhost", "", OBTAIN_TYPE_INIT, ""}},
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

        //武器Noを取得する
        public static int GetWeaponNoFromName(string prefabName)
        {
            foreach (int no in handWeaponLineUp.Keys)
            {
                if (handWeaponLineUp[no][DETAIL_PREFAB_NAME_NO] == prefabName) return no;
            }
            foreach (int no in handDashWeaponLineUp.Keys)
            {
                if (handDashWeaponLineUp[no][DETAIL_PREFAB_NAME_NO] == prefabName) return no;
            }
            foreach (int no in shoulderWeaponLineUp.Keys)
            {
                if (shoulderWeaponLineUp[no][DETAIL_PREFAB_NAME_NO] == prefabName) return no;
            }
            foreach (int no in shoulderDashWeaponLineUp.Keys)
            {
                if (shoulderDashWeaponLineUp[no][DETAIL_PREFAB_NAME_NO] == prefabName) return no;
            }
            foreach (int no in subWeaponLineUp.Keys)
            {
                if (subWeaponLineUp[no][DETAIL_PREFAB_NAME_NO] == prefabName) return no;
            }
            foreach (int no in extraWeaponLineUp.Keys)
            {
                if (extraWeaponLineUp[no][DETAIL_PREFAB_NAME_NO] == prefabName) return no;
            }
            return 0;
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

        //武器情報セット
        public static void SetWeaponInfo(int weaponNo, string name = "", string ruby = "", string description = "")
        {
            string[] weaponInfo = GetWeaponInfo(weaponNo);
            if (!string.IsNullOrEmpty(name))
            {
                weaponInfo[DETAIL_NAME_NO] = name;
            }
            if (!string.IsNullOrEmpty(ruby))
            {
                weaponInfo[DETAIL_RUBY_NO] = ruby;
            }
            if (!string.IsNullOrEmpty(description))
            {
                weaponInfo[DETAIL_DESCRIPTION_NO] = description;
            }
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

        //武器装備箇所タイプを取得する
        public static string GetWeaponTypeName(int weaponNo)
        {
            string typeName = "";
            if (1000 <= weaponNo && weaponNo < 2000)
            {
                typeName = "L/R";
            }
            else if (2000 <= weaponNo && weaponNo < 3000)
            {
                typeName = "LD/RD";
            }
            else if (3000 <= weaponNo && weaponNo < 4000)
            {
                typeName = "S";
            }
            else if (4000 <= weaponNo && weaponNo < 5000)
            {
                typeName = "SD";
            }
            else if (5000 <= weaponNo && weaponNo < 6000)
            {
                typeName = "Sub";
            }
            else if (10000 <= weaponNo && weaponNo < 11000)
            {
                typeName = "Ex";
            }
            return typeName;
        }

        //ストアで取得する武器一覧取得
        public static Dictionary<int, string[]> GetStoreWeaponList()
        {
            Dictionary<int, string[]> weaponList = new Dictionary<int, string[]>();
            foreach (int weaponNo in handWeaponLineUp.Keys)
            {
                if (handWeaponLineUp[weaponNo][DETAIL_OBTAIN_TYPE_NO] == OBTAIN_TYPE_STORE) weaponList.Add(weaponNo, handWeaponLineUp[weaponNo]);
            }
            foreach (int weaponNo in handDashWeaponLineUp.Keys)
            {
                if (handDashWeaponLineUp[weaponNo][DETAIL_OBTAIN_TYPE_NO] == OBTAIN_TYPE_STORE) weaponList.Add(weaponNo, handDashWeaponLineUp[weaponNo]);
            }
            foreach (int weaponNo in shoulderWeaponLineUp.Keys)
            {
                if (shoulderWeaponLineUp[weaponNo][DETAIL_OBTAIN_TYPE_NO] == OBTAIN_TYPE_STORE) weaponList.Add(weaponNo, shoulderWeaponLineUp[weaponNo]);
            }
            foreach (int weaponNo in shoulderDashWeaponLineUp.Keys)
            {
                if (shoulderDashWeaponLineUp[weaponNo][DETAIL_OBTAIN_TYPE_NO] == OBTAIN_TYPE_STORE) weaponList.Add(weaponNo, shoulderDashWeaponLineUp[weaponNo]);
            }
            foreach (int weaponNo in subWeaponLineUp.Keys)
            {
                if (subWeaponLineUp[weaponNo][DETAIL_OBTAIN_TYPE_NO] == OBTAIN_TYPE_STORE) weaponList.Add(weaponNo, subWeaponLineUp[weaponNo]);
            }
            foreach (int weaponNo in extraWeaponLineUp.Keys)
            {
                if (extraWeaponLineUp[weaponNo][DETAIL_OBTAIN_TYPE_NO] == OBTAIN_TYPE_STORE) weaponList.Add(weaponNo, extraWeaponLineUp[weaponNo]);
            }
            return weaponList;
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
            { 2, new int[] { 300, 2 } },
            { 3, new int[] { 10001, 7 } },
            { 4, new int[] { 200, 1 } },
            { 5, new int[] { 10002, 8 } },
            { 6, new int[] { 100, 5 } },
            { 7, new int[] { 10003, 4 } },
            { 8, new int[] { 600, 9 } },
            { 9, new int[] { 400, 3 } },
            { 10, new int[] { 500, 6 } },
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
            { 7, "Master" },
            { 8, "Challenge" },
            { 9, "Advanced" },
            { 10, "Extra" },
            { 11, "Violent" },
            { 12, "Absolute" },
            { 13, "Heroic" },
            { 14, "Nightmare" },
            { 15, "Inferno" },
            { 16, "Eternal" },
            { 17, "Ultimate" },
            { 18, "Impossible" },
            { 19, "Infinity" },
            { 20, "Lostworks" },
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
                    int over = level - levelNameDic.Count + 1;
                    name += ""+over.ToString();
                }
            }
            return "Lv."+level.ToString()+" "+name;
        }

        //カラーチェンジするレベル
        public const int NPC_COLOR_CHANGE_LEVEL = 5;

        //武器強化状態になるレベル
        public const int NPC_CUSTOM_CHANGE_LEVEL = 10;

        public static int GetStageNpcNo(int level, int stageNo)
        {
            int npcNo = stageNpcNoDic[stageNo][STAGE_NPC_NAME];
            if (level >= NPC_COLOR_CHANGE_LEVEL && Character.characterLineUp.ContainsKey(npcNo + 1))
            {
                npcNo = npcNo + 1;
            }
            return npcNo;
        }

        //レベルによるステータス変化
        public static Dictionary<int, float[]> npcLevelStatusDic = new Dictionary<int, float[]>()
        {
                             //hp,   sp,   run,  boost,turn, atk%, atkI, boostI, tagI
            { 0, new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f } },
            { 1, new float[] { 0.5f, 0.5f, 0.8f, 0.6f, 0.7f, 0.3f, 1.0f, 2.0f, 2.0f } },
            { 2, new float[] { 0.7f, 0.7f, 0.8f, 0.7f, 0.8f, 0.5f, 1.0f, 1.6f, 1.6f } },
            { 3, new float[] { 0.8f, 0.8f, 0.9f, 0.8f, 0.9f, 0.6f, 1.0f, 1.3f, 1.3f } },
            { 4, new float[] { 0.9f, 1.0f, 1.0f, 0.8f, 1.0f, 0.8f, 1.0f, 1.2f, 1.0f } },
            { 5, new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f } },
            { 6, new float[] { 1.0f, 1.2f, 1.1f, 1.0f, 1.1f, 1.0f, 0.9f, 0.9f, 0.9f } },
            { 7, new float[] { 1.1f, 1.4f, 1.1f, 1.1f, 1.2f, 1.0f, 0.8f, 0.8f, 0.8f } },
            { 8, new float[] { 1.1f, 1.6f, 1.1f, 1.1f, 1.3f, 1.1f, 0.7f, 0.7f, 0.7f } },
            { 9, new float[] { 1.2f, 1.6f, 1.2f, 1.2f, 1.4f, 1.1f, 0.6f, 0.6f, 0.6f } },
            { 10, new float[] { 1.2f, 1.8f, 1.2f, 1.2f, 1.5f, 1.2f, 0.5f, 0.5f, 0.5f } },
        };
        //設定レベル以上の場合の追加Rate
        public static float[] overLevelState = new float[] { 0.075f, 0.1f, 0, 0, 0.05f, 0.05f, 0, 0, 0 };

        //コンティニュー時のステUP
        public static float[] continueBonus = new float[] { 0.1f, 0.1f, 0, 0, 0, 0.05f, 0, 0, 0 };

        //NPC武器
        public static Dictionary<int, int[]> npcWeaponDic = new Dictionary<int, int[]>()
        {
            { -1, new int[]{ 0, 0, 0, 0, 0, 0, 0} },
            {100, new int[]{ 2001, 2005, 2002, 2010, 3004, 4005, 5003 } },
            {101, new int[]{ 2001, 2005, 2002, 2010, 3004, 4005, 10009 } },
            {200, new int[]{ 1011, 2000, 1003, 1006, 3008, 4000, 5001 } },
            {201, new int[]{ 1011, 2000, 1003, 1006, 3008, 4000, 10003 } },
            {300, new int[]{ 1009, 2009, 2004, 2008, 3006, 4002, 5002 } },
            {301, new int[]{ 1009, 2009, 2004, 2008, 3006, 4002, 10004 } },
            {400, new int[]{ 2006, 2011, 1005, 4003, 3000, 4004, 5004 } },
            {401, new int[]{ 2006, 2011, 1005, 4003, 3000, 4004, 10011 } },
            {500, new int[]{ 1000, 3009, 4009, 4006, 4010, 4000, 5000 } },
            {501, new int[]{ 1000, 3009, 4009, 4006, 4010, 10008, 5006 } },
            {600, new int[]{ 1008, 1010, 2007, 3003, 4008, 3007, 5005 } },
            {601, new int[]{ 1008, 1010, 2007, 3003, 4008, 3007, 10012 } },
            {10000, new int[]{ 0, 0, 0, 0, 0, 0, 0 } },
            {10001, new int[]{ 1005, 2001, 1007, 2002, 3010, 4005, 5003 } },
            {10002, new int[]{ 1006, 2003, 1002, 2006, 3002, 4006, 5001 } },
            {10003, new int[]{ 1000, 2008, 1004, 2000, 4007, 3008, 5005 } },
            {10004, new int[]{ -1, -1, -1, -1, -1, -1, -1 } },
      };
    }
}
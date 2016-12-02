using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UserManager
{
    public static Dictionary<int, string> userInfo;         //ユーザー情報
    public static Dictionary<string, int> userEquipment;    //ユーザー装備
    public static Dictionary<int, int> userConfig;          //設定
    public static int userSetCharacter;                     //ユーザー設定キャラ
    public static List<int> userOpenCharacters;             //開放キャラクター
    public static List<int> userOpenWeapons;                //開放武器
    public static List<int> userOpenMissions;               //開放Mission(level, stage)
    public static List<int> userOpenMusics;                 //解放BGM
    public static Dictionary<int, int> userCustomWeapons;   //武器改造(weaponNo, type)

    public static bool isAdmin;                     //管理者FLG
    public static int userPoint;                    //所持ポイント
    public static bool isGachaFree = false;         //ガチャFreeFlg
    public static string apiToken;                  //API接続用Token

    //ユーザー情報初期値設定
    private static void InitPlayerPrefs()
    {
        userInfo = new Dictionary<int, string>();
        userEquipment = new Dictionary<string, int>();
        userConfig = new Dictionary<int, int>();
        userSetCharacter = 0;
        userOpenCharacters = new List<int>() { };
        userOpenWeapons = new List<int>() { };
        userOpenMissions = new List<int>() { 2, 1};
        userOpenMusics = new List<int>() { };
        userCustomWeapons = new Dictionary<int, int> { };
        isAdmin = false;
        apiToken = "";
        userPoint = -1;
    }

    //ユーザー情報設定
    public static void SetPlayerPrefs()
    {
        //初期化
        InitPlayerPrefs();

        //データロード
        SetUserInfo();
        SetUserEquipment();
        SetUserConfig();
        SetUserSetCharacter();
        SetUserOpenCharacters();
        SetUserOpenWeapons();
        SetUserOpenMissions();
        SetUserOpenMusics();
        SetUserCustomWeapons();
        PlayerPrefs.Save();
    }

    //◆UserInfo
    private static void SetUserInfo()
    {
        string key = Common.PP.USER_INFO;
        bool isUpdate = false;

        if (PlayerPrefs.HasKey(key))
        {
            //データ取得
            userInfo = PlayerPrefsUtility.LoadDict<int, string>(key);
        }

        //ユーザーID
        if (!userInfo.ContainsKey(Common.PP.INFO_USER_ID))
        {
            userInfo.Add(Common.PP.INFO_USER_ID, "-1");
            isUpdate = true;
        }
        //ユーザー名
        if (!userInfo.ContainsKey(Common.PP.INFO_USER_NAME))
        {
            userInfo.Add(Common.PP.INFO_USER_NAME, "Guest" + Random.Range(1, 99999));
            isUpdate = true;
        }
        //UUID
        if (!userInfo.ContainsKey(Common.PP.INFO_UUID))
        {
            userInfo.Add(Common.PP.INFO_UUID, "");
            isUpdate = true;
        }
        //password
        if (!userInfo.ContainsKey(Common.PP.INFO_PASSWORD))
        {
            userInfo.Add(Common.PP.INFO_PASSWORD, "");
            isUpdate = true;
        }

        //保存
        if (isUpdate) PlayerPrefsUtility.SaveDict<int, string>(key, userInfo);
    }

    //◆UserEquipment
    private static void SetUserEquipment()
    {
        string key = Common.PP.USER_EQUIP;
        bool isUpdate = false;

        if (PlayerPrefs.HasKey(key))
        {
            //データ取得
            userEquipment = PlayerPrefsUtility.LoadDict<string, int>(key);
        }

        //RightHand, LeftHand
        foreach (int weaponNo in Common.Weapon.handWeaponLineUp.Keys)
        {
            if (!userEquipment.ContainsKey(Common.CO.PARTS_LEFT_HAND))
            {
                userEquipment[Common.CO.PARTS_LEFT_HAND] = weaponNo;
                isUpdate = true;
                continue;
            }
            if (!userEquipment.ContainsKey(Common.CO.PARTS_RIGHT_HAND))
            {
                userEquipment[Common.CO.PARTS_RIGHT_HAND] = weaponNo;
                isUpdate = true;
                continue;
            }
            break;
        }
        //LeftHandDash, RightHandDash
        foreach (int weaponNo in Common.Weapon.handDashWeaponLineUp.Keys)
        {
            if (!userEquipment.ContainsKey(Common.CO.PARTS_LEFT_HAND_DASH))
            {
                userEquipment[Common.CO.PARTS_LEFT_HAND_DASH] = weaponNo;
                isUpdate = true;
                continue;
            }
            if (!userEquipment.ContainsKey(Common.CO.PARTS_RIGHT_HAND_DASH))
            {
                userEquipment[Common.CO.PARTS_RIGHT_HAND_DASH] = weaponNo;
                isUpdate = true;
                continue;
            }
            break;
        }
        //Shoulder
        foreach (int weaponNo in Common.Weapon.shoulderWeaponLineUp.Keys)
        {
            if (!userEquipment.ContainsKey(Common.CO.PARTS_SHOULDER))
            {
                userEquipment[Common.CO.PARTS_SHOULDER] = weaponNo;
                isUpdate = true;
                continue;
            }
            break;
        }
        //ShoulderDash
        foreach (int weaponNo in Common.Weapon.shoulderDashWeaponLineUp.Keys)
        {
            if (!userEquipment.ContainsKey(Common.CO.PARTS_SHOULDER_DASH))
            {
                userEquipment[Common.CO.PARTS_SHOULDER_DASH] = weaponNo;
                isUpdate = true;
                continue;
            }
            break;
        }
        //Sub
        foreach (int weaponNo in Common.Weapon.subWeaponLineUp.Keys)
        {
            if (!userEquipment.ContainsKey(Common.CO.PARTS_SUB))
            {
                userEquipment[Common.CO.PARTS_SUB] = weaponNo;
                isUpdate = true;
                continue;
            }
            break;
        }
        //Extra
        if (!userEquipment.ContainsKey(Common.CO.PARTS_EXTRA))
        {
            userEquipment[Common.CO.PARTS_EXTRA] = 0;
            isUpdate = true;
        }

        //保存
        if (isUpdate) PlayerPrefsUtility.SaveDict<string, int>(key, userEquipment);
    }

    //◆UserConfig
    private static void SetUserConfig()
    {
        string key = Common.PP.USER_CONFIG;
        bool isUpdate = false;

        if (PlayerPrefs.HasKey(key))
        {
            //データ取得
            userConfig = PlayerPrefsUtility.LoadDict<int, int>(key);
        }

        //BGMValue
        if (!userConfig.ContainsKey(Common.PP.CONFIG_BGM_VALUE))
        {
            userConfig.Add(Common.PP.CONFIG_BGM_VALUE, ConfigManager.SLIDER_COUNT / 2);
            isUpdate = true;
        }
        //BGMMute
        if (!userConfig.ContainsKey(Common.PP.CONFIG_BGM_MUTE))
        {
            userConfig.Add(Common.PP.CONFIG_BGM_MUTE, 0);
            isUpdate = true;
        }
        //SEValue
        if (!userConfig.ContainsKey(Common.PP.CONFIG_SE_VALUE))
        {
            userConfig.Add(Common.PP.CONFIG_SE_VALUE, ConfigManager.SLIDER_COUNT / 2);
            isUpdate = true;
        }
        //SEMute
        if (!userConfig.ContainsKey(Common.PP.CONFIG_SE_MUTE))
        {
            userConfig.Add(Common.PP.CONFIG_SE_MUTE, 0);
            isUpdate = true;
        }
        //VoiceValue
        if (!userConfig.ContainsKey(Common.PP.CONFIG_VOICE_VALUE))
        {
            userConfig.Add(Common.PP.CONFIG_VOICE_VALUE, ConfigManager.SLIDER_COUNT / 2);
            isUpdate = true;
        }
        //VoiceMute
        if (!userConfig.ContainsKey(Common.PP.CONFIG_VOICE_MUTE))
        {
            userConfig.Add(Common.PP.CONFIG_VOICE_MUTE, 0);
            isUpdate = true;
        }

        //保存
        if (isUpdate) PlayerPrefsUtility.SaveDict<int, int>(key, userConfig);
    }

    //◆UserSetCharacter
    private static void SetUserSetCharacter()
    {
        string key = Common.PP.USER_CHARACTER;

        if (PlayerPrefs.HasKey(key))
        {
            //データ取得
            userSetCharacter = PlayerPrefsUtility.Load(key, userSetCharacter);
        }
        else
        {
            //保存
            PlayerPrefsUtility.Save(key, userSetCharacter);
        }
    }

    //◆UserOpenCharacters
    private static void SetUserOpenCharacters()
    {
        string key = Common.PP.OPEN_CHARACTERS;

        if (PlayerPrefs.HasKey(key))
        {
            //データ取得
            userOpenCharacters = PlayerPrefsUtility.LoadList<int>(key);
        }
        else
        {
            //保存
            PlayerPrefsUtility.SaveList<int>(key, userOpenCharacters);
        }
    }

    //◆UserOpenWeapons
    private static void SetUserOpenWeapons()
    {
        string key = Common.PP.OPEN_WEAPONS;

        if (PlayerPrefs.HasKey(key))
        {
            //データ取得
            userOpenWeapons = PlayerPrefsUtility.LoadList<int>(key);
        }
        else
        {
            //保存
            PlayerPrefsUtility.SaveList<int>(key, userOpenWeapons);
        }
    }

    //◆UserOpenMissions
    private static void SetUserOpenMissions()
    {
        string key = Common.PP.OPEN_MISSIONS;

        if (PlayerPrefs.HasKey(key))
        {
            //データ取得
            userOpenMissions = PlayerPrefsUtility.LoadList<int>(key);
        }
        else
        {
            //保存
            PlayerPrefsUtility.SaveList<int>(key, userOpenMissions);
        }
    }

    //◆UserOpenMusics
    private static void SetUserOpenMusics()
    {
        string key = Common.PP.OPEN_MUSICS;

        if (PlayerPrefs.HasKey(key))
        {
            //データ取得
            userOpenMusics = PlayerPrefsUtility.LoadList<int>(key);
        }
        else
        {
            //保存
            PlayerPrefsUtility.SaveList<int>(key, userOpenMusics);
        }
    }

    //◆UserCustomWeapons
    private static void SetUserCustomWeapons()
    {
        string key = Common.PP.CUSTOM_WEAPONS;

        if (PlayerPrefs.HasKey(key))
        {
            //データ取得
            userCustomWeapons = PlayerPrefsUtility.LoadDict<int, int>(key);
        }
        else
        {
            //保存
            PlayerPrefsUtility.SaveDict<int, int>(key, userCustomWeapons);
        }
    }


    //##### ユーザー情報 #####

    //API用情報
    public static void SetApiInfo(string uuid, string password)
    {
        userInfo[Common.PP.INFO_UUID] = uuid;
        userInfo[Common.PP.INFO_PASSWORD] = password;
        PlayerPrefsUtility.SaveDict<int, string>(Common.PP.USER_INFO, userInfo);
        PlayerPrefs.Save();
    }

    //ユーザー名変更
    public static void SetUserName(string userName)
    {
        userInfo[Common.PP.INFO_USER_NAME] = userName;
        PlayerPrefsUtility.SaveDict<int, string>(Common.PP.USER_INFO, userInfo);
        PlayerPrefs.Save();
    }

    //ユーザーID取得
    public static int GetUserId()
    {
        int uid = -1;
        if (userInfo.ContainsKey(Common.PP.INFO_USER_ID))
        {
            uid = int.Parse(userInfo[Common.PP.INFO_USER_ID]);
        }
        return uid;
    }

    //##### ユーザーコンフィグ #####

    public static void SaveConfig(bool isSave = true)
    {
        PlayerPrefsUtility.SaveDict<int, int>(Common.PP.USER_CONFIG, userConfig);
        if (isSave) PlayerPrefs.Save();
    }

    //##### ユーザーキャラクター #####

    public static bool OpenNewCharacter(int npcNo)
    {
        if (!Common.Character.characterLineUp.ContainsKey(npcNo)) return false;
        if (Common.Character.characterLineUp[npcNo][Common.Character.DETAIL_OBTAIN_TYPE_NO] != Common.Character.OBTAIN_TYPE_MISSION) return false;
        if (userOpenCharacters.Contains(npcNo)) return false;

        //解放
        userOpenCharacters.Add(npcNo);
        PlayerPrefsUtility.SaveList<int>(Common.PP.OPEN_CHARACTERS, userOpenCharacters);
        PlayerPrefs.Save();

        return true;
    }

    //##### 解放ミッション #####

    public static int OpenNextMission(int nowLevel, int nowStage)
    {
        int newLevel = -1;
        if (userOpenMissions[Common.PP.MISSION_LEVEL] != nowLevel) return newLevel;
        if (userOpenMissions[Common.PP.MISSION_STAGE] != nowStage) return newLevel;

        if (nowStage == Common.Mission.stageNpcNoDic.Count)
        {
            //NextLevel
            newLevel = nowLevel + 1;
            userOpenMissions[Common.PP.MISSION_LEVEL] = newLevel;
            userOpenMissions[Common.PP.MISSION_STAGE] = 1;
        }
        else
        {
            //NextStage
            userOpenMissions[Common.PP.MISSION_STAGE] = nowStage + 1;
        }
        PlayerPrefsUtility.SaveList<int>(Common.PP.OPEN_MISSIONS, userOpenMissions);
        PlayerPrefs.Save();

        return newLevel;
    }


    //##### 解放武器 #####

    public static void AddOpenWeapon(int weaponNo)
    {
        userOpenWeapons.Add(weaponNo);
        PlayerPrefsUtility.SaveList<int>(Common.PP.OPEN_WEAPONS, userOpenWeapons);
        PlayerPrefs.Save();
    }


    //##### 解放BGM #####

    public static void AddOpenMusic(int bgmIndex)
    {
        userOpenMusics.Add(bgmIndex);
        PlayerPrefsUtility.SaveList<int>(Common.PP.OPEN_MUSICS, userOpenMusics);
        PlayerPrefs.Save();
    }


    //##### 改造武器 #####

    //強化系統取得
    public static int GetWeaponCustomType(int weaponNo)
    {
        int customType = 0;
        if (userCustomWeapons.ContainsKey(weaponNo))
        {
            customType = userCustomWeapons[weaponNo];
        }
        return customType;
    }

    //保存
    public static void SaveWeaponCustomInfo(int weaponNo, int type = 0)
    {
        if (userCustomWeapons.ContainsKey(weaponNo))
        {
            userCustomWeapons[weaponNo] = type;
        }
        else
        {
            userCustomWeapons.Add(weaponNo, type);
        }
        PlayerPrefsUtility.SaveDict<int, int>(Common.PP.CUSTOM_WEAPONS, userCustomWeapons);
        PlayerPrefs.Save();
    }


    //##### デバッグ #####

    public static void DispUserInfo()
    {
        foreach (int key in userInfo.Keys)
        {
            Debug.Log(key+" >> "+userInfo[key]);
        }
        Debug.Log("isAdmin >> "+isAdmin);
    }
    public static void DispUserConfig()
    {
        foreach (int key in userConfig.Keys)
        {
            Debug.Log(key + " >> " + userConfig[key]);
        }
    }
    public static void DeleteUser()
    {
        //データ削除(debug用)
        PlayerPrefs.DeleteAll();
        InitPlayerPrefs();
        PlayerPrefs.Save();
    }
}

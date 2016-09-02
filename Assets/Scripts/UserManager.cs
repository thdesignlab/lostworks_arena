using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UserManager
{
    public static Dictionary<int, string> userInfo = new Dictionary<int, string>();   //ユーザー情報
    public static Dictionary<int, int> userResult = new Dictionary<int, int>();       //ユーザー戦歴
    public static Dictionary<string, int> userEquipment = new Dictionary<string, int>();    //ユーザー装備
    public static Dictionary<int, int> userConfig = new Dictionary<int, int>();    //設定
    public static int userSetCharacter = 0;   //ユーザー設定キャラ
    public static List<int> userOpenCharacters = new List<int>();    //開放キャラクター
    public static List<int> userOpenWeapons = new List<int>();       //開放武器
    public static List<int> userOpenMissions = new List<int>() { 1, 1 };       //開放Mission(level, stage)

    //##### ユーザー情報 #####

    //初期データ作成
    private static void InitUserInfo()
    {
        //ユーザー情報
        if (!PlayerPrefs.HasKey(Common.PP.USER_INFO))
        {
            userInfo[Common.PP.INFO_USER_ID] = SystemInfo.deviceUniqueIdentifier;
            userInfo[Common.PP.INFO_USER_NAME] = "Guest" + Random.Range(1, 99999);
            PlayerPrefsUtility.SaveDict<int, string>(Common.PP.USER_INFO, userInfo);
        }

        //戦歴
        if (!PlayerPrefs.HasKey(Common.PP.USER_RESULT))
        {
            userResult[Common.PP.RESULT_BATTLE_COUNT] = 0;
            userResult[Common.PP.RESULT_WIN_COUNT] = 0;
            userResult[Common.PP.RESULT_LOSE_COUNT] = 0;
            userResult[Common.PP.RESULT_BATTLE_RATE] = 1000;
            PlayerPrefsUtility.SaveDict<int, int>(Common.PP.USER_RESULT, userResult);
        }

        //装備
        if (!PlayerPrefs.HasKey(Common.PP.USER_EQUIP))
        {
            foreach (int key in Common.Weapon.handWeaponLineUp.Keys)
            {
                if (!userEquipment.ContainsKey(Common.CO.PARTS_LEFT_HAND))
                {
                    userEquipment[Common.CO.PARTS_LEFT_HAND] = key;
                    continue;
                }
                else if (!userEquipment.ContainsKey(Common.CO.PARTS_RIGHT_HAND))
                {
                    userEquipment[Common.CO.PARTS_RIGHT_HAND] = key;
                    continue;
                }
                break;
            }
            foreach (int key in Common.Weapon.handDashWeaponLineUp.Keys)
            {
                if (!userEquipment.ContainsKey(Common.CO.PARTS_LEFT_HAND_DASH))
                {
                    userEquipment[Common.CO.PARTS_LEFT_HAND_DASH] = key;
                    continue;
                }
                else if (!userEquipment.ContainsKey(Common.CO.PARTS_RIGHT_HAND_DASH))
                {
                    userEquipment[Common.CO.PARTS_RIGHT_HAND_DASH] = key;
                    continue;
                }
                break;
            }
            foreach (int key in Common.Weapon.shoulderWeaponLineUp.Keys)
            {
                if (!userEquipment.ContainsKey(Common.CO.PARTS_SHOULDER))
                {
                    userEquipment[Common.CO.PARTS_SHOULDER] = key;
                    continue;
                }
                break;
            }
            foreach (int key in Common.Weapon.shoulderDashWeaponLineUp.Keys)
            {
                if (!userEquipment.ContainsKey(Common.CO.PARTS_SHOULDER_DASH))
                {
                    userEquipment[Common.CO.PARTS_SHOULDER_DASH] = key;
                    continue;
                }
                break;
            }
            foreach (int key in Common.Weapon.subWeaponLineUp.Keys)
            {
                if (!userEquipment.ContainsKey(Common.CO.PARTS_SUB))
                {
                    userEquipment[Common.CO.PARTS_SUB] = key;
                    continue;
                }
                break;
            }
            userEquipment[Common.CO.PARTS_EXTRA] = Common.Weapon.GetExtraWeaponNo(userSetCharacter);
            PlayerPrefsUtility.SaveDict<string, int>(Common.PP.USER_EQUIP, userEquipment);
        }

        //コンフィグ
        if (!PlayerPrefs.HasKey(Common.PP.USER_CONFIG))
        {
            userConfig[Common.PP.CONFIG_BGM_VALUE] = ConfigManager.SLIDER_COUNT / 2;
            userConfig[Common.PP.CONFIG_BGM_MUTE] = 0;
            userConfig[Common.PP.CONFIG_SE_VALUE] = ConfigManager.SLIDER_COUNT / 2;
            userConfig[Common.PP.CONFIG_SE_MUTE] = 0;
            userConfig[Common.PP.CONFIG_VOICE_VALUE] = ConfigManager.SLIDER_COUNT / 2;
            userConfig[Common.PP.CONFIG_VOICE_MUTE] = 0;
            SaveConfig(false);
        }
        

        //設定キャラ
        if (!PlayerPrefs.HasKey(Common.PP.USER_CHARACTER))
        {
            PlayerPrefsUtility.Save(Common.PP.USER_CHARACTER, userSetCharacter);
        }

        //開放キャラ
        if (!PlayerPrefs.HasKey(Common.PP.OPEN_CHARACTERS))
        {
            PlayerPrefsUtility.SaveList<int>(Common.PP.OPEN_CHARACTERS, userOpenCharacters);
        }

        //開放武器
        if (!PlayerPrefs.HasKey(Common.PP.OPEN_WEAPONS))
        {
            PlayerPrefsUtility.SaveList<int>(Common.PP.OPEN_WEAPONS, userOpenWeapons);
        }

        //開放Mission
        if (!PlayerPrefs.HasKey(Common.PP.OPEN_MISSIONS))
        {
            PlayerPrefsUtility.SaveList<int>(Common.PP.OPEN_MISSIONS, userOpenMissions);
        }

        PlayerPrefs.Save();
    }

    private static bool IsInitUser()
    {
        if (!PlayerPrefs.HasKey(Common.PP.USER_INFO)) return true;
        if (!PlayerPrefs.HasKey(Common.PP.USER_RESULT)) return true;
        if (!PlayerPrefs.HasKey(Common.PP.USER_EQUIP)) return true;
        if (!PlayerPrefs.HasKey(Common.PP.USER_CONFIG)) return true;
        if (!PlayerPrefs.HasKey(Common.PP.USER_CHARACTER)) return true;
        if (!PlayerPrefs.HasKey(Common.PP.OPEN_CHARACTERS)) return true;
        if (!PlayerPrefs.HasKey(Common.PP.OPEN_WEAPONS)) return true;
        if (!PlayerPrefs.HasKey(Common.PP.OPEN_MISSIONS)) return true;
        return false;
    }

    public static void SetUserInfo()
    {
        //データ削除(debug用)
        //PlayerPrefs.DeleteAll();
        //PlayerPrefs.DeleteKey(Common.PP.USER_INFO);
        //PlayerPrefs.DeleteKey(Common.PP.USER_RESULT);

        if (IsInitUser())
        {
            //新規ユーザー
            InitUserInfo();
        }

        //ユーザー情報取得
        userInfo = PlayerPrefsUtility.LoadDict<int, string>(Common.PP.USER_INFO);
        userResult = PlayerPrefsUtility.LoadDict<int, int>(Common.PP.USER_RESULT);
        userEquipment = PlayerPrefsUtility.LoadDict<string, int>(Common.PP.USER_EQUIP);
        userConfig = PlayerPrefsUtility.LoadDict<int, int>(Common.PP.USER_CONFIG);
        userSetCharacter = PlayerPrefsUtility.Load(Common.PP.USER_CHARACTER, 0);
        userOpenCharacters = PlayerPrefsUtility.LoadList<int>(Common.PP.OPEN_CHARACTERS);
        userOpenWeapons = PlayerPrefsUtility.LoadList<int>(Common.PP.OPEN_WEAPONS);
        userOpenMissions = PlayerPrefsUtility.LoadList<int>(Common.PP.OPEN_MISSIONS);
    }

    public static void SetUserName(string userName)
    {
        //PlayerPrefs.SetString(Common.PP.INFO_USER_NAME, userName);
    }

    //##### ユーザー戦績 #####


    //##### ユーザーコンフィグ #####

    public static void SaveConfig(bool isSave = true)
    {
        PlayerPrefsUtility.SaveDict<int, int>(Common.PP.USER_CONFIG, userConfig);
        if (isSave) PlayerPrefs.Save();
    }

    //##### ユーザーキャラクター #####


    //##### デバッグ #####

    public static void DispUserInfo()
    {
        foreach (int key in userInfo.Keys)
        {
            Debug.Log(key+" >> "+userInfo[key]);
        }
    }
    public static void DispUserConfig()
    {
        foreach (int key in userConfig.Keys)
        {
            Debug.Log(key + " >> " + userConfig[key]);
        }
    }

}

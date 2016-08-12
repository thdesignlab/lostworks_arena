using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UserManager
{
    public static Dictionary<string, string> userInfo = new Dictionary<string, string>();   //ユーザー情報
    public static Dictionary<string, int> userResult = new Dictionary<string, int>();       //ユーザー戦歴
    public static Dictionary<string, int> userEquipment = new Dictionary<string, int>();    //ユーザー装備
    public static int userSetCharacter = 0;   //ユーザー設定キャラ
    public static List<int> userOpenCharacters = new List<int>();    //開放キャラクター
    public static List<int> userOpenWeapons = new List<int>();       //開放武器

    //##### ユーザー情報 #####

    //初期データ作成
    private static void InitUserInfo()
    {
        //ユーザー情報
        if (!PlayerPrefs.HasKey(Common.PP.USER_INFO))
        {
            userInfo[Common.PP.INFO_USER_ID] = SystemInfo.deviceUniqueIdentifier;
            userInfo[Common.PP.INFO_USER_NAME] = "Guest" + Random.Range(1, 99999);
            PlayerPrefsUtility.SaveDict<string, string>(Common.PP.USER_INFO, userInfo);
        }

        //戦歴
        if (!PlayerPrefs.HasKey(Common.PP.USER_RESULT))
        {
            userResult[Common.PP.RESULT_BATTLE_COUNT] = 0;
            userResult[Common.PP.RESULT_WIN_COUNT] = 0;
            userResult[Common.PP.RESULT_LOSE_COUNT] = 0;
            userResult[Common.PP.RESULT_BATTLE_RATE] = 1000;
            PlayerPrefsUtility.SaveDict<string, int>(Common.PP.USER_RESULT, userResult);
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

        PlayerPrefs.Save();
    }

    private static bool IsInitUser()
    {
        if (!PlayerPrefs.HasKey(Common.PP.USER_INFO)) return true;
        if (!PlayerPrefs.HasKey(Common.PP.USER_RESULT)) return true;
        if (!PlayerPrefs.HasKey(Common.PP.USER_EQUIP)) return true;
        if (!PlayerPrefs.HasKey(Common.PP.USER_CHARACTER)) return true;
        if (!PlayerPrefs.HasKey(Common.PP.OPEN_CHARACTERS)) return true;
        if (!PlayerPrefs.HasKey(Common.PP.OPEN_WEAPONS)) return true;
        return false;
    }

    public static void SetUserInfo()
    {
        //データ削除
        //PlayerPrefs.DeleteAll();

        if (IsInitUser())
        {
            //新規ユーザー
            InitUserInfo();
        }
        else
        {
            //ユーザー情報取得
            userInfo = PlayerPrefsUtility.LoadDict<string, string>(Common.PP.USER_INFO);
            userResult = PlayerPrefsUtility.LoadDict<string, int>(Common.PP.USER_RESULT);
            userEquipment = PlayerPrefsUtility.LoadDict<string, int>(Common.PP.USER_EQUIP);
            userSetCharacter = PlayerPrefsUtility.Load(Common.PP.USER_CHARACTER, 0);
            userOpenCharacters = PlayerPrefsUtility.LoadList<int>(Common.PP.OPEN_CHARACTERS);
            userOpenWeapons = PlayerPrefsUtility.LoadList<int>(Common.PP.OPEN_WEAPONS);
        }
    }

    public static void SetUserName(string userName)
    {
        //PlayerPrefs.SetString(Common.PP.INFO_USER_NAME, userName);
    }

    //##### ユーザー戦績 #####


    //##### ユーザーキャラクター #####


    //##### デバッグ #####

    public static void DispUserInfo()
    {
        foreach (string key in userInfo.Keys)
        {
            Debug.Log(key+" >> "+userInfo[key]);
        }
    }

}

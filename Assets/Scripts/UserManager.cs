using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UserManager
{
    public static Dictionary<string, string> userInfo = new Dictionary<string, string>();   //ユーザー情報
    public static Dictionary<string, int> userResult = new Dictionary<string, int>();       //ユーザー戦歴
    public static Dictionary<string, int> userEquipment = new Dictionary<string, int>();    //ユーザー装備
    public static List<int> userCharacters;    //開放キャラクター
    public static List<int> userWeapons;       //開放武器

    //##### ユーザー情報 #####

    private static void InitUserInfo()
    {
        userInfo[Common.PP.INFO_USER_ID] = SystemInfo.deviceUniqueIdentifier;
        userInfo[Common.PP.INFO_USER_NAME] = "Guest" + Random.Range(1, 99999);
        PlayerPrefsUtility.SaveDict<string, string>(Common.PP.USER_INFO, userInfo);
        PlayerPrefs.Save();
    }

    public static void SetUserInfo()
    {
        //PlayerPrefs.DeleteAll();

        if (!PlayerPrefs.HasKey(Common.PP.USER_INFO))
        {
            //新規ユーザー
            InitUserInfo();
        }
        else
        {
            //ユーザー情報取得
            userInfo = PlayerPrefsUtility.LoadDict<string, string>(Common.PP.USER_INFO);
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

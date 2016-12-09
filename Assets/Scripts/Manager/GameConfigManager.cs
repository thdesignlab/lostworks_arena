using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class GameConfigManager
{
    const string KEY_HP_URL = "hp_url";
    const string KEY_HP_TUTORIAL_URL = "hp_tutorial_url";
    const string KEY_MAX_ROOM_COUNT = "max_room_count";

    //データ取得
    private static string GetValue(string key)
    {
        if (ModelManager.gameConfigList == null) return "";

        string value = "";
        if (ModelManager.gameConfigList.ContainsKey(key))
        {
            value = ModelManager.gameConfigList[key];
        }
        return value;
    }

    //##### 各データ取得 #####

    //HPURL
    public static string getHpUrl()
    {
        return GetValue(KEY_HP_URL);
    }

    //HPチュートリアルURL
    public static string getHpTutorialUrl()
    {
        return GetValue(KEY_HP_TUTORIAL_URL);
    }

    //PhotonRoom最大数
    public static int getMaxRoomCount(int def = 0)
    {
        string value = GetValue(KEY_MAX_ROOM_COUNT);
        return string.IsNullOrEmpty(value) ? def : int.Parse(value);
    }
}

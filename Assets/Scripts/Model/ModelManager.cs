using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ModelManager
{
    //ゲームコンフィグ
    public static Dictionary<string, string> gameConfigList = null;

    //ミッション記録
    public static MissionRecord missionRecord = new MissionRecord();
    //ミッションランキング
    public static MissionRanking missionRanking = new MissionRanking();

    //ルーム情報
    public static RoomData roomData = new RoomData();
    public static List<RoomData> roomDataList = new List<RoomData>();

    //バトル戦績
    public static BattleRecord battleRecord = new BattleRecord();
    //バトル中ID
    public static BattleInfo battleInfo = new BattleInfo();
    //バトルランキング
    public static BattleRanking battleRanking = new BattleRanking();

    //Tips
    public static TipsInfo tipsInfo = null;

    //ポイント取得テーブル
    public static List<MasterPoint> mstPointList = null;
    //武器データ
    public static List<MasterWeapon> mstWeaponList = null;
}



using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class ModelManager
{
    //ミッション記録
    public static MissionRecord missionRecord = new MissionRecord();
    //ミッションランキング
    public static MissionRanking missionRanking = new MissionRanking();

    //バトル戦績
    public static BattleRecord battleRecord = new BattleRecord();
    //バトル中ID
    public static BattleInfo battleInfo = new BattleInfo();
    //バトルランキング
    public static BattleRanking battleRanking = new BattleRanking();

    //Tips
    public static TipsInfo tipsInfo = new TipsInfo();

    //ポイント取得テーブル
    public static List<MasterPoint> mstPointList = new List<MasterPoint>();
}



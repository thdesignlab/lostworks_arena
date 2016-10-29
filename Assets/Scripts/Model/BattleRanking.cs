using UnityEngine;
using System;
using System.Collections.Generic;

public class BattleRanking
{
    public BattleRankingRecord my_ranking;
    public List<BattleRankingRecord> ranking;
}

public class BattleRankingRecord
{
    public int rank;
    public int user_id;
    public string user_name;
    public int character_id;
    public int battle_rate;
    public int battle_count;
    public int win;
    public int lose;
    public double win_rate;
}
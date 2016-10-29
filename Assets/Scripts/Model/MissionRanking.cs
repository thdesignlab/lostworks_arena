using UnityEngine;
using System;
using System.Collections.Generic;

public class MissionRanking
{
    public MissionRankingRecord my_ranking;
    public List<MissionRankingRecord> ranking;
}

public class MissionRankingRecord
{
    public int rank;
    public int user_id;
    public string user_name;
    public int character_id;
    public int level;
    public int stage;
    public int continue_count;
}

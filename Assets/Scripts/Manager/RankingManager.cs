using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

public class RankingManager : SingletonMonoBehaviour<RankingManager>
{
    [SerializeField]
    private Transform rankingArea;
    [SerializeField]
    private GameObject rankingObj;

    private Sprite[] characterSprites;
    private int defaultCharacterId;

    private Transform myRankingTran;

    private float getArenaRankTime = -1;
    private float getMissionRankTime = -1;
    const float RANK_CACHE_TIME = 600;

    protected override void Awake()
    {
        isDontDestroyOnLoad = false;
        base.Awake();

        foreach (int id in Common.Character.characterLineUp.Keys)
        {
            defaultCharacterId = id;
            break;
        }

        //自分のランキング枠
        myRankingTran = GameObject.Find("RankingRow").transform;

        //ランキング取得
        ArenaRanking(true);
    }

    public void CloseRanking()
    {
        ScreenManager.Instance.Load(Common.CO.SCENE_TITLE, DialogController.MESSAGE_LOADING);
    }

    public void ArenaRanking(bool flg)
    {
        if (!flg) return;

        ClearRanking();
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);
        if (getArenaRankTime > 0 && Time.time - getArenaRankTime <= RANK_CACHE_TIME)
        {
            //キャッシュから
            ArenaRankingCallback();
        }
        else
        {
            Battle.Ranking battleRanking = new Battle.Ranking();
            battleRanking.SetApiFinishCallback(ArenaRankingCallback);
            battleRanking.Exe();
            getArenaRankTime = Time.time;
        }
    }
    private void ArenaRankingCallback()
    {
        //MyRank
        SetArenaRank(myRankingTran, ModelManager.battleRanking.my_ranking);

        //TopRank
        foreach (BattleRankingRecord ranking in ModelManager.battleRanking.ranking)
        {
            Transform rankingTran = Instantiate(rankingObj).transform;
            rankingTran.SetParent(rankingArea, false);
            SetArenaRank(rankingTran, ranking);
        }

        DialogController.CloseMessage();
    }

    public void MissionRanking(bool flg)
    {
        if (!flg) return;

        ClearRanking();
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);
        if (getMissionRankTime > 0 && Time.time - getMissionRankTime <= RANK_CACHE_TIME)
        {
            //キャッシュから
            MissionRankingCallback();
        }
        else
        {
            Mission.Ranking missionRanking = new Mission.Ranking();
            missionRanking.SetApiFinishCallback(MissionRankingCallback);
            missionRanking.Exe();
            getMissionRankTime = Time.time;
        }
    }
    private void MissionRankingCallback()
    {
        //MyRank
        SetMissionRank(myRankingTran, ModelManager.missionRanking.my_ranking);

        //TopRank
        foreach (MissionRankingRecord ranking in ModelManager.missionRanking.ranking)
        {
            Transform rankingTran = Instantiate(rankingObj).transform;
            rankingTran.SetParent(rankingArea, false);
            SetMissionRank(rankingTran, ranking);
        }

        DialogController.CloseMessage();
    }

    private void SetArenaRank(Transform rankTran, BattleRankingRecord battleRanking = null)
    {
        if (battleRanking == null) return;
        rankTran.FindChild("Rank").GetComponent<Text>().text = battleRanking.rank.ToString() + "位";
        rankTran.FindChild("Name").GetComponent<Text>().text = battleRanking.user_name;
        rankTran.FindChild("Character").GetComponent<Image>().sprite = GetCharaIcon(battleRanking.character_id);
        rankTran.FindChild("ResultArea/Result1").GetComponent<Text>().text = "Rate：" + battleRanking.battle_rate.ToString();
        rankTran.FindChild("ResultArea/Result2").GetComponent<Text>().text = "勝率：" + battleRanking.win_rate.ToString() + "%";
    }

    private void SetMissionRank(Transform rankTran, MissionRankingRecord missionRanking = null)
    {
        if (missionRanking == null) return;
        rankTran.FindChild("Rank").GetComponent<Text>().text = missionRanking.rank.ToString() + "位";
        rankTran.FindChild("Name").GetComponent<Text>().text = missionRanking.user_name;
        rankTran.FindChild("Character").GetComponent<Image>().sprite = GetCharaIcon(missionRanking.character_id);
        rankTran.FindChild("ResultArea/Result1").GetComponent<Text>().text = Common.Mission.levelNameDic[missionRanking.level];
        rankTran.FindChild("ResultArea/Result2").GetComponent<Text>().text = "stage" + missionRanking.stage.ToString();
    }

    private Sprite GetCharaIcon(int characterId)
    {
        characterSprites = Resources.LoadAll<Sprite>(Common.Func.GetResourceSprite("CharaIcon/chara_"+ characterId.ToString()));
        if (characterSprites.Length == 0) 
        {
            characterSprites = Resources.LoadAll<Sprite>(Common.Func.GetResourceSprite("CharaIcon/chara_" + defaultCharacterId));
        }
        return characterSprites[0];
    }

    private void ClearRanking()
    {
        myRankingTran.FindChild("Rank").GetComponent<Text>().text = "NoData";
        myRankingTran.FindChild("Name").GetComponent<Text>().text = UserManager.userInfo[Common.PP.INFO_USER_NAME];
        myRankingTran.FindChild("Character").GetComponent<Image>().sprite = GetCharaIcon(UserManager.userSetCharacter);
        myRankingTran.FindChild("ResultArea/Result1").GetComponent<Text>().text = "";
        myRankingTran.FindChild("ResultArea/Result2").GetComponent<Text>().text = "";
        foreach (Transform child in rankingArea)
        {
            Destroy(child.gameObject);
        }
    }
}

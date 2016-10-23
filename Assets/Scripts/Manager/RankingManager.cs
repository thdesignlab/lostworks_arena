using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class RankingManager : SingletonMonoBehaviour<RankingManager>
{

    protected override void Awake()
    {
        isDontDestroyOnLoad = false;
        base.Awake();
    }

    public void CloseRanking()
    {
        ScreenManager.Instance.Load(Common.CO.SCENE_TITLE, DialogController.MESSAGE_LOADING);
    }
}

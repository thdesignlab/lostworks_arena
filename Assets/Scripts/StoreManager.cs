using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class StoreManager : SingletonMonoBehaviour<StoreManager>
{
    [SerializeField]
    private Text textPoint;

    //point >> rate
    private Dictionary<int, int> pointTable = new Dictionary<int, int>()
    {
        { 20, 200 },
        { 30, 200 },
        { 40, 50 },
        { 50, 50 },
        { 100, 10 },
        { 500, 1 },
    };

    protected override void Awake()
    {
        isDontDestroyOnLoad = false;
        base.Awake();
    }

    void Start()
    {
        //所持ポイント表示
        textPoint.text = UserManager.userPoint.ToString();

        //ポイントテーブル取得
        Point.Table pointTable = new Point.Table();
        pointTable.SetApiFinishCallback(() => SetPointTable());
        pointTable.SetApiErrorIngnore();
        pointTable.Exe();
    }

    public void SetPointTable()
    {
        if (MasterManager.mstPointList.Count != 0)
        {
            pointTable = new Dictionary<int, int>();
            foreach (MasterPoint mstPoint in MasterManager.mstPointList)
            {
                pointTable.Add(mstPoint.point, mstPoint.rate);
            }
        }
    }

    public void CloseStore()
    {
        ScreenManager.Instance.Load(Common.CO.SCENE_TITLE, DialogController.MESSAGE_LOADING);
    }

    public void PlayGacha()
    {
        System.Action onFinish = () => AddPoint();
        System.Action onSkipped = () => AddPoint(0.5f);
        System.Action onFailed = () => GachaErrorAction();
        UnityAds.Instance.Play(null, null, onFinish, onFailed, onSkipped);
    }
    private void GachaErrorAction()
    {
        if (Common.Func.IsPc())
        {
            AddPoint();
        }
        else
        {
            DialogController.OpenDialog("接続Error");
        }
    }

    //ポイント付与
    private void AddPoint(float ptRate = 1)
    {
        int pt = Draw<int>(pointTable);
        pt = (int)(pt * ptRate);

        //pt追加API
        Point.Add pointAdd = new Point.Add();
        pointAdd.SetApiFinishCallback(() => PointGetDialog(pt));
        pointAdd.Exe(pt);
    }

    //獲得ポイントダイアログ
    private void PointGetDialog(int pt)
    {
        textPoint.text = UserManager.userPoint.ToString();
        //DialogController.OpenDialog(pt + "pt獲得!!", "もう一度", () => PlayGacha(), true);
        DialogController.OpenDialog(pt + "pt獲得!!");
    }

    //抽選
    private T Draw<T>(Dictionary<T, int> targets)
    {
        T drawObj = default(T);
        int sumRate = 0;
        List<T> targetValues = new List<T>();
        foreach (T obj in targets.Keys)
        {
            sumRate += targets[obj];
            targetValues.Add(obj);
        }
        if (sumRate == 0) return drawObj;

        int drawNum = Random.Range(1, sumRate + 1);
        sumRate = 0;
        for (int i = 0; i < targets.Count; i++)
        {
            int key = Random.Range(0, targetValues.Count);
            sumRate += targets[targetValues[key]];
            if (sumRate >= drawNum)
            {
                drawObj = targetValues[key];
                break;
            }
            targetValues.RemoveAt(key);
        }
        return drawObj;
    }
}

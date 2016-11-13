using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class StoreManager : SingletonMonoBehaviour<StoreManager>
{
    [SerializeField]
    private Text textPoint;
    [SerializeField]
    private Transform storeWeaponContent;
    [SerializeField]
    private Object storeWeaponObj;


    //point >> rate
    private Dictionary<int, int> pointTable = new Dictionary<int, int>()
    {
        { 200, 200 },
        { 500, 10 },
        { 1000, 1 },
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

        //武器リスト
        DispStoreWeaponList();
    }

    public void SetPointTable()
    {
        if (ModelManager.mstPointList.Count != 0)
        {
            pointTable = new Dictionary<int, int>();
            foreach (MasterPoint mstPoint in ModelManager.mstPointList)
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
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);
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
            DialogController.CloseMessage();
            DialogController.OpenDialog("接続Error");
        }
    }

    //ポイント付与
    private void AddPoint(float ptRate = 1)
    {
        int pt = Draw<int>(pointTable);
        pt = (int)(pt * ptRate);

        ////pt追加API
        //Point.Add pointAdd = new Point.Add();
        //pointAdd.SetApiFinishCallback(() => PointGetDialog(pt));
        //pointAdd.Exe(pt);
        //ガチャAPI
        Gacha.Play pointAdd = new Gacha.Play();
        pointAdd.SetApiFinishCallback(() => PointGetDialog(pt));
        pointAdd.Exe(pt);
    }

    //獲得ポイントダイアログ
    private void PointGetDialog(int pt)
    {
        DialogController.CloseMessage();

        //所持pt更新
        textPoint.text = UserManager.userPoint.ToString();

        string text = pt + "pt獲得";
        string nextBtn = "OK";
        UnityAction nextAction = null;
        bool cancelBtn = false;
        if (!string.IsNullOrEmpty(ModelManager.tipsInfo.text))
        {
            text += "\n\n<tips>\n";
            text += ModelManager.tipsInfo.text;

            nextBtn = "もう一度";
            nextAction = PlayGacha;
            cancelBtn = true;
            if (ModelManager.tipsInfo.last_flg == 0)
            {
                //続きあり
                nextBtn += "\nnext tips";
            }
        }

        //DialogController.OpenDialog(pt + "pt獲得!!", "もう一度", () => PlayGacha(), true);
        DialogController.OpenDialog(text, nextBtn, nextAction, cancelBtn);
    }

    //購入
    public void Buy(int weaponNo)
    {
        int pt = Common.Weapon.GetStoreNeedPoint(weaponNo);

        //point消費
        Weapon.Buy WeaponBuy = new Weapon.Buy();
        WeaponBuy.SetApiFinishCallback(() => BuyProc(weaponNo));
        WeaponBuy.SetApiFinishErrorCallback(BuyErrorProc);
        WeaponBuy.SetApiErrorIngnore();
        WeaponBuy.Exe(pt, weaponNo);
    }

    //購入成功処理
    public void BuyProc(int weaponNo)
    {
        //所持ポイント表示
        textPoint.text = UserManager.userPoint.ToString();

        //武器リスト更新
        UserManager.AddOpenWeapon(weaponNo);
        DispStoreWeaponList();

        DialogController.OpenDialog("武器GET！");
    }

    //購入失敗処理
    public void BuyErrorProc(string errorCode)
    {
        string errorMessage = "購入失敗";
        switch (errorCode)
        {
            case "300000":
                errorMessage += "\nポイントが足りません";
                break;
        }
        DialogController.OpenDialog(errorMessage);
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

    //購入可能武器リスト取得
    private void DispStoreWeaponList()
    {
        //リストクリア
        foreach (Transform child in storeWeaponContent)
        {
            Destroy(child.gameObject);
        }

        //武器リスト
        Dictionary<int, string[]> weaponList = Common.Weapon.GetStoreWeaponList();
        foreach (int weaponNo in weaponList.Keys)
        {
            //OPENチェック
            if (UserManager.userOpenWeapons.IndexOf(weaponNo) >= 0) continue;
            string[] weaponInfo = weaponList[weaponNo];

            GameObject row = (GameObject)Instantiate(storeWeaponObj);
            Transform rowTran = row.transform;
            rowTran.FindChild("WeaponName").GetComponent<Text>().text = Common.Weapon.GetWeaponName(weaponNo);
            rowTran.FindChild("TypeName").GetComponent<Text>().text = Common.Weapon.GetWeaponTypeName(weaponNo);
            rowTran.FindChild("Description").GetComponent<Text>().text = weaponInfo[Common.Weapon.DETAIL_DESCRIPTION_NO];
            rowTran.FindChild("NeedPoint").GetComponent<Text>().text = Common.Weapon.GetStoreNeedPoint(weaponNo).ToString();
            int param = weaponNo;
            rowTran.GetComponent<Button>().onClick.AddListener(() => Buy(param));
            rowTran.SetParent(storeWeaponContent, false);
        }
    }
}

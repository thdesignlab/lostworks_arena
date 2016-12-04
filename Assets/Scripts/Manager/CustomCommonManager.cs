using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class CustomCommonManager : SingletonMonoBehaviour<CustomCommonManager>
{
    [SerializeField]
    protected Text textPoint;

    //カスタムボタン
    [SerializeField]
    protected Object powerCustomBtn;
    [SerializeField]
    protected Object technicCustomBtn;
    [SerializeField]
    protected Object uniqueCustomBtn;
    [SerializeField]
    protected Object cancelCustomBtn;

    protected GameObject playMovieObj;

    protected int needWeaponBuyPoint = 500;
    protected int needWeaopnCustomPoint = 1000;

    //point >> rate
    protected Dictionary<int, int> pointTable = new Dictionary<int, int>()
    {
        { 200, 1 },
    };

    protected override void Awake()
    {
        isDontDestroyOnLoad = false;
        base.Awake();
    }

    protected virtual void Start()
    {
        //所持ポイント表示
        textPoint.text = (UserManager.userPoint >= 0) ? UserManager.userPoint.ToString() : "-";

        //ポイントテーブル取得
        Point.Table pointTable = new Point.Table();
        pointTable.SetApiFinishCallback(() => SetPointTable());
        pointTable.SetApiErrorIngnore();
        pointTable.Exe();
    }

    protected void SwitchBonusText()
    {
        if (playMovieObj == null) return;
        bool flg = UserManager.isGachaFree;
        playMovieObj.transform.FindChild("BonusText").gameObject.SetActive(flg);
    }

    //##### Point処理 #####

    //pt抽選テーブル設定
    protected void SetPointTable()
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

    //ガチャ実行
    protected void PlayGacha()
    {
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);

        if (UserManager.isGachaFree)
        {
            AddPoint();
        }
        else
        {
            System.Action onFinish = () => AddPoint();
            System.Action onSkipped = () => AddPoint();
            System.Action onFailed = () => GachaErrorAction();
            UnityAds.Instance.Play(null, null, onFinish, onFailed, onSkipped);
        }
    }
    protected void GachaErrorAction()
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
    protected void AddPoint(float ptRate = 1)
    {
        int pt = Common.Func.Draw<int>(pointTable);
        pt = (int)(pt * ptRate);

        //ガチャAPI
        Gacha.Play pointAdd = new Gacha.Play();
        pointAdd.SetApiFinishCallback(() => PointGetDialog(pt));
        pointAdd.Exe(pt);
    }

    //獲得ポイントダイアログ
    protected void PointGetDialog(int pt)
    {
        //所持pt更新
        textPoint.text = UserManager.userPoint.ToString();
        SwitchBonusText();

        string title = pt + "pt獲得";
        string text = "";
        string imgName = "";
        string nextBtn = "もう一度";
        Dictionary<string, UnityAction> btnActionDic = new Dictionary<string, UnityAction>();
        if (!string.IsNullOrEmpty(ModelManager.tipsInfo.text))
        {
            //Tipsあり
            text = "## " + ModelManager.tipsInfo.title + " ##\n";
            text += ModelManager.tipsInfo.text;
            imgName = ModelManager.tipsInfo.image;

            if (ModelManager.tipsInfo.no != ModelManager.tipsInfo.last_no)
            {
                //Tips続きあり
                string nextTipsBtn = nextBtn + "(next tips)";
                btnActionDic.Add(nextTipsBtn, PlayGacha);
                string newTipsBtn = nextBtn + "(new tips)";
                UnityAction newTipsAction = () => {
                    ModelManager.tipsInfo = new TipsInfo();
                    PlayGacha();
                };
                btnActionDic.Add(newTipsBtn, newTipsAction);
            }
            else
            {
                //Tips続きなし
                btnActionDic.Add(nextBtn, PlayGacha);
            }
        }
        else
        {
            //Tipsなし
            btnActionDic.Add(nextBtn, PlayGacha);
        }
        List<Color> btnColors = new List<Color>() { DialogController.blueColor, DialogController.purpleColor };
        DialogController.OpenSelectDialog(title, text, imgName, btnActionDic, true, btnColors);
    }
    

    //##### 武器カスタム #####

    //武器改造
    protected void WeaponCustom(int weaponNo, UnityAction callback = null)
    {
        int pt = needWeaopnCustomPoint;
        string weaponName = Common.Weapon.GetWeaponName(weaponNo);

        //現在の強化系統
        int nowCustomType = UserManager.GetWeaponCustomType(weaponNo);

        //確認ダイアログ
        string title = "強化系統を選択してください";
        string text = "※1系統のみ強化できます\n";
        text += "※強化系統の切り替えはできますがpointは戻りません\n\n";
        text += "「" + weaponName + "」\n";
        text += pt + "pt消費";
        Dictionary<string, UnityAction> btnList = new Dictionary<string, UnityAction>();
        //List<Color> btnColors = new List<Color>();
        List<Object> customBtns = new List<Object>();
        foreach (int type in Common.Weapon.customTypeNameDic.Keys)
        {
            int customType = type;
            string btnText = Common.Weapon.customTypeNameDic[customType];
            UnityAction action = () => WeaponCustomExe(pt, weaponNo, customType, callback);
            //Color btnColor = DialogController.blueColor;
            Object btnObj = null;
            if (customType == nowCustomType)
            {
                //解除
                btnText += "【解除】";
                action = () => WeaponCustomReset(weaponNo, callback);
            }
            switch (type)
            {
                case Common.Weapon.CUSTOM_TYPE_POWER:
                    //btnColor = DialogController.redColor;
                    btnObj = powerCustomBtn;
                    break;

                case Common.Weapon.CUSTOM_TYPE_TECHNIC:
                    //btnColor = DialogController.blueColor;
                    btnObj = technicCustomBtn;
                    break;

                case Common.Weapon.CUSTOM_TYPE_UNIQUE:
                    //btnColor = DialogController.greenColor;
                    btnObj = uniqueCustomBtn;
                    break;
            }
            btnList.Add(btnText, action);
            //btnColors.Add(btnColor);
            customBtns.Add(btnObj);
        }

        //キャンセル
        btnList.Add("Cancel", null);
        customBtns.Add(cancelCustomBtn);

        DialogController.OpenSelectDialog(title, text, "", btnList, false, customBtns);
    }
    
    //カスタム実行
    private void WeaponCustomExe(int pt, int weaponNo, int type, UnityAction callback = null)
    {
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);

        //point消費
        Weapon.Custom WeaponCustom = new Weapon.Custom();
        WeaponCustom.SetApiFinishCallback(() => WeaponCustomResult(weaponNo, type, callback));
        WeaponCustom.SetApiFinishErrorCallback(CustomErrorProc);
        WeaponCustom.Exe(pt, weaponNo, type);
    }

    //カスタム解除
    private void WeaponCustomReset(int weaponNo, UnityAction callback = null)
    {
        //武器カスタム状態更新
        UserManager.SaveWeaponCustomInfo(weaponNo);
        if (callback != null) callback.Invoke();
        DialogController.OpenDialog("強化解除");
    }

    //武器カスタム成功処理
    private void WeaponCustomResult(int weaponNo, int type, UnityAction callback = null)
    {
        DialogController.CloseMessage();

        //所持ポイント表示
        textPoint.text = UserManager.userPoint.ToString();

        //武器カスタム状態更新
        UserManager.SaveWeaponCustomInfo(weaponNo, type);

        if (callback != null) callback.Invoke();

        DialogController.OpenDialog("強化成功!!");
    }

    //武器カスタム失敗処理
    private void CustomErrorProc(string errorCode)
    {
        DialogController.CloseMessage();

        string errorMessage = "エラー";
        switch (errorCode)
        {
            case "300000":
                errorMessage += "\nポイントが足りません";
                break;
        }
        DialogController.OpenDialog(errorMessage);
    }
}

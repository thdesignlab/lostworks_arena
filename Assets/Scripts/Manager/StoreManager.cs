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
    private Transform SelectMenuList;
    [SerializeField]
    private Transform WeaponGetList;
    [SerializeField]
    private Transform WeaponCustomList;
    [SerializeField]
    private Transform MusicGetList;

    [SerializeField]
    private Object storeWeaponObj;
    [SerializeField]
    private Sprite weaponBuyImage;
    [SerializeField]
    private Sprite weaponCustomImage;

    [SerializeField]
    private Color normalFontColor = new Color(0, 255, 223);
    [SerializeField]
    private Color playFontColor = new Color(255, 0, 152);

    [SerializeField]
    private Object storeMusicObj;

    private int mode = 0;
    const int MODE_MENU = 0;
    const int MODE_WEAPON_BUY = 1;
    const int MODE_WEAPON_CUSTOM = 2;
    const int MODE_MUSIC = 3;

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

        //初期表示
        DispMenu();
    }

    public void CloseStore()
    {
        if (SelectMenuList.gameObject.GetActive())
        {
            //タイトルシーンへ移動
            ScreenManager.Instance.Load(Common.CO.SCENE_TITLE, DialogController.MESSAGE_LOADING);
        }
        else
        {
            //メニュー表示
            DispMenu();
        }
    }

    //メニュー表示
    public void DispMenu()
    {
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);
        DispListClear();
        SelectMenuList.gameObject.SetActive(true);
        DialogController.CloseMessage();
    }

    //リスト表示初期化
    private void DispListClear()
    {
        SelectMenuList.gameObject.SetActive(false);
        WeaponGetList.gameObject.SetActive(false);
        WeaponCustomList.gameObject.SetActive(false);
        MusicGetList.gameObject.SetActive(false);
    }

    //##### Point処理 #####

    //pt抽選テーブル設定
    private void SetPointTable()
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
    private void PlayGacha()
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
        int pt = Common.Func.Draw<int>(pointTable);
        pt = (int)(pt * ptRate);

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

        DialogController.OpenDialog(text, nextBtn, nextAction, cancelBtn);
    }


    //##### 武器購入 #####

    //購入可能武器リスト表示
    public void DispStoreWeaponBuyList()
    {
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);
        DispListClear();
        WeaponGetList.gameObject.SetActive(true);

        Transform storeWeaponContent = WeaponGetList.FindChild("View/Content");

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
            rowTran.FindChild("Button").GetComponent<Image>().sprite = weaponBuyImage;
            int param = weaponNo;
            rowTran.GetComponent<Button>().onClick.AddListener(() => WeaponBuy(param));
            rowTran.SetParent(storeWeaponContent, false);
        }
        DialogController.CloseMessage();
    }

    //武器購入
    private void WeaponBuy(int weaponNo)
    {
        int pt = Common.Weapon.GetStoreNeedPoint(weaponNo);
        string weaponName = Common.Weapon.GetWeaponName(weaponNo);

        UnityAction buy = () =>
        {
            DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);

            //point消費
            Weapon.Buy WeaponBuy = new Weapon.Buy();
            WeaponBuy.SetApiFinishCallback(() => WeaponBuyResult(weaponNo));
            WeaponBuy.SetApiFinishErrorCallback(BuyErrorProc);
            WeaponBuy.SetApiErrorIngnore();
            WeaponBuy.Exe(pt, weaponNo);
        };

        //確認ダイアログ
        string text = "購入しますか？";
        text += "「"+weaponName+"」";
        text += pt+"pt消費";
        DialogController.OpenDialog(text, buy, true);
    }

    //武器購入成功処理
    private void WeaponBuyResult(int weaponNo)
    {
        DialogController.CloseMessage();

        //所持ポイント表示
        textPoint.text = UserManager.userPoint.ToString();

        //武器リスト更新
        UserManager.AddOpenWeapon(weaponNo);
        DispStoreWeaponBuyList();

        DialogController.OpenDialog("購入成功!!");
    }

    //武器購入失敗処理
    private void BuyErrorProc(string errorCode)
    {
        DialogController.CloseMessage();

        string errorMessage = "購入失敗";
        switch (errorCode)
        {
            case "300000":
                errorMessage += "\nポイントが足りません";
                break;
        }
        DialogController.OpenDialog(errorMessage);
    }


    //##### 武器カスタム #####

    //改造武器リスト表示
    public void DispWeaponCustomList()
    {
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);
        DispListClear();
        WeaponCustomList.gameObject.SetActive(true);

        DialogController.OpenDialog("武器改造は準備中です");

        Transform weaponCustomContent = WeaponCustomList.FindChild("View/Content");

        //リストクリア
        foreach (Transform child in weaponCustomContent)
        {
            Destroy(child.gameObject);
        }

        //武器リスト
        Dictionary<int, string[]> weaponList;
        int[] partsNoArray = new int[] 
        {
            Common.CO.PARTS_LEFT_HAND_NO,
            Common.CO.PARTS_LEFT_HAND_DASH_NO,
            Common.CO.PARTS_SHOULDER_NO,
            Common.CO.PARTS_SHOULDER_DASH_NO,
            Common.CO.PARTS_SUB_NO,
        };

        foreach (int partsNo in partsNoArray)
        {
            weaponList = Common.Weapon.GetWeaponList(partsNo);
            foreach (int weaponNo in weaponList.Keys)
            {
                string[] weaponInfo = weaponList[weaponNo];
                //OPENチェック
                if (!WeaponStore.Instance.IsEnabledEquip(weaponNo, true, weaponInfo)) continue;

                GameObject row = (GameObject)Instantiate(storeWeaponObj);
                Transform rowTran = row.transform;
                rowTran.FindChild("WeaponName").GetComponent<Text>().text = Common.Weapon.GetWeaponName(weaponNo);
                rowTran.FindChild("TypeName").GetComponent<Text>().text = Common.Weapon.GetWeaponTypeName(weaponNo);
                rowTran.FindChild("Description").GetComponent<Text>().text = weaponInfo[Common.Weapon.DETAIL_DESCRIPTION_NO];
                rowTran.FindChild("NeedPoint").GetComponent<Text>().text = Common.Weapon.GetStoreNeedPoint(weaponNo).ToString();
                rowTran.FindChild("Button").GetComponent<Image>().sprite = weaponCustomImage;
                int param = weaponNo;
                rowTran.GetComponent<Button>().onClick.AddListener(() => WeaponCustom(param));
                rowTran.SetParent(weaponCustomContent, false);
            }
        }
        DialogController.CloseMessage();
    }

    //武器改造
    private void WeaponCustom(int weaponNo)
    {
        DialogController.OpenDialog("準備中です");
        return;

        int pt = Common.Weapon.GetStoreNeedPoint(weaponNo);
        string weaponName = Common.Weapon.GetWeaponName(weaponNo);

        UnityAction custom = () =>
        {
            DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);

            //point消費
            //Weapon.Buy WeaponBuy = new Weapon.Buy();
            //WeaponBuy.SetApiFinishCallback(() => WeaponCustomResult(weaponNo));
            //WeaponBuy.SetApiFinishErrorCallback(CustomErrorProc);
            //WeaponBuy.SetApiErrorIngnore();
            //WeaponBuy.Exe(pt, weaponNo);
            WeaponCustomResult(weaponNo);
        };

        //確認ダイアログ
        string text = "改造します";
        text += "「" + weaponName + "」";
        text += pt + "pt消費";
        DialogController.OpenDialog(text, custom, true);
    }

    //武器カスタム成功処理
    private void WeaponCustomResult(int weaponNo)
    {
        DialogController.CloseMessage();

        //所持ポイント表示
        textPoint.text = UserManager.userPoint.ToString();

        //武器カスタム状態更新
        //UserManager.AddOpenWeapon(weaponNo);
        DispWeaponCustomList();

        DialogController.OpenDialog("カスタム成功!!");
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


    //##### 音楽購入 #####


    //購入可能音楽リスト表示
    public void DispStoreMusicList()
    {
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);
        DispListClear();
        MusicGetList.gameObject.SetActive(true);

        Transform storeMusicContent = MusicGetList.FindChild("View/Content");

        //再生中BGM名
        string playingBgmName = "";
        if (PlayingText != null) playingBgmName = PlayingText.text;

        //リストクリア
        foreach (Transform child in storeMusicContent)
        {
            Destroy(child.gameObject);
        }

        //音楽リスト
        List<BgmManager> battleBgmList = SoundManager.Instance.GetBattleBgmList();
        foreach (BgmManager battleBgm in battleBgmList)
        {
            ////OPENチェック
            //if (UserManager.userOpenWeapons.IndexOf(weaponNo) >= 0) continue;
            //string[] weaponInfo = weaponList[weaponNo];

            GameObject row = (GameObject)Instantiate(storeMusicObj);
            Transform rowTran = row.transform;
            string musicName = battleBgm.GetAudioClipName();
            Text musicText = rowTran.FindChild("Name").GetComponent<Text>();
            musicText.text = musicName;
            musicText.color = (playingBgmName == musicName) ? playFontColor : normalFontColor;
            BgmManager bgmMgr = battleBgm;
            rowTran.GetComponent<Button>().onClick.AddListener(() => PlayMusic(rowTran, bgmMgr));
            rowTran.SetParent(storeMusicContent, false);
        }
        DialogController.CloseMessage();
    }

    //音楽再生
    private Text PlayingText;
    private void PlayMusic(Transform rowTran, BgmManager bgmMgr)
    {
        if (PlayingText != null) PlayingText.color = normalFontColor;
        PlayingText = rowTran.FindChild("Name").GetComponent<Text>();
        PlayingText.color = playFontColor;
        bgmMgr.Play();
    }

    ////武器購入
    //public void WeaponBuy(int weaponNo)
    //{
    //    int pt = Common.Weapon.GetStoreNeedPoint(weaponNo);
    //    string weaponName = Common.Weapon.GetWeaponName(weaponNo);

    //    UnityAction buy = () =>
    //    {
    //        //point消費
    //        Weapon.Buy WeaponBuy = new Weapon.Buy();
    //        WeaponBuy.SetApiFinishCallback(() => WeaponBuyResult(weaponNo));
    //        WeaponBuy.SetApiFinishErrorCallback(BuyErrorProc);
    //        WeaponBuy.SetApiErrorIngnore();
    //        WeaponBuy.Exe(pt, weaponNo);
    //    };

    //    //確認ダイアログ
    //    string text = "購入しますか？";
    //    text += "「" + weaponName + "」";
    //    text += pt + "pt消費";
    //    DialogController.OpenDialog(text, buy, true);
    //}

    ////武器購入成功処理
    //public void WeaponBuyResult(int weaponNo)
    //{
    //    //所持ポイント表示
    //    textPoint.text = UserManager.userPoint.ToString();

    //    //武器リスト更新
    //    UserManager.AddOpenWeapon(weaponNo);
    //    DispStoreWeaponBuyList();

    //    DialogController.OpenDialog("購入成功!!");
    //}

    ////武器購入失敗処理
    //public void BuyErrorProc(string errorCode)
    //{
    //    string errorMessage = "購入失敗";
    //    switch (errorCode)
    //    {
    //        case "300000":
    //            errorMessage += "\nポイントが足りません";
    //            break;
    //    }
    //    DialogController.OpenDialog(errorMessage);
    //}

}

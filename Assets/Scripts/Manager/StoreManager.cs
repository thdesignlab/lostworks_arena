using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class StoreManager : CustomCommonManager
{
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
    private Object storeCustomObj;
    
    //Music用文字色
    [SerializeField]
    private Color closeFontColor = new Color32(0, 255, 223, 255);
    [SerializeField]
    private Color normalFontColor = new Color32(0, 255, 223, 255);
    [SerializeField]
    private Color playFontColor = new Color32(255, 0, 152, 255);

    [SerializeField]
    private Object storeMusicObj;

    const int MODE_MENU = 0;
    const int MODE_WEAPON_BUY = 1;
    const int MODE_WEAPON_CUSTOM = 2;
    const int MODE_MUSIC = 3;

    private int PlayingMusicIndex = -1;
    const int NEED_MUSIC_POINT = 100;
    

    protected override void Awake()
    {
        base.Awake();

        playMovieObj = GameObject.Find("PointGetList");
        SwitchBonusText();

        //フレームレート
        Application.targetFrameRate = 15;
    }

    protected override void Start()
    {
        base.Start();

        //初期表示
        DispMenu();
    }

    public void ModeSelect(int mode)
    {
        StartCoroutine(ModeSelectProc(mode));
    }
    IEnumerator ModeSelectProc(int mode)
    {
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);
        yield return null;
        switch (mode)
        {
            case MODE_MENU:
                DispMenu();
                break;

            case MODE_WEAPON_BUY:
                DispStoreWeaponBuyList();
                break;

            case MODE_WEAPON_CUSTOM:
                DispWeaponCustomList();
                break;

            case MODE_MUSIC:
                DispStoreMusicList();
                break;
        }
        yield return null;
        DialogController.CloseMessage();
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
    private void DispMenu()
    {
        DispListClear();
        SelectMenuList.gameObject.SetActive(true);
    }

    //リスト表示初期化
    private void DispListClear()
    {
        SelectMenuList.gameObject.SetActive(false);
        WeaponGetList.gameObject.SetActive(false);
        WeaponCustomList.gameObject.SetActive(false);
        MusicGetList.gameObject.SetActive(false);
    }

    //##### 武器購入 #####

    //購入可能武器リスト表示
    private Transform _storeWeaponContent;
    private Transform storeWeaponContent
    {
        get { return _storeWeaponContent ? _storeWeaponContent : _storeWeaponContent = WeaponGetList.FindChild("List/View/Content"); }
    }
    private Transform _storeWeaponPartsTab;
    private Transform storeWeaponPartsTab
    {
        get { return _storeWeaponPartsTab ? _storeWeaponPartsTab : _storeWeaponPartsTab = WeaponGetList.FindChild("PartsTabs"); }
    }
    private int selectedBuyPartsNo = 0;

    private void DispStoreWeaponBuyList()
    {
        DispListClear();
        WeaponGetList.gameObject.SetActive(true);

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
            rowTran.FindChild("NeedPoint").GetComponent<Text>().text = needWeaponBuyPoint.ToString();
            int param = weaponNo;
            rowTran.GetComponent<Button>().onClick.AddListener(() => WeaponBuy(param));
            rowTran.SetParent(storeWeaponContent, false);
        }
        //フィルター
        WeaponBuyListFilter(selectedBuyPartsNo);
        //Tagセット
        SetWeaponBuyTag();
    }

    //表示フィルター
    public void WeaponBuyListFilter(int baseWeaponNo = 0)
    {
        selectedBuyPartsNo = baseWeaponNo;
        string targetTypeName = Common.Weapon.GetWeaponTypeName(baseWeaponNo);
        foreach (Transform rowTran in storeWeaponContent)
        {
            bool isDisp = true;
            if (!string.IsNullOrEmpty(targetTypeName))
            {
                string typeName = rowTran.FindChild("TypeName").GetComponent<Text>().text;
                isDisp = (targetTypeName == typeName);
            }
            rowTran.gameObject.SetActive(isDisp);
        }
    }

    //Tag設定
    private void SetWeaponBuyTag()
    {
        //Tagチェック
        string targetTypeName = Common.Weapon.GetWeaponTypeName(selectedBuyPartsNo);
        if (string.IsNullOrEmpty(targetTypeName)) targetTypeName = "All";
        foreach (Transform tabTran in storeWeaponPartsTab)
        {
            string tabName = tabTran.FindChild("Text").GetComponent<Text>().text;
            if (tabName == targetTypeName)
            {
                tabTran.GetComponent<Toggle>().isOn = true;
                break;
            }
        }
    }

    //武器購入
    private void WeaponBuy(int weaponNo)
    {
        int pt = needWeaponBuyPoint;
        string weaponName = Common.Weapon.GetWeaponName(weaponNo);

        UnityAction buy = () =>
        {
            DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);

            //point消費
            Weapon.Buy WeaponBuy = new Weapon.Buy();
            WeaponBuy.SetApiFinishCallback(() => WeaponBuyResult(weaponNo));
            WeaponBuy.SetApiFinishErrorCallback(BuyErrorProc);
            WeaponBuy.Exe(pt, weaponNo);
        };

        //確認ダイアログ
        string text = "購入しますか？\n\n";
        text += "「"+weaponName+"」\n";
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
        //Tagセット
        SetWeaponBuyTag();
        DialogController.OpenDialog(errorMessage);
    }


    //##### 武器カスタム #####
    private Transform _weaponCustomContent;
    private Transform weaponCustomContent
    {
        get { return _weaponCustomContent ? _weaponCustomContent : _weaponCustomContent = WeaponCustomList.FindChild("List/View/Content"); }
    }
    private Transform _weaponCustomPartsTab;
    private Transform weaponCustomPartsTab
    {
        get { return _weaponCustomPartsTab ? _weaponCustomPartsTab : _weaponCustomPartsTab = WeaponCustomList.FindChild("PartsTabs"); }
    }
    private int selectedCustomPartsNo = 0;

    //改造武器リスト表示
    private void DispWeaponCustomList()
    {
        DispListClear();
        WeaponCustomList.gameObject.SetActive(true);
        
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
            Common.CO.PARTS_EXTRA_NO,
        };

        foreach (int partsNo in partsNoArray)
        {
            weaponList = Common.Weapon.GetWeaponList(partsNo);
            foreach (int weaponNo in weaponList.Keys)
            {
                string[] weaponInfo = weaponList[weaponNo];
                //OPENチェック
                if (!WeaponStore.Instance.IsEnabledEquip(weaponNo, true, weaponInfo)) continue;
                //カスタム可能チェック
                string prefabName = weaponInfo[Common.Weapon.DETAIL_PREFAB_NAME_NO];
                GameObject weapon = (GameObject)Resources.Load(Common.Func.GetResourceWeapon(prefabName));
                if (weapon == null) continue;
                WeaponLevelController weaponLevelCtrl = weapon.GetComponent<WeaponLevelController>();
                if (weaponLevelCtrl == null) continue;

                //現在の強化状態
                int nowCustomType = UserManager.GetWeaponCustomType(weaponNo);
                Sprite customIcon = Common.Func.GetCustomIcon(nowCustomType);

                GameObject row = (GameObject)Instantiate(storeCustomObj);
                Transform rowTran = row.transform;
                if (customIcon != null)
                {
                    Transform customIconTran = rowTran.FindChild("NameArea/CustomIcon");
                    Image imgObj = customIconTran.GetComponent<Image>();
                    imgObj.sprite = customIcon;
                    imgObj.preserveAspect = true;
                    customIconTran.gameObject.SetActive(true);
                }
                rowTran.FindChild("NameArea/WeaponName").GetComponent<Text>().text = Common.Weapon.GetWeaponName(weaponNo);
                rowTran.FindChild("TypeName").GetComponent<Text>().text = Common.Weapon.GetWeaponTypeName(weaponNo);
                rowTran.FindChild("Description").GetComponent<Text>().text = weaponInfo[Common.Weapon.DETAIL_DESCRIPTION_NO];
                rowTran.FindChild("NeedPoint").GetComponent<Text>().text = needWeaopnCustomPoint.ToString();
                int param = weaponNo;
                rowTran.GetComponent<Button>().onClick.AddListener(() => WeaponCustom(param));
                rowTran.SetParent(weaponCustomContent, false);
            }
        }
        //フィルター
        WeaponBuyListFilter(selectedCustomPartsNo);
        //Tagセット
        SetWeaponBuyTag();
    }


    //表示フィルター
    public void WeaponCustomListFilter(int baseWeaponNo = 0)
    {
        selectedCustomPartsNo = baseWeaponNo;
        string targetTypeName = Common.Weapon.GetWeaponTypeName(baseWeaponNo);
        foreach (Transform rowTran in weaponCustomContent)
        {
            bool isDisp = true;
            if (!string.IsNullOrEmpty(targetTypeName))
            {
                string typeName = rowTran.FindChild("TypeName").GetComponent<Text>().text;
                isDisp = (targetTypeName == typeName);
            }
            rowTran.gameObject.SetActive(isDisp);
        }
    }

    //Tag設定
    private void SetWeaponCustomTag()
    {
        //Tagチェック
        string targetTypeName = Common.Weapon.GetWeaponTypeName(selectedCustomPartsNo);
        if (string.IsNullOrEmpty(targetTypeName)) targetTypeName = "All";
        foreach (Transform tabTran in weaponCustomPartsTab)
        {
            string tabName = tabTran.FindChild("Text").GetComponent<Text>().text;
            if (tabName == targetTypeName)
            {
                tabTran.GetComponent<Toggle>().isOn = true;
                break;
            }
        }
    }


    //##### BGM #####

    //購入可能BGMリスト表示
    private void DispStoreMusicList()
    {
        DispListClear();
        MusicGetList.gameObject.SetActive(true);

        Transform storeMusicContent = MusicGetList.FindChild("View/Content");

        ////再生中BGM名
        //string playingBgmName = "";
        //if (PlayingText != null) playingBgmName = PlayingText.text;

        //リストクリア
        foreach (Transform child in storeMusicContent)
        {
            Destroy(child.gameObject);
        }

        //音楽リスト
        List<BgmManager> battleBgmList = SoundManager.Instance.GetBattleBgmList();
        for (int i = 0; i < battleBgmList.Count; i++)
        {
            int musicIndex = i;
            BgmManager battleBgm = battleBgmList[musicIndex];

            //OPENチェック
            bool isOpen = false;
            if (UserManager.userOpenMusics.IndexOf(musicIndex) >= 0) isOpen = true;
 
            GameObject row = (GameObject)Instantiate(storeMusicObj);
            Transform rowTran = row.transform;
            string musicName = battleBgm.GetAudioClipName();
            Text musicText = rowTran.FindChild("Name").GetComponent<Text>();
            musicText.text = musicName;
            BgmManager bgmMgr = battleBgm;
            if (isOpen)
            {
                //再生
                musicText.color = (PlayingMusicIndex == musicIndex) ? playFontColor : normalFontColor;
                rowTran.GetComponent<Button>().onClick.AddListener(() => PlayMusic(musicIndex, bgmMgr));
            }
            else
            {
                //購入
                musicText.color = closeFontColor;
                rowTran.GetComponent<Button>().onClick.AddListener(() => BuyMusic(musicIndex, battleBgm));
            }
            rowTran.SetParent(storeMusicContent, false);
        }
    }

    //BGM再生
    private void PlayMusic(int musicIndex, BgmManager bgmMgr)
    {
        UnityAction play = () => {
            Transform storeMusicContent = MusicGetList.FindChild("View/Content");
            int i = 0;
            foreach (Transform child in storeMusicContent)
            {
                if (PlayingMusicIndex == i)
                {
                    //色を通常に戻す
                    child.GetComponentInChildren<Text>().color = normalFontColor;
                }
                if (musicIndex == i)
                {
                    //色をプレイ中に変更
                    child.GetComponentInChildren<Text>().color = playFontColor;
                }
                i++;
            }
            PlayingMusicIndex = musicIndex;
            bgmMgr.Play();
        };
        if (UserManager.userConfig[Common.PP.CONFIG_BGM_MUTE] == 1)
        {
            UnityAction muteReset = () => {
                ConfigManager.Instance.OnChangeMuteBgm(false);
                UserManager.SaveConfig(true);
                play();
            };
            DialogController.OpenDialog("BGMがMuteに設定されています\n解除しますか？", "解除", muteReset, true );
        }
        else
        {
            play();
        }
    }

    //BGM購入
    private void BuyMusic(int musicIndex, BgmManager battleBgm)
    {
        string musicName = battleBgm.GetAudioClipName();

        UnityAction buy = () =>
        {
            //point消費
            Point.Use PointUse = new Point.Use();
            PointUse.SetApiFinishCallback(() => MusicBuyResult(musicIndex));
            PointUse.SetApiFinishErrorCallback(BuyMusicErrorProc);
            PointUse.Exe(NEED_MUSIC_POINT, Common.API.POINT_LOG_KIND_MUSIC, musicIndex);
        };

        //確認ダイアログ
        string text = "未開放のBGMです\n";
        text += "解放しますか？\n\n";
        text += "「" + musicName + "」\n";
        text += NEED_MUSIC_POINT + "pt消費";
        DialogController.OpenDialog(text, buy, true);
    }

    //BGM購入成功処理
    private void MusicBuyResult(int musicIndex)
    {
        //所持ポイント表示
        textPoint.text = UserManager.userPoint.ToString();

        //BGMリスト更新
        UserManager.AddOpenMusic(musicIndex);
        DispStoreMusicList();

        DialogController.OpenDialog("BGM解放!!");
    }

    //BGM購入失敗処理
    private void BuyMusicErrorProc(string errorCode)
    {
        string errorMessage = "BGM解放失敗";
        switch (errorCode)
        {
            case "300000":
                errorMessage += "\nポイントが足りません";
                break;
        }
        DialogController.OpenDialog(errorMessage);
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class CustomManager : CustomCommonManager
{
    [SerializeField]
    private Transform statusBar;
    [SerializeField]
    private Transform charaTable;
    [SerializeField]
    private List<Transform> spawnPoints;
    public float charaSize;
    [SerializeField]
    private Text partsNameText;
    [SerializeField]
    private Transform partsSelectArea;
    [SerializeField]
    private RectTransform weaponSelectArea;

    [SerializeField]
    private GameObject charaLeftArrow;
    [SerializeField]
    private GameObject charaRightArrow;
    [SerializeField]
    private GameObject partsUpArrow;
    [SerializeField]
    private GameObject weaponCloseArrow;
    [SerializeField]
    private GameObject colorChangeButton;

    [SerializeField]
    private Sprite partsSelectedSprite;
    [SerializeField]
    private Sprite partsNotSelectedSprite;

    [SerializeField]
    private List<Sprite> bitTypeSprites;

    [SerializeField]
    private GameObject equipWeaponRow;
    [SerializeField]
    private Sprite equipWeaponRowOnImg;
    [SerializeField]
    private Sprite equipWeaponRowOffImg;
    //[SerializeField]
    private Color weaponOtherSelectedColor = new Color32(128, 128, 128, 255);
    //[SerializeField]
    private Color weaponSelectedColor = new Color32(0, 234, 255, 255);
    //[SerializeField]
    private Color weaponNotSelectedColor = new Color32(255, 235, 4, 255);
    //private int maxWeaponButtonCount = 8;
    private float buttonHeight;
    private float buttonWidth;
    private float descriptionHeight;
    private float weaponListHeight;

    private Transform charaTran;
    private PlayerController playerCtrl;
    private Animator charaAnimator;
    private int waitHash = Animator.StringToHash("Base Layer.Wait");
    private List<List<int>> selectableCharaList = new List<List<int>>();

    //キャラテーブルステータス
    private int charaIndex = 0;
    private int colorIndex = 0;
    private int tableIndex = 0;
    private float charaChangeAngle = 120.0f;
    private float charaChangeTime = 1.0f;
    //private float charaSideAngle = 10.0f;
    //private float charaSideTime = 0.2f;
    private bool isTurnTable = false;

    //キャンバスステータス
    private int selectedPartsNo = -1;
    //private float selectModeTime = 0.2f;

    //武器セレクトエリア
    //private bool isOpenWeaponCanvas = false;
    private ScrollRect weaponScrollView;
    private LayoutElement weaponScrollViewLayout;
    private RectTransform weaponButtonAreaRectTran;
    private Transform weaponButtonArea;

    private Dictionary<int, List<Image>> partsFrameMap = new Dictionary<int, List<Image>>();
    private Dictionary<int, List<Image>> bitImgMap = new Dictionary<int, List<Image>>();

    private int fireNo = 0;

    private const string PARTS_AREA_NAME_LEFT = "Left-Weapon";
    private const string PARTS_AREA_NAME_LEFT_DASH = "Left-Dash-Weapon";
    private const string PARTS_AREA_NAME_RIGHT = "Right-Weapon";
    private const string PARTS_AREA_NAME_RIGHT_DASH = "Right-Dash-Weapon";
    private const string PARTS_AREA_NAME_SHOULDER = "Shoulder-Weapon";
    private const string PARTS_AREA_NAME_SHOULDER_DASH = "Shoulder-Dash-Weapon";
    private const string PARTS_AREA_NAME_SUB = "Sub-Weapon";
    private const string PARTS_AREA_NAME_EXTRA = "Extra-Weapon";
    private const string BIT_IMG_AREA = "BitImg";

    private const float SHOOT_INTERVAL = 1.0f;
    private float leftShootInterval = 0;
    private bool isWeaponSelecting = false;

    //partsフレームの設置順
    private Dictionary<int, string> partsSelectNameMap = new Dictionary<int, string>()
    {
        { Common.CO.PARTS_LEFT_HAND_NO, PARTS_AREA_NAME_LEFT },
        { Common.CO.PARTS_LEFT_HAND_DASH_NO, PARTS_AREA_NAME_LEFT_DASH },
        { Common.CO.PARTS_SHOULDER_NO, PARTS_AREA_NAME_SHOULDER },
        { Common.CO.PARTS_SHOULDER_DASH_NO, PARTS_AREA_NAME_SHOULDER_DASH },
        { Common.CO.PARTS_RIGHT_HAND_NO, PARTS_AREA_NAME_RIGHT },
        { Common.CO.PARTS_RIGHT_HAND_DASH_NO, PARTS_AREA_NAME_RIGHT_DASH },
        { Common.CO.PARTS_SUB_NO, PARTS_AREA_NAME_SUB },
        { Common.CO.PARTS_EXTRA_NO, PARTS_AREA_NAME_EXTRA },
    };

    protected override void Awake()
    {
        base.Awake();

        DialogController.OpenMessage(DialogController.MESSAGE_JOIN_ROOM, DialogController.MESSAGE_POSITION_RIGHT);

        //所持ポイント取得
        Point.Get pointGet = new Point.Get();
        pointGet.SetApiErrorIngnore();
        pointGet.SetApiFinishCallback(SetCustom);
        pointGet.SetApiFinishErrorCallback(SetNoCustom);
        pointGet.SetConnectErrorCallback(SetNoCustom);
        pointGet.Exe();

        //武器リストエリア取得
        Transform weaponScrollViewTran = weaponSelectArea.FindChild("ScrollView");
        weaponScrollView = weaponScrollViewTran.GetComponent<ScrollRect>();
        weaponScrollViewLayout = weaponScrollViewTran.GetComponent<LayoutElement>();
        weaponButtonArea = weaponScrollViewTran.FindChild("Viewport/ButtonArea");
        weaponButtonAreaRectTran = weaponButtonArea.GetComponent<RectTransform>();
        buttonHeight = equipWeaponRow.GetComponent<LayoutElement>().preferredHeight + weaponButtonArea.GetComponent<VerticalLayoutGroup>().spacing;
        descriptionHeight = equipWeaponRow.transform.FindChild("WeaponDescription").GetComponent<RectTransform>().rect.height;
        weaponListHeight = weaponSelectArea.rect.height;
        weaponSelectArea.gameObject.SetActive(false);

        playMovieObj = GameObject.Find("PointGetArea");
        SwitchPointGetArea(false);

        partsNameText.text = "";

        //ステータスバー対策
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
                statusBar.GetComponent<LayoutElement>().preferredHeight = 60;
                statusBar.GetComponent<Image>().color = new Color(0, 0, 0, 1);
                break;

            default:
                statusBar.GetComponent<LayoutElement>().preferredHeight = 30;
                statusBar.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                break;
        }
    }

    protected override void Start()
    {
        base.Start();

        //PartsNo紐付け
        foreach (int partsNo in partsSelectNameMap.Keys)
        {
            partsFrameMap[partsNo] = new List<Image>();
            bitImgMap[partsNo] = new List<Image>();
        }
        foreach (Transform partsAreaCopy in partsSelectArea)
        {
            foreach (int partsNo in partsSelectNameMap.Keys)
            {
                partsFrameMap[partsNo].Add(partsAreaCopy.FindChild(partsSelectNameMap[partsNo]).GetComponent<Image>());
                bitImgMap[partsNo].Add(partsAreaCopy.FindChild(partsSelectNameMap[partsNo] + "/" + BIT_IMG_AREA).GetComponent<Image>());
            }
        }

        //ユーザー情報取得
        int charaNo = UserManager.userSetCharacter;

        //所持キャラクター取得
        Dictionary<string, List<int>> tmpCharaList = CharacterManager.GetSelectableCharacter();
        int index = 0;
        foreach (string name in tmpCharaList.Keys)
        {
            int col = tmpCharaList[name].IndexOf(charaNo);
            if (col >= 0)
            {
                charaIndex = index;
                colorIndex = col;
            }
            selectableCharaList.Add(tmpCharaList[name]);
            index++;
        }

        //キャラ生成
        SpawnCharacter();
    }

    void Update()
    {
        if (leftShootInterval > 0)
        {
            leftShootInterval -= Time.deltaTime;
        }
    }

    //ネットワーク接続なし
    private void SetNoCustom()
    {
        isConnectedNetwork = false;
        if (!UserManager.isCheckCustomSceneNetwork)
        {
            UserManager.isCheckCustomSceneNetwork = true;
            string message = "Network接続に失敗したため\n";
            message += "武器の強化は行えません。";
            DialogController.OpenDialog(message);
        }
        DialogController.CloseMessage();
    }

    //ネットワーク接続あり
    private void SetCustom()
    {
        isConnectedNetwork = true;
        textPoint.text = UserManager.userPoint.ToString();
        DialogController.CloseMessage();
    }

    //タイトルへ戻る
    public void GoToTitle()
    {
        PlayerPrefsUtility.Save(Common.PP.USER_CHARACTER, UserManager.userSetCharacter);
        PlayerPrefsUtility.SaveDict<string, int>(Common.PP.USER_EQUIP, UserManager.userEquipment);
        ScreenManager.Instance.Load(Common.CO.SCENE_TITLE, DialogController.MESSAGE_LOADING);
    }

    //キャラ回転
    private void CharacterRotate(float diff)
    {
        charaTran.Rotate(charaTran.up, diff);
    }

    //キャラ生成
    private void SpawnCharacter()
    {
        //キャラ情報取得
        charaIndex = charaIndex % selectableCharaList.Count;
        colorIndex = colorIndex % selectableCharaList[charaIndex].Count;
        int charaNo = selectableCharaList[charaIndex][colorIndex];
        string[] charaInfo = CharacterManager.GetCharacterInfo(charaNo);
        if (charaInfo == null) return;

        //カラー変更ボタン
        SwitchColorChangeBtn(true);

        //キャラセット情報更新
        UserManager.userSetCharacter = charaNo;

        //特別武器更新
        int extraWeaponNo = UserManager.userEquipment[Common.CO.PARTS_EXTRA];
        if (!Common.Weapon.IsEnabledEquipExtraWeapon(charaNo, extraWeaponNo))
        {
            UserManager.userEquipment[Common.CO.PARTS_EXTRA] = Common.Weapon.GetExtraWeaponNo(charaNo);
        }

        //ベースボディ生成
        GameObject charaBaseObj = PhotonNetwork.Instantiate(Common.CO.CHARACTER_BASE, spawnPoints[tableIndex].position, spawnPoints[tableIndex].rotation, 0);
        charaTran = charaBaseObj.transform;
        Rigidbody charaBody = charaBaseObj.GetComponent<Rigidbody>();
        charaBody.useGravity = false;
        charaBody.isKinematic = false;

        //メインボディ生成
        //PlayerSettingで生成

        //キャラ設定
        playerCtrl = charaTran.GetComponent<PlayerController>();
        charaTran.localScale = new Vector3(charaSize, charaSize, charaSize);
        charaTran.parent = spawnPoints[tableIndex];
        charaAnimator = charaTran.GetComponentInChildren<Animator>();

        //装備を呼び出す
        WeaponLoad();

        //非表示のキャラを削除
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (i != tableIndex)
            {
                foreach (Transform child in spawnPoints[i])
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    //カラーチェンジボタン切り替え
    private void SwitchColorChangeBtn(bool flg)
    {
        if (colorChangeButton != null)
        {
            bool isExistsColor = (selectableCharaList[charaIndex].Count >= 2);
            if (isExistsColor)
            {
                colorChangeButton.SetActive(flg);
            }
            else
            {
                colorChangeButton.SetActive(false);
            }
        }
    }

    //装備(partsNo指定)
    private GameObject EquipWeapon(int partsNo, int weaponNo)
    {
        Transform parts = charaTran.FindChild(Common.Func.GetPartsStructure(partsNo));
        return EquipWeapon(parts, weaponNo);
    }

    //装備(partsTransform指定)
    private GameObject EquipWeapon(Transform parts, int weaponNo)
    {
        //すでに装備している場合は破棄
        foreach (Transform child in parts)
        {
            child.parent = null;
            Destroy(child.gameObject);
        }

        //武器取得
        string weaponName = Common.Weapon.GetWeaponName(weaponNo, true);
        if (weaponName == "") return null;

        //武器召喚
        GameObject ob = PhotonNetwork.Instantiate(Common.Func.GetResourceWeapon(weaponName), parts.position, parts.rotation, 0);
        if (ob == null) return null;
        ob.transform.localScale = new Vector3(charaSize, charaSize, charaSize);
        ob.name = ob.name.Replace("(Clone)", "");
        ob.transform.parent = parts;

        //装備情報保存
        UserManager.userEquipment[parts.name] = weaponNo;

        //装備使用準備
        playerCtrl.SetWeapon();

        return ob;
    }

    //装備武器読み込み
    private void WeaponLoad()
    {
        foreach (int partsNo in Common.CO.partsNameArray.Keys)
        {
            string partsName = Common.Func.GetPartsStructure(Common.CO.partsNameArray[partsNo]);

            Transform parts = charaTran.FindChild(partsName);
            if (parts != null)
            {
                //装備
                GameObject weaponObj = EquipWeapon(parts, UserManager.userEquipment[parts.name]);

                //Bit画像設定
                SetBitIcon(partsNo, weaponObj);
            }
        }
    }

    //武器試射可否チェック
    private bool IsEnabledFire()
    {
        if (leftShootInterval > 0) return false;
        if (charaAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash != waitHash) return false;
        return true;
    }

    //試射
    private bool Fire(int partsNo)
    {
        if (!IsEnabledFire()) return false;

        leftShootInterval = SHOOT_INTERVAL;
        playerCtrl.CustomSceaneFire(partsNo);
        return true;
    }

    //#########################
    //##### TourchManager #####
    //#########################
    private float touchPosX = 0;
    private float touchPosY = 0;
    private float prePosX = 0;
    private float prePosY = 0;
    private bool isSwipe = false;
    GameObject tapObj;

    void OnEnable()
    {
        TouchManager.Instance.Drag += OnSwipe;
        TouchManager.Instance.TouchStart += OnTouchStart;
        TouchManager.Instance.TouchEnd += OnTouchEnd;
        TouchManager.Instance.FlickStart += OnFlickStart;
        TouchManager.Instance.FlickComplete += OnFlickComplete;
    }

    void OnDisable()
    {
        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.Drag -= OnSwipe;
            TouchManager.Instance.TouchStart -= OnTouchStart;
            TouchManager.Instance.TouchEnd -= OnTouchEnd;
            TouchManager.Instance.FlickStart -= OnFlickStart;
            TouchManager.Instance.FlickComplete -= OnFlickComplete;
        }
    }

    void OnTouchStart(object sender, CustomInputEventArgs e)
    {
        touchPosX = e.Input.ScreenPosition.x;
        touchPosY = e.Input.ScreenPosition.y;
        prePosX = touchPosX;
        prePosY = touchPosY;

        //UIタッチチェック
        tapObj = OnTapedObj<Image>(e.Input.ScreenPosition);
    }

    void OnTouchEnd(object sender, CustomInputEventArgs e)
    {
        if (!isSwipe)
        {
            //if (tapObj != null) Debug.Log(tapObj.name + " : " + tapObj.tag);

            if (tapObj != null)
            {
                //Debug.Log(tapObj.name + " : " + tapObj.tag);
                //if (tapObj.GetComponent<Button>() != null)
                //{
                //    //ボタン押下
                //}
            }
            else
            {
                //ためし撃ち
                int partsNo = fireNo % Common.CO.partsNameArray.Count;
                if (Fire(partsNo)) fireNo++;
            }
        }
        isSwipe = false;
        tapObj = null;
    }

    void OnSwipe(object sender, CustomInputEventArgs e)
    {
        float preDiffX = e.Input.ScreenPosition.x - prePosX;
        float preDiffY = e.Input.ScreenPosition.y - prePosY;
        prePosX = e.Input.ScreenPosition.x;
        prePosY = e.Input.ScreenPosition.y;

        float minSwitpeDiff = 5;
        if (Mathf.Abs(preDiffX) < minSwitpeDiff && Mathf.Abs(preDiffY) < minSwitpeDiff) return;
        isSwipe = true;

        bool isCharaRotate = true;
        if (tapObj != null)
        {
            if (tapObj.tag == "PartsSelect"
                || tapObj.tag == "WeaponSelect"
                || tapObj.name == "WeaponDescription"
            )
            {
                isCharaRotate = false;
            }
        }
        if (isCharaRotate)
        {
            //キャラ回転
            CharacterRotate(preDiffX * -1);
        }
    }

    void OnFlickStart(object sender, FlickEventArgs e)
    {
    }

    void OnFlickComplete(object sender, FlickEventArgs e)
    {
    }

    private GameObject OnTapedObj<T>(Vector3 _scrPos)
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = _scrPos;
        List<RaycastResult> result = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, result);

        GameObject obj = null;
        foreach (RaycastResult hit in result)
        {
            T component = hit.gameObject.GetComponent<T>();
            if (component != null)
            {
                obj = hit.gameObject;
                break;
            }
        }
        return obj;
    }


    //#########################
    //##### TourchEvent #####
    //#########################

    //parts選択
    public void PartsSelectOn(int partsNo)
    {
        //Debug.Log("PartsSelectOn");
        if (isTurnTable) return;
        if (isWeaponAreaAnimation) return;
        if (tapObj == null) return;

        //UI表示切り替え
        partsUpArrow.SetActive(false);
        charaLeftArrow.SetActive(false);
        charaRightArrow.SetActive(false);
        SwitchColorChangeBtn(false);

        //部位名表示
        partsNameText.text = tapObj.name;

        //選択していた部位背景元に戻す
        SetPartsFrame(selectedPartsNo, false);
        if (selectedPartsNo == -1)
        {
            //部位未選択時キャラを脇へ移動
            //StartCoroutine(TurnCharaTable(charaSideAngle, charaSideTime, false));
        }

        //partsNo取得
        selectedPartsNo = partsNo;

        //選択部位背景変更
        SetPartsFrame(selectedPartsNo, true);

        OpenWeaponList();
    }

    //parts選択解除
    public void PartsSelectOff()
    {
        if (isTurnTable) return;
        if (isWeaponAreaAnimation) return;

        //部位名表示
        partsNameText.text = "";

        //UI表示切替
        partsUpArrow.SetActive(true);
        charaLeftArrow.SetActive(true);
        charaRightArrow.SetActive(true);
        SwitchColorChangeBtn(true);

        SetPartsFrame(selectedPartsNo, false);
        selectedPartsNo = -1;

        CloseWeaponList();
    }

    //武器選択
    private void WeaponSelect(int weaponNo)
    {
        //Debug.Log("WeaponSelect:"+weaponNo);
        if (isWeaponSelecting) return;
        if (isTurnTable) return;
        if (isWeaponAreaAnimation) return;

        //現在装備中チェック
        string partsName = Common.CO.partsNameArray[selectedPartsNo];
        int nowWeaponNo = UserManager.userEquipment[partsName];

        if (nowWeaponNo != weaponNo)
        {
            //装備可能チェック
            if (!WeaponStore.Instance.IsEnabledEquip(weaponNo)) return;

            //前装備情報更新
            Transform preTran = weaponButtonArea.Find(nowWeaponNo.ToString());
            if (preTran != null)
            {
                SetEquip(preTran.FindChild("WeaponSelectBtn"), false);
                StartCoroutine(WeaponDescriptionAnimation(preTran, false));
            }

            //装備変更
            GameObject weaponObj = EquipWeapon(selectedPartsNo, weaponNo);

            //Bit画像設定
            SetBitIcon(selectedPartsNo, weaponObj);

            //装備情報更新
            Transform nowTran = weaponButtonArea.Find(weaponNo.ToString());
            SetEquip(nowTran.FindChild("WeaponSelectBtn"), true);
            StartCoroutine(WeaponDescriptionAnimation(nowTran, true));
        }
        else
        {
            //試射
            //Fire(selectedPartsNo);
        }
    }

    //装備状態チェック
    const int EQUIP_STATUS_NONE = 1;
    const int EQUIP_STATUS_EQUIP = 2;
    const int EQUIP_STATUS_OTHER = 3;
    private int CheckEquipStatus(int weaponNo, int partsNo = -1)
    {
        if (partsNo < 0) partsNo = selectedPartsNo;
        int status = EQUIP_STATUS_NONE;
        if (UserManager.userEquipment.ContainsValue(weaponNo))
        {
            //どこかに装備している
            status = EQUIP_STATUS_OTHER;
            if (UserManager.userEquipment[Common.CO.partsNameArray[partsNo]] == weaponNo)
            {
                //該当部位に装備している
                status = EQUIP_STATUS_EQUIP;
            }
        }
        return status;
    }

    //武器選択リストオープン
    bool isWeaponAreaAnimation = false;
    private void OpenWeaponList()
    {
        weaponSelectArea.gameObject.SetActive(true);
        SwitchPointGetArea(true);

        //武器リスト初期化
        initWeaponList();

        //装備中武器取得
        int nowWeaponNo = UserManager.userEquipment[Common.CO.partsNameArray[selectedPartsNo]];

        //装備可能武器取得
        List<int> weaponNoList = WeaponStore.Instance.GetSelectableWeaponNoList(selectedPartsNo, true);

        //ボタン設置
        foreach (int weaponNo in weaponNoList)
        {
            //武器情報
            int paramWeaponNo = weaponNo;
            string[] weaponInfo = Common.Weapon.GetWeaponInfo(paramWeaponNo);
            if (weaponInfo.Length == 0) continue;

            //武器ライン生成
            GameObject row = (GameObject)Instantiate(equipWeaponRow, Vector3.zero, Quaternion.identity);
            Transform rowTran = row.transform;
            rowTran.SetParent(weaponButtonArea, false);
            rowTran.name = paramWeaponNo.ToString();

            //Nameエリア
            Transform btnTran = rowTran.FindChild("WeaponSelectBtn");
            //Descriptionエリア
            Transform descriptionTran = rowTran.FindChild("WeaponDescription");

            //装備状態
            int equipStatus = CheckEquipStatus(paramWeaponNo);

            //強化アイコン
            SetCustom(btnTran, paramWeaponNo);

            //装備表示
            string weaponName = Common.Weapon.GetWeaponName(paramWeaponNo);
            string ruby = weaponInfo[Common.Weapon.DETAIL_RUBY_NO];
            if (!string.IsNullOrEmpty(ruby)) weaponName += "[" + ruby + "]";
            SetEquip(btnTran, equipStatus, weaponName);

            //装備イベント
            btnTran.GetComponent<Button>().onClick.AddListener(() => WeaponSelect(paramWeaponNo));

            //詳細
            descriptionTran.localScale = new Vector3(1, 0, 1);
            descriptionTran.FindChild("Description").GetComponent<Text>().text = weaponInfo[Common.Weapon.DETAIL_DESCRIPTION_NO];
            //強化可能チェック
            Transform customInfoTran = descriptionTran.FindChild("CustomInfo");
            if (isConnectedNetwork && IsEnabledCustom(paramWeaponNo))
            {
                customInfoTran.gameObject.SetActive(true);
                customInfoTran.FindChild("Point").GetComponent<Text>().text = needWeaopnCustomPoint.ToString();
                //強化後コールバック
                UnityAction callback = () => SetCustom(btnTran, paramWeaponNo);
                customInfoTran.FindChild("CustomBtn").GetComponent<Button>().onClick.AddListener(() => WeaponCustom(paramWeaponNo, callback));
            }
            else
            {
                customInfoTran.gameObject.SetActive(false);
            }
        }

        //エリアサイズ変更
        float dispWeaponCount = weaponNoList.Count;
        weaponScrollViewLayout.preferredHeight = dispWeaponCount * buttonHeight;
        if (isConnectedNetwork) weaponScrollViewLayout.preferredHeight += descriptionHeight;
        weaponButtonAreaRectTran.sizeDelta = new Vector2(weaponButtonAreaRectTran.rect.width, weaponScrollViewLayout.preferredHeight);
        if (weaponScrollViewLayout.preferredHeight > weaponListHeight) weaponScrollViewLayout.preferredHeight = weaponListHeight;

        //アニメーション
        if (dispWeaponCount > 0) StartCoroutine(WeaponRowAnimation(nowWeaponNo));
    }

    //カスタム可能チェック
    private bool IsEnabledCustom(int weaponNo)
    {
        string prefabName = Common.Weapon.GetWeaponName(weaponNo, true);
        GameObject weapon = Resources.Load<GameObject>(Common.Func.GetResourceWeapon(prefabName));
        if (weapon == null) return false;
        WeaponLevelController weaponLevelCtrl = weapon.GetComponent<WeaponLevelController>();
        if (weaponLevelCtrl == null) return false;

        return true;
    }


    //武器リストアニメーション
    float rowSideTime = 0.15f;
    float rowDelayTime = 0.015f;
    IEnumerator WeaponRowAnimation(int equipWeaponNo)
    {
        isWeaponAreaAnimation = true;

        Vector3 rowStartScale = new Vector3(0, 1, 1);
        Vector3 rowLastScale = Vector3.one;

        //初期設定
        foreach (Transform row in weaponButtonArea)
        {
            row.localScale = rowStartScale;
        }
        weaponScrollView.verticalNormalizedPosition = 1;
        yield return null;

        //武器リストSlideIn
        Transform equipRowTran = null;
        Coroutine slide = null;
        foreach (Transform row in weaponButtonArea)
        {
            slide = StartCoroutine(WeaponRowSlide(row, rowStartScale, rowLastScale));
            if (row.name == equipWeaponNo.ToString()) equipRowTran = row;
            yield return new WaitForSeconds(rowDelayTime);
        }
        //スクロール
        Coroutine scroll = StartCoroutine(WeaponAreaScroll(equipWeaponNo));

        yield return slide;

        //詳細オープン
        Coroutine description = StartCoroutine(WeaponDescriptionAnimation(equipRowTran, true));

        yield return scroll;
        yield return description;

        isWeaponAreaAnimation = false;
    }

    //スライド
    IEnumerator WeaponRowSlide(Transform rowTran, Vector3 startScale, Vector3 lastScale)
    {
        float procTime = 0;
        for (;;)
        {
            procTime += Time.deltaTime;
            float rate = procTime / rowSideTime;
            if (rowTran == null) yield break;
            rowTran.localScale = Vector3.Lerp(startScale, lastScale, rate);
            if (rate >= 1) break;
            yield return null;
        }
    }

    //スクロール
    float scrollTime = 0.2f;
    IEnumerator WeaponAreaScroll(int weaponNo)
    {
        float scrollHeight = (float)weaponScrollViewLayout.preferredHeight;
        float ButtonAreaHeight = weaponButtonAreaRectTran.rect.height + descriptionHeight;
        if (ButtonAreaHeight <= scrollHeight) yield break;

        float targetHeight = 0;
        foreach (Transform row in weaponButtonArea)
        {
            if (row.name == weaponNo.ToString()) break;
            targetHeight += buttonHeight;
        }

        float startScrollPos = weaponScrollView.verticalNormalizedPosition;
        float lastScrollPos = 1 - ((targetHeight) / (ButtonAreaHeight - scrollHeight));
        if (lastScrollPos < 0) lastScrollPos = 0;
        float procTime = 0;
        for (;;)
        {
            procTime += Time.deltaTime;
            float rate = procTime / scrollTime;
            weaponScrollView.verticalNormalizedPosition = Mathf.Lerp(startScrollPos, lastScrollPos, rate);
            if (rate >= 1) break;
            yield return null;
        }
    }

    //詳細エリアアニメーション
    float descriptionOpenTime = 0.15f;
    IEnumerator WeaponDescriptionAnimation(Transform targetRowTran, bool isOpen = true)
    {
        if (!isConnectedNetwork) yield break;

        float procTime = 0;
        //詳細エリアスケール
        Vector3 closeScale = new Vector3(1, 0, 1);
        Vector3 openScale = Vector3.one;
        Vector3 startScale = (isOpen) ? closeScale : openScale;
        Vector3 lastScale = (isOpen) ? openScale : closeScale;
        //全体高さ
        float closeHeight = buttonHeight;
        float openHeight = buttonHeight + descriptionHeight;
        float startHeight = (isOpen) ? closeHeight : openHeight;
        float lastHeight = (isOpen) ? openHeight : closeHeight;

        //初期設定
        if (targetRowTran == null) yield break;
        Transform targetTran = targetRowTran.FindChild("WeaponDescription");
        targetTran.localScale = startScale;
        LayoutElement targetRowLayout = targetRowTran.GetComponent<LayoutElement>();
        targetRowLayout.preferredHeight = startHeight;

        for (;;)
        {
            procTime += Time.deltaTime;
            float rate = procTime / descriptionOpenTime;
            if (targetTran == null) yield break;
            targetTran.localScale = Vector3.Lerp(startScale, lastScale, rate);
            targetRowLayout.preferredHeight = Mathf.Lerp(startHeight, lastHeight, rate);
            if (rate >= 1) break;
            yield return null;
        }
    }

    //強化アイコンセット
    private void SetCustom(Transform rowTran, int weaponNo)
    {
        //現在の強化状態
        int nowCustomType = UserManager.GetWeaponCustomType(weaponNo);
        Sprite customIcon = Common.Func.GetCustomIcon(nowCustomType);
        Image customIconImage = rowTran.FindChild("CustomIcon").GetComponent<Image>();
        if (customIcon != null)
        {
            //強化済
            customIconImage.sprite = customIcon;
            customIconImage.preserveAspect = true;
            customIconImage.color = new Color(1, 1, 1, 1);
        }
        else
        {
            //未強化
            customIconImage.color = new Color(1, 1, 1, 0);
        }
    }

    //装備中表示切替
    private void SetEquip(Transform rowTran, bool isEquip, string weaponName = "")
    {
        int equipStatus = (isEquip) ? EQUIP_STATUS_EQUIP : EQUIP_STATUS_NONE;
        SetEquip(rowTran, equipStatus, weaponName);
    }
    private void SetEquip(Transform rowTran, int equipStatus, string weaponName = "")
    {
        if (rowTran == null) return;

        Sprite windowImg = equipWeaponRowOffImg;
        Color textColor = weaponNotSelectedColor;
        Color equipIconColor = new Color(1, 1, 1, 0);
        switch (equipStatus)
        {
            case EQUIP_STATUS_EQUIP:
                windowImg = equipWeaponRowOnImg;
                textColor = weaponSelectedColor;
                equipIconColor = new Color(1, 1, 1, 1);
                break;

            case EQUIP_STATUS_NONE:
                break;

            case EQUIP_STATUS_OTHER:
                equipIconColor = new Color(1, 1, 1, 1);
                textColor = weaponOtherSelectedColor;
                break;
        }
        rowTran.GetComponent<Image>().sprite = windowImg;
        Text rowText = rowTran.FindChild("WeaponName").GetComponent<Text>();
        if (!string.IsNullOrEmpty(weaponName)) rowText.text = weaponName;
        rowText.color = textColor;
        rowTran.FindChild("EquipIcon").GetComponent<Image>().color = equipIconColor;
    }

    //武器選択エリア初期
    private void initWeaponList()
    {
        //ボタン削除
        foreach (Transform child in weaponButtonArea)
        {
            Destroy(child.gameObject);
        }
    }

    //武器選択リストクローズ
    private void CloseWeaponList()
    {
        weaponSelectArea.gameObject.SetActive(false);
        SwitchPointGetArea(false);
    }

    //キャラ切り替え
    public void CharaSelect(bool isRight = true)
    {
        int factor = 1;
        if (isRight) factor *= -1;

        //テーブルIndex
        tableIndex += factor;
        if (tableIndex < 0)
        {
            tableIndex = spawnPoints.Count - 1;
        }
        else if (tableIndex >= spawnPoints.Count)
        {
            tableIndex = 0;
        }

        //キャラIndex
        charaIndex += factor;
        if (charaIndex < 0)
        {
            charaIndex = selectableCharaList.Count - 1;
        }
        else if (charaIndex >= selectableCharaList.Count)
        {
            charaIndex = 0;
        }

        //カラーIndex
        colorIndex = 0;

        StartCoroutine(TurnCharaTable(charaChangeAngle * factor, charaChangeTime, true));
        SpawnCharacter();
    }

    //カラー切り替え
    public void ColorSelect()
    {
        colorIndex++;
        foreach (Transform child in spawnPoints[tableIndex])
        {
            Destroy(child.gameObject);
        }
        SpawnCharacter();
    }

    //キャラテーブル移動制御
    IEnumerator TurnCharaTable(float angle, float time, bool isArrowActive)
    {
        for (;;)
        {
            if (!isTurnTable) break;
            yield return null;
        }
        isTurnTable = true;
        Quaternion startQuat = charaTable.localRotation;
        charaLeftArrow.SetActive(false);
        charaRightArrow.SetActive(false);
        float totalTime = 0;
        for (;;)
        {
            float frameTime = Time.deltaTime;
            totalTime += frameTime;
            if (totalTime > time) frameTime = totalTime - time;
            charaTable.Rotate(Vector3.up, angle * frameTime / time);
            if (totalTime >= time) break;
            yield return null;
        }
        charaTable.localRotation = startQuat * Quaternion.AngleAxis(angle, Vector3.up);
        isTurnTable = false;
        charaLeftArrow.SetActive(isArrowActive);
        charaRightArrow.SetActive(isArrowActive);
    }

    //オブジェクト名からpartsNo取得
    private int GetSelectPartsNo(string partsSelectName)
    {
        int no = -1;
        foreach (int partsNo in partsSelectNameMap.Keys)
        {
            if (partsSelectNameMap[partsNo] == partsSelectName)
            {
                no = partsNo;
                break;
            }
        }
        return no;
    }

    //parts枠画像セット
    private void SetPartsFrame(int partsNo, bool flg)
    {
        if (!partsFrameMap.ContainsKey(partsNo)) return;

        foreach (Image img in partsFrameMap[partsNo])
        {
            if (flg)
            {
                img.sprite = partsSelectedSprite;
            }
            else
            {
                img.sprite = partsNotSelectedSprite;
            }
        }
    }

    //Bitアイコンセット
    private void SetBitIcon(int partsNo, GameObject weaponObj)
    {
        if (!bitImgMap.ContainsKey(partsNo)) return;

        Sprite bitSprite = null;
        if (weaponObj != null)
        {
            int bitType = weaponObj.GetComponent<WeaponController>().GetBitMotion();
            switch (bitType)
            {
                case Common.CO.BIT_MOTION_TYPE_GUN:
                    //銃タイプ
                    switch (partsNo)
                    {
                        case Common.CO.PARTS_LEFT_HAND_NO:
                        case Common.CO.PARTS_LEFT_HAND_DASH_NO:
                            bitSprite = bitTypeSprites[1];
                            break;

                        case Common.CO.PARTS_RIGHT_HAND_NO:
                        case Common.CO.PARTS_RIGHT_HAND_DASH_NO:
                            bitSprite = bitTypeSprites[2];
                            break;

                        default:
                            bitSprite = bitTypeSprites[3];
                            break;
                    }
                    break;

                case Common.CO.BIT_MOTION_TYPE_MISSILE:
                    //ミサイルタイプ
                    bitSprite = bitTypeSprites[3];
                    break;

                case Common.CO.BIT_MOTION_TYPE_LASER:
                    //レーザータイプ
                    bitSprite = bitTypeSprites[4];
                    break;

                default:
                    //その他
                    bitSprite = bitTypeSprites[0];
                    break;
            }
        }

        foreach (Image img in bitImgMap[partsNo])
        {
            if (bitSprite == null)
            {
                img.enabled = false;
            }
            else
            {
                img.enabled = true;
            }
            img.sprite = bitSprite;
        }
    }

    public void PlayMovie()
    {
        PlayGacha();
    }

    //ポイントGETエリア表示切替
    protected void SwitchPointGetArea(bool flg)
    {
        if (!isConnectedNetwork) flg = false;
        SwitchBonusText();
        playMovieObj.SetActive(flg);
    }
}

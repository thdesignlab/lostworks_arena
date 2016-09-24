using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class CustomManager : Photon.MonoBehaviour
{
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
    private RectTransform weaponDetailArea;

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
    private GameObject selectedWeaponButton;
    [SerializeField]
    private Color weaponOtherSelectedColor = Color.gray;
    [SerializeField]
    private Color weaponSelectedColor = Color.blue;
    [SerializeField]
    private Color weaponNotSelectedColor = Color.yellow;
    private int maxWeaponButtonCount = 8;
    private float buttonHeight;
    
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
    private float charaSideAngle = 10.0f;
    private float charaSideTime = 0.2f;
    private bool isTurnTable = false;

    //キャンバスステータス
    //private bool isSelectedParts = false;
    private int selectedPartsNo = -1;
    private float selectModeTime = 0.2f;
    private Vector3 startWeaponListPos;
    private Vector3 lastWeaponListPos;
    private Vector3 startWeaponDetailPos;
    private Vector3 lastWeaponDetailPos;

    //武器セレクトエリア
    private ScrollRect weaponScrollView;
    private LayoutElement weaponScrollViewLayout;
    private RectTransform weaponButtonAreaRectTran;
    private Transform weaponButtonArea;
    private Text weaponDescriptionText;
    //private GameObject weaponArrowU;
    //private GameObject weaponArrowD;

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


    void Awake()
    {
        //UI初期設定
        startWeaponListPos = weaponSelectArea.localPosition;
        lastWeaponListPos = startWeaponListPos + Vector3.right * weaponSelectArea.rect.width;
        startWeaponDetailPos = weaponDetailArea.localPosition;
        lastWeaponDetailPos = startWeaponDetailPos + Vector3.left * weaponDetailArea.rect.width;

        //武器リストエリア取得
        Transform weaponScrollViewTran = weaponSelectArea.FindChild("ScrollView");
        weaponScrollView = weaponScrollViewTran.GetComponent<ScrollRect>();
        weaponScrollViewLayout = weaponScrollViewTran.GetComponent<LayoutElement>();
        weaponButtonArea = weaponSelectArea.FindChild("ScrollView/Viewport/ButtonArea");
        weaponButtonAreaRectTran = weaponButtonArea.GetComponent<RectTransform>();
        weaponDescriptionText = weaponDetailArea.FindChild("ScrollView/Viewport/Text").GetComponent<Text>();
        buttonHeight = selectedWeaponButton.GetComponent<LayoutElement>().preferredHeight + weaponButtonArea.GetComponent<VerticalLayoutGroup>().spacing;
        //weaponArrowU = weaponScrollViewTran.FindChild("WeaponArrowU").gameObject;
        //weaponArrowD = weaponScrollViewTran.FindChild("WeaponArrowD").gameObject;

        partsNameText.text = "";
    }

    void Start()
    {
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
        if (colorChangeButton != null)
        {
            colorChangeButton.SetActive(selectableCharaList[charaIndex].Count >= 2);
        }

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

        float minSwitpeDiff = 1;
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

        //部位名表示
        partsNameText.text = tapObj.name;

        //選択していた部位背景元に戻す
        SetPartsFrame(selectedPartsNo, false);
        if (selectedPartsNo == -1)
        {
            //部位未選択時キャラを脇へ移動
            StartCoroutine(TurnCharaTable(charaSideAngle, charaSideTime, false));
        }

        //partsNo取得
        selectedPartsNo = partsNo;

        //選択部位背景変更
        SetPartsFrame(selectedPartsNo, true);

        partsUpArrow.SetActive(false);
        OpenWeaponList();
    }

    //parts選択解除
    public void PartsSelectOff()
    {
        //Debug.Log("PartsSelectOff");
        //部位名表示
        partsNameText.text = "";

        SetPartsFrame(selectedPartsNo, false);
        if (selectedPartsNo != -1)
        {
            //キャラを中央へ移動
            StartCoroutine(TurnCharaTable(charaSideAngle * -1, charaSideTime, true));
        }
        selectedPartsNo = -1;

        partsUpArrow.SetActive(true);
        CloseWeaponList();
    }

    //武器選択
    private void WeaponSelect(int weaponNo)
    {
        //Debug.Log("WeaponSelect:"+weaponNo);
        if (isWeaponSelecting) return;

        //現在装備中チェック
        string partsName = Common.CO.partsNameArray[selectedPartsNo];

        if (UserManager.userEquipment[partsName] != weaponNo)
        {
            //装備可能チェック
            if (!WeaponStore.Instance.IsEnabledEquip(weaponNo)) return;

            //装備
            StartCoroutine(SetWeapon(weaponNo));
        }
        else
        {
            //試射
            Fire(selectedPartsNo);
        }
    }

    IEnumerator SetWeapon(int weaponNo, bool isFire = true)
    {
        isWeaponSelecting = true;
        DialogController.OpenMessage(DialogController.MESSAGE_LOADING, DialogController.MESSAGE_POSITION_RIGHT);
        for (;;)
        {
            if (IsEnabledFire()) break;
            yield return null;
        }
        DialogController.CloseMessage();

        //武器説明表示
        SetWeaponDescription(weaponNo);

        //装備
        GameObject weaponObj = EquipWeapon(selectedPartsNo, weaponNo);

        //Bit画像設定
        SetBitIcon(selectedPartsNo, weaponObj);

        //武器文字色変更
        foreach (Transform btn in weaponButtonArea)
        {
            btn.FindChild("Text").GetComponent<Text>().color = GetWeaponTextColor(int.Parse(btn.name));
        }

        //試射
        if (isFire) Fire(selectedPartsNo);
        isWeaponSelecting = false;
    }

    //武器文字色取得
    private Color GetWeaponTextColor(int weaponNo)
    {
        Color col = weaponNotSelectedColor;
        if (UserManager.userEquipment.ContainsValue(weaponNo))
        {
            if (UserManager.userEquipment[Common.CO.partsNameArray[selectedPartsNo]] == weaponNo)
            {
                col = weaponSelectedColor;
            }
            else
            {
                col = weaponOtherSelectedColor;
            }
        }

        return col;
    }

    //武器選択リストオープン
    private void OpenWeaponList()
    {
        //Debug.Log("OpenWeaponList");

        //武器リスト初期化
        initWeaponList();

        //装備中武器取得
        int nowWeaponNo = UserManager.userEquipment[Common.CO.partsNameArray[selectedPartsNo]];

        //装備可能武器取得
        List<int> weaponNoList = WeaponStore.Instance.GetSelectableWeaponNoList(selectedPartsNo, true);

        //ボタン作成
        foreach (int weaponNo in weaponNoList)
        {
            int paramWeaponNo = weaponNo;
            string weaponName = Common.Weapon.GetWeaponName(paramWeaponNo);
            GameObject btn = (GameObject)GameObject.Instantiate(selectedWeaponButton, Vector3.zero, Quaternion.identity);
            btn.name = paramWeaponNo.ToString();
            btn.transform.SetParent(weaponButtonArea, false);
            btn.GetComponent<Button>().onClick.AddListener(() => WeaponSelect(paramWeaponNo));
            Text btnText = btn.transform.FindChild("Text").GetComponent<Text>();
            btnText.text = weaponName;
            btnText.color = GetWeaponTextColor(int.Parse(btn.name));
        }

        //装備中武器説明表示
        SetWeaponDescription(nowWeaponNo);

        //エリアサイズ変更
        float dispWeaponCount = weaponNoList.Count;
        if (dispWeaponCount > maxWeaponButtonCount) dispWeaponCount = maxWeaponButtonCount + 0.5f;
        weaponScrollViewLayout.preferredHeight = dispWeaponCount * buttonHeight;
        weaponButtonAreaRectTran.sizeDelta = new Vector2(weaponButtonAreaRectTran.rect.width, weaponNoList.Count * buttonHeight);

        //エリア移動
        StartCoroutine(MoveObject(weaponSelectArea, startWeaponListPos, lastWeaponListPos, selectModeTime));
        StartCoroutine(MoveObject(weaponDetailArea, startWeaponDetailPos, lastWeaponDetailPos, selectModeTime));
    }

    //武器選択エリア初期
    private void initWeaponList()
    {
        //ボタン削除
        foreach (Transform child in weaponButtonArea)
        {
            Destroy(child.gameObject);
        }

        //エリア初期化
        weaponScrollViewLayout.preferredHeight = 0;
        weaponButtonAreaRectTran.sizeDelta = new Vector2(weaponButtonAreaRectTran.rect.width, 0);
        weaponScrollView.verticalNormalizedPosition = 1;

        //説明クリア
        weaponDescriptionText.text = "";
    }

    //武器説明表示
    private void SetWeaponDescription(int weaponNo, GameObject weaponObj = null)
    {
        string[] weaponInfo = Common.Weapon.GetWeaponInfo(weaponNo);
        if (weaponInfo.Length <= 0) return;
        weaponDescriptionText.text = weaponInfo[Common.Weapon.DETAIL_DESCRIPTION_NO];

        //武器詳細説明取得
        if (weaponObj == null)
        {
            weaponObj = (GameObject)Resources.Load(Common.Func.GetResourceWeapon(weaponInfo[Common.Weapon.DETAIL_PREFAB_NAME_NO]));
        }
        
        string detailDescription = weaponObj.GetComponent<WeaponController>().GetDescriptionText();
        if (detailDescription != "")
        {
            if (weaponDescriptionText.text != "") weaponDescriptionText.text += "\n";
            weaponDescriptionText.text += detailDescription;
        }
    }

    //武器選択リストクローズ
    private void CloseWeaponList()
    {
        //Debug.Log("CloseWeaponList");
        StartCoroutine(MoveObject(weaponSelectArea, lastWeaponListPos, startWeaponListPos, selectModeTime));
        StartCoroutine(MoveObject(weaponDetailArea, lastWeaponDetailPos, startWeaponDetailPos, selectModeTime));
    }

    //キャラ切り替え
    public void CharaSelect(bool isRight = true)
    {
        //Debug.Log("CharaSelect");

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

    //UI移動制御
    IEnumerator MoveObject(RectTransform rectTran, Vector3 startVector, Vector3 lastVector, float time)
    {
        isTurnTable = true;
        float totalTime = 0;
        for (;;)
        {
            totalTime += Time.deltaTime;
            if (totalTime > time) totalTime = time;
            rectTran.localPosition = Vector3.Lerp(startVector, lastVector, totalTime / time);
            if (totalTime >= time) break;
            yield return null;
        }
        rectTran.localPosition = lastVector;
        isTurnTable = false;
    }

    //キャラテーブル移動制御
    IEnumerator TurnCharaTable(float angle, float time, bool isArrowActive)
    {
        Quaternion startQuat = charaTable.localRotation;
        isTurnTable = true;
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
}

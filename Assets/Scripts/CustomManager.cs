using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CustomManager : Photon.MonoBehaviour
{
    [SerializeField]
    private Transform charaTable;
    [SerializeField]
    private List<Transform> spawnPoints;
    [SerializeField]
    private float charaSize;
    [SerializeField]
    private Text partsNameText;
    [SerializeField]
    private Transform partsSelectArea;
    //[SerializeField]
    //private List<Image> bitImages;
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
    private Sprite partsSelectedSprite;
    [SerializeField]
    private Sprite partsNotSelectedSprite;

    [SerializeField]
    private List<Sprite> bitTypeSprites;

    [SerializeField]
    private Color weaponSelectedColor = Color.gray;
    [SerializeField]
    private Color weaponNotSelectedColor = Color.yellow;

    private WeaponStore weaponStore;

    private Transform charaTran;
    private PlayerController playerCtrl;
    private GameObject[] equipWeapons;

    //キャラテーブルステータス
    private int charaIndex = 0;
    private int tableIndex = 0;
    private float charaChangeAngle = 120.0f;
    private float charaChangeTime = 1.0f;
    private float charaSideAngle = 10.0f;
    private float charaSideTime = 0.2f;
    private bool isTurnTable = false;

    //キャンバスステータス
    //private bool isSelectedParts = false;
    private Image selectedPartsImage;
    private float selectModeTime = 0.2f;
    private Vector3 startWeaponListPos;
    private Vector3 lastWeaponListPos;
    private Vector3 startWeaponDetailPos;
    private Vector3 lastWeaponDetailPos;

    private Dictionary<int, List<Image>> bitImgMap = new Dictionary<int, List<Image>>();
    private int fireNo = 0;

    private const string PARTS_AREA_NAME_LEFT = "Left-Weapon";
    private const string PARTS_AREA_NAME_LEFT_DASH = "Left-Dash-Weapon";
    private const string PARTS_AREA_NAME_RIGHT = "Shoulder-Weapon";
    private const string PARTS_AREA_NAME_RIGHT_DASH = "Shoulder-Dash-Weapon";
    private const string PARTS_AREA_NAME_SHOULDER = "Right-Weapon";
    private const string PARTS_AREA_NAME_SHOULDER_DASH = "Right-Dash-Weapon";
    private const string PARTS_AREA_NAME_SUB = "Sub-Weapon";

    private const string BIT_IMG_AREA = "BitImg";

    void Awake()
    {
        weaponStore = GameObject.Find("WeaponStore").GetComponent<WeaponStore>();

        //UI初期設定
        startWeaponListPos = weaponSelectArea.localPosition;
        lastWeaponListPos = startWeaponListPos + Vector3.right * weaponSelectArea.rect.width;
        startWeaponDetailPos = weaponDetailArea.localPosition;
        lastWeaponDetailPos = startWeaponDetailPos + Vector3.left * weaponDetailArea.rect.width;

        partsNameText.text = "";

    }

    void Start()
    {
        //PartsNo紐付け
        bitImgMap[Common.CO.PARTS_LEFT_HAND_NO] = new List<Image>();
        bitImgMap[Common.CO.PARTS_LEFT_HAND_DASH_NO] = new List<Image>();
        bitImgMap[Common.CO.PARTS_RIGHT_HAND_NO] = new List<Image>();
        bitImgMap[Common.CO.PARTS_RIGHT_HAND_DASH_NO] = new List<Image>();
        bitImgMap[Common.CO.PARTS_SHOULDER_NO] = new List<Image>();
        bitImgMap[Common.CO.PARTS_SHOULDER_DASH_NO] = new List<Image>();
        bitImgMap[Common.CO.PARTS_SUB_NO] = new List<Image>();
        foreach (Transform partsAreaCopy in partsSelectArea)
        {
            bitImgMap[Common.CO.PARTS_LEFT_HAND_NO].Add(partsAreaCopy.FindChild(PARTS_AREA_NAME_LEFT + "/" + BIT_IMG_AREA).GetComponent<Image>());
            bitImgMap[Common.CO.PARTS_LEFT_HAND_DASH_NO].Add(partsAreaCopy.FindChild(PARTS_AREA_NAME_LEFT_DASH + "/" + BIT_IMG_AREA).GetComponent<Image>());
            bitImgMap[Common.CO.PARTS_RIGHT_HAND_NO].Add(partsAreaCopy.FindChild(PARTS_AREA_NAME_RIGHT + "/" + BIT_IMG_AREA).GetComponent<Image>());
            bitImgMap[Common.CO.PARTS_RIGHT_HAND_DASH_NO].Add(partsAreaCopy.FindChild(PARTS_AREA_NAME_RIGHT_DASH + "/" + BIT_IMG_AREA).GetComponent<Image>());
            bitImgMap[Common.CO.PARTS_SHOULDER_NO].Add(partsAreaCopy.FindChild(PARTS_AREA_NAME_SHOULDER + "/" + BIT_IMG_AREA).GetComponent<Image>());
            bitImgMap[Common.CO.PARTS_SHOULDER_DASH_NO].Add(partsAreaCopy.FindChild(PARTS_AREA_NAME_SHOULDER_DASH + "/" + BIT_IMG_AREA).GetComponent<Image>());
            bitImgMap[Common.CO.PARTS_SUB_NO].Add(partsAreaCopy.FindChild(PARTS_AREA_NAME_SUB + "/" + BIT_IMG_AREA).GetComponent<Image>());
        }


        //ユーザー情報取得

        //キャラ生成
        SpawnCharacter();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit raycastHit;
            if (Physics.Raycast(ray, out raycastHit))
            {
                Transform touchTran = raycastHit.transform;
                WeaponController weaponCtrl = null;
                if (touchTran == charaTran)
                {
                    //キャラタッチ
                    if (equipWeapons.Length > 0)
                    {
                        GameObject weaponObj = equipWeapons[fireNo % equipWeapons.Length];
                        if (weaponObj != null)
                        {
                            weaponCtrl = weaponObj.GetComponent<WeaponController>();
                            playerCtrl.CustomSceaneFire(weaponCtrl);
                        }
                        fireNo++;
                    }
                }
                else if (touchTran.tag == Common.CO.TAG_WEAPON)
                {
                    //Bitタッチ
                    weaponCtrl = touchTran.GetComponent<WeaponController>();
                    playerCtrl.CustomSceaneFire(weaponCtrl);
                }
            }
        }
    }

    //タイトルへ戻る
    public void GoToTitle()
    {
        PlayerPrefsUtility.Save(Common.PP.USER_CHARACTER, UserManager.userSetCharacter);
        PlayerPrefsUtility.SaveDict<string, int>(Common.PP.USER_EQUIP, UserManager.userEquipment);
        PhotonNetwork.LoadLevel(Common.CO.SCENE_TITLE);
    }

    //キャラ回転
    private void CharacterRotate(float diff)
    {
        charaTran.Rotate(charaTran.up, diff);
    }

    //キャラ生成
    private void SpawnCharacter()
    {
        //★キャラIndexからキャラ取得
        string characterName = "Hero";

        GameObject charaObj = PhotonNetwork.Instantiate(characterName, spawnPoints[tableIndex].position, spawnPoints[tableIndex].rotation, 0);
        Rigidbody charaBody = charaObj.GetComponent<Rigidbody>();
        charaBody.useGravity = false;
        charaBody.isKinematic = false;
        charaTran = charaObj.transform;
        playerCtrl = charaTran.GetComponent<PlayerController>();
        charaTran.localScale = new Vector3(charaSize, charaSize, charaSize);
        charaTran.parent = spawnPoints[tableIndex];

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
            Destroy(child.gameObject);
        }

        //武器取得
        string weaponName = Common.Weapon.GetWeaponName(weaponNo);

        //武器召喚
        GameObject ob = PhotonNetwork.Instantiate(Common.Func.GetResourceWeapon(weaponName), parts.position, parts.rotation, 0);
        if (ob == null) return null;
        ob.transform.localScale = new Vector3(charaSize, charaSize, charaSize);
        ob.name = ob.name.Replace("(Clone)", "");
        ob.transform.parent = parts;

        //装備情報保存
        UserManager.userEquipment[parts.name] = weaponNo;

        //武器使用準備
        playerCtrl.SetWeapon();

        return ob;
    }

    //装備武器読み込み
    private void WeaponLoad()
    {
        foreach (Transform child in charaTran)
        {
            foreach (int partsNo in Common.CO.partsNameArray.Keys)
            {
                if (child.name == Common.CO.partsNameArray[partsNo])
                {
                    GameObject weaponObj = EquipWeapon(child, UserManager.userEquipment[child.name]);
                    //Bit画像設定
                    SetBitIcon(partsNo, weaponObj);
                    break;
                }
            }
        }
        equipWeapons = GameObject.FindGameObjectsWithTag(Common.CO.TAG_WEAPON);
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
        tapObj = OnTapedObj(e.Input.ScreenPosition);
    }

    void OnTouchEnd(object sender, CustomInputEventArgs e)
    {
        if (!isSwipe)
        {
            if (tapObj != null)
            {
                //Debug.Log(tapObj.name + " : " + tapObj.tag);
                if (tapObj == charaLeftArrow)
                {
                    //キャラ変更左
                    if (!isTurnTable) CharaSelect(false);
                }
                else if (tapObj == charaRightArrow)
                {
                    //キャラ変更右
                    if (!isTurnTable) CharaSelect(true);
                }
                else if (tapObj.tag == "PartsSelect")
                {
                    //Parts選択
                    if (!isTurnTable) PartsSelectOn();
                }
                else if (tapObj.tag == "WeaponSelect")
                {
                    //武器選択
                    WeaponSelect();
                }
                else if (tapObj == weaponCloseArrow)
                {
                    //parts選択解除
                    PartsSelectOff();
                }
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

        if (tapObj == null || tapObj.tag != "PartsSelect")
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

    private GameObject OnTapedObj(Vector3 _scrPos)
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = _scrPos;
        List<RaycastResult> result = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, result);

        GameObject tapOnj = null;
        foreach (RaycastResult hit in result)
        {
            Image img = hit.gameObject.GetComponent<Image>();
            if (img != null)
            {
                tapOnj = hit.gameObject;
                break;
            }
        }
        return tapOnj;
    }


    //#########################
    //##### TourchEvent #####
    //#########################

    //parts選択
    private void PartsSelectOn()
    {
        //Debug.Log("PartsSelectOn");
        //部位名表示
        partsNameText.text = tapObj.name;


        if (selectedPartsImage == null)
        {
            //部位未選択時キャラを脇へ移動
            StartCoroutine(TurnCharaTable(charaSideAngle, charaSideTime, false));
        }
        else
        {
            //選択していた部位背景元に戻す
            selectedPartsImage.sprite = partsNotSelectedSprite;
        }

        //選択部位背景変更
        selectedPartsImage = tapObj.GetComponent<Image>();
        selectedPartsImage.sprite = partsSelectedSprite;

        partsUpArrow.SetActive(false);
        OpenWeaponList();
    }

    //parts選択解除
    private void PartsSelectOff()
    {
        //Debug.Log("PartsSelectOff");
        //部位名表示
        partsNameText.text = "";

        //キャラを中央へ移動
        if (selectedPartsImage != null)
        {
            StartCoroutine(TurnCharaTable(charaSideAngle * -1, charaSideTime, true));
            selectedPartsImage.sprite = partsNotSelectedSprite;
            selectedPartsImage = null;
        }
        partsUpArrow.SetActive(true);
        CloseWeaponList();
    }

    //武器選択
    private void WeaponSelect()
    {
        //Debug.Log("WeaponSelect");

    }

    //武器選択リストオープン
    private void OpenWeaponList()
    {
        //Debug.Log("OpenWeaponList");
        StartCoroutine(MoveObject(weaponSelectArea, startWeaponListPos, lastWeaponListPos, selectModeTime));
        StartCoroutine(MoveObject(weaponDetailArea, startWeaponDetailPos, lastWeaponDetailPos, selectModeTime));
    }

    //武器選択リストクローズ
    private void CloseWeaponList()
    {
        //Debug.Log("CloseWeaponList");
        StartCoroutine(MoveObject(weaponSelectArea, lastWeaponListPos, startWeaponListPos, selectModeTime));
        StartCoroutine(MoveObject(weaponDetailArea, lastWeaponDetailPos, startWeaponDetailPos, selectModeTime));
    }

    //キャラ切り替え
    private void CharaSelect(bool isRight = true)
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

        //★キャラIndex

        StartCoroutine(TurnCharaTable(charaChangeAngle * factor, charaChangeTime, true));
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
        isTurnTable = false;
    }

    //キャラテーブル移動制御
    IEnumerator TurnCharaTable(float angle, float time, bool isArrowActive)
    {
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
        isTurnTable = false;
        charaLeftArrow.SetActive(isArrowActive);
        charaRightArrow.SetActive(isArrowActive);
    }

    //Bitアイコンセット
    private void SetBitIcon(int partsNo, GameObject weaponObj)
    {
        if (!bitImgMap.ContainsKey(partsNo)) return;
        if (weaponObj == null) return;

        int bitType = weaponObj.GetComponent<WeaponController>().GetBitMotion();
        Sprite bitSprite = bitTypeSprites[0];
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

        foreach (Image img in bitImgMap[partsNo])
        {
            if (bitSprite == null) img.enabled = false;
            img.sprite = bitSprite;
        }
    }
}

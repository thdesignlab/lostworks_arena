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
    private List<RectTransform> partsSelectArea;
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
    private Sprite partsSelectedTexture;
    [SerializeField]
    private Sprite partsNotSelectedTexture;

    [SerializeField]
    private List<Texture> weaponTypeTextures;

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

    //キャンバスステータス
    private bool isSelectedParts = false;
    private float selectModeTime = 0.2f;
    private Vector3 startWeaponListPos;
    private Vector3 lastWeaponListPos;
    private Vector3 startWeaponDetailPos;
    private Vector3 lastWeaponDetailPos;


    private int fireNo = 0;

    void Awake()
    {
        weaponStore = GameObject.Find("WeaponStore").GetComponent<WeaponStore>();

        //ユーザー情報取得

        //キャラ生成
        SpawnCharacter();

        //UI初期設定
        startWeaponListPos = weaponSelectArea.localPosition;
        lastWeaponListPos = startWeaponListPos + Vector3.right * weaponSelectArea.rect.width;
        startWeaponDetailPos = weaponDetailArea.localPosition;
        lastWeaponDetailPos = startWeaponDetailPos + Vector3.left * weaponDetailArea.rect.width;
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
        equipWeapons = GameObject.FindGameObjectsWithTag(Common.CO.TAG_WEAPON);

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

    //装備
    private void EquipWeapon(Transform parts, int weaponNo)
    {
        //すでに装備している場合は破棄
        foreach (Transform child in parts)
        {
            Destroy(child.gameObject);
        }

        //武器取得
        GameObject weapon = weaponStore.GetWeapon(parts, weaponNo);

        //武器召喚
        GameObject ob = PhotonNetwork.Instantiate(Common.Func.GetResourceWeapon(weapon.name), parts.position, parts.rotation, 0);
        ob.transform.localScale = new Vector3(charaSize, charaSize, charaSize);
        ob.name = ob.name.Replace("(Clone)", "");
        ob.transform.parent = parts;

        playerCtrl.SetWeapon();
    }

    //装備武器読み込み
    private void WeaponLoad()
    {
        foreach (Transform child in charaTran)
        {
            foreach (string partsName in Common.CO.partsNameArray)
            {
                if (child.name == partsName)
                {
                    //装備箇所の武器No
                    int weaponNo = -1;

                    EquipWeapon(child, weaponNo);
                    break;
                }
            }
        }
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
                    CharaSelect(false);
                }
                else if (tapObj == charaRightArrow)
                {
                    //キャラ変更右
                    CharaSelect(true);
                }
                else if (tapObj.tag == "PartsSelect")
                {
                    //Parts選択
                    PartsSelectOn();
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
        if (!isSelectedParts)
        {
            StartCoroutine(TurnCharaTable(charaSideAngle, charaSideTime));
        }
        isSelectedParts = true;
        charaLeftArrow.SetActive(false);
        charaRightArrow.SetActive(false);
        partsUpArrow.SetActive(false);
        OpenWeaponList();
    }

    //parts選択解除
    private void PartsSelectOff()
    {
        //Debug.Log("PartsSelectOff");
        if (isSelectedParts)
        {
            StartCoroutine(TurnCharaTable(charaSideAngle * -1, charaSideTime));
        }
        isSelectedParts = false;
        charaLeftArrow.SetActive(true);
        charaRightArrow.SetActive(true);
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

        StartCoroutine(TurnCharaTable(charaChangeAngle * factor, charaChangeTime));
        SpawnCharacter();
    }

    //UI移動制御
    IEnumerator MoveObject(RectTransform rectTran, Vector3 startVector, Vector3 lastVector, float time)
    {
        float totalTime = 0;
        for (;;)
        {
            totalTime += Time.deltaTime;
            if (totalTime > time) totalTime = time;
            rectTran.localPosition = Vector3.Lerp(startVector, lastVector, totalTime / time);
            if (totalTime >= time) break;
            yield return null;
        }
    }

    //キャラテーブル移動制御
    IEnumerator TurnCharaTable(float angle, float time)
    {
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
        charaLeftArrow.SetActive(true);
        charaRightArrow.SetActive(true);
    }
}

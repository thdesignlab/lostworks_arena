using UnityEngine;
using System.Collections;

public class CustomManager : Photon.MonoBehaviour
{
    [SerializeField]
    private Transform spawnPoint;
    [SerializeField]
    private float charaSize;
    private Transform charaTran;

    private WeaponStore weaponStore;
    private PlayerController playerCtrl;

    private int fireNo = 0;

    void Awake()
    {
        //ユーザー情報取得


        //キャラ生成
        GameObject charaObj = PhotonNetwork.Instantiate("Hero", spawnPoint.position, spawnPoint.rotation, 0);
        Rigidbody charaBody = charaObj.GetComponent<Rigidbody>();
        charaBody.useGravity = false;
        charaBody.isKinematic = false;
        charaTran = charaObj.transform;
        charaTran.localScale = new Vector3(charaSize, charaSize, charaSize);

        weaponStore = GameObject.Find("WeaponStore").GetComponent<WeaponStore>();
    }

    void Start()
    {
        playerCtrl = charaTran.GetComponent<PlayerController>();

        //装備を呼び出す
        WeaponLoad();
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
                    GameObject[] weapons = GameObject.FindGameObjectsWithTag(Common.CO.TAG_WEAPON);
                    if (weapons.Length > 0)
                    {
                        weaponCtrl = weapons[fireNo % weapons.Length].GetComponent<WeaponController>();
                        fireNo++;
                    }
                }
                else if (touchTran.tag == Common.CO.TAG_WEAPON)
                {
                    //Bitタッチ
                    weaponCtrl = touchTran.GetComponent<WeaponController>();
                }

                //ふぁいやー
                if (weaponCtrl != null) playerCtrl.CustomSceaneFire(weaponCtrl);
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
        ob.name = ob.name.Replace("(Clone)", "");
        ob.transform.parent = parts;

        playerCtrl.SetWeapon();
    }

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
    }

    void OnTouchEnd(object sender, CustomInputEventArgs e)
    {
    }

    void OnSwipe(object sender, CustomInputEventArgs e)
    {
        float preDiffX = e.Input.ScreenPosition.x - prePosX;
        float preDiffY = e.Input.ScreenPosition.y - prePosY;
        prePosX = e.Input.ScreenPosition.x;
        prePosY = e.Input.ScreenPosition.y;

        //キャラ回転
        CharacterRotate(preDiffX * -1);
    }

    void OnFlickStart(object sender, FlickEventArgs e)
    {
    }

    void OnFlickComplete(object sender, FlickEventArgs e)
    {
    }
}

using UnityEngine;
using System.Collections;

public class PlayerSetting : Photon.MonoBehaviour
{
    Transform myTran;

    [SerializeField]
    private PlayerController playerCtrl;
    [SerializeField]
    private GameObject playerCam;
    //[SerializeField]
    //private GameObject playerCanvas;

    public bool isNpc = false;

    const string WEAPON_FOLDER = "Weapon/";

    private GameController gameCtrl;
    private WeaponStore weaponStroe;

    private bool isCustomEnd = false;
    private int customizeTime = 15;
    private int leftCustomizeTime = 0;

    void Awake ()
    {
        myTran = transform;
        gameCtrl = GameObject.Find("GameController").GetComponent<GameController>();
        weaponStroe = GameObject.Find("WeaponStore").GetComponent<WeaponStore>();

        myTran.name = "Hero" + photonView.viewID.ToString();

        if (photonView.isMine)
        {
            //Debug.Log("isMine:"+transform.name);
            if (isNpc)
            {
                myTran.name = Common.CO.NPC_NAME;
                EquipWeaponRandom();
                //Debug.Log("NPC: " + transform.name);
                gameCtrl.SetTarget(myTran);
                gameCtrl.ResetGame();
                isCustomEnd = true;
            }
            else
            {
                //コントローラーを有効
                playerCtrl.enabled = true;
                playerCam.SetActive(true);
                gameCtrl.SetMyTran(myTran);
                EquipWeaponRandom();
                weaponStroe.CustomMenuOpen();
                StartCoroutine(CustomizeCountDown());
            }
            //playerCanvas.SetActive(true);
        }
        else
        {
            //Debug.Log("target:" + transform.name);
            //ターゲットを登録
            gameCtrl.SetTarget(myTran);
            gameCtrl.ResetGame();
            StartCoroutine(CustomizeCountDown());
        }
    }

    IEnumerator CustomizeCountDown()
    {
        isCustomEnd = false;
        leftCustomizeTime = customizeTime;
        for (;;)
        {
            yield return new WaitForSeconds(1);
            leftCustomizeTime--;
            if (isCustomEnd) break;
            if (leftCustomizeTime <= 0)
            {
                weaponStroe.CustomMenuClose();
                break;
            }
        }
    }

    private void EquipWeaponRandom()
    {
        foreach (string partsName in Common.CO.partsNameArray)
        {
            EquipWeapon(myTran.FindChild(partsName));
        }
    }

    public void EquipWeapon(Transform partsTran, GameObject weapon = null)
    {
        if (partsTran == null) return;

        if (weapon == null)
        {
            //ランダム取得
            weapon = weaponStroe.GetWeapon(partsTran);
        }
        SpawnWeapon(partsTran, weapon);
    }

    private void SpawnWeapon(Transform parts, GameObject weapon)
    {
        if (parts == null || weapon == null) return;

        //すでに装備している場合は破棄
        foreach (Transform child in parts)
        {
            //Debug.Log("Del: "+ child.name);
            PhotonNetwork.Destroy(child.gameObject);
        }

        GameObject ob = PhotonNetwork.Instantiate(WEAPON_FOLDER+weapon.name, parts.position, parts.rotation, 0);

        //装備をPartsの子に設定
        int partsViewId = PhotonView.Get(parts.gameObject).viewID;
        int weaponViewId = PhotonView.Get(ob).viewID;
        //Debug.Log(partsViewId.ToString()+" : "+ weaponViewId.ToString());
        object[] args = new object[] { partsViewId, weaponViewId };
        photonView.RPC("SetParentRPC", PhotonTargets.All, args);

        //装備再セット
        StartCoroutine(SetWeapon(parts));
    }

    [PunRPC]
    private void SetParentRPC(int parentViewId, int childViewId)
    {
        PhotonView parent = PhotonView.Find(parentViewId);
        PhotonView child = PhotonView.Find(childViewId);
        if (parent == null || child == null) return;
        child.gameObject.transform.parent = parent.gameObject.transform;
    }

    IEnumerator SetWeapon(Transform parts)
    {
        PlayerController playerCtrl = GetComponent<PlayerController>();
        if (playerCtrl == null) yield break;
        for (;;)
        {
            yield return null;
            WeaponController weaponCtrl = parts.GetComponentInChildren<WeaponController>();
            if (weaponCtrl != null)
            {
                playerCtrl.SetWeapon();
                if (photonView.isMine)
                {
                    if (gameCtrl.isDebugMode)
                    {
                        weaponCtrl.SetEnable(true);
                    }
                    else
                    {
                        weaponCtrl.SetEnable(false, true);
                    }
                }
                break;
            }
        }
    }

    public void CustomEnd()
    {
        if (isCustomEnd) return;
        photonView.RPC("CustomEndRPC", PhotonTargets.All);
    }
    [PunRPC]
    private void CustomEndRPC()
    {
        isCustomEnd = true;
        leftCustomizeTime = 0;
    }
    public bool IsCustomEnd()
    {
        return isCustomEnd;
    }
    public int GetLeftCustomTime()
    {
        return leftCustomizeTime;
    }
}

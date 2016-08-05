using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class PlayerSetting : Photon.MonoBehaviour
{
    Transform myTran;

    [SerializeField]
    private GameObject playerCam;

    private PlayerStatus playerStatus;
    private PlayerController playerCtrl;
    private PlayerMotionController motionCtrl;

    public bool isNpc = false;

    private GameController gameCtrl;
    private WeaponStore weaponStroe;

    private bool isCustomEnd = false;
    private int customizeTime = 15;
    private int leftCustomizeTime = 0;

    private Dictionary<int, int> weaponMap = new Dictionary<int, int>();

    private bool isActiveSceane = true;

    void Awake()
    {
        myTran = transform;
        myTran.name = "Hero" + photonView.viewID.ToString();
        weaponStroe = GameObject.Find("WeaponStore").GetComponent<WeaponStore>();
        playerStatus = myTran.GetComponent<PlayerStatus>();
        playerCtrl = myTran.GetComponent<PlayerController>();
        motionCtrl = myTran.GetComponent<PlayerMotionController>();

        if (SceneManager.GetActiveScene().name == Common.CO.SCENE_CUSTOM)
        {
            //カスタム画面
            isActiveSceane = false;
            playerStatus.enabled = true;
            playerCtrl.enabled = true;
            motionCtrl.enabled = true;
            playerCam.SetActive(false);
            return;
        }

        gameCtrl = GameObject.Find("GameController").GetComponent<GameController>();
        
        if (photonView.isMine)
        {
            //Debug.Log("isMine:"+transform.name);
            if (isNpc)
            {
                myTran.name = Common.CO.NPC_NAME;
                playerStatus.enabled = true;
                EquipWeaponRandom();
                //Debug.Log("NPC: " + transform.name);
                gameCtrl.SetTarget(myTran);
                gameCtrl.SetNpcTran(myTran);
                isCustomEnd = true;
            }
            else
            {
                //コントローラーを有効
                playerStatus.enabled = true;
                playerCtrl.enabled = true;
                motionCtrl.enabled = true;
                playerCam.SetActive(true);
                gameCtrl.SetMyTran(myTran);
                EquipWeaponRandom();
                isCustomEnd = true;
                //weaponStroe.CustomMenuOpen();
                //StartCoroutine(CustomizeCountDown());
            }
            //playerCanvas.SetActive(true);
        }
        else
        {
            //Debug.Log("target:" + transform.name);
            playerStatus.enabled = true;
            playerCtrl.enabled = false;
            motionCtrl.enabled = false;
            playerCam.SetActive(false);

            //ターゲットを登録
            gameCtrl.SetTarget(myTran);
            SetWeaponParent();
            isCustomEnd = true;
            SetCustomStatus();
            //StartCoroutine(CustomizeCountDown());
        }
    }

    void Start()
    {
        if (!isActiveSceane) return;

        if (isNpc || !photonView.isMine)
        {
            gameCtrl.ResetGame();
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
        foreach (Transform child in myTran)
        {
            if (child.name == Common.CO.PARTS_BODY)
            {
                //Bodyの中を検索
                foreach (Transform grandson in child)
                {
                    foreach (string partsName in Common.CO.partsNameArray)
                    {
                        if (grandson.name == partsName)
                        {
                            EquipWeapon(grandson);
                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (string partsName in Common.CO.partsNameArray)
                {
                    if (child.name == partsName)
                    {
                        EquipWeapon(child);
                        break;
                    }
                }
            }
        }
    }

    public void EquipWeapon(Transform partsTran, GameObject weapon = null)
    {
        if (partsTran == null) return;

        if (weapon == null)
        {
            //ランダム取得
            weapon = weaponStroe.GetRandomWeapon(partsTran, weaponMap);
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

        GameObject ob = PhotonNetwork.Instantiate(Common.Func.GetResourceWeapon(weapon.name), parts.position, parts.rotation, 0);
        ob.name = ob.name.Replace("(Clone)", "");

        //装備をPartsの子に設定
        int partsViewId = PhotonView.Get(parts.gameObject).viewID;
        int weaponViewId = PhotonView.Get(ob).viewID;
        weaponMap[partsViewId] = weaponViewId;
        //Debug.Log(partsViewId.ToString()+" : "+ weaponViewId.ToString());
        object[] args = new object[] { partsViewId, weaponViewId };
        photonView.RPC("SetParentRPC", PhotonTargets.Others, args);
        ob.transform.parent = parts.transform;

        //装備再セット
        StartCoroutine(SetWeapon(parts));
    }

    [PunRPC]
    private void SetParentRPC(int parentViewId, int childViewId)
    {
        //Debug.Log("SetParentRPC: " + myTran.name + ": " + parentViewId.ToString() + " >> " + childViewId.ToString());

        PhotonView parent = PhotonView.Find(parentViewId);
        PhotonView child = PhotonView.Find(childViewId);
        if (parent == null || child == null) return;
        child.gameObject.transform.parent = parent.gameObject.transform;
        child.gameObject.transform.localPosition = Vector3.zero;
    }

    IEnumerator SetWeapon(Transform parts)
    {
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
    private void SetWeaponParent()
    {
        photonView.RPC("SetWeaponParentRPC", PhotonTargets.Others);
    }
    [PunRPC]
    private void SetWeaponParentRPC()
    {
        if (photonView.isMine)
        {
            //Debug.Log("[SetWeaponParent]" + myTran.name + ": " + weaponMap.Count.ToString());
            foreach (int key in weaponMap.Keys)
            {
                object[] args = new object[] { key, weaponMap[key] };
                photonView.RPC("SetParentRPC", PhotonTargets.Others, args);
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
        //Debug.Log(myTran.name+" : "+isCustomEnd.ToString());
        return isCustomEnd;
    }
    public int GetLeftCustomTime()
    {
        return leftCustomizeTime;
    }
    private void SetCustomStatus()
    {
        photonView.RPC("SetCustomStatusRPC", PhotonTargets.All);
    }
    [PunRPC]
    private void SetCustomStatusRPC()
    {
        if (photonView.isMine && isCustomEnd)
        {
            photonView.RPC("CustomEndRPC", PhotonTargets.Others);
        }
    }

}

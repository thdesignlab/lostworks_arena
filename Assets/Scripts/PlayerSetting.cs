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
    private int bodyViewId;

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

            //メインボディ生成
            CreateMainBody();
            return;
        }

        gameCtrl = GameObject.Find("GameController").GetComponent<GameController>();

        if (photonView.isMine)
        {
            //Debug.Log("isMine:"+transform.name);
            if (isNpc)
            {
                //##### NPCの場合 #####
                //NPC名変更
                myTran.name = Common.CO.NPC_NAME;

                //コントローラー設定
                playerStatus.enabled = true;

                //メインボディ生成
                CreateNpcBody();

                //装備設定
                EquipWeaponRandom();
                //myTran.GetComponent<NpcController>().SetWeapons();

                //NPC情報設定
                gameCtrl.SetTarget(myTran);
                gameCtrl.SetNpcTran(myTran);

                //カスタマイズ完了FLG
                isCustomEnd = true;

                gameCtrl.ResetGame();
            }
            else
            {
                //##### 自分のキャラクターの場合 #####
                myTran.name = UserManager.userInfo[Common.PP.INFO_USER_NAME];

                //コントローラーを有効化
                playerStatus.enabled = true;
                playerCtrl.enabled = true;
                motionCtrl.enabled = true;
                playerCam.SetActive(true);

                //自分の情報を保存
                gameCtrl.SetMyTran(myTran);

                //メインボディ生成
                CreateMainBody();

                //装備設定
                EquipWeaponUserInfo();
                isCustomEnd = true;
                //weaponStroe.CustomMenuOpen();
                //StartCoroutine(CustomizeCountDown());
            }
            //playerCanvas.SetActive(true);
        }
        else
        {
            //##### 対戦相手のキャラクターの場合 #####
            //Debug.Log("target:" + transform.name);
            //コントローラーを無効化
            playerStatus.enabled = true;
            playerCtrl.enabled = false;
            motionCtrl.enabled = false;
            playerCam.SetActive(false);

            //ターゲットを登録
            gameCtrl.SetTarget(myTran);

            //親子設定
            SetMainBodyParent();
            SetWeaponParent();

            //カスタマイズ完了FLG
            isCustomEnd = true;
            SetCustomStatus();
            //StartCoroutine(CustomizeCountDown());
        }
    }

    //void Start()
    //{
    //    if (!isActiveSceane) return;

    //    if (isNpc || !photonView.isMine)
    //    {
    //        gameCtrl.ResetGame();
    //    }
    //}
    private void CreateNpcBody()
    {
        //NpcNo取得
        int stageNo = gameCtrl.GetStageNo();

        //メインボディ名取得
        string npcName = "Npc";

        //メインボディ生成
        GameObject charaMainObj = PhotonNetwork.Instantiate(Common.Func.GetResourceCharacter(npcName), Vector3.zero, Quaternion.identity, 0);
        charaMainObj.name = Common.CO.PARTS_BODY;
        Transform charaMainTran = charaMainObj.transform;

        //メインボディ紐付け
        charaMainTran.SetParent(myTran, false);
        charaMainTran.localPosition = Vector3.zero;
        charaMainTran.rotation = myTran.rotation;
    }

    private void CreateMainBody()
    {
        //キャラ情報取得
        int charaNo = UserManager.userSetCharacter;
        string[] charaInfo = CharacterManager.GetCharacterInfo(charaNo);
        if (charaInfo == null)
        {
            charaInfo = CharacterManager.GetCharacterInfo(0);
        }

        //メインボディ生成
        GameObject charaMainObj = PhotonNetwork.Instantiate(Common.Func.GetResourceCharacter(charaInfo[Common.Character.DETAIL_PREFAB_NAME_NO]), Vector3.zero, Quaternion.identity, 0);
        charaMainObj.name = Common.CO.PARTS_BODY;
        Transform charaMainTran = charaMainObj.transform;

        //メインボディ紐付け
        charaMainTran.SetParent(myTran, false);
        charaMainTran.localPosition = Vector3.zero;
        charaMainTran.rotation = myTran.rotation;
        //lockonCtrl.enabled = true;

        //parent紐付け用
        bodyViewId = PhotonView.Get(charaMainObj).viewID;
    }

    private void SetMainBodyParent()
    {
        photonView.RPC("SetMainBodyParentRPC", PhotonTargets.Others);
    }
    [PunRPC]
    private void SetMainBodyParentRPC()
    {
        if (photonView.isMine)
        {
            //Debug.Log(myTran.name+" : "+PhotonView.Get(gameObject).viewID+" >> "+ bodyViewId);
            object[] args = new object[] { PhotonView.Get(gameObject).viewID, bodyViewId };
            photonView.RPC("SetParentRPC", PhotonTargets.Others, args);
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

    private void EquipWeaponUserInfo()
    {
        foreach (int partsNo in Common.CO.partsNameArray.Keys)
        {
            //部位取得
            string partsName = Common.Func.GetPartsStructure(Common.CO.partsNameArray[partsNo]);
            Transform parts = myTran.FindChild(partsName);
            if (parts != null)
            {
                if (!UserManager.userEquipment.ContainsKey(parts.name)) continue;

                //武器取得
                string weaponName = Common.Weapon.GetWeaponName(UserManager.userEquipment[parts.name], true);
                GameObject weapon = (GameObject)Resources.Load(Common.Func.GetResourceWeapon(weaponName));

                //装備
                EquipWeapon(parts, weapon);
            }
        }
    }

    private void EquipWeaponRandom()
    {
        foreach (int partsNo in Common.CO.partsNameArray.Keys)
        {
            //部位取得
            string partsName = Common.Func.GetPartsStructure(Common.CO.partsNameArray[partsNo]);
            Transform parts = myTran.FindChild(partsName);
            if (parts != null)
            {
                //装備
                EquipWeapon(parts);
            }
        }

        //foreach (Transform child in myTran)
        //{
        //    foreach (int key in Common.CO.partsNameArray.Keys)
        //    {
        //        if (child.name == Common.CO.partsNameArray[key])
        //        {
        //            EquipWeapon(child);
        //            break;
        //        }
        //    }
        //}
    }

    public void EquipWeapon(Transform partsTran, GameObject weapon = null)
    {
        if (partsTran == null) return;

        if (weapon == null)
        {
            //ランダム取得(NPC用)
            weapon = weaponStroe.GetRandomWeaponForNpc(partsTran, weaponMap);
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
        ob.transform.SetParent(parts.transform, true);

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
        child.gameObject.transform.SetParent(parent.gameObject.transform, false);
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
                if (photonView.isMine && isActiveSceane)
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

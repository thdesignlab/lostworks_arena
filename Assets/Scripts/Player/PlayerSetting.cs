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
    string[] charaInfo = new string[] { };

    private bool isCustomEnd = false;
    private int customizeTime = 15;
    private int leftCustomizeTime = 0;

    private Dictionary<int, int> weaponMap = new Dictionary<int, int>();

    private bool isActiveSceane = true;

    void Awake()
    {
        myTran = transform;
        myTran.name = "Hero" + photonView.viewID.ToString();
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
            //playerCam.SetActive(false);

            //メインボディ生成
            CreateMainBody();
            return;
        }

        if (photonView.isMine)
        {
            //Debug.Log("isMine:"+transform.name);
            if (isNpc)
            {
                //##### NPCの場合 #####
                //コントローラー設定
                playerStatus.enabled = true;
                motionCtrl.enabled = true;


                //NPC名変更
                charaInfo = Common.Character.GetCharacterInfo(GameController.Instance.npcNo);
                myTran.name = charaInfo.Length > 0 ? charaInfo[Common.Character.DETAIL_NAME_NO] : "NPC";

                //メインボディ生成
                CreateNpcBody();

                //装備設定
                EquipWeaponNpc();

                //NPC情報設定
                GameController.Instance.SetTarget(myTran);
                GameController.Instance.SetNpcTran(myTran);

                //カスタマイズ完了FLG
                isCustomEnd = true;

                GameController.Instance.ResetGame();
            }
            else
            {
                //##### 自分のキャラクターの場合 #####
                myTran.name = UserManager.userInfo[Common.PP.INFO_USER_NAME];

                //コントローラーを有効化
                playerStatus.enabled = true;
                playerCtrl.enabled = true;
                motionCtrl.enabled = true;
                //playerCam.SetActive(true);

                //カメラ紐づけ
                Transform camTran = playerCam.transform;
                Camera.main.transform.SetParent(myTran);
                Camera.main.transform.localPosition = camTran.localPosition;
                Camera.main.transform.localRotation = camTran.localRotation;

                //自分の情報を保存
                GameController.Instance.SetMyTran(myTran);

                //メインボディ生成
                CreateMainBody();

                //装備設定
                EquipWeaponUserInfo();
                isCustomEnd = true;
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
            //playerCam.SetActive(false);

            //ターゲットを登録
            GameController.Instance.SetTarget(myTran);

            //親子設定
            SetMainBodyParent();
            SetWeaponParent();

            //カスタマイズ完了FLG
            isCustomEnd = true;
            SetCustomStatus();
        }
    }

    private void CreateNpcBody()
    {
        //メインボディ名取得
        string npcName = charaInfo[Common.Character.DETAIL_PREFAB_NAME_NO];

        //メインボディ生成
        GameObject charaMainObj = PhotonNetwork.Instantiate(Common.Func.GetResourceCharacter(npcName), Vector3.zero, Quaternion.identity, 0);
        charaMainObj.name = Common.CO.PARTS_BODY;
        Transform charaMainTran = charaMainObj.transform;

        //カラー設定
        CharacterColor charaColor = charaMainTran.GetComponentInChildren<CharacterColor>();
        if (charaColor != null) charaColor.SetColor(charaInfo[Common.Character.DETAIL_COLOR_NO]);

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
            //charaInfo = CharacterManager.GetCharacterInfo();
            foreach (int index in Common.Character.characterLineUp.Keys)
            {
                charaNo = index;
                charaInfo = Common.Character.characterLineUp[index];
                UserManager.userSetCharacter = charaNo;
                break;
            }
        }

        //メインボディ生成
        GameObject charaMainObj = PhotonNetwork.Instantiate(Common.Func.GetResourceCharacter(charaInfo[Common.Character.DETAIL_PREFAB_NAME_NO]), Vector3.zero, Quaternion.identity, 0);
        charaMainObj.name = Common.CO.PARTS_BODY;
        Transform charaMainTran = charaMainObj.transform;

        //カラー設定
        Transform charaBoxTran = charaMainTran.FindChild(Common.CO.PARTS_MAIN_BODY);
        if (charaBoxTran != null)
        {
            CharacterColor charaColor = charaBoxTran.GetComponentInChildren<CharacterColor>();
            if (charaColor != null) charaColor.SetColor(charaInfo[Common.Character.DETAIL_COLOR_NO]);
        }

        //メインボディ紐付け
        charaMainTran.SetParent(myTran, false);
        charaMainTran.localPosition = Vector3.zero;
        charaMainTran.rotation = myTran.rotation;
        //lockonCtrl.enabled = true;

        //parent紐付け用
        bodyViewId = PhotonView.Get(charaMainObj).viewID;

        //キャラステータス取得
        int[] npcStatusArray = Common.Character.StatusDic[charaNo];
        float[] statusLevelRate = Common.Mission.npcLevelStatusDic[0];

        //ステータス設定
        playerStatus.SetStatus(npcStatusArray, statusLevelRate);
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
                WeaponStore.Instance.CustomMenuClose();
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
                int wepNo = -1;

                //装備中武器取得
                if (!UserManager.userEquipment.ContainsKey(parts.name)) continue;
                wepNo = UserManager.userEquipment[parts.name];
                if (wepNo <= 0 && parts.name == Common.CO.PARTS_EXTRA)
                {
                    int[] weapons = Common.Weapon.GetExtraWeaponNoArray(UserManager.userSetCharacter);
                    wepNo = weapons[0];
                }

                //武器取得
                string weaponName = Common.Weapon.GetWeaponName(wepNo, true);
                GameObject weapon = (GameObject)Resources.Load(Common.Func.GetResourceWeapon(weaponName));
                //装備
                EquipWeapon(parts, weapon);
            }
        }
    }

    private void EquipWeaponNpc()
    {
        //武器リスト取得
        int[] weaponArray = Common.Mission.npcWeaponDic[-1];
        if (Common.Mission.npcWeaponDic.ContainsKey(GameController.Instance.npcNo))
        {
            weaponArray = Common.Mission.npcWeaponDic[GameController.Instance.npcNo];
        }

        foreach (int partsNo in Common.CO.partsNameArray.Keys)
        {
            //武器No取得
            int weaponNo = 0;
            if (partsNo == Common.CO.PARTS_EXTRA_NO)
            {
                weaponNo = Common.Weapon.GetExtraWeaponNo(GameController.Instance.npcNo);
            }
            else if (0 < weaponArray.Length && partsNo < weaponArray.Length)
            {
                weaponNo = weaponArray[partsNo];
            }
            if (weaponNo == -1)
            {
                //ユーザーの武器をコピー
                weaponNo = UserManager.userEquipment[Common.CO.partsNameArray[partsNo]];
            }

            //武器取得
            string weaponName = Common.Weapon.GetWeaponName(weaponNo, true);
            GameObject weapon = (GameObject)Resources.Load(Common.Func.GetResourceWeapon(weaponName));

            //部位取得
            string partsName = Common.Func.GetPartsStructure(Common.CO.partsNameArray[partsNo]);
            Transform parts = myTran.FindChild(partsName);
            if (parts != null)
            {
                //装備
                EquipWeapon(parts, weapon);
            }
        }
    }

    public void EquipWeapon(Transform partsTran, GameObject weapon = null)
    {
        if (partsTran == null) return;

        if (weapon == null)
        {
            //ランダム取得(NPC用)
            weapon = WeaponStore.Instance.GetRandomWeaponForNpc(partsTran, weaponMap);
        }
        SpawnWeapon(partsTran, weapon);
    }

    private void SpawnWeapon(Transform parts, GameObject weapon)
    {
        if (parts == null || weapon == null) return;

        //すでに装備している場合は破棄
        foreach (Transform child in parts)
        {
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
                    if (MyDebug.Instance.isDebugMode)
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

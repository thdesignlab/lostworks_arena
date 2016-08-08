using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

public class WeaponStore : Photon.MonoBehaviour
{
    [SerializeField]
    private GameObject weaponCanvas;
    [SerializeField]
    private GameObject weaponListPanel;
    [SerializeField]
    private GameObject weaponEquipCancelButton;
    [SerializeField]
    private GameObject weaponEquipButton;

    [SerializeField]
    private bool isEnableSomeWeapon;

    //バトル用
    private int customPartsNo = -1;
    private Transform myPlayerTran;
    private List<string> excludeWeapons = new List<string>();


    //装備可能な武器番号を取得する(ユーザー用)
    public List<int> GetSelectableWeaponNoList(int partsNo)
    {
        List<int> selectableWeaponNoList = new List<int>();

        //部位ごとの武器リスト取得
        Dictionary<int, string[]> weaponList = new Dictionary<int, string[]>();
        switch (partsNo)
        {
            case Common.CO.PARTS_LEFT_HAND_NO:
            case Common.CO.PARTS_RIGHT_HAND_NO:
                weaponList = Common.Weapon.handWeaponLineUp;
                break;

            case Common.CO.PARTS_LEFT_HAND_DASH_NO:
            case Common.CO.PARTS_RIGHT_HAND_DASH_NO:
                weaponList = Common.Weapon.handDashWeaponLineUp;
                break;

            case Common.CO.PARTS_SHOULDER_NO:
                weaponList = Common.Weapon.shoulderWeaponLineUp;
                break;

            case Common.CO.PARTS_SHOULDER_DASH_NO:
                weaponList = Common.Weapon.shoulderDashWeaponLineUp;
                break;

            case Common.CO.PARTS_SUB_NO:
                weaponList = Common.Weapon.subWeaponLineUp;
                break;
        }

        //装備可能かチェック
        foreach (int weaponNo in weaponList.Keys)
        {
            bool isEnabled = true;

            //重複チェック
            if (!isEnableSomeWeapon)
            {
                //重複装備不可排除
                foreach (string partsName in UserManager.userEquipment.Keys)
                {
                    if (UserManager.userEquipment[partsName] == weaponNo)
                    {
                        //装備済み
                        isEnabled = false;
                    }
                }
            }

            if (isEnabled)
            {
                //取得済みチェック
                switch (weaponList[weaponNo][Common.Weapon.DETAIL_OBTAIN_TYPE_NO])
                {
                    case Common.Weapon.OBTAIN_TYPE_INIT:
                        //初期所持
                        break;

                    case Common.Weapon.OBTAIN_TYPE_NONE:
                        //使用不可
                        isEnabled = false;
                        break;

                    default:
                        if (!UserManager.userOpenWeapons.Contains(weaponNo))
                        {
                            //未所持
                            isEnabled = false;
                        }
                        break;
                }
            }

            if (!isEnabled) continue;
            selectableWeaponNoList.Add(weaponNo);
        }

        return selectableWeaponNoList;
    }


    // ##### バトル用 #####

    public void SetMyTran()
    {
        myPlayerTran = GameObject.Find("GameController").GetComponent<GameController>().GetMyTran();
    }

    public void CustomMenuOpen()
    {
        weaponCanvas.SetActive(true);

        //現在装備中の名前を表示
        foreach (int key in Common.CO.partsNameArray.Keys)
        {
            SetEquipWeaponName(Common.CO.partsNameArray[key]);
        }
    }

    public void CustomMenuClose()
    {
        weaponCanvas.SetActive(false);
        myPlayerTran.gameObject.GetComponent<PlayerSetting>().CustomEnd();
    }

    public void OnCancelEquipButton()
    {
        weaponListPanel.SetActive(false);
    }

    private void SetEquipWeaponName(string partsName, string weaponName = "")
    {
        if (weaponName == "")
        {
            Transform partsTran = myPlayerTran.FindChild(Common.Func.GetPartsStructure(partsName));
            if (partsTran == null || partsTran.childCount == 0) return;
            Transform weaponTran = partsTran.GetChild(0);
            if (weaponTran != null)
            {
                weaponName = weaponTran.name;
            }
        }

        Transform weaponNameText = weaponCanvas.transform.FindChild("CustomMenu/PartsSelect/" + partsName + "/WeqponName");
        if (weaponNameText == null) return;

        weaponNameText.gameObject.GetComponent<Text>().text = weaponName;
    }

    //装備可能なリストを表示する
    public void OnEquipListButton(int partsNo)
    {
        //中身をリセットする
        foreach (Transform child in weaponListPanel.transform)
        {
            Destroy(child.gameObject);
        }
        GameObject cancelBtn = (GameObject)GameObject.Instantiate(weaponEquipCancelButton, Vector3.zero, Quaternion.identity);
        cancelBtn.transform.SetParent(weaponListPanel.transform, false);
        cancelBtn.GetComponent<Button>().onClick.AddListener(() => OnCancelEquipButton());

        //装備可能武器No
        List<int> selectableWeaponNoList = GetSelectableWeaponNoList(partsNo);
        if (selectableWeaponNoList.Count == 0) return;

        weaponListPanel.SetActive(true);

        //int paramWeaponNo = -1;
        //string weaponName = "";
        foreach (int weaponNo in selectableWeaponNoList)
        {
            int paramWeaponNo = weaponNo;
            string weaponName = Common.Weapon.GetWeaponName(paramWeaponNo);
            GameObject ob = (GameObject)GameObject.Instantiate(weaponEquipButton, Vector3.zero, Quaternion.identity);
            ob.transform.SetParent(weaponListPanel.transform, false);
            ob.GetComponent<Button>().onClick.AddListener(() => OnEquipButton(paramWeaponNo));
            ob.transform.FindChild("Text").GetComponent<Text>().text = weaponName;
        }
        customPartsNo = partsNo;
    }

    //装備する
    public void OnEquipButton(int weaponNo)
    {
        string weaponName = Common.Weapon.GetWeaponName(weaponNo);
        if (weaponName == "") return;

        Transform partsTran = myPlayerTran.FindChild(Common.Func.GetPartsStructure(customPartsNo));
        GameObject weapon = (GameObject)Resources.Load(Common.Func.GetResourceWeapon(weaponName));
        PlayerSetting playerSetting = myPlayerTran.gameObject.GetComponent<PlayerSetting>();
        playerSetting.EquipWeapon(partsTran, weapon);
        if (!playerSetting.isNpc)
        {
            //装備情報保存
            UserManager.userEquipment[partsTran.name] = weaponNo;
            PlayerPrefsUtility.SaveDict<string, int>(Common.PP.USER_EQUIP, UserManager.userEquipment);
        }

        SetEquipWeaponName(Common.CO.partsNameArray[customPartsNo], weaponName);
        weaponListPanel.SetActive(false);
    }

    //武器ランダム取得(NPC用)
    public GameObject GetRandomWeaponForNpc(Transform parts, Dictionary<int, int> weaponMap)
    {
        if (weaponMap != null)
        {
            excludeWeapons = new List<string>();
            foreach (int partsViewId in weaponMap.Keys)
            {
                PhotonView weaponView = PhotonView.Find(weaponMap[partsViewId]);
                excludeWeapons.Add(weaponView.name);
            }
        }

        GameObject weapon = null;
        if (parts == null) return weapon;

        switch (parts.tag)
        {
            case Common.CO.PARTS_KIND_HAND:
                weapon = SelectWeapon(Common.Weapon.handWeaponLineUp);
                break;

            case Common.CO.PARTS_KIND_HAND_DASH:
                weapon = SelectWeapon(Common.Weapon.handDashWeaponLineUp);
                break;

            case Common.CO.PARTS_KIND_SHOULDER:
                weapon = SelectWeapon(Common.Weapon.shoulderWeaponLineUp);
                break;

            case Common.CO.PARTS_KIND_SHOULDER_DASH:
                weapon = SelectWeapon(Common.Weapon.shoulderDashWeaponLineUp);
                break;

            case Common.CO.PARTS_KIND_SUB:
                weapon = SelectWeapon(Common.Weapon.subWeaponLineUp);
                break;
        }

        return weapon;

    }

    //武器リストから武器選択
    private GameObject SelectWeapon(Dictionary<int, string[]> weaponList, int weaponNo = -1)
    {
        if (weaponList == null || weaponList.Count == 0) return null;

        List<int> SelectableWeaponNoList = new List<int>();
        if (weaponNo < 0)
        {
            //ランダム選択(NPC用)
            foreach (int no in weaponList.Keys)
            {
                if (!isEnableSomeWeapon)
                {
                    //重複装備不可
                    if (excludeWeapons.Contains(weaponList[no][Common.Weapon.DETAIL_PREFAB_NAME_NO]))
                    {
                        continue;
                    }
                }
                SelectableWeaponNoList.Add(no);
            }
            if (SelectableWeaponNoList.Count == 0) return null;
            int index = Random.Range(0, SelectableWeaponNoList.Count);
            weaponNo = SelectableWeaponNoList[index];
        }
        string weaponName = weaponList[weaponNo][Common.Weapon.DETAIL_PREFAB_NAME_NO];
        GameObject weapon = (GameObject)Resources.Load(Common.Func.GetResourceWeapon(weaponName));

        return weapon;
    }
}

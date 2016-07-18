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
    private List<GameObject> handWeaponList = new List<GameObject>();
    [SerializeField]
    private List<GameObject> shoulderWeaponList = new List<GameObject>();
    [SerializeField]
    private List<GameObject> subWeaponList = new List<GameObject>();

    private int customPartsNo = -1;
    private List<GameObject> selectableWeaponList = new List<GameObject>();

    [SerializeField]
    private GameObject weaponEquipCancelButton;
    [SerializeField]
    private GameObject weaponEquipButton;

    private Transform myPlayerTran;

    public void SetMyTran()
    {
        myPlayerTran = GameObject.Find("GameController").GetComponent<GameController>().GetMyTran();
    }

    public GameObject GetWeapon(Transform parts, int weaponNo = -1)
    {
        GameObject weapon = null;
        if (parts == null) return weapon;

        switch (parts.tag)
        {
            case "Hand":
                weapon = SelectWeapon(handWeaponList, weaponNo);
                break;

            case "Shoulder":
                weapon = SelectWeapon(shoulderWeaponList, weaponNo);
                break;

            case "Sub":
                weapon = SelectWeapon(subWeaponList, weaponNo);
                break;
        }

        return weapon;
    }

    private GameObject SelectWeapon(List<GameObject> weaponList, int weaponNo)
    {
        if (weaponList == null || weaponList.Count == 0) return null;
        if (weaponNo < 0 || weaponList.Count <= weaponNo)
        {
            weaponNo = Random.Range(0, weaponList.Count);
        }
        return weaponList[weaponNo];
    }

    public void CustomMenuOpen()
    {
        weaponCanvas.SetActive(true);

        //現在装備中の名前を表示
        foreach (string parts in Common.CO.partsNameArray)
        {
            SetEquipWeaponName(parts);
        }
    }
    public void CustomMenuClose()
    {
        weaponCanvas.SetActive(false);
        myPlayerTran.gameObject.GetComponent<PlayerSetting>().CustomEnd();
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

        switch (Common.CO.partsNameArray[partsNo])
        {
            case Common.CO.PARTS_LEFT_HAND:
            case Common.CO.PARTS_RIGHT_HAND:
                selectableWeaponList = handWeaponList;
                break;

            case Common.CO.PARTS_SHOULDER:
                selectableWeaponList = shoulderWeaponList;
                break;

            case Common.CO.PARTS_SUB:
                selectableWeaponList = subWeaponList;
                break;
        }

        if (selectableWeaponList.Count == 0) return;

        customPartsNo = partsNo;
        weaponListPanel.SetActive(true);

        int index = 0;
        foreach (GameObject weapon in selectableWeaponList)
        {
            int weaponNo = index;
            GameObject ob = (GameObject)GameObject.Instantiate(weaponEquipButton, Vector3.zero, Quaternion.identity);
            ob.transform.SetParent(weaponListPanel.transform, false);
            ob.GetComponent<Button>().onClick.AddListener(() => OnEquipButton(weaponNo));
            ob.transform.FindChild("Text").GetComponent<Text>().text = weapon.name;
            index++;
        }
    }

    //装備する
    public void OnEquipButton(int index = 0)
    {
        if (index < 0 || selectableWeaponList.Count <= index) return;

        Transform partsTran = myPlayerTran.FindChild(Common.CO.partsNameArray[customPartsNo]);
        myPlayerTran.gameObject.GetComponent<PlayerSetting>().EquipWeapon(partsTran, selectableWeaponList[index]);

        SetEquipWeaponName(Common.CO.partsNameArray[customPartsNo], selectableWeaponList[index].name);
        weaponListPanel.SetActive(false);
    }
    private void SetEquipWeaponName(string partsName, string weaponName = "")
    {
        if (weaponName == "")
        {
            Transform partsTran = myPlayerTran.FindChild(partsName);
            if (partsTran == null || partsTran.childCount == 0) return;
            Transform weaponTran = partsTran.GetChild(0);
            if (weaponTran != null)
            {
                weaponName = weaponTran.name;
            }
        }

        Transform weaponNameText = weaponCanvas.transform.FindChild("CustomMenu/PartsSelect/" + partsName + "/WeqponName");
        if (weaponNameText == null) return;

        weaponNameText.gameObject.GetComponent<Text>().text = weaponName.Replace("(Clone)", "");
    }

    public void OnCancelEquipButton()
    {
        weaponListPanel.SetActive(false);
    }
}
